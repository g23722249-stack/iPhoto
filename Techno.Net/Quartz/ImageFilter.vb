Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Namespace Quartz

    ''' <summary>
    ''' Managed replacement for the Quartz.Image / Quartz.GDI filters the photo viewer uses. Ports of
    ''' Quartz\Class\cImageProcessDIB.cls (ContrastAndBrightness, GrayScale, Colourise, RedEye, the
    ''' eSoften / eSharpen convolution kernels), Quartz\Lib\LibHLSRGB.bas and Color.ColorToHSL, with
    ''' the same arithmetic -- including VB6's rounding when a Double lands in a Long or Byte (banker's
    ''' rounding, which CInt / CByte do too) and "\" integer division for the kernels.
    ''' Every function returns a new 24bpp Bitmap and leaves the source alone. The optional progress
    ''' callback gets 0..100 (VB6: InitProgress / Progress / Complete events).
    ''' </summary>
    Public Module ImageFilter

        '==============================================================================================
        ' 24bpp pixel access (VB6 used a DIB section: B, G, R per pixel, rows padded to 4 bytes)
        '==============================================================================================
        Private NotInheritable Class Pixels
            Implements IDisposable

            Public ReadOnly Bitmap As Bitmap
            Public ReadOnly Width As Integer
            Public ReadOnly Height As Integer
            Public ReadOnly Stride As Integer
            Public ReadOnly Data() As Byte

            Public Sub New(ByVal source As Image)
                Using copy As New Bitmap(source)
                    Bitmap = copy.Clone(New Rectangle(0, 0, copy.Width, copy.Height), PixelFormat.Format24bppRgb)
                End Using
                Width = Bitmap.Width
                Height = Bitmap.Height
                Dim bd As BitmapData = Bitmap.LockBits(New Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb)
                Stride = bd.Stride
                ReDim Data(Stride * Height - 1)
                Marshal.Copy(bd.Scan0, Data, 0, Data.Length)
                Bitmap.UnlockBits(bd)
            End Sub

            Public Function Offset(ByVal x As Integer, ByVal y As Integer) As Integer
                Return y * Stride + x * 3
            End Function

            ''' <summary>Writes the (changed) bytes back and hands over the bitmap.</summary>
            Public Function Commit() As Bitmap
                Dim bd As BitmapData = Bitmap.LockBits(New Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb)
                Marshal.Copy(Data, 0, bd.Scan0, Data.Length)
                Bitmap.UnlockBits(bd)
                Return Bitmap
            End Function

            Public Sub Dispose() Implements IDisposable.Dispose
                ' the bitmap is returned by Commit; nothing else to free
            End Sub
        End Class

        Private Sub Report(ByVal progress As Action(Of Integer), ByVal done As Integer, ByVal total As Integer)
            If progress Is Nothing OrElse total <= 0 Then Return
            progress(CInt(100L * done \ total))
        End Sub

        Private Function Clamp(ByVal v As Double) As Double
            If v > 255 Then Return 255
            If v < 0 Then Return 0
            Return v
        End Function

        '==============================================================================================
        ' Contrast / brightness (cImageProcessDIB.ContrastAndBrightness)
        '==============================================================================================
        ''' <summary>Contrast -100..100 (0 = unchanged), brightness in steps (x 1.3), relative to the image.</summary>
        Public Function ContrastAndBrightness(ByVal source As Image, ByVal contrast As Double, ByVal brightness As Integer, Optional ByVal progress As Action(Of Integer) = Nothing) As Bitmap
            Dim c As Double = GetContrast(contrast)
            Dim b As Integer = CInt(brightness * 1.3)
            Dim p As New Pixels(source)
            For y As Integer = 0 To p.Height - 1
                Dim o As Integer = p.Offset(0, y)
                For x As Integer = 0 To p.Width - 1
                    For k As Integer = 0 To 2
                        p.Data(o + k) = CByte(Clamp((p.Data(o + k) - 128.0) * c + 128 + b))
                    Next
                    o += 3
                Next
                Report(progress, y + 1, p.Height)
            Next
            Return p.Commit()
        End Function

        Private Function GetContrast(ByVal contrast As Double) As Double
            Dim lV As Integer = CInt(contrast + 100)
            If lV = 100 Then Return 1.0
            If lV < 100 Then Return 1.0 / (5.0 - lV / 25.0)   ' 100 == 1, 0 == 1/5
            Return (lV - 100.0) / 25.0 + 1.0                  ' 100 == 1, 200 == 5
        End Function

        '==============================================================================================
        ' Gray scale (ITU weights)
        '==============================================================================================
        Public Function GrayScale(ByVal source As Image, Optional ByVal progress As Action(Of Integer) = Nothing) As Bitmap
            Dim p As New Pixels(source)
            For y As Integer = 0 To p.Height - 1
                Dim o As Integer = p.Offset(0, y)
                For x As Integer = 0 To p.Width - 1
                    Dim gray As Byte = CByte(Clamp(CInt((222 * CInt(p.Data(o + 2)) + 707 * CInt(p.Data(o + 1)) + 71 * CInt(p.Data(o))) / 1000)))
                    p.Data(o) = gray : p.Data(o + 1) = gray : p.Data(o + 2) = gray
                    o += 3
                Next
                Report(progress, y + 1, p.Height)
            Next
            Return p.Commit()
        End Function

        '==============================================================================================
        ' Colourise (泛黃 uses RGB(251, 209, 123)): every pixel takes the colour's hue
        '==============================================================================================
        Public Function Colourise(ByVal source As Image, ByVal color As Color, Optional ByVal progress As Action(Of Integer) = Nothing) As Bitmap
            Dim fHue, fSat, fLum As Single
            RGBToHLS(color.R, color.G, color.B, fHue, fSat, fLum)

            Dim p As New Pixels(source)
            For y As Integer = 0 To p.Height - 1
                Dim o As Integer = p.Offset(0, y)
                For x As Integer = 0 To p.Width - 1
                    Dim h, s, l As Single
                    RGBToHLS(p.Data(o + 2), p.Data(o + 1), p.Data(o), h, s, l)
                    If h = 0 Then s = 0.5F   ' grey pixels get a fixed saturation
                    Dim r, g, b As Integer
                    HLSToRGB(fHue, s, l, r, g, b)
                    p.Data(o) = CByte(Clamp(b)) : p.Data(o + 1) = CByte(Clamp(g)) : p.Data(o + 2) = CByte(Clamp(r))
                    o += 3
                Next
                Report(progress, y + 1, p.Height)
            Next
            Return p.Commit()
        End Function

        ''' <summary>LibHLSRGB.RGBToHLS: H in -1..5 (sectors, not degrees), S and L in 0..1.</summary>
        Public Sub RGBToHLS(ByVal r As Integer, ByVal g As Integer, ByVal b As Integer, ByRef h As Single, ByRef s As Single, ByRef l As Single)
            Dim rR As Single = r / 255.0F, rG As Single = g / 255.0F, rB As Single = b / 255.0F
            Dim max As Single = Math.Max(rR, Math.Max(rG, rB))
            Dim min As Single = Math.Min(rR, Math.Min(rG, rB))
            l = (max + min) / 2
            If max = min Then
                s = 0 : h = 0
                Return
            End If
            s = If(l <= 0.5F, (max - min) / (max + min), (max - min) / (2 - max - min))
            Dim delta As Single = max - min
            If rR = max Then
                h = (rG - rB) / delta
            ElseIf rG = max Then
                h = 2 + (rB - rR) / delta
            Else
                h = 4 + (rR - rG) / delta
            End If
        End Sub

        ''' <summary>LibHLSRGB.HLSToRGB.</summary>
        Public Sub HLSToRGB(ByVal h As Single, ByVal s As Single, ByVal l As Single, ByRef r As Integer, ByRef g As Integer, ByRef b As Integer)
            Dim rR, rG, rB As Single
            If s = 0 Then
                rR = l : rG = l : rB = l
            Else
                Dim min As Single = If(l <= 0.5F, l * (1 - s), l - s * (1 - l))
                Dim max As Single = 2 * l - min
                If h < 1 Then
                    rR = max
                    If h < 0 Then
                        rG = min
                        rB = rG - h * (max - min)
                    Else
                        rB = min
                        rG = h * (max - min) + rB
                    End If
                ElseIf h < 3 Then
                    rG = max
                    If h < 2 Then
                        rB = min
                        rR = rB - (h - 2) * (max - min)
                    Else
                        rR = min
                        rB = (h - 2) * (max - min) + rR
                    End If
                Else
                    rB = max
                    If h < 4 Then
                        rR = min
                        rG = rR - (h - 4) * (max - min)
                    Else
                        rG = min
                        rR = (h - 4) * (max - min) + rG
                    End If
                End If
            End If
            r = CInt(rR * 255) : g = CInt(rG * 255) : b = CInt(rB * 255)
        End Sub

        '==============================================================================================
        ' Soften / Sharpen (cImageProcessDIB 3x3 / 5x5 kernels; the border pixels are left as they are)
        '==============================================================================================
        Public Function Soften(ByVal source As Image, Optional ByVal more As Boolean = False, Optional ByVal progress As Action(Of Integer) = Nothing) As Bitmap
            Dim size As Integer = If(more, 5, 3)
            Dim off As Integer = size \ 2
            Dim k(size - 1, size - 1) As Integer
            For i As Integer = -off To off
                For j As Integer = -off To off
                    Dim lm As Integer = Math.Max(Math.Abs(i), Math.Abs(j))
                    k(i + off, j + off) = If(lm = 0, CInt(size * (size / 2.0)), off - lm + 1)
                Next
            Next
            Return Convolve(source, k, progress)
        End Function

        Public Function Sharpen(ByVal source As Image, Optional ByVal more As Boolean = False, Optional ByVal progress As Action(Of Integer) = Nothing) As Bitmap
            Dim k(,) As Integer
            If more Then
                k = New Integer(,) {{0, -1, 0}, {-1, 5, -1}, {0, -1, 0}}
            Else
                k = New Integer(,) {{-1, -1, -1}, {-1, 15, -1}, {-1, -1, -1}}
            End If
            Return Convolve(source, k, progress)
        End Function

        Private Function Convolve(ByVal source As Image, ByVal k(,) As Integer, ByVal progress As Action(Of Integer)) As Bitmap
            Dim size As Integer = k.GetLength(0)
            Dim off As Integer = size \ 2
            Dim weight As Integer = 0
            For Each v As Integer In k
                weight += v
            Next
            If weight = 0 Then weight = 1

            Dim p As New Pixels(source)
            Dim src() As Byte = CType(p.Data.Clone(), Byte())
            For y As Integer = off To p.Height - 1 - off
                For x As Integer = off To p.Width - 1 - off
                    Dim sb As Integer = 0, sg As Integer = 0, sr As Integer = 0
                    For i As Integer = -off To off
                        For j As Integer = -off To off
                            Dim w As Integer = k(i + off, j + off)
                            Dim o As Integer = (y + j) * p.Stride + (x + i) * 3
                            sb += w * src(o) : sg += w * src(o + 1) : sr += w * src(o + 2)
                        Next
                    Next
                    Dim d As Integer = p.Offset(x, y)
                    p.Data(d) = CByte(Clamp(sb \ weight))
                    p.Data(d + 1) = CByte(Clamp(sg \ weight))
                    p.Data(d + 2) = CByte(Clamp(sr \ weight))
                Next
                Report(progress, y + 1, p.Height)
            Next
            Return p.Commit()
        End Function

        '==============================================================================================
        ' Red eye (cImageProcessDIB.RedEye): inside the rectangle, red pixels with a magenta-side hue
        ' (340..360 degrees in Color.ColorToHSL) become grey
        '==============================================================================================
        Public Function RedEye(ByVal source As Image, ByVal area As Rectangle, Optional ByVal progress As Action(Of Integer) = Nothing) As Bitmap
            Const q As Integer = 70, q2 As Integer = 185, q3 As Integer = -4
            Dim p As New Pixels(source)
            Dim r As Rectangle = Rectangle.Intersect(area, New Rectangle(0, 0, p.Width, p.Height))
            For x As Integer = r.Left To r.Right - 1
                For y As Integer = r.Top To r.Bottom - 1
                    Dim o As Integer = p.Offset(x, y)
                    Dim bl As Integer = p.Data(o), gr As Integer = p.Data(o + 1), rd As Integer = p.Data(o + 2)
                    Dim hue As Integer = PhotoshopHue(rd, gr, bl)
                    If hue >= 340 AndAlso hue <= 360 AndAlso rd > q AndAlso gr < q2 AndAlso bl < q2 Then
                        Dim v As Integer = CInt((gr + bl) / 2) + q3
                        Dim gray As Byte = CByte(Clamp(v))
                        p.Data(o) = gray : p.Data(o + 1) = gray : p.Data(o + 2) = gray
                    End If
                Next
                Report(progress, x - r.Left + 1, r.Width)
            Next
            Return p.Commit()
        End Function

        ''' <summary>Color.ColorToHSL's final hue (degrees, the "Adobe Photoshop" formula it ends with).</summary>
        Private Function PhotoshopHue(ByVal r As Integer, ByVal g As Integer, ByVal b As Integer) As Integer
            Dim max As Integer = Math.Max(r, Math.Max(g, b))
            Dim min As Integer = Math.Min(r, Math.Min(g, b))
            Dim diff As Integer = max - min
            If diff = 0 Then Return 0
            Dim q As Single = 60.0F / diff
            If max = r Then
                If g < b Then Return CInt(360.0F + q * (g - b))
                If g = b Then Return 0
                Return CInt(q * (g - b))
            ElseIf max = g Then
                Return CInt(120.0F + q * (b - r))
            Else
                Return CInt(240.0F + q * (r - g))
            End If
        End Function

        '==============================================================================================
        ' Geometry (Quartz.GDI.Rotate: +90 = clockwise; PaintPicture crop)
        '==============================================================================================
        Public Function Rotate(ByVal source As Image, ByVal angle As Integer) As Bitmap
            Dim bmp As New Bitmap(source)
            Select Case ((angle Mod 360) + 360) Mod 360
                Case 90 : bmp.RotateFlip(RotateFlipType.Rotate90FlipNone)
                Case 180 : bmp.RotateFlip(RotateFlipType.Rotate180FlipNone)
                Case 270 : bmp.RotateFlip(RotateFlipType.Rotate270FlipNone)
            End Select
            Return bmp
        End Function

        ''' <summary>Quartz.Image.Resize: exactly Width x Height (the picture is stretched).</summary>
        Public Function Resize(ByVal source As Image, ByVal width As Integer, ByVal height As Integer) As Bitmap
            Dim bmp As New Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format24bppRgb)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                g.DrawImage(source, New Rectangle(0, 0, bmp.Width, bmp.Height))
            End Using
            Return bmp
        End Function

        ''' <summary>Scaled to fit inside maxWidth x maxHeight with its aspect kept; with allowEnlarge False a
        ''' smaller picture keeps its size (a copy is returned either way).</summary>
        Public Function Fit(ByVal source As Image, ByVal maxWidth As Integer, ByVal maxHeight As Integer, Optional ByVal allowEnlarge As Boolean = False) As Bitmap
            Dim scale As Double = Math.Min(maxWidth / CDbl(source.Width), maxHeight / CDbl(source.Height))
            If Not allowEnlarge Then scale = Math.Min(1.0, scale)
            Return Resize(source, Math.Max(1, CInt(source.Width * scale)), Math.Max(1, CInt(source.Height * scale)))
        End Function

        Public Function Crop(ByVal source As Image, ByVal area As Rectangle) As Bitmap
            Dim r As Rectangle = Rectangle.Intersect(area, New Rectangle(0, 0, source.Width, source.Height))
            If r.Width <= 0 OrElse r.Height <= 0 Then Return New Bitmap(source)
            Dim bmp As New Bitmap(r.Width, r.Height, PixelFormat.Format24bppRgb)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.DrawImage(source, New Rectangle(0, 0, r.Width, r.Height), r, GraphicsUnit.Pixel)
            End Using
            Return bmp
        End Function

    End Module

End Namespace
