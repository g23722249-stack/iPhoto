' 全圖瀏覽 turns the page when it moves to another photo (new in the .NET port; 設定 › 全圖瀏覽).
' The main window changes photos as Clear() then ShowPicture(); reading the next picture takes a moment
' in between. So Clear() keeps a frame of the photo on screen and lays PageTurnView over the picture
' area showing it (nothing goes black meanwhile), and ShowPicture() turns from it to a frame of the new
' photo. Style: 設定's choice, or -- the default -- by the clock: an even second 整頁翻, an odd one 翻頁角.
' Direction: TurnDirection (+1 next, -1 prior), set by the prior / next buttons here and by the main
' window's list; it goes back to +1 after each turn. No turn for videos, while the viewer is hidden,
' when 設定 turned it off, or when the user asks for reduced animation (Windows' "顯示動畫" off).
' Going on while a turn runs finishes it at once and starts the next one from where it was going.
Partial Class frmViewerLarge

    Private m_turn As PageTurnView

    ''' <summary>+1 when the next photo is coming, -1 for the prior one.</summary>
    Public Property TurnDirection As Integer = 1

    Private ReadOnly Property TurnEnabled As Boolean
        Get
            Return g_lpConfig IsNot Nothing AndAlso g_lpConfig.ViewerPageTurn <> Config.enumPageTurn.ptOff AndAlso
                   SystemInformation.UIEffectsEnabled
        End Get
    End Property

    Private Sub EnsureTurnView()
        If m_turn IsNot Nothing Then Return
        m_turn = New PageTurnView With {.Dock = DockStyle.Fill}
        picPhoto.Controls.Add(m_turn)
        AddHandler m_turn.Click, Sub(s, e) If m_turn.IsTurning Then m_turn.Cancel()
    End Sub

    ''' <summary>Called first thing in Clear(): the photo on screen stays up (still) for the turn.</summary>
    Private Sub KeepFrameForTurn()
        If Not TurnEnabled OrElse Not Visible OrElse WindowState = FormWindowState.Minimized Then
            m_turn?.Cancel()
            Return
        End If
        EnsureTurnView()
        If m_turn.HasStill Then Return              ' ShowPicture clears again after the main window did
        Dim box As RectangleF
        Dim still As Bitmap = m_turn.TakeTarget(box)   ' a turn still running: go on from its end
        If still Is Nothing Then
            If m_enumMediaMode <> enumMediaMode.mmImage OrElse Not picPhoto.Visible OrElse Not imgPhoto.Visible OrElse imgPhoto.Image Is Nothing Then
                m_turn.Cancel()
                Return
            End If
            still = PageTurnView.Frame(picPhoto.ClientSize, imgPhoto.Image, imgPhoto.Bounds)
            box = imgPhoto.Bounds
        End If
        m_turn.ShowStill(still, box)
    End Sub

    ''' <summary>Called at the end of ShowPicture(): turns to the photo now laid out.</summary>
    Private Sub TurnToPicture()
        Dim dir As Integer = TurnDirection
        TurnDirection = 1
        If m_turn Is Nothing OrElse Not m_turn.HasStill Then Return
        If imgPhoto.Image Is Nothing OrElse picPhoto.ClientSize <> m_turn.ClientSize Then
            m_turn.Cancel()
            Return
        End If
        Dim style As PageTurnView.enumTurnStyle
        Select Case g_lpConfig.ViewerPageTurn
            Case Config.enumPageTurn.ptBook : style = PageTurnView.enumTurnStyle.tsBook
            Case Config.enumPageTurn.ptCorner : style = PageTurnView.enumTurnStyle.tsCorner
            Case Else : style = If(DateTime.Now.Second Mod 2 = 0, PageTurnView.enumTurnStyle.tsBook, PageTurnView.enumTurnStyle.tsCorner)
        End Select
        m_turn.Turn(PageTurnView.Frame(picPhoto.ClientSize, imgPhoto.Image, imgPhoto.Bounds), imgPhoto.Bounds, dir, style, g_lpConfig.ViewerPageTurnMs)
    End Sub

    ''' <summary>ShowVideo: no turn.</summary>
    Private Sub CancelTurn()
        TurnDirection = 1
        m_turn?.Cancel()
    End Sub

End Class
