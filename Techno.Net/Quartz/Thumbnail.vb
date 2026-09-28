Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Runtime.InteropServices

Namespace Quartz

    ''' <summary>
    ''' Port of Quartz\CoClass\Thumbnail.cls: a picture of the file fitted inside Width x Height (aspect
    ''' kept). VB6 asked the Windows Shell for it (IExtractImage), which also works for videos; here
    ''' pictures are scaled with GDI+ (better quality at print sizes) and everything else -- videos --
    ''' goes through the Shell's IShellItemImageFactory, as before.
    ''' </summary>
    Public Class Thumbnail

        Private m_strFileName As String = ""

        ''' <summary>VB6 FixedSize (always True in iPhoto): kept for compatibility, no effect.</summary>
        Public Property FixedSize As Boolean = True

        ''' <summary>Size of the last thumbnail made.</summary>
        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer

        Public Property FileName As String
            Get
                Return m_strFileName
            End Get
            Set(value As String)
                m_strFileName = If(value, "")
            End Set
        End Property

        Public Sub Clear()
            _Width = 0
            _Height = 0
        End Sub

        ''' <summary>The file as a bitmap fitted inside Width x Height (enlarged if smaller, like the Shell
        ''' thumbnailer); Nothing when the file can't be read.</summary>
        Public Function GetThumbnail(ByVal Width As Integer, ByVal Height As Integer) As Bitmap
            Clear()
            If Not File.Exists(m_strFileName) OrElse Width <= 0 OrElse Height <= 0 Then Return Nothing
            Dim bmp As Bitmap = Nothing
            If IsPicture(m_strFileName) Then
                Try
                    Using src As Bitmap = ImageFile.LoadPicture(m_strFileName)
                        bmp = ImageFilter.Fit(src, Width, Height, True)
                    End Using
                Catch
                    bmp = Nothing
                End Try
            End If
            If bmp Is Nothing Then bmp = ShellThumbnail(m_strFileName, Width, Height)
            If bmp IsNot Nothing Then
                _Width = bmp.Width
                _Height = bmp.Height
            End If
            Return bmp
        End Function

        Private Shared Function IsPicture(ByVal file As String) As Boolean
            Select Case Path.GetExtension(file).ToUpperInvariant()
                Case ".JPG", ".JPEG", ".BMP", ".GIF", ".PNG", ".TIF", ".TIFF"
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        '==============================================================================================
        ' Shell thumbnails (IShellItemImageFactory)
        '==============================================================================================
        <StructLayout(LayoutKind.Sequential)>
        Private Structure NativeSize
            Public cx As Integer
            Public cy As Integer
        End Structure

        <ComImport(), Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
        Private Interface IShellItemImageFactory
            Sub GetImage(<[In]()> size As NativeSize, flags As Integer, ByRef phbm As IntPtr)
        End Interface

        <DllImport("shell32.dll", CharSet:=CharSet.Unicode, PreserveSig:=False)>
        Private Shared Sub SHCreateItemFromParsingName(<MarshalAs(UnmanagedType.LPWStr)> path As String, pbc As IntPtr,
                                                       <MarshalAs(UnmanagedType.LPStruct)> riid As Guid,
                                                       <MarshalAs(UnmanagedType.Interface)> ByRef ppv As IShellItemImageFactory)
        End Sub

        <DllImport("gdi32.dll")>
        Private Shared Function DeleteObject(hObject As IntPtr) As Boolean
        End Function

        ''' <summary>Explorer's thumbnail of any file, fitted inside width x height; Nothing on failure.</summary>
        Public Shared Function ShellThumbnail(ByVal path As String, ByVal width As Integer, ByVal height As Integer) As Bitmap
            Dim factory As IShellItemImageFactory = Nothing
            Dim hBitmap As IntPtr = IntPtr.Zero
            Try
                SHCreateItemFromParsingName(path, IntPtr.Zero, GetType(IShellItemImageFactory).GUID, factory)
                If factory Is Nothing Then Return Nothing
                Const SIIGBF_RESIZETOFIT As Integer = &H0
                factory.GetImage(New NativeSize With {.cx = width, .cy = height}, SIIGBF_RESIZETOFIT, hBitmap)
                If hBitmap = IntPtr.Zero Then Return Nothing
                Using tmp As Bitmap = Image.FromHbitmap(hBitmap)
                    Return New Bitmap(tmp)
                End Using
            Catch
                Return Nothing
            Finally
                If hBitmap <> IntPtr.Zero Then DeleteObject(hBitmap)
                If factory IsNot Nothing Then Marshal.ReleaseComObject(factory)
            End Try
        End Function

    End Class

    ''' <summary>Port of Quartz\CoClass\ImageInfo.cls: a picture file's pixel size, read without decoding it.</summary>
    Public Class ImageInfo

        Private m_strFileName As String = ""

        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer

        Public Property FileName As String
            Get
                Return m_strFileName
            End Get
            Set(value As String)
                m_strFileName = If(value, "")
                _Width = 0
                _Height = 0
                Try
                    Using fs As New FileStream(m_strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        Using img As Image = Image.FromStream(fs, False, False)
                            _Width = img.Width
                            _Height = img.Height
                        End Using
                    End Using
                Catch
                    ' not a picture GDI+ can read: 0 x 0, as VB6 returned for unknown formats
                End Try
            End Set
        End Property

    End Class

    ''' <summary>Quartz's file functions: LoadPicture / SavePicture (VB6 Quartz.SavePicture).</summary>
    Public Module ImageFile

        ''' <summary>The picture as a new Bitmap; the file is not kept open (it may be rotated or replaced next).
        ''' GDI+ reports running out of memory as "Parameter is not valid" (ArgumentException) -- easy to hit
        ''' in this 32-bit program with big photos while the previous ones wait for the collector -- so a
        ''' failed load is retried once after a full collection. Throws when the file really can't be read.</summary>
        Public Function LoadPicture(ByVal file As String) As Bitmap
            Try
                Return LoadPictureOnce(file)
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException
                GC.Collect()
                GC.WaitForPendingFinalizers()
                GC.Collect()
                Return LoadPictureOnce(file)
            End Try
        End Function

        Private Function LoadPictureOnce(ByVal file As String) As Bitmap
            Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using img As Image = Image.FromStream(fs)
                    Return New Bitmap(img)
                End Using
            End Using
        End Function

        ''' <summary>Saves as JPEG (quality 1..100); False on failure (VB6 Quartz.SavePicture).</summary>
        Public Function SavePicture(ByVal img As Image, ByVal file As String, Optional ByVal quality As Integer = 100) As Boolean
            If img Is Nothing Then Return False
            Try
                Dim codec = Imaging.ImageCodecInfo.GetImageEncoders().First(Function(c) c.FormatID = Imaging.ImageFormat.Jpeg.Guid)
                Using ps As New Imaging.EncoderParameters(1)
                    ps.Param(0) = New Imaging.EncoderParameter(Imaging.Encoder.Quality, CLng(Math.Max(1, Math.Min(100, quality))))
                    img.Save(file, codec, ps)
                End Using
                Return True
            Catch
                Return False
            End Try
        End Function

    End Module

End Namespace
