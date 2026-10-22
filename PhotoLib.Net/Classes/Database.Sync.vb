Imports System.Data

' PhotoIndex brought in line with the .Exif files (new in the .NET port, IndexSync): the .Exif is what
' counts -- iPhoto writes it first, so a row that differs means the .Exif was changed outside iPhoto
' (put back from a backup, edited by another program) or the row is missing.
Partial Public Class Database

    ''' <summary>Every PhotoIndex row by its file (full path).</summary>
    Public Function IndexRowsByFile() As Dictionary(Of String, DataRow)
        Dim result As New Dictionary(Of String, DataRow)(StringComparer.OrdinalIgnoreCase)
        Dim t As DataTable = Nothing
        SearchFileRecordSet("", t)
        If t Is Nothing Then Return result
        For Each r As DataRow In t.Rows
            Dim f As String = AsText(r("FileName"))
            If f <> "" Then result(f) = r
        Next
        Return result
    End Function

    ''' <summary>True when <paramref name="row"/> isn't what InsertPhoto would write for <paramref name="p"/>
    ''' now (the same columns, made the same way).</summary>
    Public Function IndexDiffers(ByVal p As Photo, ByVal row As DataRow) As Boolean
        Dim t = row.Table
        Dim text = Function(col As String) As String
                       Return If(t.Columns.Contains(col), AsText(row(col)), "")
                   End Function
        If text("Exif_Date") <> PadNum(p.Exif(enumPhotoExif.peDate), 8) Then Return True
        If text("Exif_Title") <> If(p.Exif(enumPhotoExif.peTitle), "") Then Return True
        If text("Exif_Character") <> If(p.Exif(enumPhotoExif.peCharacter), "") Then Return True
        If text("Exif_Spot") <> If(p.Exif(enumPhotoExif.peSpot), "") Then Return True
        If text("Exif_Remark") <> If(p.Exif(enumPhotoExif.peRemark), "") Then Return True
        If text("Exif_KeyWord") <> If(p.Exif(enumPhotoExif.peKeyWord), "") Then Return True
        If CInt(Val(text("Exif_Ranking"))) <> CInt(Val(p.Exif(enumPhotoExif.peRanking))) Then Return True
        If m_bolGpsColumns AndAlso t.Columns.Contains("Exif_Latitude") Then
            Dim lat, lon As Double
            Dim has As Boolean = PlaceNames.TryParseGps(p.Exif(enumPhotoExif.peGps), lat, lon)
            Dim rowHas As Boolean = Not IsDBNull(row("Exif_Latitude")) AndAlso Not IsDBNull(row("Exif_Longitude"))
            If has <> rowHas Then Return True
            If has AndAlso (Math.Abs(CDbl(row("Exif_Latitude")) - lat) > 0.0000001 OrElse Math.Abs(CDbl(row("Exif_Longitude")) - lon) > 0.0000001) Then Return True
        End If
        If m_bolPlaceColumns Then
            For Each c In {(Col:="Exif_Country", F:=enumPhotoExif.peCountry), (Col:="Exif_City", F:=enumPhotoExif.peCity), (Col:="Exif_Town", F:=enumPhotoExif.peTown)}
                If text(c.Col) <> Left(If(p.Exif(c.F), ""), 80) Then Return True
            Next
        End If
        Return False
    End Function

End Class
