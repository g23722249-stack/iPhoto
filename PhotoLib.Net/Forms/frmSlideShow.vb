' Port of Lib\Form\frmSlideShow.frm: a full-screen slide show of every album, starting at the given
' album / class / photo, one picture every Second seconds with a random transition, and random
' background music from Config.Slide(sliMusicPath). Esc closes it.
'
' The transitions are VB6's, drawn with the same loop counts; VB6 worked in twips (15 per pixel on its
' screens), so the positions are divided by 15 when drawn. VB6 painted with raster op &HCC0029, which
' is not a real ROP code -- plain copy (SRCCOPY &HCC0020) is what showed.
' CoreMedia.Audio -> Techno.Net (MCI); Quartz.Thumbnail -> Techno.Net.
' Fixed from VB6: an album with no pictures at all (only videos, or empty classes) looped forever
' looking for the next picture; now the show ends.
Public Class frmSlideShow

    Private Const TwipsPerPixel As Integer = 15

    Private m_intSecond As Integer
    Private m_intPassed As Integer
    Private m_intAlbumIndex As Integer
    Private m_intClassIndex As Integer
    Private m_intPhotoIndex As Integer

    Private m_objCurrent As Bitmap
    Private m_objPreliminary As Bitmap
    Private m_intLastEffective As Integer = -1   ' 用來判斷特效是否重複
    Private m_intLastSound As Integer = -1       ' 用來判斷最後撥放的音樂

    Private m_strMusic() As String = {}
    Private m_objAudio As CoreMedia.Audio
    Private ReadOnly m_random As New Random()
    Private m_bolClosing As Boolean               ' Esc / Close during a transition: stop drawing

    Public Sub SlideShow(ByVal Second As Integer, ByVal Album As Integer, ByVal [Class] As Integer, ByVal Photo As Integer)
        m_intSecond = Math.Max(1, Second)
        m_intAlbumIndex = Album
        m_intClassIndex = [Class]
        m_intPhotoIndex = Photo
        If g_lpStorage Is Nothing OrElse g_lpStorage.AlbumCount = 0 Then Return
        If m_intAlbumIndex < 0 OrElse m_intAlbumIndex >= g_lpStorage.AlbumCount Then
            m_intAlbumIndex = 0
            m_intClassIndex = 0
            m_intPhotoIndex = 0
        End If
        If m_intClassIndex < 0 Then m_intClassIndex = 0
        If m_intPhotoIndex < 0 Then m_intPhotoIndex = 0

        m_intPassed = 0
        m_bolClosing = False
        m_intLastEffective = -1
        m_intLastSound = -1
        ShowDialog()
    End Sub

    Private Sub Form_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then Close()
    End Sub

    Private Sub LoadMusic()
        Dim files() As String = Nothing
        Dim n As Integer = frmResAlbum.ExactFiles(g_lpConfig.Slide(Config.enumSlide.sliMusicPath), gc_strMusicPattern, files, True)
        m_strMusic = If(n > 0, files, New String() {})
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackColor = Color.Black
        LoadMusic()
        If Not LoadPhoto() Then
            BeginInvoke(New Action(AddressOf Close))
            Return
        End If
        MovePhotoToScreen()
        Timer1.Enabled = True

        If m_strMusic.Length > 0 Then
            m_objAudio = New CoreMedia.Audio
            tmSound.Enabled = True
        End If
    End Sub

    Private Sub Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        m_bolClosing = True
        Timer1.Enabled = False
    End Sub

    Private Sub Form_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Timer1.Enabled = False
        tmSound.Enabled = False
        m_intPassed = 0
        Dim shown As Image = BackgroundImage
        BackgroundImage = Nothing
        If shown IsNot Nothing AndAlso shown IsNot m_objCurrent Then shown.Dispose()
        If m_objCurrent IsNot Nothing Then m_objCurrent.Dispose()
        If m_objPreliminary IsNot Nothing Then m_objPreliminary.Dispose()
        m_objCurrent = Nothing
        m_objPreliminary = Nothing
        If m_objAudio IsNot Nothing Then m_objAudio.Dispose()
        m_objAudio = Nothing
    End Sub

    Private ReadOnly Property ScreenSize As Size
        Get
            Return Screen.FromControl(Me).Bounds.Size
        End Get
    End Property

    ''' <summary>A black screen-sized picture with the photo centred (VB6 GetCurrentSizePhoto).</summary>
    Private Function GetCurrentSizePhoto(ByVal pic As Image) As Bitmap
        Dim sz As Size = ScreenSize
        Dim bmp As New Bitmap(sz.Width, sz.Height)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Black)
            If pic IsNot Nothing Then g.DrawImage(pic, (sz.Width - pic.Width) \ 2, (sz.Height - pic.Height) \ 2, pic.Width, pic.Height)
        End Using
        Return bmp
    End Function

    Private Function CurrentPhoto() As Photo
        Dim alb As Albums = g_lpStorage.Album(m_intAlbumIndex)
        If m_intClassIndex >= alb.ItemCount Then Return Nothing
        Dim cls As PhotoSet = CType(alb.Item(m_intClassIndex), PhotoSet)
        If m_intPhotoIndex >= cls.PhotoCount Then Return Nothing
        Return cls.Photo(m_intPhotoIndex)
    End Function

    Private Function IsPicture(ByVal p As Photo) As Boolean
        Return p IsNot Nothing AndAlso p.MediaType = enumPhotoMediaType.mdImage
    End Function

    ''' <summary>Moves to the next picture (the current one too when <paramref name="includeCurrent"/>);
    ''' False when a whole round finds none.</summary>
    Private Function SeekPicture(ByVal includeCurrent As Boolean) As Boolean
        EnsureLoaded()
        If includeCurrent AndAlso IsPicture(CurrentPhoto()) Then Return True
        Dim start As (Integer, Integer, Integer) = (-1, -1, -1)
        Do
            CalcPhotoIndex()
            If IsPicture(CurrentPhoto()) Then Return True
            Dim pos = (m_intAlbumIndex, m_intClassIndex, m_intPhotoIndex)
            If start.Item1 < 0 Then
                start = pos
            ElseIf pos.Equals(start) Then
                Return False
            End If
        Loop
    End Function

    ''' <summary>The previously pre-loaded picture becomes current and the next one is pre-loaded
    ''' (VB6 kept both); False when no album holds any picture.</summary>
    Private Function LoadPhoto() As Boolean
        If m_objPreliminary Is Nothing Then
            If Not SeekPicture(True) Then Return False
            m_objPreliminary = LoadScreenPhoto(CurrentPhoto().FileDesc)
        End If
        ' the picture on screen is freed by MovePhotoToScreen once it is replaced
        m_objCurrent = m_objPreliminary
        m_objPreliminary = Nothing
        If Not SeekPicture(False) Then Return False
        m_objPreliminary = LoadScreenPhoto(CurrentPhoto().FileDesc)
        Return True
    End Function

    Private Function LoadScreenPhoto(ByVal file As String) As Bitmap
        Dim thumb As New Quartz.Thumbnail With {.FileName = file}
        Dim sz As Size = ScreenSize
        Using pic As Bitmap = thumb.GetThumbnail(sz.Width, sz.Height)
            Return GetCurrentSizePhoto(pic)
        End Using
    End Function

    Private Sub EnsureLoaded()
        Dim alb As Albums = g_lpStorage.Album(m_intAlbumIndex)
        alb.Load()
        If m_intClassIndex < alb.ItemCount Then CType(alb.Item(m_intClassIndex), PhotoSet).Load()
    End Sub

    ''' <summary>Next photo, then next class, then next album, then back to the first (VB6 GoSub CalcPhotoIndex).</summary>
    Private Sub CalcPhotoIndex()
        Dim alb As Albums = g_lpStorage.Album(m_intAlbumIndex)
        Dim cls As PhotoSet = If(m_intClassIndex < alb.ItemCount, CType(alb.Item(m_intClassIndex), PhotoSet), Nothing)
        If cls Is Nothing OrElse m_intPhotoIndex + 1 >= cls.PhotoCount Then
            m_intPhotoIndex = 0
            If m_intClassIndex + 1 >= alb.ItemCount Then
                m_intClassIndex = 0
                m_intAlbumIndex = If(m_intAlbumIndex + 1 >= g_lpStorage.AlbumCount, 0, m_intAlbumIndex + 1)
            Else
                m_intClassIndex += 1
            End If
            EnsureLoaded()
        Else
            m_intPhotoIndex += 1
        End If
    End Sub

    Private Sub MovePhotoToScreen()
        BackgroundImageLayout = ImageLayout.None
        Dim old As Image = BackgroundImage
        BackgroundImage = m_objCurrent
        If old IsNot Nothing AndAlso old IsNot m_objCurrent Then old.Dispose()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        m_intPassed += 1
        If m_intPassed < m_intSecond Then Return
        Timer1.Enabled = False

        If Not LoadPhoto() Then
            Close()
            Return
        End If
        SlideShowScreen()
        If m_bolClosing OrElse IsDisposed Then Return
        MovePhotoToScreen()

        m_intPassed = 0
        Timer1.Enabled = True
    End Sub

    '==================================================================================================
    ' Transitions (VB6 SlideShowScreen, coordinates in twips)
    '==================================================================================================
    ''' <summary>VB6 Me.PaintPicture pic, x, y, w, h, sx, sy, sw, sh (twips; the source part is scaled
    ''' into the destination).</summary>
    Private Sub PaintPart(ByVal g As Graphics, ByVal x As Double, ByVal y As Double, ByVal w As Double, ByVal h As Double,
                      ByVal sx As Double, ByVal sy As Double, ByVal sw As Double, ByVal sh As Double)
        Dim dst As New Rectangle(CInt(x / TwipsPerPixel), CInt(y / TwipsPerPixel), CInt(Math.Ceiling(w / TwipsPerPixel)), CInt(Math.Ceiling(h / TwipsPerPixel)))
        Dim src As New Rectangle(CInt(sx / TwipsPerPixel), CInt(sy / TwipsPerPixel), CInt(Math.Ceiling(sw / TwipsPerPixel)), CInt(Math.Ceiling(sh / TwipsPerPixel)))
        If dst.Width <= 0 OrElse dst.Height <= 0 OrElse src.Width <= 0 OrElse src.Height <= 0 Then Return
        g.DrawImage(m_objCurrent, dst, src, GraphicsUnit.Pixel)
    End Sub

    Private Function Pump() As Boolean
        Application.DoEvents()
        Return Not m_bolClosing AndAlso Not IsDisposed AndAlso Visible
    End Function

    Private Sub SlideShowScreen()
        Dim intRandom As Integer
        Do
            intRandom = m_random.Next(0, 25)
        Loop Until m_intLastEffective <> intRandom
        m_intLastEffective = intRandom

        Dim sw As Integer = ScreenSize.Width * TwipsPerPixel, sh As Integer = ScreenSize.Height * TwipsPerPixel
        Dim intBarSize As Integer, intBarNumber As Integer
        Using g As Graphics = CreateGraphics()
            g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            Select Case intRandom
                Case 0, 24
                    intBarSize = 300 : intBarNumber = sw \ intBarSize
                    For I As Integer = 1 To intBarSize Step 20
                        For J As Integer = 0 To intBarNumber
                            PaintPart(g, J * intBarSize, 0, I, sh, J * intBarSize, 0, I, sh)
                            If Not Pump() Then Return
                        Next
                    Next
                Case 1, 23
                    intBarSize = 300 : intBarNumber = sw \ intBarSize
                    For I As Integer = 1 To intBarSize Step 20
                        For J As Integer = 0 To intBarNumber
                            PaintPart(g, 0, J * intBarSize, sw, I, 0, J * intBarSize, sw, I)
                            If Not Pump() Then Return
                        Next
                    Next
                Case 2, 22
                    For I As Integer = 1 To sw Step 320
                        PaintPart(g, 0, 0, I, sh, 0, 0, sw, sh)
                        If Not Pump() Then Return
                    Next
                Case 3, 21
                    For I As Integer = 1 To sw Step 320
                        PaintPart(g, 0, 0, sw, sh, 0, 0, I, sh)
                        If Not Pump() Then Return
                    Next
                Case 4, 20
                    For I As Integer = 1 To sh Step 320
                        PaintPart(g, 0, 0, sw, sh, 0, 0, sw, I)
                        If Not Pump() Then Return
                    Next
                Case 5, 19
                    For I As Integer = 1 To sh Step 320
                        PaintPart(g, 0, 0, sw, I, 0, 0, sw, sh)
                        If Not Pump() Then Return
                    Next
                Case 6, 18, 7, 17
                    For I As Integer = 1 To sw Step 160
                        PaintPart(g, sw - I, 0, I, sh, sw - I, 0, I, sh)
                        If Not Pump() Then Return
                    Next
                Case 8, 16
                    For I As Integer = 1 To sw Step 120
                        PaintPart(g, 0, 0, I, sh, 0, 0, I, sh)
                        PaintPart(g, 0, 0, sw, I, 0, 0, sw, I)
                        If Not Pump() Then Return
                    Next
                Case 9, 15
                    intBarSize = 300 : intBarNumber = sw \ intBarSize
                    For I As Integer = 1 To intBarSize \ 2 Step 60
                        For J As Integer = 0 To intBarNumber
                            PaintPart(g, J * intBarSize, 0, I, sh, J * intBarSize, 0, I, sh)
                            If Not Pump() Then Return
                        Next
                    Next
                    For I As Integer = 1 To intBarSize Step 220
                        For J As Integer = 0 To intBarNumber
                            PaintPart(g, 0, J * intBarSize, sw, I, 0, J * intBarSize, sw, I)
                            If Not Pump() Then Return
                        Next
                    Next
                Case 10, 14
                    intBarNumber = 16
                    Dim dx As Integer = sw \ intBarNumber, dy As Integer = sh \ intBarNumber
                    For I As Integer = 1 To intBarNumber
                        PaintPart(g, (sw - dx * I) / 2, (sh - dy * I) / 2, dx * I, dy * I, 0, 0, sw, sh)
                        If Not Pump() Then Return
                    Next
                Case 11, 13
                    intBarNumber = 8
                    Dim dx As Integer = (sw \ 2) \ intBarNumber
                    For I As Integer = intBarNumber To 0 Step -1
                        PaintPart(g, dx * I, 0, sw - (dx * I) * 2, sh, 0, 0, sw, sh)
                        If Not Pump() Then Return
                    Next
                Case Else
                    intBarSize = 600 : intBarNumber = sw \ intBarSize
                    For I As Integer = 1 To intBarSize \ 2 Step 40
                        For J As Integer = 0 To intBarNumber
                            PaintPart(g, 0, J * intBarSize, sw, I, 0, J * intBarSize, sw, I)
                            If Not Pump() Then Return
                        Next
                    Next
                    For I As Integer = 1 To intBarSize \ 2 Step 20
                        For J As Integer = 0 To intBarNumber
                            PaintPart(g, J * intBarSize, 0, I, sh, J * intBarSize, 0, I, sh)
                            If Not Pump() Then Return
                        Next
                    Next
            End Select
        End Using
    End Sub

    '==================================================================================================
    ' Background music
    '==================================================================================================
    Private Sub tmSound_Tick(sender As Object, e As EventArgs) Handles tmSound.Tick
        If m_objAudio Is Nothing OrElse m_strMusic.Length = 0 Then Return
        If m_objAudio.FileName = "" Then
            Dim intRandom As Integer = If(m_strMusic.Length > 1, m_random.Next(0, m_strMusic.Length), 0)
            m_intLastSound = intRandom
            m_objAudio.FileName = m_strMusic(intRandom)
            m_objAudio.Play()
        ElseIf m_objAudio.Finished Then
            m_objAudio.Stopped()
            Dim intRandom As Integer = 0
            If m_strMusic.Length > 1 Then
                Do
                    intRandom = m_random.Next(0, m_strMusic.Length)
                Loop Until intRandom <> m_intLastSound
            End If
            m_intLastSound = intRandom
            m_objAudio.FileName = m_strMusic(intRandom)
            m_objAudio.Play()
        End If
    End Sub

End Class
