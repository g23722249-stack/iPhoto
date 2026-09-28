' Port of iPhoto\Form\frmCover.frm: makes a "面孔" cover -- drag a box around a face in the photo (Shift
' = square, right-click = start again); a 4:3 crop of it goes on the name card (Picture1) under the
' typed name, which OK saves as <FaceCover folder>\<name>.Jpg and adds to the database.
' Only the frmMain variants that were not ported (1024x768 / 1440x900) opened it in VB6.
' Callers use "Using f As New frmCover": f.SetCover(file) then f.Result.
' Fixed from VB6: a box dragged up or to the left cropped the area right of / below the start point;
' the crop is clipped to the photo.
Friend Class frmCover

    Private m_strFileDesc As String = ""
    Private m_lpImage As Bitmap
    Private m_lpFaceImage As Bitmap
    Private m_dblScale As Double = 1              ' shown pixels per photo pixel
    Private m_blnDrawing As Boolean
    Private m_p1 As Point, m_p2 As Point           ' the box, in imgPhoto pixels
    Private m_bolResult As Boolean

    Public ReadOnly Property Result As Boolean
        Get
            Return m_bolResult
        End Get
    End Property

    Public Sub SetCover(ByVal strFileDesc As String)
        m_strFileDesc = strFileDesc
        ShowDialog()
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_bolResult = False
        Try
            m_lpImage = Quartz.LoadPicture(m_strFileDesc)
        Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IO.IOException OrElse TypeOf ex Is OutOfMemoryException
            m_lpImage = Nothing
        End Try
        MovePictureToScreen()
        InitialSelection()
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        imgPhoto.Image?.Dispose()
        imgPhoto.Image = Nothing
        picFace.Image = Nothing
        m_lpFaceImage?.Dispose()
        m_lpImage?.Dispose()
    End Sub

    ''' <summary>The photo shrunk (never enlarged) into picPhoto and centred.</summary>
    Private Sub MovePictureToScreen()
        If m_lpImage Is Nothing Then Return
        Dim shown As Bitmap = Quartz.ImageFilter.Fit(m_lpImage, picPhoto.ClientSize.Width, picPhoto.ClientSize.Height, False)
        imgPhoto.SetBounds((picPhoto.ClientSize.Width - shown.Width) \ 2, (picPhoto.ClientSize.Height - shown.Height) \ 2, shown.Width, shown.Height)
        imgPhoto.Image?.Dispose()
        imgPhoto.Image = shown
        m_dblScale = shown.Width / m_lpImage.Width
    End Sub

    Private Sub InitialSelection()
        m_p1 = Point.Empty
        m_p2 = Point.Empty
        lblSelPoint.Text = ""
        m_blnDrawing = False
        imgPhoto.Invalidate()
    End Sub

    Private Function ToPhoto(ByVal p As Point) As Point
        Return New Point(CInt(Math.Floor(p.X / m_dblScale)), CInt(Math.Floor(p.Y / m_dblScale)))
    End Function

    Private Sub imgPhoto_MouseDown(sender As Object, e As MouseEventArgs) Handles imgPhoto.MouseDown
        If e.Button = MouseButtons.Left Then
            m_blnDrawing = True
            m_p1 = e.Location
            m_p2 = m_p1
        Else
            InitialSelection()
        End If
    End Sub

    Private Sub imgPhoto_MouseMove(sender As Object, e As MouseEventArgs) Handles imgPhoto.MouseMove
        If e.Button <> MouseButtons.Left OrElse Not m_blnDrawing OrElse imgPhoto.Image Is Nothing Then Return
        AdjustP2(e.X, e.Y, ModifierKeys)
        imgPhoto.Invalidate()
        Dim a As Point = ToPhoto(m_p1), b As Point = ToPhoto(m_p2)
        lblSelPoint.Text = "(" & a.X & "," & a.Y & ")～(" & b.X & "," & b.Y & ")"
    End Sub

    Private Sub imgPhoto_MouseUp(sender As Object, e As MouseEventArgs) Handles imgPhoto.MouseUp
        If e.Button <> MouseButtons.Left OrElse Not m_blnDrawing Then Return
        AdjustP2(e.X, e.Y, ModifierKeys)
        m_blnDrawing = False
        imgPhoto.Invalidate()
        CropPicture()
    End Sub

    Private Sub imgPhoto_Paint(sender As Object, e As PaintEventArgs) Handles imgPhoto.Paint
        If m_p1 = m_p2 Then Return
        Dim r As Rectangle = Rectangle.FromLTRB(Math.Min(m_p1.X, m_p2.X), Math.Min(m_p1.Y, m_p2.Y), Math.Max(m_p1.X, m_p2.X), Math.Max(m_p1.Y, m_p2.Y))
        ' VB6 drew the box with an XOR pen; a black-and-white dashed box shows on any photo
        Using p As New Pen(Color.White)
            e.Graphics.DrawRectangle(p, r)
        End Using
        Using p As New Pen(Color.Black) With {.DashStyle = Drawing2D.DashStyle.Dash}
            e.Graphics.DrawRectangle(p, r)
        End Using
    End Sub

    ''' <summary>Shift keeps the box square (VB6 AdjustP2).</summary>
    Private Sub AdjustP2(ByVal X As Integer, ByVal Y As Integer, ByVal keys As Keys)
        If (keys And Keys.Shift) = Keys.Shift Then
            If Math.Abs(X - m_p1.X) <= Math.Abs(Y - m_p1.Y) Then
                m_p2 = New Point(X, If(Y > m_p1.Y, m_p1.Y + Math.Abs(X - m_p1.X), m_p1.Y - Math.Abs(X - m_p1.X)))
            Else
                m_p2 = New Point(If(X > m_p1.X, m_p1.X + Math.Abs(Y - m_p1.Y), m_p1.X - Math.Abs(Y - m_p1.Y)), Y)
            End If
        Else
            m_p2 = New Point(X, Y)
        End If
    End Sub

    ''' <summary>A 4:3 piece of the photo, as wide as the box, from its top-left corner.</summary>
    Private Sub CropPicture()
        If m_lpImage Is Nothing Then Return
        Dim a As Point = ToPhoto(m_p1), b As Point = ToPhoto(m_p2)
        If a.X = b.X OrElse a.Y = b.Y Then Return
        Dim x As Integer = Math.Max(0, Math.Min(a.X, b.X)), y As Integer = Math.Max(0, Math.Min(a.Y, b.Y))
        Dim w As Integer = Math.Abs(b.X - a.X)
        Dim h As Integer = w * 3 \ 4
        w = Math.Min(w, m_lpImage.Width - x)
        h = Math.Min(h, m_lpImage.Height - y)
        If w <= 0 OrElse h <= 0 Then Return

        Dim face As New Bitmap(w, h)
        Using g As Graphics = Graphics.FromImage(face)
            g.DrawImage(m_lpImage, New Rectangle(0, 0, w, h), New Rectangle(x, y, w, h), GraphicsUnit.Pixel)
        End Using
        picFace.Image = face
        m_lpFaceImage?.Dispose()
        m_lpFaceImage = face
    End Sub

    ''' <summary>The name card as it shows in Picture1: the frame, the face and the name.</summary>
    Private Function PaintFaceCard() As Bitmap
        Dim card As New Bitmap(Picture1.ClientSize.Width, Picture1.ClientSize.Height)
        Using g As Graphics = Graphics.FromImage(card)
            g.Clear(Picture1.BackColor)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            Using p As New Pen(Color.FromArgb(224, 224, 224))     ' VB6 Shape1.BorderColor &H00E0E0E0
                g.DrawRectangle(p, Shape1.Left, Shape1.Top, Shape1.Width, Shape1.Height)
            End Using
            g.DrawImage(m_lpFaceImage, picFace.Bounds)
            ' 列印名稱
            Dim sz As Size = TextRenderer.MeasureText(Text1.Text, Text1.Font)
            TextRenderer.DrawText(g, Text1.Text, Text1.Font, New Point((card.Width - sz.Width) \ 2, Text1.Top), Color.Black)
        End Using
        Return card
    End Function

    Private Sub butExit_Click(sender As Object, e As EventArgs) Handles butExit.Click
        Close()
    End Sub

    Private Sub butOk_Click(sender As Object, e As EventArgs) Handles butOk.Click
        If m_lpFaceImage Is Nothing Then
            frmMsgBox.ShowCriticalMessage("請先選取照片中的面孔", "錯誤")
            Return
        End If
        Dim name As String = Text1.Text.Trim()
        If name = "" OrElse name = "請輸入姓名" Then
            frmMsgBox.ShowCriticalMessage("請輸入屬於這個面孔的姓名", "錯誤")
            Return
        End If
        If Not CheckNameRule(name) Then
            frmMsgBox.ShowCriticalMessage("姓名含有不合法的字元", "錯誤")
            Return
        End If
        Dim folder As String = g_lpConfig.Attached(Config.enumAttachedFile.filFaceCover)
        If Not g_lpFileSystem.CreateFolder(folder) Then
            frmMsgBox.ShowCriticalMessage("建立存放面孔的資料夾失敗", "錯誤")
            Return
        End If
        Dim strFileDesc As String = folder & "\" & name & ".Jpg"
        If g_lpFileSystem.FileExists(strFileDesc) AndAlso Not frmQueryMsgBox.ShowMessage("封面已經存在，要覆蓋嗎？", "注意") Then Return

        Using card As Bitmap = PaintFaceCard()
            If Not Quartz.SavePicture(card, strFileDesc) Then
                frmMsgBox.ShowCriticalMessage("儲存檔案失敗", "錯誤")
                Return
            End If
        End Using
        If g_lpDatabase IsNot Nothing Then g_lpDatabase.AddFaceCover(name, strFileDesc)
        m_bolResult = True
        Close()
    End Sub

    Private Sub Text1_GotFocus(sender As Object, e As EventArgs) Handles Text1.GotFocus
        Text1.SelectAll()
    End Sub

    Private Sub Text1_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles Text1.Validating
        Text1.Text = Text1.Text.Trim()
        If Text1.Text = "" Then Text1.Text = "請輸入姓名"
    End Sub

End Class
