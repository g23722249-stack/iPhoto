' Port of iPhoto\Form\frmImport.frm: picks photos from a folder, creates the new album (class) folder
' with its note, and hands the file list to frmMain through g_lpImport (frmMain copies the files).
'
' Layout, captions and defaults (subject icon icoPeople, title "新相簿") live in the designer; open the
' form with "Using f As New frmImport" so every import starts from them, as the VB6 form did after Unload.
' Only values that depend on today / the settings are filled in here.
Friend Class frmImport

    Private m_lpExpNode As TreeNode
    Private m_bolImport As Boolean

    ''' <summary>True when the album was created and the files were queued in g_lpImport.</summary>
    Public ReadOnly Property Import As Boolean
        Get
            Return m_bolImport
        End Get
    End Property

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_bolImport = False
        FileListBox1.Pattern = GetPhotoPatterns()
        LibUserInterface.AddStorageSectionPathFakeKeyToTreeView(enumExeMode.exeAlbums, g_lpConfig, tvClass)

        If tvClass.Nodes.Count > 0 Then NodeClick(tvClass.Nodes(0))

        Dim strDate As String = Date.Today.ToString("MM") & "月" & Date.Today.ToString("dd") & "日"
        txtFolder.Text = strDate
        txtDate.Text = strDate

        frmDefaultPhotoInfo.Clear()
        g_lpImport.Clear()

        If tvClass.Nodes.Count > 0 Then LibUserInterface.LoadTreeViewImportRecentSection(g_lpStorage, tvClass)
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    '輸入預設的相片資訊
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        frmDefaultPhotoInfo.ShowDefaultPhotoInfo()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        If FileListBox1.SelectedCount <= 0 Then
            frmMsgBox.ShowCriticalMessage("未選擇任何匯入的檔案", "")
            Return
        End If
        If tvClass.SelectedNode Is Nothing Then
            frmMsgBox.ShowCriticalMessage("請選取匯入的資料夾", "")
            Return
        End If
        If tvClass.SelectedNode.Parent Is Nothing Then
            frmMsgBox.ShowCriticalMessage("請選取匯入的相簿位置", "")
            Return
        End If
        If txtFolder.Text.Trim() = "" Then
            frmMsgBox.ShowCriticalMessage("請輸入相簿資料夾名稱", "")
            txtFolder.Focus()
            Return
        End If
        If Not CheckNameRule(txtFolder.Text) Then
            frmMsgBox.ShowCriticalMessage("相簿名稱含有不合法的字元", "")
            txtFolder.Focus()
            Return
        End If
        If txtTitle.Text.Trim() = "" Then
            frmMsgBox.ShowCriticalMessage("請輸入相簿名稱", "")
            txtTitle.Focus()
            Return
        End If
        If txtDate.Text.Trim() = "" Then
            frmMsgBox.ShowCriticalMessage("請輸入相簿的日期", "")
            txtDate.Focus()
            Return
        End If

        Dim strAlbumPath As String = tvClass.SelectedNode.Name
        Dim strFolder As String = strAlbumPath & "\" & txtFolder.Text.Trim()
        If g_lpFileSystem.FolderExists(strFolder) Then
            frmMsgBox.ShowCriticalMessage("相簿已存在", "")
            txtTitle.Focus()
            Return
        End If

        If Not g_lpFileSystem.CreateFolder(strFolder) Then
            frmMsgBox.ShowCriticalMessage("建立相簿失敗", "")
            txtTitle.Focus()
            Return
        End If
        CreateOlympusCAMediaFile(strFolder)
        CreateAlbumClassNote(strFolder)

        If ImportAlbumPhotos(strAlbumPath, strFolder) Then
            m_bolImport = True
            Close()
        End If
    End Sub

    Private Sub DeskTop1_SelectedChanged(sender As Object, e As EventArgs) Handles DeskTop1.SelectedChanged
        FileListBox1.Path = DeskTop1.Path
        FileListBox1.Refresh()
        If FileListBox1.Count > 0 Then
            For I As Integer = 0 To FileListBox1.Count - 1
                FileListBox1.Checked(I) = True
            Next
            FileListBox1.Selected(0) = True
            FileListBox1_Click(FileListBox1, EventArgs.Empty)
        End If
    End Sub

    ''' <summary>Shows the selected file (thumbnail, size, date, resolution); with 自動帶入相簿資訊 its
    ''' date also becomes the album folder / title / date.</summary>
    Private Sub FileListBox1_Click(sender As Object, e As EventArgs) Handles FileListBox1.SelectedChanged
        If FileListBox1.File.Trim() = "" Then Return
        Dim strFileName As String = FileListBox1.FileName
        If Not g_lpFileSystem.FileExists(strFileName) Then Return

        Dim dblLength As Double, strUnit As String = ""
        CalcFileLength(New IO.FileInfo(strFileName).Length, dblLength, strUnit)

        MediaItem1.FileName = strFileName
        lblFileName.Text = FileListBox1.File
        lblFileLength.Text = dblLength & " " & strUnit

        Dim strFileDateTime As String = GetExifFileDateTime(g_lpFileSystem, strFileName)
        Dim dt As Date
        lblFileDateTime.Text = If(Date.TryParseExact(strFileDateTime, "yyyyMMddHHmmss", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, dt), dt.ToString(), "")

        lblResolution.Text = MediaItem1.ItemWidth & " x " & MediaItem1.ItemHeight

        If chkAutoInfo.Checked AndAlso strFileDateTime.Length >= 8 Then
            Dim strInfoDate As String = Mid(strFileDateTime, 5, 2) & "月" & Mid(strFileDateTime, 7, 2) & "日"
            txtFolder.Text = strInfoDate
            txtTitle.Text = strInfoDate
            txtDate.Text = strInfoDate
        End If
    End Sub

    Private Sub imgSubject_Click(sender As Object, e As EventArgs) Handles imgSubject.Click
        imgSubject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Application.DoEvents()
        Darwin.Wait(80)
        imgSubject.BorderStyle = System.Windows.Forms.BorderStyle.None
        Application.DoEvents()

        Dim pt As Point = picSubject.PointToScreen(Point.Empty)
        Dim strIcon As String = frmSubject.ShowSubject(pt.X, pt.Y)
        If strIcon.Trim() = "" Then Return

        Dim objImageList As ImageList = g_lpConfig.ImageListSubject
        If objImageList.Images.ContainsKey(strIcon) Then imgSubject.Image = objImageList.Images(strIcon)
        imgSubject.Tag = strIcon
    End Sub

    Private Sub tvClass_GotFocus(sender As Object, e As EventArgs) Handles tvClass.GotFocus
        frmClass.BorderColor = FileListBox1.BorderFocusColor
    End Sub

    Private Sub tvClass_LostFocus(sender As Object, e As EventArgs) Handles tvClass.LostFocus
        frmClass.BorderColor = FileListBox1.BorderColor
    End Sub

    ''' <summary>Only one album root stays open at a time.</summary>
    Private Sub CollapseOtherRoot(ByVal Node As TreeNode)
        If m_lpExpNode IsNot Nothing AndAlso m_lpExpNode IsNot Node AndAlso Node.Parent Is Nothing Then
            m_lpExpNode.Collapse()
        End If
    End Sub

    Private Sub tvClass_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles tvClass.BeforeExpand
        CollapseOtherRoot(e.Node)
        m_lpExpNode = e.Node
        LibUserInterface.ExpandFakeSectionKeyPathToTreeView(enumExeMode.exeAlbums, tvClass, e.Node)
    End Sub

    Private Sub tvClass_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvClass.NodeMouseClick
        If e.Button <> MouseButtons.Left Then Return
        If (tvClass.HitTest(e.Location).Location And TreeViewHitTestLocations.PlusMinus) <> 0 Then Return
        tvClass.SelectedNode = e.Node
        NodeClick(e.Node)
    End Sub

    ''' <summary>VB6 tvClass_NodeClick: open the node and remember the album root chosen for imports.</summary>
    Private Sub NodeClick(ByVal Node As TreeNode)
        CollapseOtherRoot(Node)
        If Not Node.IsExpanded Then Node.Expand()

        '紀錄最近開啟的相本
        If Not Visible Then Return
        If tvClass.SelectedNode Is Nothing Then Return
        If tvClass.SelectedNode.Parent IsNot Nothing Then
            LibUserInterface.SaveImportRecentSectionValue(tvClass.SelectedNode.Name)
        End If
    End Sub

    Private Function ImportAlbumPhotos(ByVal strAlbumPath As String, ByVal strFolder As String) As Boolean
        g_lpImport.Clear()
        With g_lpImport
            .AlbumPath = strAlbumPath
            .ClassPath = strFolder
            .ImportPath = strFolder
            .Title = txtTitle.Text.Trim()
            .DateTime = txtDate.Text.Trim()
        End With

        For I As Integer = 0 To FileListBox1.Count - 1
            If FileListBox1.Checked(I) Then
                g_lpImport.AddItem(FileListBox1.Path.TrimEnd("\"c) & "\" & FileListBox1.Item(I))
            End If
        Next
        Return True
    End Function

    ''' <summary>The Olympus CAMEDIA album marker VB6 wrote into every new album folder.</summary>
    Private Function CreateOlympusCAMediaFile(ByVal strFolder As String) As Boolean
        Dim lpProfile As New Carbon.IniFile
        lpProfile.FileName = strFolder & "\" & gc_strOlympus_camedia_inf
        lpProfile.SimpleSetValue("AlbumInfo", "AlbumType", "2")
        Return True
    End Function

    Private Function CreateAlbumClassNote(ByVal strFolder As String) As Boolean
        Dim strFile As String = strFolder & "\" & gc_strNote
        If Not g_lpFileSystem.DeleteFile(strFile) Then Return False

        Dim lpProfile As New Carbon.IniFile
        lpProfile.FileName = strFile
        lpProfile.SimpleSetValue("Create", "Date", Date.Now.ToString("yyyyMMdd"))
        lpProfile.SimpleSetValue("Create", "Time", Date.Now.ToString("HHmmss"))

        lpProfile.SimpleSetValue("Note", "Title", txtTitle.Text.Trim())
        lpProfile.SimpleSetValue("Note", "Description", txtTitle.Text.Trim())
        lpProfile.SimpleSetValue("Note", "Date", txtDate.Text.Trim())
        lpProfile.SimpleSetValue("Note", "Spot", txtSpot.Text.Trim())
        Dim strIcon As String = If(TryCast(imgSubject.Tag, String), "").Trim()
        lpProfile.SimpleSetValue("Note", "Icon", If(strIcon = "", "icoPeople", strIcon))
        lpProfile.SimpleSetValue("Note", "Remark", txtRemark.Text.Replace(vbCrLf, vbTab))
        Return True
    End Function

    Private Sub frmImport_MenuSelected(sender As Object, item As Aqua.MenuItem) Handles MyBase.MenuSelected

    End Sub
End Class
