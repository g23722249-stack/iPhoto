' Picks one or more people (找合照, new in the .NET port): the names with their photo counts, ticked
' in a list that a few typed letters narrow down. Made in code (no designer file).
Friend Class frmPickPeople
    Inherits Form

    Private ReadOnly lblPrompt As New Label
    Private ReadOnly txtFilter As New TextBox
    Private ReadOnly lstPeople As New CheckedListBox
    Private ReadOnly butOk As New Aqua.FlashButton
    Private ReadOnly butCancel As New Aqua.FlashButton
    Private m_lpAll As List(Of FaceCatalog.PersonEntry)
    Private ReadOnly m_lpTicked As New HashSet(Of FaceCatalog.PersonEntry)
    Private m_bolFilling As Boolean
    Private m_bolOk As Boolean

    Public Sub New()
        Text = "找合照"
        Font = New Font("Microsoft JhengHei UI", 11.0F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False : MinimizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(380, 470)
        BackColor = Color.White

        lblPrompt.SetBounds(16, 12, 350, 24)
        txtFilter.SetBounds(16, 42, 348, 28)
        txtFilter.PlaceholderText = "輸入名字篩選"
        lstPeople.SetBounds(16, 76, 348, 330)
        lstPeople.CheckOnClick = True
        lstPeople.IntegralHeight = False
        ' wide enough for 「找合照（12）」 in 華康細圓體 13
        butOk.SetBounds(40, 418, 170, 36) : butOk.Text = "找合照"
        butCancel.SetBounds(226, 418, 114, 36) : butCancel.Text = "放棄"
        For Each b In {butOk, butCancel}
            b.Font = New Font("華康細圓體", 13.0F)
        Next
        Controls.AddRange({lblPrompt, txtFilter, lstPeople, butOk, butCancel})

        AddHandler txtFilter.TextChanged, Sub() Fill()
        AddHandler lstPeople.ItemCheck, AddressOf People_ItemCheck
        AddHandler butOk.Click, Sub()
                                    m_bolOk = True
                                    Close()
                                End Sub
        AddHandler butCancel.Click, Sub() Close()
    End Sub

    ''' <summary>The people ticked; Nothing when cancelled.</summary>
    Public Function Pick(ByVal prompt As String, ByVal people As List(Of FaceCatalog.PersonEntry)) As List(Of FaceCatalog.PersonEntry)
        lblPrompt.Text = prompt
        m_lpAll = people.OrderByDescending(Function(p) p.PhotoCount).ToList()
        Fill()
        ShowDialog()
        If Not m_bolOk Then Return Nothing
        Return m_lpAll.Where(Function(p) m_lpTicked.Contains(p)).ToList()
    End Function

    Private Class Row
        Public Person As FaceCatalog.PersonEntry
        Public Overrides Function ToString() As String
            Return Person.Name & "（" & Person.PhotoCount & " 張）"
        End Function
    End Class

    Private Sub Fill()
        Dim filter As String = txtFilter.Text.Trim()
        m_bolFilling = True
        lstPeople.BeginUpdate()
        lstPeople.Items.Clear()
        For Each p In m_lpAll.Where(Function(x) filter = "" OrElse x.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
            lstPeople.Items.Add(New Row With {.Person = p}, m_lpTicked.Contains(p))
        Next
        lstPeople.EndUpdate()
        m_bolFilling = False
        UpdateOk()
    End Sub

    Private Sub People_ItemCheck(sender As Object, e As ItemCheckEventArgs)
        If m_bolFilling Then Return
        Dim p As FaceCatalog.PersonEntry = CType(lstPeople.Items(e.Index), Row).Person
        If e.NewValue = CheckState.Checked Then m_lpTicked.Add(p) Else m_lpTicked.Remove(p)
        BeginInvoke(New Action(AddressOf UpdateOk))   ' after the box has its new state
    End Sub

    Private Sub UpdateOk()
        butOk.Enabled = m_lpTicked.Count > 0
        butOk.Text = If(m_lpTicked.Count > 0, "找合照（" & m_lpTicked.Count & "）", "找合照")
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
