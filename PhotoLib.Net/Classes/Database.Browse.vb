Imports System.Data
Imports System.Data.OleDb

' Browsing the photo index by time and place (new in the .NET port):
'   時間軸  -- the years / months that have photos, and one year's photos in date order (the main window
'              shows them as one list with a heading per day); 那年今日 (this day in earlier years).
'   地點    -- every photo that has a 地點 (Exif_Spot) or a 縣市區 (Exif_City / Exif_Town), for the tree
'              縣市 › 區 › 景點 and for the map.
' Everything here only reads PhotoIndex.
Partial Public Class Database

    ''' <summary>One PhotoIndex row, the columns the browsing views use.</summary>
    Public Class IndexRow
        Public FileName As String = ""
        ''' <summary>yyyyMMdd ("00000000" when the photo has no date).</summary>
        Public DateText As String = ""
        Public Ranking As Integer
        Public Spot As String = ""
        Public City As String = ""
        Public Town As String = ""
        Public Latitude As Double?
        Public Longitude As Double?

        Public ReadOnly Property Year As Integer
            Get
                Return CInt(Val(Mid(DateText, 1, 4)))
            End Get
        End Property

        Public ReadOnly Property Month As Integer
            Get
                Return CInt(Val(Mid(DateText, 5, 2)))
            End Get
        End Property

        Public ReadOnly Property Day As Integer
            Get
                Return CInt(Val(Mid(DateText, 7, 2)))
            End Get
        End Property

        ''' <summary>The date, Nothing when the photo has none (or an impossible one).</summary>
        Public ReadOnly Property ShotDate As Date?
            Get
                Dim d As Date
                If DateText.Length = 8 AndAlso Date.TryParseExact(DateText, "yyyyMMdd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, d) Then Return d
                Return Nothing
            End Get
        End Property
    End Class

    ''' <summary>Years before this are "no date" (VB6 wrote 0000; cameras without a clock say 1970 / 1975...).
    ''' They stay in their year: only 0000 / blank counts as no date.</summary>
    Private Const NoYear As String = "0000"

    '==================================================================================================
    ' 時間軸
    '==================================================================================================
    ''' <summary>Every year that has photos, oldest first, with how many.</summary>
    Public Function TimelineYears() As List(Of KeyValuePair(Of Integer, Integer))
        Dim result As New List(Of KeyValuePair(Of Integer, Integer))
        For Each r In Counts("Select Exif_Year, Count(*) From " & mc_strTableName & " Where Exif_Year <> ? And Exif_Year <> '' Group By Exif_Year", NoYear)
            Dim y As Integer = CInt(Val(r.Key))
            If y > 0 Then result.Add(New KeyValuePair(Of Integer, Integer)(y, r.Value))
        Next
        Return result.OrderBy(Function(kv) kv.Key).ToList()
    End Function

    ''' <summary>The months of <paramref name="year"/> that have photos, with how many.</summary>
    Public Function TimelineMonths(ByVal year As Integer) As List(Of KeyValuePair(Of Integer, Integer))
        Dim result As New List(Of KeyValuePair(Of Integer, Integer))
        For Each r In Counts("Select Exif_Month, Count(*) From " & mc_strTableName & " Where Exif_Year = ? Group By Exif_Month", year.ToString("0000"))
            result.Add(New KeyValuePair(Of Integer, Integer)(CInt(Val(r.Key)), r.Value))
        Next
        Return result.OrderBy(Function(kv) kv.Key).ToList()
    End Function

    ''' <summary>How many photos have no date.</summary>
    Public Function NoDateCount() As Integer
        Return Counts("Select 'x', Count(*) From " & mc_strTableName & " Where Exif_Year = ? Or Exif_Year = '' Or Exif_Year Is Null", NoYear).Sum(Function(kv) kv.Value)
    End Function

    ''' <summary>The photos of <paramref name="year"/>, by date (then file name: cameras number them in order).</summary>
    Public Function TimelineRows(ByVal year As Integer) As List(Of IndexRow)
        Return Rows("Exif_Year = ?", "Exif_Date, FileName", year.ToString("0000"))
    End Function

    Public Function NoDateRows() As List(Of IndexRow)
        Return Rows("(Exif_Year = ? Or Exif_Year = '' Or Exif_Year Is Null)", "FileName", NoYear)
    End Function

    ''' <summary>那年今日: photos taken on <paramref name="month"/>/<paramref name="day"/> in the years
    ''' before <paramref name="beforeYear"/>, newest year first.</summary>
    Public Function OnThisDayRows(ByVal month As Integer, ByVal day As Integer, ByVal beforeYear As Integer) As List(Of IndexRow)
        Return Rows("Exif_Month = ? And Exif_Day = ? And Exif_Year < ? And Exif_Year <> ?", "Exif_Year Desc, FileName",
                    month.ToString("00"), day.ToString("00"), beforeYear.ToString("0000"), NoYear)
    End Function

    '==================================================================================================
    ' 地點
    '==================================================================================================
    ''' <summary>Every photo with a 地點 or a 縣市區, by date.</summary>
    Public Function PlaceRows() As List(Of IndexRow)
        If m_bolPlaceColumns Then Return Rows("(Exif_Spot <> '' Or Exif_City <> '')", "Exif_Date, FileName")
        Return Rows("Exif_Spot <> ''", "Exif_Date, FileName")
    End Function

    ''' <summary>Every photo that has a GPS position (the map).</summary>
    Public Function GpsRows() As List(Of IndexRow)
        If Not m_bolGpsColumns Then Return New List(Of IndexRow)
        Return Rows("Exif_Latitude Is Not Null And Exif_Longitude Is Not Null", "Exif_Date, FileName")
    End Function

    ''' <summary>How many photos and videos the index holds, and how many of them have a GPS position.</summary>
    Public Class LibraryCounts
        Public Photos As Integer
        Public Videos As Integer
        Public Others As Integer
        Public PhotosWithGps As Integer
        Public VideosWithGps As Integer
    End Class

    ''' <summary>設定 › 地點: the index counted by kind (by the file's extension, as the import sorts them).</summary>
    Public Function CountLibrary() As LibraryCounts
        Dim c As New LibraryCounts
        If Not m_bolImplement Then Return c
        Dim cols As String = "FileName" & If(m_bolGpsColumns, ", Exif_Latitude, Exif_Longitude", "")
        Using cmd As OleDbCommand = NewCommand("Select " & cols & " From " & mc_strTableName, Array.Empty(Of Object)())
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    Dim gps As Boolean = m_bolGpsColumns AndAlso r(1) IsNot DBNull.Value AndAlso r(2) IsNot DBNull.Value
                    Select Case IO.Path.GetExtension(AsText(r(0))).TrimStart("."c).ToUpperInvariant()
                        Case "BMP", "GIF", "JPG", "JPEG", "PNG"
                            c.Photos += 1
                            If gps Then c.PhotosWithGps += 1
                        Case "AVI", "DAT", "MPG", "MOV", "RM", "M2P", "MPEG", "DIVX", "MP4", "3GP", "M2TS"
                            c.Videos += 1
                            If gps Then c.VideosWithGps += 1
                        Case Else
                            c.Others += 1
                    End Select
                End While
            End Using
        End Using
        Return c
    End Function

    ''' <summary>The rows of <paramref name="files"/>, by date; files not in the index are left out.</summary>
    Public Function RowsOf(ByVal files As IEnumerable(Of String)) As List(Of IndexRow)
        Dim wanted As New HashSet(Of String)(files, StringComparer.OrdinalIgnoreCase)
        If wanted.Count = 0 Then Return New List(Of IndexRow)
        Return Rows("1 = 1", "Exif_Date, FileName").Where(Function(r) wanted.Contains(r.FileName)).ToList()
    End Function

    '==================================================================================================
    ' Helpers
    '==================================================================================================
    Private Function Counts(ByVal sql As String, ParamArray values As Object()) As List(Of KeyValuePair(Of String, Integer))
        Dim result As New List(Of KeyValuePair(Of String, Integer))
        If Not m_bolImplement Then Return result
        Using cmd As OleDbCommand = NewCommand(sql, values)
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result.Add(New KeyValuePair(Of String, Integer)(AsText(r(0)), CInt(r(1))))
                End While
            End Using
        End Using
        Return result
    End Function

    Private Function Rows(ByVal where As String, ByVal orderBy As String, ParamArray values As Object()) As List(Of IndexRow)
        Dim result As New List(Of IndexRow)
        If Not m_bolImplement Then Return result
        Dim cols As String = "FileName, Exif_Date, Exif_Ranking, Exif_Spot" &
                             If(m_bolPlaceColumns, ", Exif_City, Exif_Town", "") &
                             If(m_bolGpsColumns, ", Exif_Latitude, Exif_Longitude", "")
        Using cmd As OleDbCommand = NewCommand("Select " & cols & " From " & mc_strTableName & " Where " & where & " Order By " & orderBy, values)
            Using r As OleDbDataReader = cmd.ExecuteReader()
                While r.Read()
                    Dim row As New IndexRow With {
                        .FileName = AsText(r("FileName")),
                        .DateText = PadNum(AsText(r("Exif_Date")), 8),
                        .Ranking = CInt(Val(AsText(r("Exif_Ranking")))),
                        .Spot = AsText(r("Exif_Spot")).Trim()}
                    If m_bolPlaceColumns Then
                        row.City = AsText(r("Exif_City")).Trim()
                        row.Town = AsText(r("Exif_Town")).Trim()
                    End If
                    If m_bolGpsColumns AndAlso r("Exif_Latitude") IsNot DBNull.Value AndAlso r("Exif_Longitude") IsNot DBNull.Value Then
                        row.Latitude = CDbl(r("Exif_Latitude"))
                        row.Longitude = CDbl(r("Exif_Longitude"))
                    End If
                    result.Add(row)
                End While
            End Using
        End Using
        Return result
    End Function

End Class
