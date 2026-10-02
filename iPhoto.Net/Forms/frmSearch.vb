' Port of iPhoto\Form\frmSearch.frm: search the photos by title / people / place / remark / keywords,
' date range, ranking and media type; the hits go to g_lpSearch and frmSearchResult.
'
' Designer: the 評價 / 媒體類型 choices are the dropdowns' Items (VB6 filled them in Form_Load from
' hidden picture boxes, CreateRaingingList / CreateMediaList). Open with "Using f As New frmSearch".
' VB6 MenuItem.Value is MenuItem.Name here.
' Fixed from VB6: with the database, the media-type filter read the *ranking* list; without it, the
' end date was compared with the start date; quotes in the keyword fields can't break anything (the
' database is filtered in code, as VB6 did, not in SQL).
Imports System.Data

Friend Class frmSearch

    Private m_strCurrentDate As String = ""
    Private m_strBeforeOneYearDate As String = ""

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ddRanking.SelectedIndex = 0
        ddMediaType.SelectedIndex = 0
        rbRange(0).Checked = True
        RangeChanged(0)
    End Sub

    Private Sub rbRange_CheckedChanged(sender As Object, e As EventArgs) Handles rbRange_0.CheckedChanged, rbRange_1.CheckedChanged, rbRange_2.CheckedChanged
        Dim rb As Aqua.RadioButton = CType(sender, Aqua.RadioButton)
        If rb.Checked Then RangeChanged(Array.IndexOf(rbRange, rb))
    End Sub

    ''' <summary>VB6 rbRange_ValueChanged: the chosen range label black, the others grey; the date
    ''' boxes only for 自訂範圍.</summary>
    Private Sub RangeChanged(ByVal Index As Integer)
        For I As Integer = 0 To rbRange.Length - 1
            Dim chosen As Boolean = (I = Index)
            rbRange(I).ForeColor = If(chosen, Color.Black, Color.FromArgb(&H80, &H80, &H80))
            If I = 2 Then
                meStartDate.Enabled = chosen
                meEndDate.Enabled = chosen
                lblTo.ForeColor = rbRange(I).ForeColor
            End If
        Next
        If Index = 2 AndAlso meStartDate.Enabled Then meStartDate.Focus()
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butSearch_Click(sender As Object, e As EventArgs) Handles butSearch.Click
        If {txtTitle, txtCharacter, txtSpot, txtRemark, txtKeyWord}.All(Function(t) t.Text.Trim() = "") Then
            frmMsgBox.ShowCriticalMessage("尋找的條件範圍過大！", "")
            txtTitle.Focus()
            Return
        End If

        Enabled = False
        Application.UseWaitCursor = True
        lblSearching.Visible = True
        Loading1.Visible = True
        Loading1.Play = True
        Try
            g_lpSearch = Nothing
            g_intSearch = 0
            m_strCurrentDate = Date.Now.ToString("yyyyMMdd")
            m_strBeforeOneYearDate = Date.Now.AddYears(-1).ToString("yyyyMMdd")

            If g_lpDatabase.Implement Then
                SearchDatabaseFiles()
            Else
                For I As Integer = 0 To g_lpStorage.AlbumCount - 1
                    g_lpStorage.Album(I).Load()
                    For J As Integer = 0 To g_lpStorage.Album(I).ItemCount - 1
                        Dim lpClass As [Class] = g_lpStorage.Album(I).Item(J)
                        lpClass.Load()
                        SearchClassPhoto(lpClass)
                        Application.DoEvents()
                    Next
                Next
            End If
        Finally
            Loading1.Play = False
            Loading1.Visible = False
            lblSearching.Visible = False
            Enabled = True
            Application.UseWaitCursor = False
        End Try

        If g_intSearch <= 0 Then
            frmMsgBox.ShowCriticalMessage("沒有符合條件的相片！", "")
        Else
            Dim research As Boolean
            Using f As New frmSearchResult
                f.ShowDialog(Me)
                research = f.Research
            End Using
            If Not research Then Close()
        End If
    End Sub

    Private Sub AddHit(ByVal strFileDesc As String)
        ReDim Preserve g_lpSearch(g_intSearch)
        g_lpSearch(g_intSearch).FileDesc = strFileDesc
        g_intSearch += 1
    End Sub

    ''' <summary>VB6 InStr(1, UCase$(value), UCase$(box.Text)) > 0 for every box that is not blank.</summary>
    Private Shared Function Matches(ByVal value As String, ByVal box As Aqua.TextBox) As Boolean
        If box.Text.Trim() = "" Then Return True
        Return If(value, "").ToUpperInvariant().Contains(box.Text.ToUpperInvariant())
    End Function

    ''' <summary>Ranking / media type filters; "" = 全部.</summary>
    Private Function MatchesLists(ByVal ranking As String, ByVal mediaType As enumPhotoMediaType) As Boolean
        Dim wantRank As String = If(ddRanking.SelectedItem?.Name, "").Trim()
        If wantRank <> "" AndAlso Val(wantRank) <> Val(ranking) Then Return False
        Select Case If(ddMediaType.SelectedItem?.Name, "").Trim()
            Case "0" : If mediaType <> enumPhotoMediaType.mdImage Then Return False
            Case "1" : If mediaType <> enumPhotoMediaType.mdVideo Then Return False
        End Select
        Return True
    End Function

    Private Function DateDigits(ByVal box As Aqua.MaskEdit) As Double
        Return Val(Convert.ToString(box.Value))
    End Function

    Private Sub SearchDatabaseFiles()
        Dim strSQL As String = ""
        If rbRange(1).Checked Then
            strSQL = "(Exif_Date >= '" & m_strBeforeOneYearDate & "')"
        ElseIf rbRange(2).Checked Then
            Dim strStartValue As String = If(DateDigits(meStartDate) > 0, DateDigits(meStartDate).ToString("00000000"), "00000000")
            Dim strEndValue As String = If(DateDigits(meEndDate) > 0, DateDigits(meEndDate).ToString("00000000"), m_strCurrentDate)
            strSQL = "(Exif_Date Between '" & strStartValue & "' And '" & strEndValue & "')"
        End If

        Dim rows As DataTable = Nothing
        If Not g_lpDatabase.SearchFileRecordSet(strSQL, rows) Then Return
        Dim n As Integer = 0
        For Each r As DataRow In rows.Rows
            Dim f As Func(Of String, String) = Function(col) If(IsDBNull(r(col)), "", Convert.ToString(r(col)))
            Dim strFile As String = f("FileName")
            If g_lpFileSystem.FileExists(strFile) AndAlso
               Matches(f("Exif_Title"), txtTitle) AndAlso Matches(f("Exif_Character"), txtCharacter) AndAlso
               Matches(f("Exif_Spot") & If(rows.Columns.Contains("Exif_City"), " " & f("Exif_City"), ""), txtSpot) AndAlso Matches(f("Exif_Remark"), txtRemark) AndAlso
               Matches(f("Exif_KeyWord"), txtKeyWord) AndAlso
               MatchesLists(f("Exif_Ranking"), GetMediaType(g_lpFileSystem, strFile)) Then
                AddHit(strFile)
            End If
            n += 1
            If n Mod 200 = 0 Then Application.DoEvents()
        Next
    End Sub

    Private Sub SearchClassPhoto(ByVal lpClass As [Class])
        For I As Integer = 0 To lpClass.PhotoCount - 1
            Dim p As Photo = lpClass.Photo(I)
            Dim photoDate As Double = Val(p.Exif(enumPhotoExif.peDate))
            If rbRange(1).Checked Then
                If photoDate < Val(m_strBeforeOneYearDate) Then Continue For
            ElseIf rbRange(2).Checked Then
                If DateDigits(meStartDate) > 0 AndAlso photoDate < DateDigits(meStartDate) Then Continue For
                If DateDigits(meEndDate) > 0 AndAlso photoDate > DateDigits(meEndDate) Then Continue For
            End If
            If Matches(p.Exif(enumPhotoExif.peTitle), txtTitle) AndAlso Matches(p.Exif(enumPhotoExif.peCharacter), txtCharacter) AndAlso
               Matches(p.Exif(enumPhotoExif.peSpot), txtSpot) AndAlso Matches(p.Exif(enumPhotoExif.peRemark), txtRemark) AndAlso
               Matches(p.Exif(enumPhotoExif.peKeyWord), txtKeyWord) AndAlso
               MatchesLists(p.Exif(enumPhotoExif.peRanking), p.MediaType) Then
                AddHit(p.FileDesc)
            End If
        Next
    End Sub

    Private Sub imgKeyWords_Click(sender As Object, e As EventArgs) Handles imgKeyWords_0.Click, imgKeyWords_1.Click, imgKeyWords_2.Click, imgKeyWords_3.Click, imgKeyWords_4.Click
        Dim Index As Integer = Array.IndexOf(imgKeyWords, sender)
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        Dim strKeyWords As String
        Using f As New frmKeyWords
            strKeyWords = f.ShowKeyWords(g_lpConfig.Attached(Config.enumAttachedFile.filKeyWord))
        End Using
        If strKeyWords.Trim() = "" Then Return
        Dim target As Aqua.TextBox = {txtTitle, txtCharacter, txtSpot, txtRemark, txtKeyWord}(Index)
        target.Text = strKeyWords
        target.Focus()
    End Sub

End Class
