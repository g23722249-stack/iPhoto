' 全圖瀏覽 in 單螢幕 (new in the .NET port; frmMain.SingleScreen.vb): a window of its own with the main
' window's look (Aqua iForm, brushed metal, the traffic-light buttons), opened maximised over the main
' window. It only holds the viewer (frmViewerLarge, TopLevel False) in a frame; the main window drives
' it as it drives the viewer on a second screen. Closing it (the red button) only hides it. Made in
' code (no designer file).
Friend Class frmFullView
    Inherits Aqua.iForm

    ''' <summary>The red button: the main window goes back to 縮圖.</summary>
    Public Event CloseRequested As EventHandler

    Private Const Edge As Integer = 12           ' as the main window: 12 from the sides
    Private Const MarginTop As Integer = 30      ' below the 23-pixel title bar
    Private Const MarginBottom As Integer = 18   ' room for the resize grip

    Private ReadOnly pnlFrame As New Aqua.Panel

    Public Sub New(ByVal viewer As Form)
        Dim mainRes As New ComponentModel.ComponentResourceManager(GetType(frmMain))
        Text = "全圖瀏覽"
        Font = New Font("華康細圓體", 12.0F)
        TitleFont = New Font("Times New Roman", 14.25F, FontStyle.Bold)
        Try
            Image = CType(mainRes.GetObject("$this.Image"), Image)   ' the main window's background
            SizeMode = Aqua.ImageSizeMode.StretchImage
        Catch ex As Exception When TypeOf ex Is Resources.MissingManifestResourceException OrElse TypeOf ex Is InvalidCastException
            BackColor = Color.FromArgb(236, 239, 243)
        End Try
        Shadow = False
        WindowBorderStyle = Aqua.FormBorderStyle.Sizable
        StartPosition = FormStartPosition.Manual
        ClientSize = New Size(1280, 900)   ' the size when it is restored
        MinimumSize = New Size(760, 520)
        ShowInTaskbar = False
        KeyPreview = False

        pnlFrame.PanelStyle = Aqua.PanelStyleMode.Container
        pnlFrame.BorderColor = Color.FromArgb(189, 189, 189)
        pnlFrame.BorderFocusColor = Color.FromArgb(159, 182, 244)
        pnlFrame.BackColor = Color.Black
        pnlFrame.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Controls.Add(pnlFrame)

        ' the viewer is designed Maximized (a whole screen of its own): here it is a child filling the frame
        viewer.WindowState = FormWindowState.Normal
        viewer.TopLevel = False
        viewer.FormBorderStyle = FormBorderStyle.None
        viewer.ShowInTaskbar = False
        pnlFrame.Controls.Add(viewer)
        viewer.Dock = DockStyle.Fill
        LayoutFrame()
    End Sub

    Private Sub LayoutFrame()
        pnlFrame.SetBounds(Edge, MarginTop, Math.Max(100, ClientSize.Width - 2 * Edge), Math.Max(100, ClientSize.Height - MarginTop - MarginBottom))
        pnlFrame.Padding = New Padding(4)
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        If Owner IsNot Nothing Then Icon = Owner.Icon
        UpdateMaximizedBounds()
    End Sub

    ''' <summary>A borderless window maximises over the whole monitor, task bar included: keep it to the
    ''' working area of the monitor it is on (as the main window does).</summary>
    Private Sub UpdateMaximizedBounds()
        Dim scr As Screen = Screen.FromControl(Me)
        Dim wa As Rectangle = scr.WorkingArea
        MaximizedBounds = New Rectangle(wa.X - scr.Bounds.X, wa.Y - scr.Bounds.Y, wa.Width, wa.Height)
    End Sub

    Protected Overrides Sub OnLocationChanged(e As EventArgs)
        If WindowState = FormWindowState.Normal Then UpdateMaximizedBounds()
        MyBase.OnLocationChanged(e)
    End Sub

    ''' <summary>Over the main window -- and over the 地點 window in 地點 mode, as it comes up last and active
    ''' (it stays owned by the main window: closing the 地點 window must not close it) -- maximised on its
    ''' screen (restoring to the main window's size).</summary>
    Public Sub ShowOver(ByVal main As Form)
        If Not Visible Then
            If WindowState <> FormWindowState.Maximized Then
                Dim r As Rectangle = If(main.WindowState = FormWindowState.Normal, main.Bounds, main.RestoreBounds)
                Bounds = r
                UpdateMaximizedBounds()
                WindowState = FormWindowState.Maximized
            End If
            If Owner Is Nothing Then Show(main) Else Show()
        End If
        Activate()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' the red button hides it (the viewer stays for the next 全圖); the main window closing ends it
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            RaiseEvent CloseRequested(Me, EventArgs.Empty)
        End If
        MyBase.OnFormClosing(e)
    End Sub

End Class
