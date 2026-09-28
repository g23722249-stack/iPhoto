' Port of iPhoto\Form\frmPrint.frm: prints the given photos N per row (Slider1) on the chosen printer's
' paper, laid out by MatrixPrint (mm). "自動" fills the pages in order; "手動" is one page whose cells are
' filled by dragging photos from MediaList1 onto them (right-click empties a cell). Options: the photo's
' EXIF date in the corner, a "列印時間" footer, copies; "存檔" saves every page as a JPEG.
'
' VB6 -> .NET: one drawing routine (DrawPage, in mm through Carbon.Printer) now feeds the preview, the
' printer and the JPEG pages, instead of three copies of the layout code. The preview is a bitmap on
' picPaper (the VB6 picPicture control array is gone -- picPicture_0 and picSave stay hidden in the
' designer); the printer is a PrintDocument (paper = the printable area, VB6 Printer.ScaleWidth).
' Fixed / changed from VB6:
'   - the pages chosen in frmPrintPages print as one job (VB6 sent one job per page).
'   - "存檔" pages are 200 dpi (VB6: screen resolution, 3.78 px per mm) and include the date stamp
'     like the print; cancelling the folder cancels the save (VB6 saved to C:\).
'   - the date stamp is sized to the printed picture (VB6's formula always gave its 8 pt minimum).
'   - with no usable printer the layout falls back to A4 instead of dividing by a zero paper size.
' Callers use "Using f As New frmPrint".
Imports System.Drawing.Printing

Friend Class frmPrint

    Private Class PictureSlot
        Public FileName As String
        Public Page As Integer
        Public Index As Integer
    End Class

    Private Const mc_intAssignPrintCount As Integer = 256
    Private Const DefaultPaperWidth As Integer = 210, DefaultPaperHeight As Integer = 297
    Private Const SaveDpi As Single = 200

    Private m_iPaperWidth As Integer
    Private m_iPaperHeight As Integer
    Private m_bolLoad As Boolean
    Private m_dblScale As Double                 ' preview pixels per mm
    Private m_objMatrixPrint As MatrixPrint
    Private m_printer As PrinterSettings

    Private m_intTotalPage As Integer             ' 總共列印的張數
    Private m_lpPicture As New List(Of PictureSlot)
    Private m_lpAssignPrint(mc_intAssignPrintCount - 1) As String   ' 手動列印

    Public Sub ShowPrintPhoto(ByVal FileName() As String, ByVal Count As Integer)
        m_lpPicture = New List(Of PictureSlot)
        For I As Integer = 0 To Count - 1
            m_lpPicture.Add(New PictureSlot With {.FileName = FileName(I)})
        Next
        m_bolLoad = False
        ShowDialog()
    End Sub

    Private ReadOnly Property ManualMode As Boolean
        Get
            Return rdPrintMode_1.Checked
        End Get
    End Property

    '==================================================================================================
    ' Load
    '==================================================================================================
    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Top = 6
        m_objMatrixPrint = New MatrixPrint
        With m_objMatrixPrint
            .Borderland(enumBorderland.arrHeader) = 0
            .Borderland(enumBorderland.arrFooter) = 0
            .Borderland(enumBorderland.arrLeft) = 18
            .Borderland(enumBorderland.arrRight) = 18
            .KeepWidth = 0
            .KeepHeight = 15
        End With
        Array.Clear(m_lpAssignPrint, 0, m_lpAssignPrint.Length)

        AdditionDropDownList()
        If cboDefault.Items.Count > 0 Then cboDefault.SelectedIndex = 0
        If cboType.Items.Count > 0 Then cboType.SelectedIndex = 0
        ApplyPrintMode()
        txtPage.Text = "1"
        m_bolLoad = True
        Timer1.Enabled = True
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        SetPreview(Nothing)
    End Sub

    Private Sub AdditionDropDownList()
        LibPrint.FillPrinters(cboPrinter)
        cboPrinter_SelectedChanged(cboPrinter, EventArgs.Empty)
    End Sub

    Private Sub AdditionMediaItem()
        Select Case m_lpPicture.Count
            Case Is <= 4 : MediaList1.Limit = 2
            Case Is <= 8 : MediaList1.Limit = 3
            Case Is <= 18 : MediaList1.Limit = 4
            Case Is <= 24 : MediaList1.Limit = 5
            Case Else : MediaList1.Limit = 6
        End Select
        For Each p As PictureSlot In m_lpPicture
            MediaList1.AddItem(p.FileName)
            Application.DoEvents()
        Next
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Not m_bolLoad Then Return
        Timer1.Enabled = False
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            AdditionMediaItem()
            Slider1_ValueChanged(Slider1, EventArgs.Empty)
        Finally
            Enabled = True
            Cursor = Cursors.Default
        End Try
    End Sub

    '==================================================================================================
    ' Printer and paper
    '==================================================================================================
    Private Sub cboPrinter_SelectedChanged(sender As Object, e As EventArgs) Handles cboPrinter.SelectedChanged
        m_printer = LibPrint.SettingsFor(cboPrinter)
        ApplyPaper()
    End Sub

    ''' <summary>Paper = the printable area of the printer's current page setup, in whole mm.</summary>
    Private Sub ApplyPaper()
        If Not LibPrint.PrintableMm(m_printer, m_iPaperWidth, m_iPaperHeight) Then
            m_iPaperWidth = 0
            m_iPaperHeight = 0
        End If
        If m_iPaperWidth + m_iPaperHeight = 0 Then
            lblPaper.Text = "設定紙張大小失敗  請重新選取印表機"
            lblPaper.ForeColor = Color.Red
            SetPaperPosition(DefaultPaperWidth, DefaultPaperHeight)
        Else
            lblPaper.Text = "紙張大小 = " & m_iPaperWidth & " x " & m_iPaperHeight & "（mm）"
            lblPaper.ForeColor = lblLimitInfo.ForeColor
            SetPaperPosition(m_iPaperWidth, m_iPaperHeight)
        End If
        Slider1_ValueChanged(Slider1, EventArgs.Empty)
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
            If f.ShowPaperSetup(m_objMatrixPrint) Then Slider1_ValueChanged(Slider1, EventArgs.Empty)
        End Using
    End Sub

    ''' <summary>Fits the paper into picContainer (20 px free on every side) with its shadow.</summary>
    Private Sub SetPaperPosition(ByVal intWidth As Integer, ByVal intHeight As Integer)
        m_objMatrixPrint.Width = intWidth
        m_objMatrixPrint.Height = intHeight

        Dim canvasW As Integer = picContainer.ClientSize.Width - 40, canvasH As Integer = picContainer.ClientSize.Height - 40
        m_dblScale = Math.Min(canvasW / intWidth, canvasH / intHeight)
        Dim w As Integer = CInt(intWidth * m_dblScale), h As Integer = CInt(intHeight * m_dblScale)
        picPaper.SetBounds((picContainer.ClientSize.Width - w) \ 2, (picContainer.ClientSize.Height - h) \ 2, w, h)

        picShadow.SetBounds(picPaper.Left + 3, picPaper.Top + 3, w, h)
        Dim old As Image = picShadow.Image
        picShadow.Image = LibPrint.ShadowImage(w, h, Color.FromArgb(163, 163, 163), Color.FromArgb(188, 188, 188), Color.FromArgb(204, 204, 204))
        old?.Dispose()
        picShadow.SendToBack()
    End Sub

    '==================================================================================================
    ' Layout
    '==================================================================================================
    Private Sub cboType_SelectedChanged(sender As Object, e As EventArgs) Handles cboType.SelectedChanged
        If m_bolLoad Then Slider1_ValueChanged(Slider1, EventArgs.Empty)
    End Sub

    Private Sub chkPrintExifDate_CheckedChanged(sender As Object, e As EventArgs) Handles chkPrintExifDate.CheckedChanged, chkPrintOuputDateTime.CheckedChanged
        Slider1_ValueChanged(Slider1, EventArgs.Empty)
    End Sub

    Private Sub rdPrintMode_CheckedChanged(sender As Object, e As EventArgs) Handles rdPrintMode_0.CheckedChanged, rdPrintMode_1.CheckedChanged
        If Not CType(sender, Aqua.RadioButton).Checked Then Return
        ApplyPrintMode()
        If m_bolLoad Then Slider1_ValueChanged(Slider1, EventArgs.Empty)
    End Sub

    Private Sub ApplyPrintMode()
        txtPage.Visible = Not ManualMode
        udPage.Visible = Not ManualMode
        MediaList1.DragItem = ManualMode
        If Not ManualMode Then txtPage.Text = "1"
        Array.Clear(m_lpAssignPrint, 0, m_lpAssignPrint.Length)
    End Sub

    Private Sub Slider1_ValueChanged(sender As Object, e As EventArgs) Handles Slider1.ValueChanged
        If Not m_bolLoad Then Return
        txtLimit.Text = CStr(Slider1.Value)
        m_objMatrixPrint.Orientation = If(If(cboType.SelectedItem?.Name, "H") = "V", enumOrientationMode.垂直排列, enumOrientationMode.水平排列)
        m_objMatrixPrint.TextHeight(enumText.txtFooter) = If(chkPrintOuputDateTime.Checked, 5, 0)
        m_objMatrixPrint.Limit = Slider1.Value

        Dim cells As Integer = Math.Max(1, m_objMatrixPrint.Count)
        m_intTotalPage = Math.Max(1, (m_lpPicture.Count + cells - 1) \ cells)
        lblLimitInfo.Text = "已選取 " & m_lpPicture.Count & " 張照片  將列印成 " & m_intTotalPage & " 頁"
        udPage.Minimum = 1
        udPage.Maximum = m_intTotalPage

        For I As Integer = 0 To m_lpPicture.Count - 1
            m_lpPicture(I).Page = I \ cells
            m_lpPicture(I).Index = I Mod cells
        Next
        ' a manual cell beyond the new layout is dropped
        For I As Integer = cells To m_lpAssignPrint.Length - 1
            m_lpAssignPrint(I) = Nothing
        Next

        If Not ManualMode AndAlso Val(txtPage.Text) > m_intTotalPage Then
            txtPage.Text = CStr(m_intTotalPage)      ' TextChanged repaints
        Else
            PrintPreviewPhotoProcedure()
        End If
    End Sub

    Private Sub txtPage_TextChanged(sender As Object, e As EventArgs) Handles txtPage.TextChanged
        If Not m_bolLoad OrElse Not txtPage.Visible Then Return
        If Val(txtPage.Text) <= 0 Then txtPage.Text = "1" : Return
        If Val(txtPage.Text) > m_intTotalPage Then txtPage.Text = CStr(m_intTotalPage) : Return
        If udPage.Value <> CInt(Val(txtPage.Text)) Then udPage.Value = CInt(Val(txtPage.Text))
        PrintPreviewPhotoProcedure()
    End Sub

    Private Sub udPage_ValueChanged(sender As Object, e As EventArgs) Handles udPage.ValueChanged
        txtPage.Text = CStr(udPage.Value)
    End Sub

    Private Sub txtPrintCount_TextChanged(sender As Object, e As EventArgs) Handles txtPrintCount.TextChanged
        If Val(txtPrintCount.Text) <= 0 Then txtPrintCount.Text = "1"
    End Sub

    Private Sub UpDown1_ValueChanged(sender As Object, e As EventArgs) Handles UpDown1.ValueChanged
        txtPrintCount.Text = CStr(Math.Max(1, UpDown1.Value))
    End Sub

    '==================================================================================================
    ' Drawing (mm)
    '==================================================================================================
    ''' <summary>The (cell, file) pairs of a page: page N of the automatic layout, or the manual cells.</summary>
    Private Function PageCells(ByVal page As Integer) As List(Of KeyValuePair(Of Integer, String))
        Dim list As New List(Of KeyValuePair(Of Integer, String))
        If ManualMode Then
            For I As Integer = 0 To Math.Min(m_objMatrixPrint.Count, m_lpAssignPrint.Length) - 1
                If Not String.IsNullOrWhiteSpace(m_lpAssignPrint(I)) Then list.Add(New KeyValuePair(Of Integer, String)(I, m_lpAssignPrint(I)))
            Next
        Else
            For Each p As PictureSlot In m_lpPicture
                If p.Page = page Then list.Add(New KeyValuePair(Of Integer, String)(p.Index, p.FileName))
            Next
        End If
        Return list
    End Function

    ''' <summary>The picture, turned to the chosen style: 4x3 turns portraits, 3x4 turns landscapes
    ''' (VB6 Rotate(-90)). With a size it is a thumbnail of about that many pixels (the preview).</summary>
    Private Function LoadFixedSizePicture(ByVal strFileName As String, ByVal w As Integer, ByVal h As Integer) As Bitmap
        Dim info As New Quartz.ImageInfo With {.FileName = strFileName}
        Dim pic As Bitmap
        If w <= 0 OrElse h <= 0 Then
            Try
                pic = Quartz.LoadPicture(strFileName)
            Catch ex As Exception
                pic = New Quartz.Thumbnail With {.FileName = strFileName}.GetThumbnail(Math.Max(1, info.Width), Math.Max(1, info.Height))
            End Try
        Else
            Dim thumb As New Quartz.Thumbnail With {.FileName = strFileName}
            pic = If(info.Height > info.Width, thumb.GetThumbnail(h, w), thumb.GetThumbnail(w, h))
        End If
        If pic Is Nothing Then Return Nothing
        Dim portrait As Boolean = pic.Height > pic.Width
        Select Case If(cboType.SelectedItem?.Name, "H")
            Case "H" : If portrait Then pic.RotateFlip(RotateFlipType.Rotate270FlipNone)       ' 3x4 照片轉 4x3
            Case "V" : If Not portrait AndAlso pic.Width <> pic.Height Then pic.RotateFlip(RotateFlipType.Rotate270FlipNone) ' 4x3 照片轉 3x4
        End Select
        Return pic
    End Function

    ''' <summary>One page, in mm, on any Graphics (preview bitmap, printer, JPEG page).</summary>
    Private Sub DrawPage(ByVal g As Graphics, ByVal page As Integer, ByVal preview As Boolean, Optional ByVal scale As Double = 1)
        Dim pr As New Carbon.Printer(g) With {.SetScale = scale}
        Dim cellW As Integer = m_objMatrixPrint.ItemWidth, cellH As Integer = m_objMatrixPrint.ItemHeight
        For Each cell In PageCells(page)
            Dim x As Single = m_objMatrixPrint.ItemLeft(cell.Key), y As Single = m_objMatrixPrint.ItemTop(cell.Key)
            Using pic As Bitmap = If(preview,
                                     LoadFixedSizePicture(cell.Value, CInt(cellW * m_dblScale), CInt(cellH * m_dblScale)),
                                     LoadFixedSizePicture(cell.Value, 0, 0))
                If pic Is Nothing Then Continue For
                ' fit into the cell, centred; the print never enlarges past the picture's own size
                Dim picW As Double = pic.Width * 25.4 / 96, picH As Double = pic.Height * 25.4 / 96
                Dim ratio As Double = Math.Min(cellW / picW, cellH / picH)
                If Not preview Then ratio = Math.Min(ratio, 1)
                Dim w As Single = CSng(picW * ratio), h As Single = CSng(picH * ratio)
                Dim rx As Single = x + (cellW - w) / 2, ry As Single = y + (cellH - h) / 2
                pr.PaintPicture(pic, rx, ry, w, h, False)
                PrintExifDateTimeProcedure(pr, rx, ry, w, h, cell.Value)
            End Using
        Next
        If chkPrintOuputDateTime.Checked AndAlso m_objMatrixPrint.Count > 0 Then
            Dim fx As Single = m_objMatrixPrint.ItemLeft(0)
            Dim fy As Single = m_objMatrixPrint.ItemTop(m_objMatrixPrint.Count - 1) + cellH + 1
            pr.PrintText(fx, fy, 0, "列印時間：" & Now.ToString("yyyy/M/d tt hh:mm:ss"), Color.Black, Carbon.PrintAlignment.alLeft, picPaper.Font.Name, 14)
        End If
    End Sub

    ''' <summary>The date the photo was taken as "yyyy/MM/dd": the camera's EXIF date (of the original in
    ''' Restore\ when the photo was edited), replaced by the date typed into its .Exif file; "" before 2001.</summary>
    Public Function GetPrintedExifDateTime(ByVal strFileDesc As String) As String
        Dim strFileName As String = g_lpFileSystem.AnalyseFile(Carbon.AnalyseFileConstants.fsFileName, strFileDesc)
        Dim strFolder As String = g_lpFileSystem.AnalyseFile(Carbon.AnalyseFileConstants.fsParentFolderName, strFileDesc)
        Dim strRestoreFileDesc As String = strFolder & "\Restore\" & strFileName
        Dim strDateTime As String = GetExifFileDateTime(g_lpFileSystem, If(g_lpFileSystem.FileExists(strRestoreFileDesc), strRestoreFileDesc, strFileDesc))
        Dim strDate As String = If(strDateTime.Length >= 8, strDateTime.Substring(0, 8), strDateTime)
        If Val(strDate) < 2001 Then Return ""     ' 2001 年前不生效
        Dim result As String = FormatDate(strDate)

        ' 取得自行定義的 Exif 檔日期
        Dim strExifFileDesc As String = strFolder & "\" & g_lpFileSystem.AnalyseFile(Carbon.AnalyseFileConstants.fsBaseName, strFileDesc) & "." & gc_strExifPattern
        If Not g_lpFileSystem.FileExists(strExifFileDesc) Then Return result
        Dim ini As New Carbon.IniFile With {.FileName = strExifFileDesc}
        Return FormatDate(GetClearText(ini.SimpleGetValue("Exif", "Date")))
    End Function

    Private Shared Function FormatDate(ByVal digits As String) As String
        Dim n As Long
        If Long.TryParse(If(digits, "").Trim(), n) Then Return n.ToString("0000/00/00")
        Return If(digits, "").Trim()
    End Function

    ''' <summary>The EXIF date in orange, bottom-right inside the picture.</summary>
    Private Sub PrintExifDateTimeProcedure(ByVal pr As Carbon.Printer, ByVal left As Single, ByVal top As Single, ByVal width As Single, ByVal height As Single, ByVal strFileDesc As String)
        If Not chkPrintExifDate.Checked Then Return
        Dim strDate As String = GetPrintedExifDateTime(strFileDesc)
        If strDate.Trim() = "" Then Return
        strDate = strDate.Replace("/", ".") & " "     ' 加上保留邊寬
        Const strFontName As String = "GungsuhChe"
        ' VB6: 36 pt on a picture 1024 px wide
        Dim fontSize As Single = CSng(Math.Max(8, 36.0 / 1024 * (width * 96 / 25.4)))
        Dim tw As Single = pr.TextWidth(strDate, strFontName, fontSize)
        Dim th As Single = pr.TextHeight("1", strFontName, fontSize)
        pr.PrintText(left + width - tw, top + height - th, tw, strDate, Color.FromArgb(255, 128, 0), Carbon.PrintAlignment.alLeft, strFontName, fontSize)
    End Sub

    '==================================================================================================
    ' Preview
    '==================================================================================================
    Private Sub SetPreview(ByVal bmp As Bitmap)
        Dim old As Image = picPaper.BackgroundImage
        picPaper.BackgroundImage = bmp
        old?.Dispose()
    End Sub

    Private Sub PrintPreviewPhotoProcedure()
        If picPaper.Width <= 0 OrElse picPaper.Height <= 0 Then Return
        Dim bmp As New Bitmap(picPaper.Width, picPaper.Height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            ' 1 mm = m_dblScale pixels
            DrawPage(g, If(ManualMode, 0, CInt(Val(txtPage.Text)) - 1), True, m_dblScale / (g.DpiX / 25.4))
            g.PageUnit = GraphicsUnit.Pixel
            g.ResetTransform()
            If ManualMode Then
                ' the empty cells, so there is somewhere to drop
                Using p As New Pen(Color.Silver) With {.DashStyle = Drawing2D.DashStyle.Dot}
                    For I As Integer = 0 To m_objMatrixPrint.Count - 1
                        g.DrawRectangle(p, CellRect(I))
                    Next
                End Using
            End If
            Using p As New Pen(Color.Gray)
                g.DrawRectangle(p, 0, 0, bmp.Width - 1, bmp.Height - 1)
            End Using
        End Using
        SetPreview(bmp)
    End Sub

    Private Function CellRect(ByVal index As Integer) As Rectangle
        Return New Rectangle(CInt(m_objMatrixPrint.ItemLeft(index) * m_dblScale), CInt(m_objMatrixPrint.ItemTop(index) * m_dblScale),
                             CInt(m_objMatrixPrint.ItemWidth * m_dblScale), CInt(m_objMatrixPrint.ItemHeight * m_dblScale))
    End Function

    Private Function CellAt(ByVal pt As Point) As Integer
        For I As Integer = 0 To m_objMatrixPrint.Count - 1
            If CellRect(I).Contains(pt) Then Return I
        Next
        Return -1
    End Function

    ' 手動: drop a photo from MediaList1 on a cell
    Private Sub picPaper_DragEnter(sender As Object, e As DragEventArgs) Handles picPaper.DragEnter, picPaper.DragOver
        e.Effect = If(ManualMode AndAlso e.Data.GetDataPresent(DataFormats.FileDrop) AndAlso
                      CellAt(picPaper.PointToClient(New Point(e.X, e.Y))) >= 0, DragDropEffects.Copy, DragDropEffects.None)
    End Sub

    Private Sub picPaper_DragDrop(sender As Object, e As DragEventArgs) Handles picPaper.DragDrop
        If Not ManualMode Then Return
        Dim files As String() = TryCast(e.Data.GetData(DataFormats.FileDrop), String())
        Dim index As Integer = CellAt(picPaper.PointToClient(New Point(e.X, e.Y)))
        If files Is Nothing OrElse files.Length = 0 OrElse index < 0 Then Return
        m_lpAssignPrint(index) = files(0)
        PrintPreviewPhotoProcedure()
    End Sub

    Private Sub picPaper_MouseDown(sender As Object, e As MouseEventArgs) Handles picPaper.MouseDown
        If Not ManualMode OrElse e.Button <> MouseButtons.Right Then Return
        Dim index As Integer = CellAt(e.Location)
        If index < 0 OrElse m_lpAssignPrint(index) Is Nothing Then Return
        m_lpAssignPrint(index) = Nothing
        PrintPreviewPhotoProcedure()
    End Sub

    '==================================================================================================
    ' Print / save
    '==================================================================================================
    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butPrint_Click(sender As Object, e As EventArgs) Handles butPrint.Click
        If m_printer Is Nothing OrElse m_iPaperWidth + m_iPaperHeight = 0 Then
            frmMsgBox.ShowCriticalMessage("設定紙張大小失敗  請重新選取印表機", "")
            Return
        End If
        Dim pages As List(Of Integer) = If(ManualMode, New List(Of Integer) From {0}, LibPrint.ChoosePages(CInt(Val(txtPage.Text)), m_intTotalPage))
        If pages Is Nothing Then Return

        Dim result As String
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            result = LibPrint.PrintPages(m_printer, CInt(Val(txtPrintCount.Text)), pages, Sub(g, page) DrawPage(g, page, False))
        Finally
            Cursor = Cursors.Default
            Enabled = True
        End Try
        If result <> "" Then
            frmMsgBox.ShowCriticalMessage("列印失敗：" & result, "")
        Else
            frmMsgBox.ShowExclamationMessage("列印完成", "")
        End If
    End Sub

    Private Sub butSave_Click(sender As Object, e As EventArgs) Handles butSave.Click
        Dim strPath As String = frmBrowserFolder.GetFolder("另存列印照片")
        If strPath.Trim() = "" Then Return
        If Not g_lpFileSystem.FolderExists(strPath) Then
            frmMsgBox.ShowCriticalMessage("目錄不存在", "")
            Return
        End If
        Enabled = False
        Cursor = Cursors.WaitCursor
        Try
            LibPrint.SavePagesJpeg(strPath, "iPhoto-列印", m_objMatrixPrint.Width, m_objMatrixPrint.Height,
                                   If(ManualMode, 1, m_intTotalPage), SaveDpi, Sub(g, page) DrawPage(g, page, False))
        Finally
            Cursor = Cursors.Default
            Enabled = True
        End Try
        frmMsgBox.ShowExclamationMessage("存檔完成", "")
    End Sub

End Class
