<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmImportStudio
    Inherits Aqua.AquaForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    ' The window only: its controls are made in code (frmImportStudio.vb, BuildUI) -- the thumbnail
    ' grids, the face stage and the step bar are drawn controls the designer can't lay out usefully.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        vb6ToolTip = New ToolTip(components)
        SuspendLayout()
        '
        'frmImportStudio
        '
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.FromArgb(CByte(236), CByte(239), CByte(243))
        ClientSize = New Size(1200, 812)
        Font = New Font("華康細圓體", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        ForeColor = Color.FromArgb(CByte(30), CByte(36), CByte(44))
        KeyPreview = True
        MaxButton = False
        MaximizeBox = False
        MenuFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        MinButton = False
        MinimizeBox = False
        Name = "frmImportStudio"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "輸入照片"
        TitleFont = New Font("華康細圓體", 14.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(136))
        WindowBorderStyle = Aqua.FormBorderStyle.Fixed
        ResumeLayout(False)

    End Sub

    Friend WithEvents vb6ToolTip As System.Windows.Forms.ToolTip
End Class
