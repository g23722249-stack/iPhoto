' Lets ported iPhoto code keep using the default instances of the forms that moved to PhotoLib.Net
' (frmMsgBox.ShowCriticalMessage ..., frmBrowserFile.GetFile ...). Names in the project's own namespace
' win over imported ones, so frmMsgBox.X binds here, while "New frmMsgBox" / "As frmMsgBox" still mean the
' PhotoLib type. Each property returns PhotoLib's single default instance (PhotoLib.DefaultInstances).
Friend Module LibForms

    Friend ReadOnly Property frmBrowserFile As PhotoLib.frmBrowserFile
        Get
            Return PhotoLib.DefaultInstances.Of_frmBrowserFile
        End Get
    End Property

    Friend ReadOnly Property frmBrowserFolder As PhotoLib.frmBrowserFolder
        Get
            Return PhotoLib.DefaultInstances.Of_frmBrowserFolder
        End Get
    End Property

    Friend ReadOnly Property frmInputString As PhotoLib.frmInputString
        Get
            Return PhotoLib.DefaultInstances.Of_frmInputString
        End Get
    End Property

    Friend ReadOnly Property frmLoading As PhotoLib.frmLoading
        Get
            Return PhotoLib.DefaultInstances.Of_frmLoading
        End Get
    End Property

    Friend ReadOnly Property frmLogo As PhotoLib.frmLogo
        Get
            Return PhotoLib.DefaultInstances.Of_frmLogo
        End Get
    End Property

    Friend ReadOnly Property frmMsgBox As PhotoLib.frmMsgBox
        Get
            Return PhotoLib.DefaultInstances.Of_frmMsgBox
        End Get
    End Property

    Friend ReadOnly Property frmQueryMsgBox As PhotoLib.frmQueryMsgBox
        Get
            Return PhotoLib.DefaultInstances.Of_frmQueryMsgBox
        End Get
    End Property

    Friend ReadOnly Property frmResAlbum As PhotoLib.frmResAlbum
        Get
            Return PhotoLib.DefaultInstances.Of_frmResAlbum
        End Get
    End Property

    Friend ReadOnly Property frmSaveChangedPhoto As PhotoLib.frmSaveChangedPhoto
        Get
            Return PhotoLib.DefaultInstances.Of_frmSaveChangedPhoto
        End Get
    End Property

    Friend ReadOnly Property frmSlideShow As PhotoLib.frmSlideShow
        Get
            Return PhotoLib.DefaultInstances.Of_frmSlideShow
        End Get
    End Property

    Friend ReadOnly Property frmSubject As PhotoLib.frmSubject
        Get
            Return PhotoLib.DefaultInstances.Of_frmSubject
        End Get
    End Property

    Friend ReadOnly Property frmViewerLarge As PhotoLib.frmViewerLarge
        Get
            Return PhotoLib.DefaultInstances.Of_frmViewerLarge
        End Get
    End Property

    Friend ReadOnly Property frmViewerSmall As PhotoLib.frmViewerSmall
        Get
            Return PhotoLib.DefaultInstances.Of_frmViewerSmall
        End Get
    End Property

End Module
