' VB default form instances (frmMsgBox.Show ... as VB6 code writes them) only resolve inside the
' assembly that defines the form. This exposes PhotoLib's own default instances (My.Forms) so an app
' shares the SAME instance with the PhotoLib code, e.g. a result set on the form in one place is
' read back in the other. Apps forward to these from a module of their own (iPhoto: Modules\LibForms.vb).
Public Module DefaultInstances

    Public ReadOnly Property Of_frmBrowserFile As frmBrowserFile
        Get
            Return My.Forms.frmBrowserFile
        End Get
    End Property

    Public ReadOnly Property Of_frmBrowserFolder As frmBrowserFolder
        Get
            Return My.Forms.frmBrowserFolder
        End Get
    End Property

    Public ReadOnly Property Of_frmInputString As frmInputString
        Get
            Return My.Forms.frmInputString
        End Get
    End Property

    Public ReadOnly Property Of_frmLoading As frmLoading
        Get
            Return My.Forms.frmLoading
        End Get
    End Property

    Public ReadOnly Property Of_frmLogo As frmLogo
        Get
            Return My.Forms.frmLogo
        End Get
    End Property

    Public ReadOnly Property Of_frmMsgBox As frmMsgBox
        Get
            Return My.Forms.frmMsgBox
        End Get
    End Property

    Public ReadOnly Property Of_frmQueryMsgBox As frmQueryMsgBox
        Get
            Return My.Forms.frmQueryMsgBox
        End Get
    End Property

    Public ReadOnly Property Of_frmResAlbum As frmResAlbum
        Get
            Return My.Forms.frmResAlbum
        End Get
    End Property

    Public ReadOnly Property Of_frmSaveChangedPhoto As frmSaveChangedPhoto
        Get
            Return My.Forms.frmSaveChangedPhoto
        End Get
    End Property

    Public ReadOnly Property Of_frmSlideShow As frmSlideShow
        Get
            Return My.Forms.frmSlideShow
        End Get
    End Property

    Public ReadOnly Property Of_frmSubject As frmSubject
        Get
            Return My.Forms.frmSubject
        End Get
    End Property

    Public ReadOnly Property Of_frmViewerLarge As frmViewerLarge
        Get
            Return My.Forms.frmViewerLarge
        End Get
    End Property

    Public ReadOnly Property Of_frmViewerSmall As frmViewerSmall
        Get
            Return My.Forms.frmViewerSmall
        End Get
    End Property

End Module
