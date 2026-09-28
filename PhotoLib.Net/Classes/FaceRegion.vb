' A face found in (or boxed by hand on) a photo: a row of FaceRegion in iPhoto.mdb, with the name of
' the person it is assigned to. New in the .NET port (face recognition).
Public Class FaceRegion

    ''' <summary>FaceRegion.State.</summary>
    Public Enum enumFaceState
        fsUnnamed = 0       ' found, nobody assigned
        fsAuto = 1          ' assigned by the program (sure match)
        fsSuggested = 2     ' the program thinks it is PersonID; the user hasn't confirmed
        fsConfirmed = 3     ' named / confirmed by the user
        fsManual = 4        ' boxed by hand, nobody assigned yet
        fsStranger = 8      ' the user doesn't know who it is (我不認識): kept, but never grouped or matched again
        fsNotFace = 9       ' the user said it isn't a face: never shown again
    End Enum

    Public FaceID As Integer
    Public FileName As String = ""
    ''' <summary>Fractions (0..1) of the photo's width / height.</summary>
    Public Box As RectangleF
    Public Score As Single
    Public Feature As Single()
    ''' <summary>0 = nobody.</summary>
    Public PersonID As Integer
    Public PersonName As String = ""
    Public State As enumFaceState
    Public Similarity As Single
    Public ShotYear As Integer

    ''' <summary>True when the face is assigned to a person (confirmed, from the people field, or matched
    ''' by the program).</summary>
    Public ReadOnly Property IsNamed As Boolean
        Get
            Return PersonID <> 0 AndAlso (State = enumFaceState.fsConfirmed OrElse State = enumFaceState.fsAuto)
        End Get
    End Property

    ''' <summary>A seed: the only face of a photo whose people field holds only this name (Similarity 1).</summary>
    Public ReadOnly Property IsSeed As Boolean
        Get
            Return State = enumFaceState.fsAuto AndAlso Similarity >= 0.999F
        End Get
    End Property

    ''' <summary>The name is certain: confirmed by the user, or a seed.</summary>
    Public ReadOnly Property IsCertain As Boolean
        Get
            Return PersonID <> 0 AndAlso (State = enumFaceState.fsConfirmed OrElse IsSeed)
        End Get
    End Property

    ''' <summary>The program put a name on the face that the user should confirm (✓) or reject (✕):
    ''' its own match, or a suggestion.</summary>
    Public ReadOnly Property NeedsConfirm As Boolean
        Get
            Return PersonID <> 0 AndAlso ((State = enumFaceState.fsAuto AndAlso Not IsSeed) OrElse State = enumFaceState.fsSuggested)
        End Get
    End Property

End Class
