Imports System.IO

' 拍攝資訊 (new in the .NET port): what the camera wrote into a photo's EXIF -- camera, lens, aperture,
' shutter, ISO, focal length, when -- and where (GPS). Read from the file's header only (the picture
' isn't decoded), so it is quick enough for the viewer and for indexing every photo (GeoIndex).
Public Module ShotInfo

    Public Class Shot
        Public Camera As String = ""
        Public Lens As String = ""
        Public FNumber As Double
        Public Exposure As Double          ' seconds
        Public Iso As Integer
        Public FocalLength As Double       ' mm
        Public Focal35 As Integer          ' 35 mm equivalent
        Public Taken As DateTime?
        Public Width As Integer
        Public Height As Integer
        Public Latitude As Double?
        Public Longitude As Double?
        Public Altitude As Double?

        Public ReadOnly Property HasPlace As Boolean
            Get
                Return Latitude.HasValue AndAlso Longitude.HasValue AndAlso Not (Latitude.Value = 0 AndAlso Longitude.Value = 0)
            End Get
        End Property

        ''' <summary>The lines the viewer shows (only what the photo has).</summary>
        Public Function Lines() As List(Of String)
            Dim l As New List(Of String)
            If Camera <> "" Then l.Add("相機：" & Camera)
            If Lens <> "" Then l.Add("鏡頭：" & Lens)
            Dim exp As New List(Of String)
            If FNumber > 0 Then exp.Add("f/" & FNumber.ToString("0.#"))
            If Exposure > 0 Then exp.Add(If(Exposure >= 1, Exposure.ToString("0.#") & " 秒", "1/" & Math.Round(1 / Exposure) & " 秒"))
            If Iso > 0 Then exp.Add("ISO " & Iso)
            If FocalLength > 0 Then exp.Add(FocalLength.ToString("0.#") & " mm" & If(Focal35 > 0 AndAlso Focal35 <> CInt(FocalLength), "（等效 " & Focal35 & " mm）", ""))
            If exp.Count > 0 Then l.Add(String.Join("  ", exp))
            If Taken.HasValue Then l.Add("拍攝：" & Taken.Value.ToString("yyyy/MM/dd HH:mm:ss"))
            If Width > 0 Then l.Add($"畫面：{Width} × {Height}（{Width * CLng(Height) / 1000000.0:0.#} 百萬像素）")
            If HasPlace Then l.Add("地點：" & PlaceText() & If(Altitude.HasValue, $"，海拔 {Altitude.Value:0} 公尺", ""))
            Return l
        End Function

        ''' <summary>"23.97012°N 120.52134°E".</summary>
        Public Function PlaceText() As String
            If Not HasPlace Then Return ""
            Return $"{Math.Abs(Latitude.Value):0.00000}°{If(Latitude.Value >= 0, "N", "S")} {Math.Abs(Longitude.Value):0.00000}°{If(Longitude.Value >= 0, "E", "W")}"
        End Function

        ''' <summary>The place on Google Maps (opened in the browser by the caller).</summary>
        Public Function MapUrl() As String
            If Not HasPlace Then Return ""
            Return "https://www.google.com/maps?q=" & Latitude.Value.ToString("0.000000", Globalization.CultureInfo.InvariantCulture) & "," &
                   Longitude.Value.ToString("0.000000", Globalization.CultureInfo.InvariantCulture)
        End Function
    End Class

    ''' <summary>The photo's shot data; Nothing when the file can't be read (a video, a broken file).</summary>
    Public Function Read(ByVal file As String) As Shot
        If GetMediaType(Nothing, file) <> enumPhotoMediaType.mdImage Then Return Nothing
        Try
            Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                  img As Image = Image.FromStream(fs, useEmbeddedColorManagement:=False, validateImageData:=False)
                Dim ids As New HashSet(Of Integer)(img.PropertyIdList)
                Dim s As New Shot With {.Width = img.Width, .Height = img.Height}
                Dim exifText = Function(id As Integer) As String
                               If Not ids.Contains(id) Then Return ""
                               Dim v() As Byte = img.GetPropertyItem(id).Value
                               Return Text.Encoding.ASCII.GetString(v).TrimEnd(ChrW(0)).Trim()
                           End Function
                Dim rationals = Function(id As Integer) As Double()
                                    If Not ids.Contains(id) Then Return New Double() {}
                                    Dim v() As Byte = img.GetPropertyItem(id).Value
                                    Dim r As New List(Of Double)
                                    For k = 0 To v.Length - 8 Step 8
                                        Dim num As UInteger = BitConverter.ToUInt32(v, k), den As UInteger = BitConverter.ToUInt32(v, k + 4)
                                        r.Add(If(den = 0, 0, num / CDbl(den)))
                                    Next
                                    Return r.ToArray()
                                End Function
                Dim short1 = Function(id As Integer) As Integer
                                 If Not ids.Contains(id) Then Return 0
                                 Dim v() As Byte = img.GetPropertyItem(id).Value
                                 Return If(v.Length >= 2, BitConverter.ToUInt16(v, 0), 0)
                             End Function

                Dim make As String = exifText(&H10F), model As String = exifText(&H110)
                s.Camera = If(model <> "" AndAlso make <> "" AndAlso Not model.StartsWith(make.Split(" "c)(0), StringComparison.OrdinalIgnoreCase), make & " " & model, If(model <> "", model, make))
                s.Lens = exifText(&HA434)
                s.FNumber = rationals(&H829D).FirstOrDefault()
                s.Exposure = rationals(&H829A).FirstOrDefault()
                s.Iso = short1(&H8827)
                s.FocalLength = rationals(&H920A).FirstOrDefault()
                s.Focal35 = short1(&HA405)
                Dim taken As String = exifText(&H9003)
                Dim dt As DateTime
                If DateTime.TryParseExact(taken, "yyyy:MM:dd HH:mm:ss", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, dt) Then s.Taken = dt

                ' GPS: degrees, minutes, seconds + N/S, E/W
                Dim lat() As Double = rationals(&H2), lon() As Double = rationals(&H4)
                If lat.Length = 3 AndAlso lon.Length = 3 Then
                    s.Latitude = (lat(0) + lat(1) / 60 + lat(2) / 3600) * If(exifText(&H1).StartsWith("S"), -1, 1)
                    s.Longitude = (lon(0) + lon(1) / 60 + lon(2) / 3600) * If(exifText(&H3).StartsWith("W"), -1, 1)
                End If
                Dim alt() As Double = rationals(&H6)
                If alt.Length = 1 Then
                    Dim below As Boolean = ids.Contains(&H5) AndAlso img.GetPropertyItem(&H5).Value.FirstOrDefault() = 1
                    s.Altitude = alt(0) * If(below, -1, 1)
                End If
                Return s
            End Using
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is Runtime.InteropServices.ExternalException
            Return Nothing
        End Try
    End Function

    ''' <summary>Great-circle distance in km.</summary>
    Public Function DistanceKm(ByVal lat1 As Double, ByVal lon1 As Double, ByVal lat2 As Double, ByVal lon2 As Double) As Double
        Const R As Double = 6371.0
        Dim dLat As Double = (lat2 - lat1) * Math.PI / 180, dLon As Double = (lon2 - lon1) * Math.PI / 180
        Dim a As Double = Math.Sin(dLat / 2) ^ 2 + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) ^ 2
        Return 2 * R * Math.Asin(Math.Min(1, Math.Sqrt(a)))
    End Function

End Module
