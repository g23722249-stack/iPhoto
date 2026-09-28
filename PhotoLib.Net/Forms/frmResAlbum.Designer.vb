<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmResAlbum
    Inherits System.Windows.Forms.Form

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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmResAlbum))
        Me.imgDragItem = New System.Windows.Forms.PictureBox()
        Me.imlFileType = New System.Windows.Forms.ImageList(Me.components)
        Me.imlSubject = New System.Windows.Forms.ImageList(Me.components)
        Me.File1 = New Aqua.FileListBox()
        Me.Dir1 = New Aqua.DirListBox()
        Me.imlStorage = New System.Windows.Forms.ImageList(Me.components)
        Me.picTemp = New System.Windows.Forms.PictureBox()
        Me.SuspendLayout()
        '
        'imgDragItem
        '
        Me.imgDragItem.Location = New System.Drawing.Point(70, 284)
        Me.imgDragItem.Size = New System.Drawing.Size(48, 48)
        Me.imgDragItem.Name = "imgDragItem"
        Me.imgDragItem.BackColor = System.Drawing.Color.Transparent
        Me.imgDragItem.Image = CType(resources.GetObject("imgDragItem.Image"), System.Drawing.Image)
        '
        'imlFileType
        '
        Me.imlFileType.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.imlFileType.ImageSize = New System.Drawing.Size(24, 24)
        Me.imlFileType.TransparentColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        '
        'imlSubject
        '
        Me.imlSubject.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.imlSubject.ImageSize = New System.Drawing.Size(24, 24)
        Me.imlSubject.TransparentColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        '
        'File1
        '
        Me.File1.Location = New System.Drawing.Point(177, 12)
        Me.File1.Size = New System.Drawing.Size(95, 102)
        Me.File1.Name = "File1"
        Me.File1.TabIndex = 0
        '
        'Dir1
        '
        Me.Dir1.Location = New System.Drawing.Point(82, 13)
        Me.Dir1.Size = New System.Drawing.Size(95, 104)
        Me.Dir1.Name = "Dir1"
        Me.Dir1.TabIndex = 1
        '
        'imlStorage
        '
        Me.imlStorage.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.imlStorage.ImageSize = New System.Drawing.Size(24, 24)
        Me.imlStorage.TransparentColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer))
        '
        'picTemp
        '
        Me.picTemp.Location = New System.Drawing.Point(266, 218)
        Me.picTemp.Size = New System.Drawing.Size(239, 27)
        Me.picTemp.Name = "picTemp"
        Me.picTemp.TabIndex = 2
        Me.picTemp.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.picTemp.BackColor = System.Drawing.SystemColors.Window
        Me.picTemp.ForeColor = System.Drawing.SystemColors.WindowText
        '
        'frmResAlbum
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(1113, 685)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation
        Me.Text = "用來存放共享的物件"
        Me.Name = "frmResAlbum"
        Me.Controls.Add(Me.imgDragItem)
        Me.Controls.Add(Me.File1)
        Me.Controls.Add(Me.Dir1)
        Me.Controls.Add(Me.picTemp)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents imgDragItem As System.Windows.Forms.PictureBox
    Friend WithEvents imlFileType As System.Windows.Forms.ImageList
    Friend WithEvents imlSubject As System.Windows.Forms.ImageList
    Friend WithEvents File1 As Aqua.FileListBox
    Friend WithEvents Dir1 As Aqua.DirListBox
    Friend WithEvents imlStorage As System.Windows.Forms.ImageList
    Friend WithEvents picTemp As System.Windows.Forms.PictureBox
End Class
