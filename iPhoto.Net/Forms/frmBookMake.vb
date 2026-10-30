' 建立攝影集 (the tool bar's 2 and the photo list's 加入攝影集; replaces the VB6 port's frmBuildBook):
' puts photos into a photo book (.Alm, Book) --
'   新的攝影集   where (one of the 攝影集 folders, the last one used to begin with), its name, title, date,
'                place and remark (BookSuggest's to begin with) and its cover (◀ ▶ through the pictures)
'   已有的攝影集 one of the books, the recent ones first (photos already in it are not added twice)
' The books used are remembered (RecentBooks, most recent first) for the right-click menu.
' Made in code (no designer file).
Friend Class frmBookMake
    Inherits Form

    Private ReadOnly lblInfo As New Label
    Private ReadOnly optNew As New RadioButton, optAdd As New RadioButton
    Private ReadOnly pnlNew As New Panel, pnlAdd As New Panel
    Private ReadOnly cboFolder As New ComboBox
    Private ReadOnly txtName As New TextBox, txtTitle As New TextBox, txtDate As New TextBox, txtSpot As New TextBox, txtRemark As New TextBox
    Private ReadOnly picCover As New PictureBox
    Private ReadOnly butPrev As New Button, butNext As New Button
    Private ReadOnly lblCover As New Label
    Private ReadOnly lstBooks As New ListBox
    Private ReadOnly butOk As New Aqua.ThinButton, butCancel As New Aqua.ThinButton

    Private ReadOnly m_lpFiles As List(Of String)
    Private ReadOnly m_lpPictures As List(Of String)
    Private ReadOnly m_lpFolders As New List(Of String)
    Private ReadOnly m_lpBooks As New List(Of String)
    Private m_intCover As Integer
    Private m_bolTitleTyped As Boolean

    ''' <summary>The book the photos went into (after OK).</summary>
    Public Property BookFile As String
    ''' <summary>True when the book was made now.</summary>
    Public Property Created As Boolean
    ''' <summary>How many photos went in (the ones already in the book are not counted).</summary>
    Public Property AddedCount As Integer

    ''' <param name="addTo">A book to offer first (加入已有的攝影集 chosen).</param>
    Public Sub New(ByVal files As IEnumerable(Of String), Optional ByVal addTo As String = Nothing)
        m_lpFiles = files.Where(Function(f) Not String.IsNullOrEmpty(f) AndAlso IO.File.Exists(f)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        m_lpPictures = m_lpFiles.Where(Function(f) GetMediaType(g_lpFileSystem, f) = enumPhotoMediaType.mdImage).ToList()
        Text = "建立攝影集"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False : MaximizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(660, 470)
        BackColor = Color.White

        Dim bytes As Long = m_lpFiles.Sum(Function(f) New IO.FileInfo(f).Length)
        Dim size As Double, unit As String = ""
        CalcFileLength(bytes, size, unit)
        lblInfo.SetBounds(16, 14, 628, 24)
        lblInfo.Text = $"共 {m_lpFiles.Count} 張照片，大小共 {size} {unit}"
        optNew.SetBounds(16, 44, 180, 26) : optNew.Text = "建立新的攝影集"
        optAdd.SetBounds(210, 44, 200, 26) : optAdd.Text = "加入已有的攝影集"
        pnlNew.SetBounds(16, 80, 628, 330)
        pnlAdd.SetBounds(16, 80, 628, 330)

        ' ---- 新的攝影集
        Dim lbl = Function(t As String, y As Integer) New Label With {.Text = t, .AutoSize = True, .Location = New Point(0, y + 4)}
        cboFolder.DropDownStyle = ComboBoxStyle.DropDownList
        cboFolder.SetBounds(80, 0, 320, 28)
        txtName.SetBounds(80, 38, 320, 28)
        txtTitle.SetBounds(80, 76, 320, 28)
        txtDate.SetBounds(80, 114, 320, 28)
        txtSpot.SetBounds(80, 152, 320, 28)
        txtRemark.SetBounds(80, 190, 320, 120) : txtRemark.Multiline = True : txtRemark.ScrollBars = ScrollBars.Vertical
        picCover.SetBounds(420, 0, 208, 156) : picCover.SizeMode = PictureBoxSizeMode.Zoom : picCover.BackColor = Color.FromArgb(235, 237, 240)
        butPrev.SetBounds(420, 162, 40, 28) : butPrev.Text = "◀"
        butNext.SetBounds(588, 162, 40, 28) : butNext.Text = "▶"
        lblCover.SetBounds(462, 166, 124, 24) : lblCover.TextAlign = ContentAlignment.TopCenter : lblCover.ForeColor = Color.DimGray
        pnlNew.Controls.AddRange({lbl("存放位置", 0), cboFolder, lbl("名稱", 38), txtName, lbl("標題", 76), txtTitle, lbl("日期", 114), txtDate,
                                  lbl("地點", 152), txtSpot, lbl("備註", 190), txtRemark,
                                  New Label With {.Text = "封面", .AutoSize = True, .Location = New Point(420, 196), .ForeColor = Color.DimGray},
                                  picCover, butPrev, lblCover, butNext})

        ' ---- 已有的攝影集
        lstBooks.SetBounds(0, 0, 628, 326)
        lstBooks.IntegralHeight = False
        pnlAdd.Controls.Add(lstBooks)

        butOk.SetBounds(410, 424, 116, 32)
        butCancel.SetBounds(536, 424, 108, 32) : butCancel.Text = "取消"
        For Each b In {butOk, butCancel}
            b.Font = New Font("華康細圓體", 12.0F)
            b.ForeColor = Color.Black
        Next
        Controls.AddRange({lblInfo, optNew, optAdd, pnlNew, pnlAdd, butOk, butCancel})

        FillFolders()
        FillBooks(addTo)
        Dim s As BookSuggest.Suggestion = BookSuggest.Suggest(m_lpFiles)
        txtName.Text = s.Name
        txtTitle.Text = s.Name
        txtDate.Text = s.DateText
        txtSpot.Text = s.Place
        m_intCover = Math.Max(0, m_lpPictures.IndexOf(s.Cover))
        ShowCover()

        AddHandler optNew.CheckedChanged, Sub() ShowMode()
        AddHandler txtName.TextChanged, Sub() If Not m_bolTitleTyped Then txtTitle.Text = txtName.Text
        AddHandler txtTitle.KeyPress, Sub() m_bolTitleTyped = True
        AddHandler butPrev.Click, Sub() StepCover(-1)
        AddHandler butNext.Click, Sub() StepCover(1)
        AddHandler lstBooks.DoubleClick, Sub() DoOk()
        AddHandler butOk.Click, Sub() DoOk()
        AddHandler butCancel.Click, Sub() Close()

        If addTo IsNot Nothing AndAlso lstBooks.SelectedIndex >= 0 Then optAdd.Checked = True Else optNew.Checked = True
        optAdd.Enabled = m_lpBooks.Count > 0
        ShowMode()
    End Sub

    Private Sub ShowMode()
        pnlNew.Visible = optNew.Checked
        pnlAdd.Visible = Not optNew.Checked
        butOk.Text = If(optNew.Checked, "建立", "加入")
        If optNew.Checked Then txtName.Focus() Else lstBooks.Focus()
    End Sub

    '==================================================================================================
    ' Folders, books, cover
    '==================================================================================================
    ''' <summary>The 攝影集 folders (sections); the last one used first. None at all: a new one,
    ''' 我的攝影集, in the first 攝影集 path.</summary>
    Private Sub FillFolders()
        For i = 0 To g_lpStorage.FavoriteCount - 1
            m_lpFolders.Add(g_lpStorage.Favorite(i).Path)
            cboFolder.Items.Add(g_lpStorage.Favorite(i).Name)
        Next
        If m_lpFolders.Count = 0 AndAlso g_lpConfig.FavoriteCount > 0 Then
            m_lpFolders.Add(IO.Path.Combine(g_lpConfig.FavoritePath(0), "我的攝影集"))
            cboFolder.Items.Add("我的攝影集（新增）")
        End If
        Dim last As String = GetSetting("iPhoto", "BuildBook", "Folder", "")
        Dim k As Integer = m_lpFolders.FindIndex(Function(f) String.Equals(f.TrimEnd("\"c), last.TrimEnd("\"c), StringComparison.OrdinalIgnoreCase))
        If cboFolder.Items.Count > 0 Then cboFolder.SelectedIndex = Math.Max(0, k)
    End Sub

    ''' <summary>Every book: the recent ones first, then by folder and name.</summary>
    Private Sub FillBooks(ByVal addTo As String)
        Dim all As New List(Of (File As String, Text As String))
        For i = 0 To g_lpStorage.FavoriteCount - 1
            Dim section As Albums = g_lpStorage.Favorite(i)
            section.Load()
            For j = 0 To section.ItemCount - 1
                Dim b As Book = TryCast(section.Item(j), Book)
                If b IsNot Nothing Then all.Add((b.FileName, section.Name & " › " & b.DisplayName))
            Next
        Next
        Dim recent As List(Of String) = RecentBooks()
        Dim ordered = all.OrderBy(Function(b)
                                      Dim r As Integer = recent.FindIndex(Function(x) String.Equals(x, b.File, StringComparison.OrdinalIgnoreCase))
                                      Return If(r < 0, Integer.MaxValue, r)
                                  End Function).ThenBy(Function(b) b.Text, StringComparer.CurrentCulture).ToList()
        For Each b In ordered
            m_lpBooks.Add(b.File)
            lstBooks.Items.Add(If(recent.Contains(b.File, StringComparer.OrdinalIgnoreCase), "★ ", "　 ") & b.Text)
        Next
        Dim k As Integer = If(addTo Is Nothing, -1, m_lpBooks.FindIndex(Function(f) String.Equals(f, addTo, StringComparison.OrdinalIgnoreCase)))
        If lstBooks.Items.Count > 0 Then lstBooks.SelectedIndex = Math.Max(0, k)
    End Sub

    Private Sub StepCover(ByVal delta As Integer)
        If m_lpPictures.Count = 0 Then Return
        m_intCover = (m_intCover + delta + m_lpPictures.Count) Mod m_lpPictures.Count
        ShowCover()
    End Sub

    Private Sub ShowCover()
        Dim old As Image = picCover.Image
        picCover.Image = Nothing
        old?.Dispose()
        butPrev.Enabled = m_lpPictures.Count > 1
        butNext.Enabled = m_lpPictures.Count > 1
        If m_lpPictures.Count = 0 Then
            lblCover.Text = "沒有照片可當封面"
            Return
        End If
        lblCover.Text = $"{m_intCover + 1} / {m_lpPictures.Count}"
        Try
            Using fs As New IO.FileStream(m_lpPictures(m_intCover), IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite), img As Image = Image.FromStream(fs, False, False)
                Dim k As Double = Math.Min(picCover.Width / img.Width, picCover.Height / img.Height)
                Dim bmp As New Bitmap(Math.Max(1, CInt(img.Width * k)), Math.Max(1, CInt(img.Height * k)))
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    g.DrawImage(img, 0, 0, bmp.Width, bmp.Height)
                End Using
                picCover.Image = bmp
            End Using
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
    End Sub

    '==================================================================================================
    ' Making it
    '==================================================================================================
    Private Sub DoOk()
        If m_lpFiles.Count = 0 Then
            frmMsgBox.ShowCriticalMessage("沒有可以加入的照片（檔案不在了）。", "建立攝影集")
            Return
        End If
        If optNew.Checked Then MakeNew() Else AddToExisting()
    End Sub

    Private Sub MakeNew()
        If cboFolder.SelectedIndex < 0 Then
            frmMsgBox.ShowCriticalMessage("還沒有攝影集的存放位置（設定 › 相片庫 › 攝影集）。", "建立攝影集")
            Return
        End If
        Dim name As String = txtName.Text.Trim()
        If name = "" OrElse Not CheckNameRule(name) OrElse BookSuggest.CleanName(name) <> name Then
            frmMsgBox.ShowCriticalMessage("名稱不能空白，也不能有 \ / : * ? "" < > | . 這些字元。", "建立攝影集")
            txtName.Focus()
            Return
        End If
        Dim folder As String = m_lpFolders(cboFolder.SelectedIndex)
        Dim file As String = IO.Path.Combine(folder, name & "." & gc_strFavoritesPattern)
        If IO.File.Exists(file) Then
            If Not frmQueryMsgBox.ShowMessage($"已經有「{name}」這本攝影集，要把照片加進去嗎？", "建立攝影集") Then Return
            Finish(file, AddPhotos(file), created:=False)
            Return
        End If
        Try
            IO.Directory.CreateDirectory(folder)
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException
            frmMsgBox.ShowCriticalMessage("無法建立資料夾" & vbCrLf & folder, "建立攝影集")
            Return
        End Try
        Dim book As New Book
        book.Construct(file)
        For Each f In m_lpFiles
            book.AddPhoto(f)
        Next
        If Not book.Save() Then Return
        book.Create(enumCreate.crDate) = Now.ToString("yyyyMMdd")
        book.Create(enumCreate.crTime) = Now.ToString("HHmmss")
        book.Note(enumNote.ntTitle) = If(txtTitle.Text.Trim() = "", name, txtTitle.Text.Trim())
        book.Note(enumNote.ntDate) = txtDate.Text.Trim()
        book.Note(enumNote.ntSopt) = txtSpot.Text.Trim()
        book.Note(enumNote.ntRemark) = txtRemark.Text.Trim()
        If m_lpPictures.Count > 0 Then book.CoverPhoto = m_lpPictures(m_intCover)
        SaveSetting("iPhoto", "BuildBook", "Folder", folder)
        Finish(file, book.PhotoCount, created:=True)
    End Sub

    Private Sub AddToExisting()
        If lstBooks.SelectedIndex < 0 Then Return
        Dim file As String = m_lpBooks(lstBooks.SelectedIndex)
        Finish(file, AddPhotos(file), created:=False)
    End Sub

    ''' <summary>The photos into the book (the ones in it already stay once); how many went in.</summary>
    Private Function AddPhotos(ByVal file As String) As Integer
        Return AddToBook(file, m_lpFiles)
    End Function

    Private Sub Finish(ByVal file As String, ByVal added As Integer, ByVal created As Boolean)
        BookFile = file
        Me.Created = created
        AddedCount = added
        AddRecent(file)
        DialogResult = DialogResult.OK
        Close()
    End Sub

    '==================================================================================================
    ' Shared: adding without the window, the recent books
    '==================================================================================================
    ''' <summary><paramref name="files"/> into the book <paramref name="file"/> (the ones in it already stay
    ''' once, in its order); how many went in. Remembered as a recent book.</summary>
    Friend Shared Function AddToBook(ByVal file As String, ByVal files As IEnumerable(Of String)) As Integer
        Dim book As New Book
        book.Construct(file)
        Dim before As Integer = book.PhotoCount
        For Each f In files
            book.AddPhoto(f)
        Next
        Dim added As Integer = book.PhotoCount - before
        If added > 0 Then book.Save()
        AddRecent(file)
        Return added
    End Function

    ''' <summary>The books photos went into lately, most recent first (only ones still there).</summary>
    Friend Shared Function RecentBooks() As List(Of String)
        Return GetSetting("iPhoto", "BuildBook", "Recent", "").Split({"|"c}, StringSplitOptions.RemoveEmptyEntries).
               Where(Function(f) IO.File.Exists(f)).ToList()
    End Function

    Private Shared Sub AddRecent(ByVal file As String)
        Dim list As List(Of String) = RecentBooks()
        list.RemoveAll(Function(f) String.Equals(f, file, StringComparison.OrdinalIgnoreCase))
        list.Insert(0, file)
        SaveSetting("iPhoto", "BuildBook", "Recent", String.Join("|", list.Take(8)))
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        picCover.Image?.Dispose()
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
