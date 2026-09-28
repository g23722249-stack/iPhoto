' Port of iPhoto\Form\frmPhotoIndex.frm: a contact sheet of one album root (or photo-book folder) --
' each class gets a title line and its photos in rows of Slider1 thumbnails; each page carries the
' album name, the print date (optional) and the page number. "預覽" lays the pages out; the arrows page
' through them; "列印" prints the chosen pages; "存檔" saves every page as a JPEG.
'
' VB6 -> .NET: VB6 laid the sheet out twice (preview and printer) while drawing. Here LayoutPages makes
' the pages once as lists of mm drawing steps, and DrawPage replays one on the preview, the printer
' or a JPEG page (Carbon.Printer, mm). picCanvas_0 is not used (hidden in the designer).
' Fixed from VB6:
'   - the last photo of every class was left out (and a class of one photo showed none): the loop ran
'     "until iCount + 1 >= PhotoCount".
'   - "列印" printed every page whatever was chosen in frmPrintPages.
'   - photo-book folders were read from the album list (Album(i) instead of Favorite(i)).
'   - a page could end with a class title and no photos under it.
'   - cancelling the folder of "存檔" saved to C:\; pages are 200 dpi (VB6: screen resolution).
' Callers use "Using f As New frmPhotoIndex".
Imports System.Drawing.Printing

Friend Class frmPhotoIndex

    Private Const mc_szChineseFontName As String = "華康細圓體"
    Private Const mc_szEnglishFontName As String = "華康細圓體"
    Private Const DefaultPaperWidth As Integer = 210, DefaultPaperHeight As Integer = 297
    Private Const SaveDpi As Single = 200

    ''' <summary>One drawing step of a page, in mm: a text (Photo Is Nothing) or a photo.</summary>
    Private Class DrawStep
        Public X As Single, Y As Single, Width As Single, Height As Single
        Public Text As String
        Public Alignment As Carbon.PrintAlignment
        Public FontName As String
        Public FontSize As Single
        Public Photo As Photo
    End Class

    Private m_enumExeMode As enumExeMode
    Private m_iSectionIndex As Integer = -1
    Private m_lpExpNode As TreeNode

    Private m_iPaperWidth As Integer
    Private m_iPaperHeight As Integer
    Private m_dblScale As Double                  ' preview pixels per mm
    Private m_objMatrixPrint As MatrixPrint
    Private m_printer As PrinterSettings

    Private ReadOnly m_pages As New List(Of List(Of DrawStep))
    Private ReadOnly m_measure As New Bitmap(1, 1)

    Public Sub ShowPhotoIndex(ByVal Mode As enumExeMode)
        m_enumExeMode = Mode
        ShowDialog()
    End Sub

    '==================================================================================================
    ' Load
    '==================================================================================================
    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Top = 6
        m_objMatrixPrint = New MatrixPrint
        With m_objMatrixPrint
            .Borderland(enumBorderland.arrHeader) = 6
            .Borderland(enumBorderland.arrFooter) = 6
            .Borderland(enumBorderland.arrLeft) = 10
            .Borderland(enumBorderland.arrRight) = 10
            .KeepWidth = 0
            .KeepHeight = 0
        End With
        MoveStorageToScreen()
        LibPrint.FillPrinters(cboPrinter)
        cboPrinter_SelectedChanged(cboPrinter, EventArgs.Empty)
        Slider1_ValueChanged(Slider1, EventArgs.Empty)
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        SetPreview(Nothing)
        m_measure.Dispose()
    End Sub

    Private Sub MoveStorageToScreen()
        m_lpExpNode = Nothing
        LibUserInterface.AddSectionFakeKeyToTreeView(m_enumExeMode, g_lpStorage, tvList)
        If tvList.Nodes.Count = 0 Then Return
        LibUserInterface.LoadTreeViewRecentSection(m_enumExeMode, g_lpStorage, tvList)
        ' VB6 started on the first album (index 0) until another was clicked
        Dim n As TreeNode = If(tvList.SelectedNode, tvList.Nodes(0))
        Do While n.Parent IsNot Nothing
            n = n.Parent
        Loop
        m_iSectionIndex = If(TypeOf n.Tag Is Integer, CInt(n.Tag), 0)
    End Sub

    Private Function Section(ByVal index As Integer) As Albums
        Return If(m_enumExeMode = enumExeMode.exeFavorites, g_lpStorage.Favorite(index), g_lpStorage.Album(index))
    End Function

    '==================================================================================================
    ' Tree
    '==================================================================================================
    Private Sub tvList_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvList.NodeMouseClick
        If e.Button <> MouseButtons.Left Then Return
        If (tvList.HitTest(e.Location).Location And TreeViewHitTestLocations.PlusMinus) <> 0 Then Return
        tvList.SelectedNode = e.Node
        If e.Node.Parent Is Nothing Then
            ' 最頂層
            e.Node.Expand()
            If TypeOf e.Node.Tag Is Integer Then m_iSectionIndex = CInt(e.Node.Tag)
        ElseIf TypeOf e.Node.Parent.Tag Is Integer Then
            m_iSectionIndex = CInt(e.Node.Parent.Tag)
        End If
    End Sub

    Private Sub tvList_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles tvList.BeforeExpand
        If e.Node.Parent IsNot Nothing OrElse Not TypeOf e.Node.Tag Is Integer Then Return
        If m_lpExpNode IsNot Nothing AndAlso m_lpExpNode IsNot e.Node Then m_lpExpNode.Collapse()
        m_lpExpNode = e.Node
        LibUserInterface.ExpandFakeSectionKeyToTreeView(m_enumExeMode, g_lpStorage, tvList, e.Node, CInt(e.Node.Tag))
        m_iSectionIndex = CInt(e.Node.Tag)
    End Sub

    Private Sub tvList_GotFocus(sender As Object, e As EventArgs) Handles tvList.GotFocus
        frmClass.BorderColor = frmClass.BorderFocusColor
    End Sub

    '==================================================================================================
    ' Printer and paper
    '==================================================================================================
    Private Sub cboPrinter_SelectedChanged(sender As Object, e As EventArgs) Handles cboPrinter.SelectedChanged
        m_printer = LibPrint.SettingsFor(cboPrinter)
        ApplyPaper()
    End Sub

    Private Sub ApplyPaper()
        If Not LibPrint.PrintableMm(m_printer, m_iPaperWidth, m_iPaperHeight) Then
            m_iPaperWidth = 0
            m_iPaperHeight = 0
        End If
        If m_iPaperWidth + m_iPaperHeight = 0 Then
            SetPaperPosition(DefaultPaperWidth, DefaultPaperHeight)
        Else
            SetPaperPosition(m_iPaperWidth, m_iPaperHeight)
        End If
        ClearPages()
    End Sub

    Private Sub butAdviance_Click(sender As Object, e As EventArgs) Handles butAdviance.Click
        If m_printer Is Nothing Then Return
        Using dlg As New PrintDialog With {.PrinterSettings = m_printer, .UseEXDialog = True, .AllowSomePages = False}
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                m_printer = dlg.PrinterSettings
                ApplyPaper()
            End If
        End Using
    End Sub

    Private Sub butPaper_Click(sender As Object, e As EventArgs) Handles butPaper.Click
        Using f As New frmPaperSetup
            If f.ShowPaperSetup(m_objMatrixPrint) Then ClearPages()
        End Using
    End Sub

    Private Sub SetPaperPosition(ByVal intWidth As Integer, ByVal intHeight As Integer)
        m_objMatrixPrint.Width = intWidth
        m_objMatrixPrint.Height = intHeight
        Dim canvasW As Integer = picContainer.ClientSize.Width - 10, canvasH As Integer = picContainer.ClientSize.Height - 10
        m_dblScale = Math.Min(canvasW / intWidth, canvasH / intHeight)
        Dim w As Integer = CInt(intWidth * m_dblScale), h As Integer = CInt(intHeight * m_dblScale)
        picPaper.SetBounds((picContainer.ClientSize.Width - w) \ 2, (picContainer.ClientSize.Height - h) \ 2, w, h)
        picShadow.SetBounds(picPaper.Left + 3, picPaper.Top + 3, w, h)
        Dim old As Image = picShadow.Image
        picShadow.Image = LibPrint.ShadowImage(w, h, Color.FromArgb(153, 153, 153), Color.FromArgb(178, 178, 178), Color.FromArgb(204, 204, 204))
        old?.Dispose()
        picShadow.SendToBack()
    End Sub

    Private Sub Slider1_ValueChanged(sender As Object, e As EventArgs) Handles Slider1.ValueChanged
        txtLimit.Text = CStr(Slider1.Value)
        If m_objMatrixPrint Is Nothing Then Return
        m_objMatrixPrint.Limit = Slider1.Value
        ClearPages()
    End Sub

    Private Sub chkPrintNote_CheckedChanged(sender As Object, e As EventArgs) Handles chkPrintNote_0.CheckedChanged, chkPrintNote_1.CheckedChanged
        ClearPages()
    End Sub

    ''' <summary>The layout no longer matches the settings: preview again before printing.</summary>
    Private Sub ClearPages()
        m_pages.Clear()
        butSave.Enabled = False
        butPrint.Enabled = False
        UpDown1.Enabled = False
        lblPage.Text = "0 of 0 張"
        ShowBlankPaper()
    End Sub

    '==================================================================================================
    ' Layout
    '==================================================================================================
    Private Function TextHeightMm(ByVal text As String, ByVal fontName As String, ByVal fontSize As Single) As Single
        Using g As Graphics = Graphics.FromImage(m_measure)
            Return New Carbon.Printer(g).TextHeight(text, fontName, fontSize)
        End Using
    End Function

    Private Sub LayoutPages()
        m_pages.Clear()
        If m_iSectionIndex < 0 Then Return
        Dim lpAlbums As Albums = Section(m_iSectionIndex)
        lpAlbums.Load()

        Dim mp As MatrixPrint = m_objMatrixPrint
        Dim left As Single = mp.Borderland(enumBorderland.arrLeft)
        Dim relWidth As Single = mp.Width - mp.Borderland(enumBorderland.arrLeft) - mp.Borderland(enumBorderland.arrRight) - mp.KeepWidth * 2
        Dim bottom As Single = mp.Height - mp.Borderland(enumBorderland.arrFooter) - mp.KeepHeight
        Dim itemW As Single = mp.ItemWidth, itemH As Single = mp.ItemHeight
        Dim classTitleH As Single = TextHeightMm("標題", mc_szChineseFontName, 14) + 2

        Dim page As List(Of DrawStep) = Nothing
        Dim top As Single
        Dim newPage = Sub()
                          page = New List(Of DrawStep)
                          m_pages.Add(page)
                          ' 列印每頁標題
                          Dim y As Single = mp.Borderland(enumBorderland.arrHeader)
                          page.Add(New DrawStep With {.X = left, .Y = y, .Width = relWidth, .Text = lpAlbums.Name, .Alignment = Carbon.PrintAlignment.alCenter, .FontName = mc_szChineseFontName, .FontSize = 20})
                          Dim note As String = If(chkPrintNote_0.Checked, "列印時間：" & Now.ToString("yyyy/M/d") & "  頁次：" & m_pages.Count, "頁次：" & m_pages.Count)
                          page.Add(New DrawStep With {.X = left, .Y = y, .Width = relWidth, .Text = note, .Alignment = Carbon.PrintAlignment.alRight, .FontName = mc_szEnglishFontName, .FontSize = 12})
                          top = y + TextHeightMm(lpAlbums.Name, mc_szChineseFontName, 20) + 2
                      End Sub
        newPage()

        For I As Integer = 0 To lpAlbums.ItemCount - 1
            Dim lpClass As PhotoSet = lpAlbums.Item(I)
            lpClass.Load()
            If lpClass.PhotoCount <= 0 Then Continue For
            ' the class title with at least one row under it
            If top + classTitleH + itemH > bottom AndAlso page.Count > 2 Then newPage()

            ' 列印每個 Class 標題
            Dim title As String = If(chkPrintNote_1.Checked, lpClass.DisplayName & "（" & lpClass.PhotoCount & "張）", lpClass.DisplayName)
            page.Add(New DrawStep With {.X = left, .Y = top, .Width = relWidth, .Text = title, .Alignment = Carbon.PrintAlignment.alLeft, .FontName = mc_szChineseFontName, .FontSize = 14})
            top += classTitleH

            Dim iCount As Integer = 0
            Do While iCount < lpClass.PhotoCount
                If top + itemH > bottom Then newPage()
                For J As Integer = 0 To mp.Limit - 1          ' 列印每一行
                    If iCount >= lpClass.PhotoCount Then Exit For
                    page.Add(New DrawStep With {.X = mp.ItemLeft(J), .Y = top, .Width = itemW, .Height = itemH, .Photo = lpClass.Photo(iCount)})
                    iCount += 1
                Next
                top += itemH + 1
            Loop
        Next
    End Sub

    ''' <summary>The photo (a video's first frame) about w x h pixels, portraits turned sideways (VB6 Rotate(-90)).</summary>
    Private Shared Function LoadFixedSizePicture(ByVal lpPhoto As Photo, ByVal w As Integer, ByVal h As Integer) As Bitmap
        If lpPhoto.MediaType <> enumPhotoMediaType.mdImage AndAlso lpPhoto.MediaType <> enumPhotoMediaType.mdVideo Then Return Nothing
        Dim thumb As New Quartz.Thumbnail With {.FileName = lpPhoto.FileDesc}
        Dim pic As Bitmap
        If lpPhoto.MediaType = enumPhotoMediaType.mdImage Then
            Dim info As New Quartz.ImageInfo With {.FileName = lpPhoto.FileDesc}
            pic = If(info.Height > info.Width, thumb.GetThumbnail(h, w), thumb.GetThumbnail(w, h))
        Else
            pic = thumb.GetThumbnail(w, h)
        End If
        If pic IsNot Nothing AndAlso pic.Height > pic.Width Then pic.RotateFlip(RotateFlipType.Rotate270FlipNone)   ' 3x4 照片轉 4x3
        Return pic
    End Function

    ''' <summary>Replays one page on any Graphics; pxPerMm sets the thumbnail resolution.</summary>
    Private Sub DrawPage(ByVal g As Graphics, ByVal index As Integer, ByVal pxPerMm As Double, Optional ByVal scale As Double = 1)
        If index < 0 OrElse index >= m_pages.Count Then Return
        Dim pr As New Carbon.Printer(g) With {.SetScale = scale}
        For Each s As DrawStep In m_pages(index)
            If s.Photo Is Nothing Then
                pr.PrintText(s.X, s.Y, s.Width, s.Text, Color.Black, s.Alignment, s.FontName, s.FontSize)
            Else
                Using pic As Bitmap = LoadFixedSizePicture(s.Photo, Math.Max(1, CInt(s.Width * pxPerMm)), Math.Max(1, CInt(s.Height * pxPerMm)))
                    If pic Is Nothing Then Continue For
                    Dim ratio As Double = Math.Min(s.Width / pic.Width, s.Height / pic.Height)
                    Dim w As Single = CSng(pic.Width * ratio), h As Single = CSng(pic.Height * ratio)
                    pr.PaintPicture(pic, s.X + (s.Width - w) / 2, s.Y + (s.Height - h) / 2, w, h, False)
                End Using
            End If
        Next
    End Sub

    '==================================================================================================
    ' Preview
    '==================================================================================================
    Private Sub SetPreview(ByVal bmp As Bitmap)
        Dim old As Image = picPaper.Image
        picPaper.Image = bmp
        old?.Dispose()
    End Sub

    Private Function NewPaperBitmap() As Bitmap
        Dim bmp As New Bitmap(Math.Max(1, picPaper.Width), Math.Max(1, picPaper.Height))
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.White)
        End Using
        Return bmp
    End Function

    Private Shared Sub PaintPaperBorder(ByVal bmp As Bitmap)
        Using g As Graphics = Graphics.FromImage(bmp), p As New Pen(Color.Gray)
            g.DrawRectangle(p, 0, 0, bmp.Width - 1, bmp.Height - 1)
        End Using
    End Sub

    Private Sub ShowBlankPaper()
        If picPaper.Width <= 0 Then Return
        Dim bmp As Bitmap = NewPaperBitmap()
        PaintPaperBorder(bmp)
        SetPreview(bmp)
    End Sub

    Private Sub ShowPage(ByVal index As Integer)
        Dim bmp As Bitmap = NewPaperBitmap()
        Using g As Graphics = Graphics.FromImage(bmp)
            DrawPage(g, index, m_dblScale, m_dblScale / (g.DpiX / 25.4))
        End Using
        PaintPaperBorder(bmp)
        SetPreview(bmp)
        lblPage.Text = (index + 1) & " of " & m_pages.Count & " 張"
    End Sub

    Private Sub butPreview_Click(sender As Object, e As EventArgs) Handles butPreview.Click
        If m_iSectionIndex < 0 Then Return
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            LayoutPages()
            If m_pages.Count > 0 Then
                butSave.Enabled = True
                butPrint.Enabled = m_printer IsNot Nothing AndAlso m_iPaperWidth + m_iPaperHeight > 0
                UpDown1.Minimum = 1
                UpDown1.Maximum = m_pages.Count
                UpDown1.Enabled = True
                If UpDown1.Value <> 1 Then UpDown1.Value = 1 Else ShowPage(0)
            End If
        Finally
            Cursor = Cursors.Default
            Enabled = True
        End Try
    End Sub

    Private Sub UpDown1_ValueChanged(sender As Object, e As EventArgs) Handles UpDown1.ValueChanged
        If m_pages.Count = 0 Then Return      ' (UpDown1.Enabled reads False while the form is disabled)
        Cursor = Cursors.WaitCursor
        Try
            ShowPage(Math.Max(0, Math.Min(m_pages.Count - 1, UpDown1.Value - 1)))
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    '==================================================================================================
    ' Print / save
    '==================================================================================================
    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butPrint_Click(sender As Object, e As EventArgs) Handles butPrint.Click
        If m_printer Is Nothing OrElse m_pages.Count = 0 Then
            frmMsgBox.ShowCriticalMessage("設定印表機失敗", "")
            Return
        End If
        Dim pages As List(Of Integer) = LibPrint.ChoosePages(UpDown1.Value, m_pages.Count)
        If pages Is Nothing Then Return
        Dim result As String
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            ' thumbnails at 300 dpi (VB6: 10x the screen size)
            result = LibPrint.PrintPages(m_printer, 1, pages, Sub(g, page) DrawPage(g, page, 300 / 25.4))
        Finally
            Cursor = Cursors.Default
            Enabled = True
        End Try
        If result <> "" Then frmMsgBox.ShowCriticalMessage("列印失敗：" & result, "")
    End Sub

    Private Sub butSave_Click(sender As Object, e As EventArgs) Handles butSave.Click
        Dim strPath As String = frmBrowserFolder.GetFolder("另存照片目錄")
        If strPath.Trim() = "" Then Return
        If Not g_lpFileSystem.FolderExists(strPath) Then
            frmMsgBox.ShowCriticalMessage("目錄不存在", "")
            Return
        End If
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            LibPrint.SavePagesJpeg(strPath, "iPhoto-照片目錄", m_objMatrixPrint.Width, m_objMatrixPrint.Height, m_pages.Count, SaveDpi,
                                   Sub(g, page) DrawPage(g, page, SaveDpi / 25.4))
        Finally
            Cursor = Cursors.Default
            Enabled = True
        End Try
        frmMsgBox.ShowExclamationMessage("存檔完成", "")
    End Sub

End Class
