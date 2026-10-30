' 年度回顧 (the tool bar's 16; frmMain.YearReview.vb): which year, and what to make of it -- 拼貼 or
' 自由拼貼 with a number of picks, or 馬賽克 with the whole year as its tiles. The picks themselves are
' YearPicks'. Made in code (no designer file), as frmCleanup.
Friend Class frmYearReview
    Inherits Form

    Private ReadOnly cboYear As New ComboBox
    Private ReadOnly optCollage As New RadioButton
    Private ReadOnly optFree As New RadioButton
    Private ReadOnly optMosaic As New RadioButton
    Private ReadOnly cboCount As New ComboBox
    Private ReadOnly butStart As New Aqua.ThinButton
    Private ReadOnly butCancel As New Aqua.ThinButton
    Private ReadOnly m_lpYears As List(Of KeyValuePair(Of Integer, Integer))

    Private Shared ReadOnly CollageCounts As Integer() = {12, 20, 30, 48}
    Private Shared ReadOnly FreeCounts As Integer() = {8, 12, 20, 30}
    Public Const MosaicCount As Integer = 2000

    ''' <param name="years">The years with photos (year, how many), any order.</param>
    Public Sub New(ByVal years As IEnumerable(Of KeyValuePair(Of Integer, Integer)))
        m_lpYears = years.OrderByDescending(Function(y) y.Key).ToList()
        Text = "年度回顧"
        Font = New Font("Microsoft JhengHei UI", 10.5F)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False : MaximizeBox = False : ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(460, 300)
        BackColor = Color.White

        Dim lblYear As New Label With {.Text = "年份", .Bounds = New Rectangle(20, 22, 60, 24)}
        cboYear.SetBounds(90, 18, 220, 28)
        cboYear.DropDownStyle = ComboBoxStyle.DropDownList
        For Each y In m_lpYears
            cboYear.Items.Add($"{y.Key} 年（{y.Value:#,0} 張）")
        Next
        If cboYear.Items.Count > 0 Then cboYear.SelectedIndex = If(m_lpYears(0).Key = Date.Today.Year AndAlso m_lpYears.Count > 1 AndAlso Date.Today.Month < 3, 1, 0)

        Dim lblMake As New Label With {.Text = "做成", .Bounds = New Rectangle(20, 62, 60, 24)}
        optCollage.SetBounds(90, 58, 120, 26) : optCollage.Text = "拼貼" : optCollage.Checked = True
        optFree.SetBounds(90, 88, 120, 26) : optFree.Text = "自由拼貼"
        optMosaic.SetBounds(90, 118, 340, 26) : optMosaic.Text = $"馬賽克（全年的照片當素材，最多 {MosaicCount:#,0} 張）"
        Dim lblCount As New Label With {.Text = "挑選", .Bounds = New Rectangle(220, 62, 50, 24)}
        cboCount.SetBounds(270, 58, 100, 28)
        cboCount.DropDownStyle = ComboBoxStyle.DropDownList

        Dim hint As New Label With {
            .Bounds = New Rectangle(20, 156, 420, 70), .ForeColor = Color.DimGray,
            .Text = "依評價、人物與地點挑選，每個月都有、同一天不會挑太多。" & vbCrLf &
                    "挑好的照片會放進蒙太奇拼貼，在那裡還可以再加減照片、換版型；" & vbCrLf & "匯出時選擇作品存到哪本相簿。"}

        butStart.SetBounds(230, 248, 100, 30) : butStart.Text = "開始"
        butCancel.SetBounds(340, 248, 100, 30) : butCancel.Text = "取消"
        For Each b In {butStart, butCancel}
            b.Font = New Font("華康細圓體", 12.0F)
            b.ForeColor = Color.Black
        Next
        Controls.AddRange({lblYear, cboYear, lblMake, optCollage, optFree, optMosaic, lblCount, cboCount, hint, butStart, butCancel})

        AddHandler optCollage.CheckedChanged, Sub() FillCounts()
        AddHandler optFree.CheckedChanged, Sub() FillCounts()
        AddHandler optMosaic.CheckedChanged, Sub() FillCounts()
        AddHandler butStart.Click, Sub()
                                       If cboYear.SelectedIndex < 0 Then Return
                                       DialogResult = DialogResult.OK
                                       Close()
                                   End Sub
        AddHandler butCancel.Click, Sub() Close()
        FillCounts()
    End Sub

    Private Sub FillCounts()
        Dim counts As Integer() = If(optFree.Checked, FreeCounts, CollageCounts)
        Dim keep As Integer = Count
        cboCount.Items.Clear()
        For Each n In counts
            cboCount.Items.Add(n & " 張")
        Next
        Dim i As Integer = Array.IndexOf(counts, keep)
        cboCount.SelectedIndex = If(i >= 0, i, 1)
        cboCount.Enabled = Not optMosaic.Checked
    End Sub

    Public ReadOnly Property Year As Integer
        Get
            Return If(cboYear.SelectedIndex >= 0, m_lpYears(cboYear.SelectedIndex).Key, 0)
        End Get
    End Property

    Public ReadOnly Property Mode As PhotoMontage.Core.MontageMode
        Get
            If optMosaic.Checked Then Return PhotoMontage.Core.MontageMode.Mosaic
            If optFree.Checked Then Return PhotoMontage.Core.MontageMode.Free
            Return PhotoMontage.Core.MontageMode.Collage
        End Get
    End Property

    ''' <summary>How many photos to pick.</summary>
    Public ReadOnly Property Count As Integer
        Get
            If optMosaic.Checked Then Return MosaicCount
            If cboCount.SelectedIndex < 0 Then Return 0
            Return If(optFree.Checked, FreeCounts, CollageCounts)(cboCount.SelectedIndex)
        End Get
    End Property

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = Keys.Escape Then
            Close()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
