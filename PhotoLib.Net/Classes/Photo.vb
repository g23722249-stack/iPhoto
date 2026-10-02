' Port of 電子相簿\Lib\Class\Photo.cls: one photo/video file plus its "<name>.Exif" side file
' (an ini holding date, time, title, people, place, ranking, keywords, remark).
'
' Construct was Friend in VB6, which meant "the whole project" -- the Lib classes were compiled into
' each app. It is Public here because the app (iPhoto.Net) is a different assembly.
Public Class Photo

    Private m_szExifFile As String
    Private m_szFileDesc As String
    Private m_szFolderDesc As String
    Private m_szFileName As String
    Private m_szFileBaseName As String
    Private m_szExtensionName As String
    Private m_enumPhotoMediaType As enumPhotoMediaType
    Private m_bCreate As Boolean = False

    ''' <summary>Copies the file to "Restore\" next to it (once), so edits can be reverted.</summary>
    Public Function Backup() As Boolean
        Dim fs As New Carbon.FileSystem
        If Not fs.FileExists(RestoreFile) Then
            If Not fs.CreateFolder(m_szFolderDesc & "\Restore") Then Return False
            If Not fs.CopyFile(m_szFileDesc, RestoreFile, True) Then Return False
        End If
        Return True
    End Function

    Public ReadOnly Property Name As String
        Get
            Return m_szFileName
        End Get
    End Property

    Public ReadOnly Property FileDesc As String
        Get
            If Not CheckCreate() Then Return Nothing
            Return m_szFileDesc
        End Get
    End Property

    Public ReadOnly Property RestoreFile As String
        Get
            Return m_szFolderDesc & "\Restore\" & m_szFileName
        End Get
    End Property

    Public ReadOnly Property FolderDesc As String
        Get
            If Not CheckCreate() Then Return Nothing
            Return m_szFolderDesc
        End Get
    End Property

    Public ReadOnly Property MediaType As enumPhotoMediaType
        Get
            If Not CheckCreate() Then Return Nothing
            Return m_enumPhotoMediaType
        End Get
    End Property

    ''' <summary>A field of the .Exif side file. Setting peDate / peTime also writes [Create].</summary>
    Public Property Exif(ByVal Mode As enumPhotoExif) As String
        Get
            If Not CheckCreate() Then Return Nothing
            Dim ini As New Carbon.IniFile With {.FileName = m_szExifFile}
            Select Case Mode
                Case enumPhotoExif.peDate : Return GetClearText(ini.SimpleGetValue("Exif", "Date"))
                Case enumPhotoExif.peTime : Return GetClearText(ini.SimpleGetValue("Exif", "Time"))
                Case enumPhotoExif.peTitle : Return GetClearText(ini.SimpleGetValue("Exif", "Title"))
                Case enumPhotoExif.peCharacter : Return GetClearText(ini.SimpleGetValue("Exif", "Character"))
                Case enumPhotoExif.peSpot : Return GetClearText(ini.SimpleGetValue("Exif", "Spot"))
                Case enumPhotoExif.peIcon : Return "ico" & m_szExtensionName
                Case enumPhotoExif.peRanking : Return GetClearText(ini.SimpleGetValue("Exif", "Ranking"))
                Case enumPhotoExif.peKeyWord : Return GetClearText(ini.SimpleGetValue("Exif", "KeyWord"))
                Case enumPhotoExif.peRemark : Return GetClearText(ini.SimpleGetValue("Exif", "Remark").Replace(vbCrLf, vbTab))
                Case enumPhotoExif.peGps : Return GetClearText(ini.SimpleGetValue("Exif", "GPS"))
                Case enumPhotoExif.peCountry : Return GetClearText(ini.SimpleGetValue("Exif", "Country"))
                Case enumPhotoExif.peCity : Return GetClearText(ini.SimpleGetValue("Exif", "City"))
                Case enumPhotoExif.peTown : Return GetClearText(ini.SimpleGetValue("Exif", "Town"))
            End Select
            Return ""
        End Get
        Set(value As String)
            If Not CheckCreate() Then Return
            Dim ini As New Carbon.IniFile With {.FileName = m_szExifFile}
            Select Case Mode
                Case enumPhotoExif.peDate
                    ini.SimpleSetValue("Exif", "Date", value)
                    ini.SimpleSetValue("Create", "Date", value)
                Case enumPhotoExif.peTime
                    ini.SimpleSetValue("Exif", "Time", value)
                    ini.SimpleSetValue("Create", "Time", value)
                Case enumPhotoExif.peTitle : ini.SimpleSetValue("Exif", "Title", value)
                Case enumPhotoExif.peCharacter : ini.SimpleSetValue("Exif", "Character", value)
                Case enumPhotoExif.peSpot : ini.SimpleSetValue("Exif", "Spot", value)
                Case enumPhotoExif.peIcon
                Case enumPhotoExif.peRanking : ini.SimpleSetValue("Exif", "Ranking", value)
                Case enumPhotoExif.peKeyWord : ini.SimpleSetValue("Exif", "KeyWord", value)
                Case enumPhotoExif.peRemark : ini.SimpleSetValue("Exif", "Remark", If(value, "").Replace(vbCrLf, vbTab))
                Case enumPhotoExif.peGps : ini.SimpleSetValue("Exif", "GPS", If(value, ""))
                Case enumPhotoExif.peCountry : ini.SimpleSetValue("Exif", "Country", If(value, ""))
                Case enumPhotoExif.peCity : ini.SimpleSetValue("Exif", "City", If(value, ""))
                Case enumPhotoExif.peTown : ini.SimpleSetValue("Exif", "Town", If(value, ""))
            End Select
        End Set
    End Property

    Public Sub Construct(ByVal szFileDesc As String)
        Dim fs As New Carbon.FileSystem
        m_szFileDesc = szFileDesc
        m_szFolderDesc = fs.AnalyseFile(fsParentFolderName, szFileDesc)
        m_szFileName = fs.AnalyseFile(fsFileName, szFileDesc)
        m_szFileBaseName = fs.AnalyseFile(fsBaseName, szFileDesc)
        Dim ext As String = fs.AnalyseFile(fsExtensionName, szFileDesc).Trim()
        m_szExtensionName = If(ext.Length = 0, "", ext.Substring(0, 1).ToUpperInvariant() & ext.Substring(1).ToLowerInvariant())
        m_enumPhotoMediaType = GetMediaType(fs, m_szExtensionName)
        m_szExifFile = m_szFolderDesc & "\" & m_szFileBaseName & "." & gc_strExifPattern
        m_bCreate = True
    End Sub

    Private Function CheckCreate() As Boolean
        If Not m_bCreate Then MsgBox("Photo 物件尚未執行 Create...", MsgBoxStyle.Critical, "錯誤")
        Return m_bCreate
    End Function

End Class
