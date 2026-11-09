' 攝影集 in the main window (new in the .NET port, with frmBookMake):
'   建立攝影集   the tool bar's 2: the Dock's photos, or -- the Dock empty -- the ones selected in the list;
'                when done the book opens (攝影集 mode), and photos from the Dock may leave it
'   右鍵         加入攝影集 › 新的攝影集… / 其他攝影集… / the recent books (straight in, no window);
'                整份清單存成攝影集… (a search result, 那年今日, a year ...)
'   in a book    攝影集 › 移到最前 / 往前移 / 往後移 / 移到最後 and dragging a photo onto another (the book
'                keeps its own order then, Book.MovePhotos); 依拍攝時間排序 goes back; 做成蒙太奇拼貼,
'                匯出整本…, 播放幻燈片
Partial Class frmMain

    Private Const RecentBookItems As Integer = 5
    Private ReadOnly m_lpRecentBookFiles As New List(Of String)

    '==================================================================================================
    ' Making / adding
    '==================================================================================================
    ''' <summary>The tool bar's 建立攝影集.</summary>
    Private Sub BuildBookFromToolBar()
        If g_lpDock.Count > 0 Then
            Dim files As New List(Of String)
            For i = 0 To g_lpDock.Count - 1
                files.Add(g_lpDock.FileName(i))
            Next
            BuildBook(files, fromDock:=True)
        ElseIf mlList.Count > 0 AndAlso mlList.SelectedIndex >= 0 AndAlso Not ListShowsCards Then
            BuildBook(ListFiles(Targets(mlList.SelectedIndex)), fromDock:=False)
        Else
            frmMsgBox.ShowExclamationMessage("先在清單裡選照片（可以多選），或把照片加入 Dock。", "建立攝影集")
        End If
    End Sub

    Private Function ListFiles(ByVal indexes As IEnumerable(Of Integer)) As List(Of String)
        Return indexes.Select(Function(i) mlList.Item(i).FileName).Where(Function(f) f <> "").ToList()
    End Function

    Private Sub BuildBook(ByVal files As List(Of String), ByVal fromDock As Boolean, Optional ByVal addTo As String = Nothing)
        If g_lpConfig.ReadOnly Then Return
        Dim book As String, created As Boolean, added As Integer
        Using f As New frmBookMake(files, addTo)
            If f.ShowDialog(Me) <> DialogResult.OK Then Return
            book = f.BookFile : created = f.Created : added = f.AddedCount
        End Using
        If fromDock AndAlso frmQueryMsgBox.ShowMessage(If(created, "攝影集建好了。", $"已加入 {added} 張。") & vbCrLf & "要清空 Dock 嗎？", "建立攝影集") Then ClearDock()
        OpenBook(book)
    End Sub

    ''' <summary>Straight into a recent book (no window).</summary>
    Private Sub AddToRecentBook(ByVal book As String, ByVal files As List(Of String))
        If g_lpConfig.ReadOnly OrElse Not IO.File.Exists(book) Then Return
        Dim added As Integer = frmBookMake.AddToBook(book, files)
        Dim name As String = IO.Path.GetFileNameWithoutExtension(book)
        frmMsgBox.ShowSmileMessage(If(added = 0, $"這些照片都已經在「{name}」裡了。", $"已加入 {added} 張到「{name}」。"), "加入攝影集")
        If TypeOf CurrentSection() Is Book AndAlso String.Equals(DirectCast(CurrentSection(), Book).FileName, book, StringComparison.OrdinalIgnoreCase) Then
            RefreshCurrentList(Math.Max(0, mlList.SelectedIndex))
        End If
    End Sub

    Private Sub ClearDock()
        g_lpDock.Clear()
        g_lpDock.Save()
        For i = 0 To mlList.Count - 1
            If mlList.Item(i).Marked Then mlList.Item(i).Marked = False
        Next
        SetDockProperty()
    End Sub

    ''' <summary>攝影集 mode with <paramref name="book"/> open.</summary>
    Private Sub OpenBook(ByVal book As String)
        LibUserInterface.SaveRecentSectionValue(enumExeMode.exeFavorites, IO.Path.GetDirectoryName(book))
        LibUserInterface.SaveRecentKeyValue(enumExeMode.exeFavorites, book)
        g_lpStorage.Refresh()
        If butMode.SelectedIndex <> 1 Then
            butMode.SelectedIndex = 1   ' butMode_SelectedChanged lists the books
        Else
            butMode_SelectedChanged(butMode, EventArgs.Empty)
        End If
        If LoadTreeViewRecentKey(enumExeMode.exeFavorites, g_lpStorage, tvList) Then tvList_Click()
    End Sub

    '==================================================================================================
    ' The photo list's right-click menu (frmMain.FileOps.vb calls these)
    '==================================================================================================
    ''' <summary>Called by FileOps_Load: the items go after 移出 Dock.</summary>
    Private Sub BooksMenuLoad()
        With AquaMenu1.Items
            Dim at As Integer = 0
            For i = 0 To .Count - 1
                If String.Equals(.Item(i).Name, "mnuDockRemove", StringComparison.OrdinalIgnoreCase) Then at = i + 1
            Next
            Dim items As New List(Of Aqua.MenuItem) From {
                New Aqua.MenuItem("mnuBookAdd", "加入攝影集", 1, Nothing, Nothing),
                New Aqua.MenuItem("mnuBookNew", "新的攝影集…", 2, Nothing, Nothing),
                New Aqua.MenuItem("mnuBookOther", "其他攝影集…", 2, Nothing, Nothing),
                New Aqua.MenuItem("mnuBookRecentSep", "-", 2, Nothing, Nothing)}
            For k = 0 To RecentBookItems - 1
                items.Add(New Aqua.MenuItem("mnuBookRecent" & k, "", 2, Nothing, Nothing))
            Next
            items.Add(New Aqua.MenuItem("mnuBookFromList", "整份清單存成攝影集…", 1, Nothing, Nothing))
            items.Add(New Aqua.MenuItem("mnuBookOps", "這本攝影集", 1, Nothing, Nothing))
            For Each it In {("mnuBookFirst", "移到最前"), ("mnuBookUp", "往前移"), ("mnuBookDown", "往後移"), ("mnuBookLast", "移到最後"),
                            ("mnuBookSep1", "-"), ("mnuBookByDate", "依拍攝時間排序"), ("mnuBookSep2", "-"),
                            ("mnuBookMontage", "做成蒙太奇拼貼"), ("mnuBookExport", "匯出整本…"), ("mnuBookSlide", "播放幻燈片")}
                items.Add(New Aqua.MenuItem(it.Item1, it.Item2, 2, Nothing, Nothing))
            Next
            For k = 0 To items.Count - 1
                .Insert(at + k, items(k))
            Next
        End With
    End Sub

    ''' <summary>Called by SetFileOpsMenu.</summary>
    Private Sub SetBooksMenu(ByVal n As Integer, ByVal ro As Boolean)
        Dim book As Book = TryCast(CurrentSection(), Book)
        Dim inBook As Boolean = book IsNot Nothing AndAlso m_lpAppEnv.ExeMode = enumExeMode.exeFavorites
        Dim many As String = If(n > 1, "（" & n & " 張）", "")
        m_lpRecentBookFiles.Clear()
        m_lpRecentBookFiles.AddRange(frmBookMake.RecentBooks().
                                     Where(Function(f) Not inBook OrElse Not String.Equals(f, book.FileName, StringComparison.OrdinalIgnoreCase)).Take(RecentBookItems))
        ' FindMenu: most of these are second-level items (Item(name) only finds first-level ones)
        With AquaMenu1
            .FindMenu("mnuBookAdd").Text = "加入攝影集" & many
            .FindMenu("mnuBookAdd").Enabled = Not ro AndAlso n > 0
            .FindMenu("mnuBookRecentSep").Visible = m_lpRecentBookFiles.Count > 0
            For k = 0 To RecentBookItems - 1
                Dim it As Aqua.MenuItem = .FindMenu("mnuBookRecent" & k)
                it.Visible = k < m_lpRecentBookFiles.Count
                If it.Visible Then it.Text = IO.Path.GetFileNameWithoutExtension(m_lpRecentBookFiles(k))
            Next
            .FindMenu("mnuBookFromList").Visible = Not inBook
            .FindMenu("mnuBookFromList").Text = "整份清單存成攝影集（" & mlList.Count & " 張）…"
            .FindMenu("mnuBookFromList").Enabled = Not ro AndAlso mlList.Count > 0
            .FindMenu("mnuBookOps").Visible = inBook
            For Each itemName In {"mnuBookFirst", "mnuBookUp", "mnuBookDown", "mnuBookLast", "mnuBookByDate"}
                .FindMenu(itemName).Enabled = inBook AndAlso Not ro
            Next
            .FindMenu("mnuBookByDate").Enabled = inBook AndAlso Not ro AndAlso book.ManualOrder
        End With
    End Sub

    ''' <summary>Called first by FileOpsMenuSelected; True when it was one of these.</summary>
    Private Function BooksMenuSelected(ByVal name As String, ByVal index As Integer) As Boolean
        Dim upper As String = name.ToUpperInvariant()
        If upper.StartsWith("MNUBOOKRECENT") AndAlso upper <> "MNUBOOKRECENTSEP" Then
            Dim k As Integer = CInt(upper.Substring("MNUBOOKRECENT".Length))
            If k < m_lpRecentBookFiles.Count Then AddToRecentBook(m_lpRecentBookFiles(k), ListFiles(Targets(index)))
            Return True
        End If
        Select Case upper
            Case "MNUBOOKNEW" : BuildBook(ListFiles(Targets(index)), fromDock:=False)
            Case "MNUBOOKOTHER" : BuildBook(ListFiles(Targets(index)), fromDock:=False, addTo:=If(frmBookMake.RecentBooks().FirstOrDefault(), ""))
            Case "MNUBOOKFROMLIST" : BuildBook(ListFiles(Enumerable.Range(0, mlList.Count)), fromDock:=False)
            Case "MNUBOOKFIRST" : MoveInBook(Targets(index), "first")
            Case "MNUBOOKUP" : MoveInBook(Targets(index), "up")
            Case "MNUBOOKDOWN" : MoveInBook(Targets(index), "down")
            Case "MNUBOOKLAST" : MoveInBook(Targets(index), "last")
            Case "MNUBOOKBYDATE"
                Dim book As Book = TryCast(CurrentSection(), Book)
                If book Is Nothing Then Return True
                book.SortByDate()
                RefreshCurrentList(0)
            Case "MNUBOOKMONTAGE"
                Dim book As Book = TryCast(CurrentSection(), Book)
                If book IsNot Nothing Then OpenMontage(book.Files().Where(Function(f) GetMediaType(g_lpFileSystem, f) = enumPhotoMediaType.mdImage).ToList(), book.DisplayName, PhotoMontage.Core.MontageMode.Collage)
            Case "MNUBOOKEXPORT"
                Dim book As Book = TryCast(CurrentSection(), Book)
                If book IsNot Nothing Then
                    Using f As New frmExport With {.Files = book.Files()}
                        f.ShowDialog(Me)
                    End Using
                End If
            Case "MNUBOOKSLIDE" : imgToolBox_Click(imgToolBox(6), EventArgs.Empty)
            Case "MNUBOOKADD", "MNUBOOKOPS", "MNUBOOKRECENTSEP", "MNUBOOKSEP1", "MNUBOOKSEP2"
            Case Else : Return False
        End Select
        Return True
    End Function

    '==================================================================================================
    ' The book's own order
    '==================================================================================================
    ''' <summary>The selected photos (<paramref name="indexes"/>) to the front / one up / one down / to the end.</summary>
    Private Sub MoveInBook(ByVal indexes As List(Of Integer), ByVal how As String)
        Dim book As Book = TryCast(CurrentSection(), Book)
        If book Is Nothing OrElse indexes.Count = 0 OrElse g_lpConfig.ReadOnly Then Return
        Dim order As List(Of String) = book.Files()
        Dim moved As List(Of String) = ListFiles(indexes)
        Dim set_ As New HashSet(Of String)(moved, StringComparer.OrdinalIgnoreCase)
        Dim positions = order.Select(Function(f, i) (f, i)).Where(Function(x) set_.Contains(x.f)).Select(Function(x) x.i).ToList()
        If positions.Count = 0 Then Return
        Dim before As String
        Select Case how
            Case "first" : before = order.FirstOrDefault(Function(f) Not set_.Contains(f))
            Case "up"
                Dim i As Integer = positions.Min() - 1
                If i < 0 Then Return
                before = order(i)
            Case "down"
                Dim j As Integer = positions.Max() + 2
                before = If(j < order.Count, order(j), Nothing)
                If positions.Max() + 1 >= order.Count Then Return
            Case Else : before = Nothing
        End Select
        book.MovePhotos(moved, before)
        ShowBookAgain(moved)
    End Sub

    ''' <summary>The book listed again in its new order, <paramref name="selected"/> selected.</summary>
    Private Sub ShowBookAgain(ByVal selected As List(Of String))
        RefreshCurrentList(0)
        Dim first As Integer = -1
        For i = 0 To mlList.Count - 1
            If selected.Contains(mlList.Item(i).FileName, StringComparer.OrdinalIgnoreCase) Then
                If first < 0 Then first = i
            End If
        Next
        If first >= 0 Then
            mlList.SelectedIndex = first
            mlList.EnsureVisible(first)
        End If
    End Sub

    ''' <summary>Dragging a photo of the book onto another: it (and the photos selected with it) go before
    ''' that one. Only while a book is shown (SetBookDrag).</summary>
    Private Sub BookDrop(ByVal item As Aqua.MediaItem, ByVal fileName As String) Handles mlList.ItemDropMedia
        If Not mlList.DropItem OrElse g_lpConfig.ReadOnly Then Return
        Dim book As Book = TryCast(CurrentSection(), Book)
        If book Is Nothing OrElse item Is Nothing Then Return
        If String.Equals(item.FileName, fileName, StringComparison.OrdinalIgnoreCase) Then Return   ' the drag's own end, or onto itself
        Dim order As List(Of String) = book.Files()
        If Not order.Contains(fileName, StringComparer.OrdinalIgnoreCase) OrElse Not order.Contains(item.FileName, StringComparer.OrdinalIgnoreCase) Then Return
        Dim selected As List(Of String) = ListFiles(mlList.SelectedIndices)
        Dim moved As List(Of String) = If(selected.Contains(fileName, StringComparer.OrdinalIgnoreCase), selected, New List(Of String) From {fileName})
        If moved.Contains(item.FileName, StringComparer.OrdinalIgnoreCase) Then Return
        book.MovePhotos(moved, item.FileName)
        BeginInvoke(New Action(Sub() ShowBookAgain(moved)))   ' after the drag has finished
    End Sub

    ''' <summary>Photos can be dragged within the list only while it shows a book.</summary>
    Private Sub SetBookDrag(ByVal value As Boolean)
        value = value AndAlso Not g_lpConfig.ReadOnly
        If mlList.DragItem <> value Then mlList.DragItem = value
        If mlList.DropItem <> value Then mlList.DropItem = value
    End Sub

End Class
