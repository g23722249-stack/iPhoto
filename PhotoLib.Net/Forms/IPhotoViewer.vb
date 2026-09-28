' What the main window uses of frmViewerLarge / frmViewerSmall (VB6 held them in "Dim m_frmViewer As
' Object" and called these late bound). Both viewers are Forms: cast to Form to Show / Hide them.
Public Interface IPhotoViewer

    ''' <summary>The prior / next button of the viewer (VB6 events ShowPriorPhoto / ShowNextPhoto).</summary>
    Event ShowPriorPhoto As EventHandler
    Event ShowNextPhoto As EventHandler

    ''' <summary>True when the picture was edited in the viewer (the caller then offers to save it).</summary>
    ReadOnly Property Changed As Boolean

    ''' <summary>The (edited) picture shown.</summary>
    ReadOnly Property Photo As Image

    ''' <summary>VB6 Create(width, height): the screen size the viewer covers.</summary>
    Sub Create(ByVal Width As Integer, ByVal Height As Integer)

    Sub Clear()

    ''' <summary>Shows a picture; the viewer takes ownership of <paramref name="Picture"/>.</summary>
    Sub ShowPicture(ByVal FileName As String, ByVal Picture As Image, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean)

    Sub ShowVideo(ByVal FileName As String, ByVal FirstPhoto As Boolean, ByVal LastPhoto As Boolean)

End Interface
