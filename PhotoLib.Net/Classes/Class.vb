' Port of 電子相簿\Lib\Class\Class.cls: one album folder ("類別") -- its photos and its Note.Ini.
' Shared logic with Book is in PhotoSet.
'
' Fixed from VB6: CreatePhotos assigned the file NAME to the integer index when a file was already
' listed (a type-mismatch error in VB6); PhotoSet returns the real index, as Book.cls already did.
Public Class [Class]
    Inherits PhotoSet

    Private m_szAlbumPath As String = ""
    Private m_szNoteIni As String = ""

    Protected Overrides ReadOnly Property NoteFile As String
        Get
            Return m_szNoteIni
        End Get
    End Property

    Public ReadOnly Property Path As String
        Get
            Return m_szAlbumPath
        End Get
    End Property

    Public Overrides ReadOnly Property Key As String
        Get
            Return m_szAlbumPath
        End Get
    End Property

    Public Sub Construct(ByVal szPath As String)
        szPath = If(szPath, "").Trim()
        If Not String.Equals(m_szAlbumPath, szPath, StringComparison.OrdinalIgnoreCase) Then
            m_szName = ExactFolderName(szPath)
            m_szAlbumPath = szPath
            m_szNoteIni = szPath & "\" & gc_strNote
            m_bReadOnly = New Carbon.FileSystem().AttributeFolder(Carbon.AttributeConstants.attReadonly, m_szAlbumPath)
        End If
        m_bConstruct = True
        If m_bolLoad Then Load()
    End Sub

    ''' <summary>Loads the folder's photos and videos, oldest first (by .Exif date, else file time).
    ''' Only the first call reads the folder. VB6 never summed the file sizes here (the line was
    ''' commented out), so FileLength stays 0.</summary>
    Public Overrides Function Load() As Boolean
        If Not CheckCreate() Then Return False
        If Not m_bolLoad Then
            m_lpPhoto.Clear()
            Dim files() As String = Nothing
            Dim count As Integer = ExactFiles(m_szAlbumPath, GetPhotoPatterns(), files, True)
            count = SortPhotoFileByCreateDateTime(enumSortOrder.Ascending, files, count)
            For i = 0 To count - 1
                CreatePhotos(files(i))
            Next
            If m_lpPhoto.Count > 0 Then
                m_bolLoad = True
                CalcFileLength(0, m_dblFileLength, m_strFileLengthUnit)
            Else
                m_dblFileLength = 0
                m_strFileLengthUnit = ""
            End If
        End If
        Return True
    End Function

End Class
