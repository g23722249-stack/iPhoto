Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO

' Port of 電子相簿\Lib\Module\LibAlbum.bas.
'
' Differences from VB6:
'   - The Sort*ByCreateDateTime functions used a disconnected ADODB.Recordset purely to sort; they use
'     a stable LINQ OrderBy on the same keys now.
'   - GetExifFileDateTime and RotatePicture used Quartz.Exif / Quartz.GDI; they use System.Drawing
'     (EXIF tag 0x9003 DateTimeOriginal, Image.RotateFlip + JPEG quality 100) instead.
'   - ExactFolders / ExactFiles (VB6: hidden Dir/FileListBoxes on frmResAlbum) live here; frmResAlbum
'     forwards to them so existing "frmResAlbum.ExactFiles(...)" calls still work.
'   - Text files are read in the system ANSI code page (AnsiText.Encoding), as VB6 did.
Public Module LibAlbum

    Public Const gc_strNote As String = "Note.Ini"
    Public Const gc_strOlympus_camedia_inf As String = "olyalbum.inf"

    Public Const gc_strExifPattern As String = "Exif"
    Public Const gc_strMusicPattern As String = "*.MP3;*.WAV;*.MID;*.WMA"
    Public Const gc_strFavoritesPattern As String = "Alm"

    Public Enum enumAlbums
        alGuide = 0
        alFavorite = 1
    End Enum

    Public Enum enumSortOrder
        Ascending = 0
        Descending = 1
    End Enum

    Public Enum enumExeMode
        exeAlbums = 0
        exeFavorites = 1
        exeSearch = 2
        exeCalendar = 3
        exeStar = 4
        exeFace = 5
    End Enum

    Public Enum enumCreate
        crDate = 0
        crTime = 1
        crSeq = 2
    End Enum

    Public Enum enumNote
        ntTitle = 0
        ntDate = 1
        ntSopt = 2
        ntIcon = 3
        ntSelIcon = 4
        ntMusic = 5
        ntRemark = 6
    End Enum

    Public Enum enumPhotoExif
        peDate = 0
        peTime = 1

        peTitle = 10
        peSpot = 20
        peCharacter = 30
        peRanking = 40
        peKeyWord = 50
        peRemark = 60
        peGps = 70          ' [Exif] GPS=lat,lon (decimal degrees; new in the .NET port, PlaceNames)
        peCountry = 71      ' [Exif] Country=臺灣           (PlaceNames.Resolve)
        peCity = 72         ' [Exif] City=臺中市西屯區     county + district
        peTown = 73         ' [Exif] Town=西屯區

        peIcon = 90
    End Enum

    Public Enum enumPhotoMediaType
        mdImage = 10
        mdVideo = 20
        mdUnknown = 90
    End Enum

    Public Structure strSearch
        Public FileDesc As String
    End Structure

    Public Structure udfApplication
        Public ExeMode As enumExeMode
        Public SectionIndex As Integer
        Public KeyIndex As Integer
    End Structure

    Public Function FetchArrayCount(ByVal lpArray As Array) As Integer
        Return If(lpArray Is Nothing, 0, lpArray.Length)
    End Function

    ''' <summary>Folders iPhoto never treats as albums/classes (compared on the folder's own name).</summary>
    Public Function IsSystemFolder(ByVal szFolderName As String) As Boolean
        Select Case ExactFolderName(szFolderName).Trim().ToUpperInvariant()
            Case "SYSTEM", "CONFIG", "SETUP", "RESOURCE", "IMAGE", "ICON", "FORM", "CLASS", "COCLASS", "MODULE", "UIMODULE"
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>"D:\a\b\" or "D:\a\b" -> "D:\a" (no trailing backslash).</summary>
    Public Function ExactParentFolder(ByVal szFolderDesc As String) As String
        Dim s As String = If(szFolderDesc, "").Trim().TrimEnd("\"c)
        Dim i As Integer = s.LastIndexOf("\"c)
        If i < 0 Then Return ""
        Return s.Substring(0, i).TrimEnd("\"c)
    End Function

    ''' <summary>"D:\a\b\" or "D:\a\b" -> "b".</summary>
    Public Function ExactFolderName(ByVal szFolderDesc As String) As String
        Dim s As String = If(szFolderDesc, "").Trim()
        If s.EndsWith("\") Then s = s.Substring(0, s.Length - 1)
        Dim i As Integer = s.LastIndexOf("\"c)
        Return If(i < 0, s, s.Substring(i + 1))
    End Function

    ''' <summary>Strips "//" comments from an ini value. VB6 quirk kept: a value with "//" after its
    ''' first character (e.g. a URL) comes back EMPTY, not cut at the comment -- the VB6 code computed
    ''' the cut text but never returned it. Every iPhoto setting that goes through here is a path, so
    ''' fixing it would only change behaviour for values VB6 already ignored.</summary>
    Public Function GetClearText(ByVal szText As String) As String
        If szText Is Nothing OrElse szText.Trim().Length = 0 Then Return ""
        szText = szText.Trim()
        If szText.IndexOf("//", StringComparison.Ordinal) >= 0 Then Return ""
        Return szText
    End Function

    Public Function GetClearFolderDesc(ByVal strFolder As String) As String
        strFolder = If(strFolder, "").Trim()
        If strFolder.Length = 0 Then Return ""
        If Not strFolder.EndsWith("\") Then Return strFolder
        ' VB6 quirk kept: for a trailing "\" it returned just the run of backslashes
        Return New String("\"c, strFolder.Length - strFolder.TrimEnd("\"c).Length)
    End Function

    Public Function GetPhotoPatterns() As String
        ' video, then image
        Return "*.AVI;*.DAT;*.MPG;*.MOV;*.RM;*.M2P;*.MPEG;*.DIVX;*.MP4;*.3GP;*.M2TS;*.BMP;*.GIF;*.JPG;*.JPEG;*.PNG"
    End Function

    ''' <summary>Media type from a file name or a bare extension ("JPG").</summary>
    Public Function GetMediaType(ByVal lpFileSystem As Carbon.FileSystem, ByVal szFileDesc As String) As enumPhotoMediaType
        If lpFileSystem Is Nothing Then lpFileSystem = New Carbon.FileSystem
        Dim ext As String = If(If(szFileDesc, "").Contains("."), lpFileSystem.AnalyseFile(fsExtensionName, szFileDesc), szFileDesc)
        Select Case If(ext, "").Trim().ToUpperInvariant()
            Case "AVI", "DAT", "MPG", "MOV", "RM", "M2P", "MPEG", "DIVX", "MP4", "3GP", "M2TS"
                Return enumPhotoMediaType.mdVideo
            Case "BMP", "GIF", "JPG", "JPEG"
                Return enumPhotoMediaType.mdImage
            Case Else
                Return enumPhotoMediaType.mdUnknown
        End Select
    End Function

    ''' <summary>Bytes -> (value truncated to 2 decimals, "K"/"M"); 0 and "Byte" for 0 or 1 byte (VB6 rules).</summary>
    Public Sub CalcFileLength(ByVal LengthByte As Long, ByRef Length As Double, ByRef Unit As String)
        Length = 0
        Unit = "Byte"
        If LengthByte <= 1 Then Return
        Dim v As Double = LengthByte / 1024 / 1024
        If Math.Floor(v) <= 0 Then
            v = LengthByte / 1024
            Unit = "K"
        Else
            Unit = "M"
        End If
        Length = Math.Truncate(v * 100) / 100
    End Sub

    Public Function CheckNameRule(ByVal strName As String) As Boolean
        Return strName.IndexOfAny({"\"c, "*"c, "/"c, "."c, """"c}) < 0
    End Function

    ''' <summary>"yyyyMMddHHmmss" from EXIF DateTimeOriginal, else the file's creation time, else its
    ''' last-write time (VB6 read the EXIF tag through Quartz.Exif).</summary>
    Public Function GetExifFileDateTime(ByVal lpFileSystem As Carbon.FileSystem, ByVal strFileName As String) As String
        Dim value As String = "0"
        If GetMediaType(lpFileSystem, strFileName) = enumPhotoMediaType.mdImage Then
            value = ReadExifDateTimeOriginal(strFileName)
        End If
        If Val(value) = 0 Then
            value = lpFileSystem.AnalyseFile(fsCreateDate, strFileName) & lpFileSystem.AnalyseFile(fsCreateTime, strFileName)
        End If
        If Val(value) = 0 Then value = File.GetLastWriteTime(strFileName).ToString("yyyyMMddHHmmss")
        Return value
    End Function

    ''' <summary>EXIF 0x9003 ("2005:12:31 23:59:59") as "20051231235959", or "0".</summary>
    Private Function ReadExifDateTimeOriginal(ByVal fileName As String) As String
        Try
            Using fs As New FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using img As Image = Image.FromStream(fs, False, False)
                    If Not img.PropertyIdList.Contains(&H9003) Then Return "0"
                    Dim raw As String = AnsiText.Encoding.GetString(img.GetPropertyItem(&H9003).Value).TrimEnd(ChrW(0))
                    Dim digits As New String(raw.Where(Function(c) Char.IsDigit(c)).ToArray())
                    Return If(digits.Length >= 14, digits.Substring(0, 14), "0")
                End Using
            End Using
        Catch
            Return "0"
        End Try
    End Function

    Public Function Random(ByVal Min As Integer, ByVal Max As Integer) As String
        Return CInt(Math.Round((Max - Min) * Rnd() + Min)).ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>Seconds -> "HH：mm：ss" (full-width colons, as VB6 displayed them).</summary>
    Public Function GetVideoDuration(ByVal Value As Long) As String
        Dim minutes As Long = Value \ 60
        Dim seconds As Long = Value Mod 60
        Dim hours As Long = minutes \ 60
        Return hours.ToString("00") & "：" & (minutes Mod 60).ToString("00") & "：" & seconds.ToString("00")
    End Function

    '==================================================================================================
    ' Sorting (VB6 sorted through a disconnected ADODB.Recordset)
    '==================================================================================================
    ''' <summary>Sorts the first intCount entries of strFiles by their photo date/time (the .Exif side
    ''' file if it has one, else the file's last-write time); strFiles is replaced by the sorted list.</summary>
    Public Function SortPhotoFileByCreateDateTime(ByVal enumSort As enumSortOrder, ByRef strFiles() As String, ByVal intCount As Integer) As Integer
        Dim fs As New Carbon.FileSystem
        Dim rows As New List(Of Tuple(Of String, String, String))
        For i = 0 To intCount - 1
            Dim f As String = If(strFiles(i), "").Trim()
            If f.Length = 0 Then Continue For
            Dim exif As String = fs.AnalyseFile(fsParentFolderName, f) & "\" & fs.AnalyseFile(fsBaseName, f) & "." & gc_strExifPattern
            rows.Add(Tuple.Create(f, IniDateOrFile(exif, "Exif", "Date", f, "yyyyMMdd", 8), IniDateOrFile(exif, "Exif", "Time", f, "HHmmss", 6)))
        Next
        strFiles = SortRows(rows, enumSort, False)
        Return strFiles.Length
    End Function

    ''' <summary>Folders sorted by the [Create] Date/Time in their Note.Ini (else the folder's time).</summary>
    Public Function SortFolderNoteDateTime(ByVal enumSort As enumSortOrder, ByRef strFolder() As String, ByVal intCount As Integer) As Integer
        Dim rows As New List(Of Tuple(Of String, String, String))
        For i = 0 To intCount - 1
            Dim f As String = If(strFolder(i), "").Trim()
            If f.Length = 0 Then Continue For
            Dim note As String = f & "\" & gc_strNote
            rows.Add(Tuple.Create(f, IniDateOrFile(note, "Create", "Date", f, "yyyyMMdd", 8), IniDateOrFile(note, "Create", "Time", f, "HHmmss", 6)))
        Next
        strFolder = SortRows(rows, enumSort, False)
        Return strFolder.Length
    End Function

    ''' <summary>Photo books (.Alm) sorted by their [Create] Date/Time (else the file's creation time), then name.</summary>
    Public Function SortFavoriteBookFileByCreateDateTime(ByVal enumSort As enumSortOrder, ByRef strFiles() As String, ByVal intCount As Integer) As Integer
        Dim fs As New Carbon.FileSystem
        Dim ini As New Carbon.IniFile
        Dim rows As New List(Of Tuple(Of String, String, String))
        For i = 0 To intCount - 1
            If Not fs.FileExists(strFiles(i)) Then Continue For
            ini.FileName = strFiles(i)
            Dim d As String = ini.SimpleGetValue("Create", "Date")
            Dim t As String = ini.SimpleGetValue("Create", "Time")
            If Val(d) = 0 Then d = fs.AnalyseFile(fsCreateDate, strFiles(i))
            If Val(t) = 0 Then t = fs.AnalyseFile(fsCreateTime, strFiles(i))
            rows.Add(Tuple.Create(strFiles(i), PadNum(d, 8), PadNum(t, 6)))
        Next
        strFiles = SortRows(rows, enumSort, True)
        Return strFiles.Length
    End Function

    Private Function SortRows(ByVal rows As List(Of Tuple(Of String, String, String)), ByVal order As enumSortOrder, ByVal thenByLabel As Boolean) As String()
        Dim q As IOrderedEnumerable(Of Tuple(Of String, String, String))
        If order = enumSortOrder.Descending Then
            q = rows.OrderByDescending(Function(r) r.Item2, StringComparer.Ordinal).ThenByDescending(Function(r) r.Item3, StringComparer.Ordinal)
        Else
            q = rows.OrderBy(Function(r) r.Item2, StringComparer.Ordinal).ThenBy(Function(r) r.Item3, StringComparer.Ordinal)
        End If
        If thenByLabel Then q = q.ThenBy(Function(r) r.Item1, StringComparer.CurrentCultureIgnoreCase)
        Return q.Select(Function(r) r.Item1.Trim()).ToArray()
    End Function

    ''' <summary>The ini value zero-padded to <paramref name="digits"/>, or the file/folder's last-write
    ''' time in <paramref name="fileFormat"/> when the ini has no (non-zero) value.</summary>
    Private Function IniDateOrFile(ByVal iniPath As String, ByVal section As String, ByVal key As String,
                                   ByVal path As String, ByVal fileFormat As String, ByVal digits As Integer) As String
        Dim ini As New Carbon.IniFile With {.FileName = iniPath}
        Dim v As String = GetClearText(ini.SimpleGetValue(section, key))
        If Val(v) > 0 Then Return PadNum(v, digits)
        Return If(Directory.Exists(path), Directory.GetLastWriteTime(path), File.GetLastWriteTime(path)).ToString(fileFormat)
    End Function

    ''' <summary>VB6 Format(numericString, "000..."): zero-padded integer; non-numeric text is returned as-is.</summary>
    Public Function PadNum(ByVal value As String, ByVal digits As Integer) As String
        Dim d As Double
        If Double.TryParse(If(value, "").Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
            Return CLng(Math.Round(d)).ToString(New String("0"c, digits), CultureInfo.InvariantCulture)
        End If
        Return If(value, "")
    End Function

    '==================================================================================================
    ' Profiles (*.Ini / *.Alm with repeated keys)
    '==================================================================================================
    ''' <summary>All values of <paramref name="strKeyName"/> inside [<paramref name="strSection"/>], in file
    ''' order -- iPhoto.Ini's [Album] has one "Path=" line per album root, which the Win32 profile API
    ''' can't return. Lines go through GetClearText (so "//" comment lines are skipped). Missing file = 0.</summary>
    Public Function LoadProfileSectionValues(ByVal strProfile As String, ByVal strSection As String, ByVal strKeyName As String, ByRef lpArray() As String) As Integer
        Dim result As New List(Of String)
        lpArray = New String() {}
        If Not File.Exists(strProfile) Then Return 0
        Dim sectionHeader As String = "[" & strSection.Trim() & "]"
        Dim keyPrefix As String = strKeyName.Trim().ToUpperInvariant() & "="
        Dim inSection As Boolean = False
        For Each raw In File.ReadAllLines(strProfile, AnsiText.Encoding)
            Dim line As String = GetClearText(raw)
            If line.Trim().Length = 0 Then Continue For
            If Not inSection Then
                If line.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase) Then inSection = True
                Continue For
            End If
            If line.Contains("[") AndAlso line.Contains("]") Then
                If Not line.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase) Then inSection = False
                Continue For
            End If
            Dim pos As Integer = line.Trim().ToUpperInvariant().IndexOf(keyPrefix, StringComparison.Ordinal)
            If pos < 0 Then Continue For
            result.Add(line.Substring(pos + keyPrefix.Length).Trim())
        Next
        lpArray = result.ToArray()
        Return lpArray.Length
    End Function

    '==================================================================================================
    ' Images
    '==================================================================================================
    ''' <summary>Rotates a JPEG/BMP/GIF file in place by a multiple of 90 degrees (positive = clockwise),
    ''' saving JPEGs at quality 100 as VB6's Quartz.SavePicture(..., 100) did.</summary>
    Public Function RotatePicture(ByVal FileDesc As String, ByVal lngAngle As Long) As Boolean
        Dim turns As Integer = CInt(((lngAngle Mod 360) + 360) Mod 360)
        Dim flip As RotateFlipType
        Select Case turns
            Case 90 : flip = RotateFlipType.Rotate90FlipNone
            Case 180 : flip = RotateFlipType.Rotate180FlipNone
            Case 270 : flip = RotateFlipType.Rotate270FlipNone
            Case 0 : Return True
            Case Else : Return False
        End Select
        Try
            Dim bmp As Bitmap
            Dim format As ImageFormat
            ' load into memory first so the file isn't locked when it is replaced
            Using fs As New FileStream(FileDesc, FileMode.Open, FileAccess.Read)
                Using src As Image = Image.FromStream(fs)
                    format = src.RawFormat
                    bmp = New Bitmap(src)
                End Using
            End Using
            Using bmp
                bmp.RotateFlip(flip)
                If Not g_lpFileSystem.DeleteFile(FileDesc) Then Return False
                If format.Guid = ImageFormat.Jpeg.Guid Then
                    Dim codec = ImageCodecInfo.GetImageEncoders().First(Function(c) c.FormatID = ImageFormat.Jpeg.Guid)
                    Using ep As New EncoderParameters(1)
                        ep.Param(0) = New EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L)
                        bmp.Save(FileDesc, codec, ep)
                    End Using
                Else
                    bmp.Save(FileDesc, format)
                End If
            End Using
            Return True
        Catch
            Return False
        End Try
    End Function

    '==================================================================================================
    ' Folder / file listing (VB6: frmResAlbum.ExactFolders / ExactFiles via hidden Dir/FileListBoxes)
    '==================================================================================================
    ''' <summary>Sub-folders of szFolderSpec as full paths, sorted like VB6's DirListBox; hidden/system
    ''' folders are left out (VB6's Dir(..., vbDirectory) never returned them). Missing folder = 0.</summary>
    Public Function ExactFolders(ByVal szFolderSpec As String, ByRef aszFolders() As String) As Integer
        aszFolders = New String() {}
        Try
            aszFolders = New DirectoryInfo(szFolderSpec).GetDirectories().
                Where(Function(d) (d.Attributes And (FileAttributes.Hidden Or FileAttributes.System)) = 0).
                Select(Function(d) d.FullName).
                OrderBy(Function(p) p, StringComparer.CurrentCultureIgnoreCase).ToArray()
        Catch
        End Try
        Return aszFolders.Length
    End Function

    ''' <summary>Files of szFolderSpec matching a ";"-separated pattern list ("*.JPG;*.BMP"), sorted like
    ''' VB6's FileListBox (hidden/system files excluded), as full paths when bMergePath.</summary>
    Public Function ExactFiles(ByVal szFolderSpec As String, ByVal szPattern As String, ByRef aszFiles() As String, ByVal bMergePath As Boolean) As Integer
        aszFiles = New String() {}
        Try
            Dim dir As New DirectoryInfo(szFolderSpec)
            Dim found As New Dictionary(Of String, FileInfo)(StringComparer.OrdinalIgnoreCase)
            For Each pat In szPattern.Split(";"c)
                Dim p As String = pat.Trim()
                If p.Length = 0 Then Continue For
                For Each f In dir.GetFiles(p)
                    ' GetFiles("*.JPG") also matches "x.JPGX" (8.3 name matching): keep exact extensions only
                    If p.StartsWith("*.") AndAlso p.IndexOf("*"c, 2) < 0 AndAlso
                       Not f.Extension.Equals(p.Substring(1), StringComparison.OrdinalIgnoreCase) Then Continue For
                    If (f.Attributes And (FileAttributes.Hidden Or FileAttributes.System)) <> 0 Then Continue For
                    found(f.FullName) = f
                Next
            Next
            aszFiles = found.Values.OrderBy(Function(f) f.Name, StringComparer.CurrentCultureIgnoreCase).
                Select(Function(f) If(bMergePath, szFolderSpec & "\" & f.Name, f.Name)).ToArray()
        Catch
        End Try
        Return aszFiles.Length
    End Function

End Module
