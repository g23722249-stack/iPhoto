' Port of iPhoto\Form\frmSearchResult.frm: the hits of frmSearch (g_lpSearch) in a grid -- tick the
' photos to add to the dock, click the info icon for a photo's details, make a photo book, or search
' again. The grid's columns are Grid1.Columns in the designer (VB6 Header.AdditionHeader).
' Change from VB6: 建立攝影集 no longer hides this (modal) dialog and shows it again around
' frmBuildBook -- hiding a modal .NET dialog ends it -- frmBuildBook just opens over it.
Friend Class frmSearchResult

    Private m_bolReSearch As Boolean

    ''' <summary>True when the user asked to search again (frmSearch then stays open).</summary>
    Public ReadOnly Property Research As Boolean
        Get
            Return m_bolReSearch
        End Get
    End Property

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Text = "尋找結果：" & g_intSearch & " 張相片"
        MoveSearchResultToGrid()
        m_bolReSearch = False
        butDock.Enabled = True
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Grid1.Clear()
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butResearch_Click(sender As Object, e As EventArgs) Handles butResearch.Click
        m_bolReSearch = True
        Close()
    End Sub

    '建立攝影集
    Private Sub butFavorite_Click(sender As Object, e As EventArgs) Handles butFavorite.Click
        If g_intSearch <= 0 Then Return
        Dim strFileName(g_intSearch - 1) As String
        For I As Integer = 0 To g_intSearch - 1
            strFileName(I) = g_lpSearch(I).FileDesc
        Next
        Using f As New frmBuildBook : f.BuildBook(strFileName, g_intSearch) : End Using
    End Sub

    ''' <summary>Columns: 0 check box (value = full path), 1 info icon, 2 name, 3 date/time, 4 folder.</summary>
    Private Sub MoveSearchResultToGrid()
        Grid1.Clear()
        Grid1.BeginUpdate()
        Try
            For I As Integer = 0 To g_intSearch - 1
                Dim strFile As String = g_lpSearch(I).FileDesc

                Dim c As Aqua.Cell = Grid1.Cell(0, I)
                c.CheckStyle = Aqua.ItemCheckStyle.Check
                c.Checked = True
                c.Value = strFile

                Grid1.Cell(1, I).Icon = imgInfo.Image

                c = Grid1.Cell(2, I)
                c.Value = g_lpFileSystem.AnalyseFile(fsFileName, strFile)
                c.Text = c.Value

                Dim p As New Photo
                p.Construct(strFile)
                Dim strDate As String = p.Exif(enumPhotoExif.peDate)
                Dim strTime As String = p.Exif(enumPhotoExif.peTime)
                c = Grid1.Cell(3, I)
                c.Value = strDate & strTime
                c.Text = FormatDigits(strDate, "0000/00/00") & " " & FormatDigits(strTime, "00:00:00")

                c = Grid1.Cell(4, I)
                c.Value = g_lpFileSystem.AnalyseFile(fsParentFolderName, strFile)
                c.Text = c.Value
            Next
        Finally
            Grid1.EndUpdate()
        End Try
    End Sub

    ''' <summary>VB6 Format("20040424", "0000/00/00").</summary>
    Private Shared Function FormatDigits(ByVal szValue As String, ByVal szPicture As String) As String
        Dim n As Long
        If Long.TryParse(If(szValue, "").Trim(), n) Then Return n.ToString(szPicture)
        Return If(szValue, "")
    End Function

    Private Sub Grid1_ItemIconClick(col As Integer, row As Integer) Handles Grid1.ItemIconClick
        If row >= Grid1.RowCount Then Return
        frmShowPhoto.ShowPhoto(Grid1.Cell(0, row).Value)
    End Sub

    '加入 Dock
    Private Sub butDock_Click(sender As Object, e As EventArgs) Handles butDock.Click
        For I As Integer = 0 To Grid1.RowCount - 1
            If Grid1.Cell(0, I).Checked Then
                Dim strFileName As String = Grid1.Cell(0, I).Value
                Dim dt As String = Grid1.Cell(3, I).Value
                g_lpDock.AddItem(strFileName, Mid(dt, 1, 8), Mid(dt, 9, 6))
            End If
        Next
        butDock.Enabled = False
    End Sub

End Class
