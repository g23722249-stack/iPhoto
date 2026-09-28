' Port of iPhoto\Form\frmBuildBook.frm: puts the docked files into a photo book (.Alm). The tree lists
' the photo-book storage paths > their sub-folders > the books in them; the book is created (or added to)
' in the chosen sub-folder under the typed name.
' Callers use "Using f As New frmBuildBook".
' Changed from VB6: clicking an existing book puts its file name in the name box (VB6 put the tree
' text, which also carries the book's title, so "add to this book" created a new file instead).
Friend Class frmBuildBook

    Private m_strFileName() As String = {}
    Private m_lpFavoriteExpNode As TreeNode
    Private m_bolTextChanged As Boolean
    Private ReadOnly m_enumMode As enumExeMode = enumExeMode.exeFavorites

    Public Sub BuildBook(ByVal FileName() As String, ByVal Count As Integer)
        m_strFileName = New String(Math.Max(0, Count) - 1) {}
        Array.Copy(FileName, m_strFileName, m_strFileName.Length)
        ShowDialog()
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LibUserInterface.AddStorageSectionPathFakeKeyToTreeView(m_enumMode, g_lpConfig, tvList)

        Dim lngFilelengthByte As Long
        For Each f As String In m_strFileName
            If IO.File.Exists(f) Then lngFilelengthByte += New IO.FileInfo(f).Length
        Next
        Dim dblFileLength As Double, strFileLengthUnit As String = ""
        CalcFileLength(lngFilelengthByte, dblFileLength, strFileLengthUnit)
        lblInfo.Text = "共選取了 " & m_strFileName.Length & " 張照片，大小共 " & dblFileLength & " " & strFileLengthUnit

        If tvList.Nodes.Count > 0 Then tvList.Nodes(0).Expand()
        txtFolder.Text = "新資料夾"
        m_bolTextChanged = False
    End Sub

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        Dim node As TreeNode = tvList.SelectedNode
        If node Is Nothing OrElse node.Parent Is Nothing Then
            frmMsgBox.ShowCriticalMessage("請選擇攝影集存放的子資料夾目錄", "")
            Return
        End If
        txtFolder.Text = txtFolder.Text.Trim()
        If txtFolder.Text = "" OrElse Not CheckNameRule(txtFolder.Text) Then
            frmMsgBox.ShowCriticalMessage("相簿名稱含有不合法的字元", "")
            txtFolder.Focus()
            Return
        End If

        ' a sub-folder, or a book inside one (then its folder)
        Dim folder As String = If(node.Parent.Parent Is Nothing, node.Name, node.Parent.Name)
        Dim strBookFile As String = folder & "\" & txtFolder.Text & "." & gc_strFavoritesPattern

        Dim bolExist As Boolean = g_lpFileSystem.FileExists(strBookFile)
        If bolExist AndAlso Not frmQueryMsgBox.ShowMessage("是否將照片匯入到已存在的攝影集中？", "") Then Return

        Dim objBook As New Book
        objBook.Construct(strBookFile)
        For Each f As String In m_strFileName
            objBook.AddPhoto(f)
        Next
        objBook.Save()
        If Not bolExist Then
            objBook.Create(enumCreate.crDate) = Now.ToString("yyyyMMdd")
            objBook.Create(enumCreate.crTime) = Now.ToString("HHmmss")
            objBook.Note(enumNote.ntTitle) = txtFolder.Text
        End If
        Close()
    End Sub

    Private Sub tvList_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvList.NodeMouseClick
        If e.Button <> MouseButtons.Left Then Return
        If (tvList.HitTest(e.Location).Location And TreeViewHitTestLocations.PlusMinus) <> 0 Then Return
        tvList.SelectedNode = e.Node
        If e.Node.Parent Is Nothing OrElse m_bolTextChanged Then Return
        If e.Node.Parent.Parent Is Nothing Then
            txtFolder.Text = e.Node.Text
        Else
            txtFolder.Text = IO.Path.GetFileNameWithoutExtension(e.Node.Name)
        End If
    End Sub

    Private Sub tvList_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles tvList.BeforeExpand
        If e.Node.Parent Is Nothing Then
            ' 按下 Favorite Node: only one storage path stays open
            If m_lpFavoriteExpNode IsNot Nothing AndAlso m_lpFavoriteExpNode IsNot e.Node Then m_lpFavoriteExpNode.Collapse()
            m_lpFavoriteExpNode = e.Node
            LibUserInterface.ExpandFakeSectionKeyPathFakeChildToTreeView(m_enumMode, tvList, e.Node)
        Else
            ' 按下 Book Node
            LibUserInterface.ExpandFakePhotoToTreeView(m_enumMode, tvList, e.Node)
        End If
    End Sub

    Private Sub tvList_GotFocus(sender As Object, e As EventArgs) Handles tvList.GotFocus
        frmClass.BorderColor = txtFolder.BorderFocusColor
    End Sub

    Private Sub tvList_LostFocus(sender As Object, e As EventArgs) Handles tvList.LostFocus
        frmClass.BorderColor = txtFolder.BorderColor
    End Sub

    Private Sub txtFolder_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFolder.KeyDown
        m_bolTextChanged = True
    End Sub

End Class
