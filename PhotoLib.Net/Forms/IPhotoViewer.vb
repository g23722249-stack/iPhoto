' What the main window uses of frmViewerLarge (VB6 held it, or frmViewerSmall, in "Dim m_frmViewer As
' Object" and called these late bound; frmViewerSmall is gone). The viewer is a Form: cast to Form to
' Show / Hide it.
Public Interface IPhotoViewer

    ''' <summary>The prior / next button of the viewer (VB6 events ShowPriorPhoto / ShowNextPhoto).</summary>
    Event ShowPriorPhoto As EventHandler
    Event ShowNextPhoto As EventHandler

    ''' <summary>左轉 / 右轉 of a picture with no unsaved edit: the file itself is turned (losslessly) by the
    ''' caller, who then shows it again. The angle in degrees, clockwise positive.</summary>
    Event RotateRequested(ByVal sender As Object, ByVal angle As Integer)
    ''' <summary>相片後製: the photo shown opens in PhotoEdit.</summary>
    Event EditRequested As EventHandler
    ''' <summary>加入 Dock / 移出 Dock of the photo shown.</summary>
    Event DockRequested As EventHandler
    ''' <summary>刪除 of the photo shown (the caller asks first).</summary>
    Event DeleteRequested As EventHandler
    ''' <summary>評價 given in the viewer (0..5).</summary>
    Event RatingChanged(ByVal sender As Object, ByVal rating As Integer)

    ''' <summary>True when the picture was edited in the viewer (the caller then offers to save it).</summary>
    ReadOnly Property Changed As Boolean

    ''' <summary>The (edited) picture shown.</summary>
    ReadOnly Property Photo As Image

    ''' <summary>The rating shown for the photo (set by the caller after ShowPicture).</summary>
    Property Rating As Integer

    ''' <summary>Whether the photo is in the Dock: the button says 加入 Dock or 移出 Dock.</summary>
    Property Docked As Boolean

    ''' <summary>True: the tool bar is off (an encrypted photo: it is only looked at).</summary>
    Property EditLocked As Boolean

    ''' <summary>VB6 Create(width, height): the screen size the viewer covers.</summary>
    Sub Create(ByVal Width As Integer, ByVal Height As Integer)

    Sub Clear()

    ''' <summary>Shows a picture; the viewer takes ownership of <paramref name="Picture"/>.</summary>
    Sub ShowPicture(ByVal FileName As String, ByVal Picture As Image, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean)

    ''' <summary>As ShowPicture, with <paramref name="Shown"/> -- the picture already scaled to fit
    ''' PictureArea (read ahead) -- used instead of scaling it here when it is the size needed. The viewer
    ''' takes ownership of both.</summary>
    Sub ShowPicture(ByVal FileName As String, ByVal Picture As Image, ByVal Shown As Image, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean)

    ''' <summary>The size a picture is fitted into (for reading ahead a scaled copy).</summary>
    ReadOnly Property PictureArea As Size

    Sub ShowVideo(ByVal FileName As String, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean)

End Interface
