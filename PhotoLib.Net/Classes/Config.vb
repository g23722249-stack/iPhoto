Imports System.IO
Imports System.Runtime.InteropServices

' Port of 電子相簿\Lib\Class\Config.cls: iPhoto.Ini (album roots, photo-book roots, sounds, slide
' show, plug-in applications, attached files, privileges) plus the app-wide read-only switch.
'
' Differences from VB6 (bug fixes, each noted where it happens):
'   - LoadSlideParameter read "WaitTime" but Save writes "ShowTime", so the setting never came back;
'     WaitTime is still read first, ShowTime is the fallback.
'   - Sound(snOpenDialogBox) returned the enum number (20) instead of the file.
'   - iPhoto.Ini is read and written in the system ANSI code page, as VB6 did.
Public Class Config

    Private Const mc_szProfile As String = "iPhoto.Ini"

    Public Enum enumSlide
        sliShowTimes = 0
        sliMusicPath = 1
    End Enum

    Public Enum enumAttachedFile
        filAddressBook = 0
        filKeyWord = 1
        filDatabase = 2
        filFaceCover = 3
    End Enum

    Public Enum enumPrivilege
        privDeleteAlbumPhotos = 0
    End Enum

    Public Enum enumSound
        snButtonEnter = 0
        snButtonExit = 1
        snButtonClick = 2
        snOpenDialogBox = 20
        snImportFinish = 90
        snExportFinish = 91
    End Enum

    Public Enum enumApplication
        AppPrint = 0
        AppImageEdit = 1
        AppVideoEdit = 2
        AppMail = 3
        AppHomePage = 4
    End Enum

    <DllImport("winmm.dll", EntryPoint:="PlaySoundW", CharSet:=CharSet.Unicode)>
    Private Shared Function PlaySoundApi(ByVal pszSound As String, ByVal hmod As IntPtr, ByVal fdwSound As Integer) As Boolean
    End Function
    Private Const SND_ASYNC As Integer = &H1
    Private Const SND_NODEFAULT As Integer = &H2
    Private Const SND_FILENAME As Integer = &H20000

    Private ReadOnly m_lpFileSystem As New Carbon.FileSystem
    Private ReadOnly m_lpProfile As New Carbon.IniFile

    Private m_strTempPath As String = ""
    Private m_lpAlbum As New List(Of String)
    Private m_lpFavorite As New List(Of String)

    Private m_strButtonEnter As String = "", m_strButtonExit As String = "", m_strButtonClick As String = ""
    Private m_strImportFinish As String = "", m_strExportFinish As String = "", m_strOpenDialogBox As String = ""
    Private m_strAppPrint As String = "", m_strAppImageEdit As String = "", m_strAppVideoEdit As String = ""
    Private m_strAppMail As String = "", m_strAppHomePage As String = ""

    Private m_strFontName As String = ""
    Private m_strStyle As String = ""
    Private m_bolScreenSingle As Boolean   ' [Interface] ScreenMode=Single (two screens: the user chose one)

    Private m_intSlideShowTimes As Integer
    Private m_strSlideMusicPath As String = ""
    Private m_strProfile As String = ""

    Private m_strAddressBook As String = "", m_strKeyWord As String = "", m_strDataBase As String = "", m_strFaceCover As String = ""
    Private m_bolPrivDeleteAlbumPhotos As Boolean
    ' [Faces] (face recognition, new in the .NET port)
    Private m_bolFaceEnabled As Boolean = True, m_bolFaceAutoScan As Boolean = True, m_bolFaceWriteNames As Boolean = True
    Private m_intFaceStrictness As Integer = 1
    ' [Import] Style: New = 輸入 opens frmImportStudio (default), Old = the ported frmImport
    Private m_bolImportNewStyle As Boolean = True
    ' [Viewer] PageTurn: how 全圖瀏覽 changes photos (frmViewerLarge, PageTurnView); PageTurnSpeed: 0 快 1 中 2 慢
    Public Enum enumPageTurn
        ptAlternate = 0   ' by the clock: even second 整頁翻, odd second 翻頁角
        ptBook = 1
        ptCorner = 2
        ptOff = 3
    End Enum
    Private m_enumPageTurn As enumPageTurn = enumPageTurn.ptAlternate
    Private m_intPageTurnSpeed As Integer = 1
    ' [Place] where the place names filled from a photo's GPS come from (PlaceNames, new in the .NET port)
    Private m_enumPlaceSource As PlaceNames.enumPlaceSource = PlaceNames.enumPlaceSource.psOffline
    Private m_bolReadOnly As Boolean
    Private ReadOnly m_strAppPath As String

    ''' <summary>VB6 Class_Initialize: the app folder must be writable (else the whole app is read-only,
    ''' e.g. running from a CD); the environment can force read-only (iPhotoReadOnly=Y) or point the
    ''' $LOCAL$ root at a test tree (iPhotoTest=Y).</summary>
    Public Sub New()
        ' fully qualified: this class has its own Application(...) property
        Dim appPath As String = System.Windows.Forms.Application.StartupPath
        Dim probe As String = Path.Combine(appPath, "Test.Txt")
        Try
            m_lpFileSystem.DeleteFile(probe)
            File.AppendAllText(probe, "xx" & vbCrLf, AnsiText.Encoding)
            m_lpFileSystem.DeleteFile(probe)
        Catch
            m_bolReadOnly = True
        End Try
        m_strAppPath = If(String.Equals(Environment.GetEnvironmentVariable("iPhotoTest"), "Y", StringComparison.OrdinalIgnoreCase), "O:\生活剪輯", appPath)
        If String.Equals(Environment.GetEnvironmentVariable("iPhotoReadOnly"), "Y", StringComparison.OrdinalIgnoreCase) Then m_bolReadOnly = True
    End Sub

    Public ReadOnly Property [ReadOnly] As Boolean
        Get
            Return m_bolReadOnly
        End Get
    End Property

    Public ReadOnly Property Profile As String
        Get
            Return m_strProfile
        End Get
    End Property

    '==================================================================================================
    ' Construct / load
    '==================================================================================================
    ''' <summary>Reads &lt;szPath&gt;\iPhoto.Ini (on the developer machine WS-CYL-DELL:
    ''' &lt;app&gt;\WS-CYL-DELL\iPhoto.Ini, as VB6 did).</summary>
    Public Sub Construct(ByVal szPath As String)
        If String.Equals(Environment.MachineName, "WS-CYL-DELL", StringComparison.OrdinalIgnoreCase) Then
            m_strProfile = m_strAppPath & "\" & Environment.MachineName & "\" & mc_szProfile
        Else
            m_strProfile = szPath & "\" & mc_szProfile
        End If
        Refresh()
    End Sub

    Public Sub Refresh()
        Clear()
        If Not m_lpFileSystem.FileExists(m_strProfile) Then Return
        LoadInterfaceParameter()
        LoadSoundParameter()
        LoadAlbumFolder()
        LoadFavoriteFolder()
        m_strTempPath = GetClearText(m_lpProfile.SimpleGetValue("Temp", "Path"))
        LoadSlideParameter()
        LoadApplicationParameter()
        LoadAttachedFiles()
        LoadPrivilege()
        LoadFaceParameter()
        LoadPlaceParameter()
        LoadImportParameter()
    End Sub

    Private Sub Clear()
        m_strImportFinish = ""
        m_lpAlbum.Clear()
        m_lpFavorite.Clear()
        m_lpProfile.FileName = m_strProfile
        m_bolPrivDeleteAlbumPhotos = False
    End Sub

    Private Function Setting(ByVal section As String, ByVal key As String) As String
        Return GetClearText(m_lpProfile.SimpleGetValue(section, key))
    End Function

    ''' <summary>"$LOCAL$\生活剪輯\..." is relative to the app folder (so a copied disc keeps working).</summary>
    Private Function RebuildPathFile(ByVal strPath As String) As String
        If strPath.StartsWith("$LOCAL$", StringComparison.OrdinalIgnoreCase) Then Return m_strAppPath & strPath.Substring(7)
        Return strPath
    End Function

    Private Sub LoadInterfaceParameter()
        m_strFontName = Setting("Interface", "FontName")
        m_bolScreenSingle = String.Equals(GetClearText(m_lpProfile.SimpleGetValue("Interface", "ScreenMode")), "Single", StringComparison.OrdinalIgnoreCase)
        If m_strFontName.Trim().Length = 0 Then m_strFontName = "華康細圓體"
        If Not CheckSystemFont() Then m_strFontName = "標楷體"
        If Not CheckSystemFont() Then m_strFontName = "細明體"
        m_strStyle = Setting("Interface", "Style")
        If m_strStyle.Trim().Length = 0 Then m_strStyle = "0"
    End Sub

    Private Sub LoadSoundParameter()
        m_strButtonEnter = RebuildPathFile(Setting("Sound", "ButtonEnter"))
        m_strButtonExit = RebuildPathFile(Setting("Sound", "ButtonExit"))
        m_strButtonClick = RebuildPathFile(Setting("Sound", "ButtonClick"))
        m_strImportFinish = RebuildPathFile(Setting("Sound", "ImportFinish"))
        m_strExportFinish = RebuildPathFile(Setting("Sound", "ExportFinish"))
        m_strOpenDialogBox = RebuildPathFile(Setting("Sound", "OpenDialogBox"))
    End Sub

    Private Sub LoadSlideParameter()
        ' Save writes "ShowTime" but VB6 read "WaitTime" (so the value never came back): read both
        Dim t As String = Setting("Slide", "WaitTime")
        If Val(t) = 0 Then t = Setting("Slide", "ShowTime")
        m_intSlideShowTimes = CInt(Val(t))
        If m_intSlideShowTimes = 0 Then m_intSlideShowTimes = 3
        m_strSlideMusicPath = RebuildPathFile(Setting("Slide", "MusicPath"))
    End Sub

    Private Sub LoadApplicationParameter()
        m_strAppPrint = Setting("Application", "Print")
        m_strAppImageEdit = Setting("Application", "ImageEdit")
        m_strAppVideoEdit = Setting("Application", "VideoEdit")
        m_strAppMail = Setting("Application", "Mail")
        m_strAppHomePage = Setting("Application", "HomePage")
    End Sub

    Private Sub LoadAttachedFiles()
        m_strAddressBook = RebuildPathFile(Setting("Attached", "AddressBook"))
        m_strKeyWord = RebuildPathFile(Setting("Attached", "KeyWordBook"))
        m_strDataBase = RebuildPathFile(Setting("Attached", "DataBase"))
        m_strFaceCover = RebuildPathFile(Setting("Attached", "FaceCover"))
    End Sub

    ''' <summary>[Faces]: Enabled / AutoScan / WriteNames (Y/N, default Y), Strictness 0..2 (default 1).</summary>
    Private Sub LoadFaceParameter()
        m_bolFaceEnabled = (Setting("Faces", "Enabled") <> "N")
        m_bolFaceAutoScan = (Setting("Faces", "AutoScan") <> "N")
        m_bolFaceWriteNames = (Setting("Faces", "WriteNames") <> "N")
        Dim s As String = Setting("Faces", "Strictness")
        m_intFaceStrictness = If(s = "", 1, Math.Max(0, Math.Min(2, CInt(Val(s)))))
    End Sub

    ''' <summary>[Import] Style: New (default) / Old.</summary>
    Private Sub LoadImportParameter()
        m_bolImportNewStyle = Not String.Equals(Setting("Import", "Style"), "Old", StringComparison.OrdinalIgnoreCase)
        ' [Viewer] PageTurn=Alternate / Book / Corner / Off, PageTurnSpeed=0..2
        Select Case If(Setting("Viewer", "PageTurn"), "").ToUpperInvariant()
            Case "BOOK" : m_enumPageTurn = enumPageTurn.ptBook
            Case "CORNER" : m_enumPageTurn = enumPageTurn.ptCorner
            Case "OFF" : m_enumPageTurn = enumPageTurn.ptOff
            Case Else : m_enumPageTurn = enumPageTurn.ptAlternate
        End Select
        Dim s As String = Setting("Viewer", "PageTurnSpeed")
        m_intPageTurnSpeed = If(s = "", 1, Math.Max(0, Math.Min(2, CInt(Val(s)))))
        ' [Viewer] StripDock=Bottom / Top / Left / Right: the 地點 thumbnail strip of 全圖瀏覽
        Select Case If(Setting("Viewer", "StripDock"), "").ToUpperInvariant()
            Case "TOP" : m_enumStripDock = DockStyle.Top
            Case "LEFT" : m_enumStripDock = DockStyle.Left
            Case "RIGHT" : m_enumStripDock = DockStyle.Right
            Case Else : m_enumStripDock = DockStyle.Bottom
        End Select
        ' [Viewer] StripDockSingle=Top (default) / Bottom / Left / Right: the strip of 全圖 in the main window (單螢幕)
        Select Case If(Setting("Viewer", "StripDockSingle"), "").ToUpperInvariant()
            Case "BOTTOM" : m_enumStripDockSingle = DockStyle.Bottom
            Case "LEFT" : m_enumStripDockSingle = DockStyle.Left
            Case "RIGHT" : m_enumStripDockSingle = DockStyle.Right
            Case Else : m_enumStripDockSingle = DockStyle.Top
        End Select
        ' [Viewer] StripOrder=Oldest (default) / Newest: the order of the 地點 strip
        m_bolStripNewestFirst = String.Equals(Setting("Viewer", "StripOrder"), "Newest", StringComparison.OrdinalIgnoreCase)
    End Sub

    Private m_bolStripNewestFirst As Boolean
    Private m_enumStripDockSingle As DockStyle = DockStyle.Top

    ''' <summary>Where the strip of 全圖 in the main window (單螢幕) sits; its own place, apart from the
    ''' 地點 strip (ViewerStripDock). Written to iPhoto.Ini at once.</summary>
    Public Property ViewerStripDockSingle As DockStyle
        Get
            Return m_enumStripDockSingle
        End Get
        Set(value As DockStyle)
            If value <> DockStyle.Bottom AndAlso value <> DockStyle.Left AndAlso value <> DockStyle.Right Then value = DockStyle.Top
            m_enumStripDockSingle = value
            If Not m_bolReadOnly AndAlso m_lpProfile.FileName <> "" Then m_lpProfile.SimpleSetValue("Viewer", "StripDockSingle", value.ToString())
        End Set
    End Property

    ''' <summary>The 地點 strip of 全圖瀏覽: newest day first (False: oldest first, the default).</summary>
    Public Property ViewerStripNewestFirst As Boolean
        Get
            Return m_bolStripNewestFirst
        End Get
        Set(value As Boolean)
            m_bolStripNewestFirst = value
        End Set
    End Property

    Private m_enumStripDock As DockStyle = DockStyle.Bottom

    ''' <summary>Where 全圖瀏覽's 地點 thumbnail strip sits (Top / Bottom / Left / Right). Written to
    ''' iPhoto.Ini at once: it is changed by dragging the strip, not in 設定.</summary>
    Public Property ViewerStripDock As DockStyle
        Get
            Return m_enumStripDock
        End Get
        Set(value As DockStyle)
            If value <> DockStyle.Top AndAlso value <> DockStyle.Left AndAlso value <> DockStyle.Right Then value = DockStyle.Bottom
            m_enumStripDock = value
            If Not m_bolReadOnly AndAlso m_lpProfile.FileName <> "" Then m_lpProfile.SimpleSetValue("Viewer", "StripDock", value.ToString())
        End Set
    End Property

    ''' <summary>[Place] Geocoder: Offline (default) / Online.</summary>
    Private Sub LoadPlaceParameter()
        m_enumPlaceSource = If(String.Equals(Setting("Place", "Geocoder"), "Online", StringComparison.OrdinalIgnoreCase),
                               PlaceNames.enumPlaceSource.psOnline, PlaceNames.enumPlaceSource.psOffline)
        ' [Place] MapMode=Online (default) / Offline; MapCacheMB: the map tiles kept on this computer
        m_bolMapOffline = String.Equals(Setting("Place", "MapMode"), "Offline", StringComparison.OrdinalIgnoreCase)
        ' [Place] RebuildFill=Y (default) / N: 重建資料庫索引 also fills blank GPS / 地點 into the .Exif
        m_bolRebuildFill = Not String.Equals(Setting("Place", "RebuildFill"), "N", StringComparison.OrdinalIgnoreCase)
        Dim mb As Integer
        m_intMapCacheMB = If(Integer.TryParse(Setting("Place", "MapCacheMB"), mb) AndAlso mb >= 50, mb, 500)
    End Sub

    Private m_bolMapOffline As Boolean
    Private m_bolRebuildFill As Boolean = True

    ''' <summary>重建資料庫索引: True = first fill each photo's blank GPS / 地點 / Country / City / Town into its
    ''' .Exif (from the picture, a .Exif backup first); False = only the index, no .Exif written.</summary>
    Public Property RebuildFillPlaces As Boolean
        Get
            Return m_bolRebuildFill
        End Get
        Set(value As Boolean)
            m_bolRebuildFill = value
        End Set
    End Property
    Private m_intMapCacheMB As Integer = 500

    ''' <summary>地點 / 輸入地點 maps: True = offline (the outline map only, no internet: OpenStreetMap's
    ''' tiles may not be used offline); False = online OpenStreetMap through a tile cache (the default).</summary>
    Public Property MapOffline As Boolean
        Get
            Return m_bolMapOffline
        End Get
        Set(value As Boolean)
            m_bolMapOffline = value
        End Set
    End Property

    ''' <summary>The most the map tile cache may hold, in MB (default 500).</summary>
    Public Property MapCacheMB As Integer
        Get
            Return m_intMapCacheMB
        End Get
        Set(value As Integer)
            m_intMapCacheMB = Math.Max(50, value)
        End Set
    End Property

    Private Sub LoadPrivilege()
        m_bolPrivDeleteAlbumPhotos = (Setting("Privilege", "DeleteAlbumPhotos") = "Y")
        If m_bolReadOnly Then m_bolPrivDeleteAlbumPhotos = False
    End Sub

    ''' <summary>[Album] Path= lines (one per root). In read-only mode roots that don't exist are skipped.</summary>
    Private Sub LoadAlbumFolder()
        m_lpAlbum.Clear()
        Dim paths() As String = Nothing
        Dim count As Integer = LoadProfileSectionValues(m_strProfile, "Album", "Path", paths)
        For i = 0 To count - 1
            Dim folder As String = RebuildPathFile(paths(i).Trim())
            If m_bolReadOnly AndAlso Not m_lpFileSystem.FolderExists(folder) Then Continue For
            m_lpAlbum.Add(folder)
        Next
    End Sub

    Private Sub LoadFavoriteFolder()
        m_lpFavorite.Clear()
        Dim paths() As String = Nothing
        Dim count As Integer = LoadProfileSectionValues(m_strProfile, "Favorite", "Path", paths)
        For i = 0 To count - 1
            m_lpFavorite.Add(RebuildPathFile(paths(i).Trim()))
        Next
    End Sub

    Private Function CheckSystemFont() As Boolean
        For Each ff In FontFamily.Families
            If ff.Name = m_strFontName Then Return True
        Next
        Return False
    End Function

    '==================================================================================================
    ' Save
    '==================================================================================================
    ''' <summary>Rewrites the whole iPhoto.Ini (ANSI), same layout and comments as VB6.</summary>
    Public Function Save() As Boolean
        If Not m_lpFileSystem.DeleteFile(m_lpProfile.FileName) Then
            MsgBox("刪除 iPhoto 設定檔 " & m_lpProfile.FileName & " 失敗...", MsgBoxStyle.Critical, "錯誤")
            Return False
        End If
        Using w As New StreamWriter(m_lpProfile.FileName, False, AnsiText.Encoding)
            w.WriteLine("//相片庫")
            w.WriteLine("[Album]")
            For Each p In m_lpAlbum
                w.WriteLine("Path=" & p.Trim())
            Next
            w.WriteLine(" ")
            w.WriteLine("//攝影集")
            w.WriteLine("[Favorite]")
            For Each p In m_lpFavorite
                w.WriteLine("Path=" & p.Trim())
            Next
            w.WriteLine(" ")
            w.WriteLine("//By 類別 說明備註檔")
            w.WriteLine("[Class]")
            w.WriteLine("NoteFile=" & gc_strNote)
            w.WriteLine(" ")
            w.WriteLine("//外觀")
            w.WriteLine("[Interface]")
            w.WriteLine("FontName=" & m_strFontName.Trim())
            w.WriteLine("ScreenMode=" & If(m_bolScreenSingle, "Single", "Dual"))
            w.WriteLine(" ")
            w.WriteLine("//音效")
            w.WriteLine("[Sound]")
            w.WriteLine("ButtonEnter=" & m_strButtonEnter)
            w.WriteLine("ButtonExit=" & m_strButtonExit)
            w.WriteLine("ButtonClick=" & m_strButtonClick)
            w.WriteLine("OpenDialogBox=" & m_strOpenDialogBox)
            w.WriteLine("ImportFinish=" & m_strImportFinish)
            w.WriteLine("ExportFinish=" & m_strExportFinish)
            w.WriteLine(" ")
            w.WriteLine("//幻燈片")
            w.WriteLine("[Slide]")
            w.WriteLine("ShowTime=" & m_intSlideShowTimes)
            w.WriteLine("MusicPath=" & m_strSlideMusicPath)
            w.WriteLine(" ")
            w.WriteLine("//外掛應用程式")
            w.WriteLine("[Application]")
            w.WriteLine("Print=" & m_strAppPrint)
            w.WriteLine("ImageEdit=" & m_strAppImageEdit)
            w.WriteLine("VideoEdit=" & m_strAppVideoEdit)
            w.WriteLine("Mail=" & m_strAppMail)
            w.WriteLine("HomePage=" & m_strAppHomePage)
            w.WriteLine(" ")
            w.WriteLine("[Attached]")
            w.WriteLine("KeyWordBook=" & m_strKeyWord)
            w.WriteLine("AddressBook=" & m_strAddressBook)
            w.WriteLine("Database=" & m_strDataBase)
            w.WriteLine("FaceCover=" & m_strFaceCover)
            w.WriteLine(" ")
            w.WriteLine("//權限")
            w.WriteLine("[Privilege]")
            w.WriteLine("DeleteAlbumPhotos=" & If(m_bolPrivDeleteAlbumPhotos, "Y", "N"))
            w.WriteLine(" ")
            w.WriteLine("//面孔（人物辨識）")
            w.WriteLine("[Faces]")
            w.WriteLine("Enabled=" & If(m_bolFaceEnabled, "Y", "N"))
            w.WriteLine("AutoScan=" & If(m_bolFaceAutoScan, "Y", "N"))
            w.WriteLine("WriteNames=" & If(m_bolFaceWriteNames, "Y", "N"))
            w.WriteLine("Strictness=" & m_intFaceStrictness)
            w.WriteLine(" ")
            w.WriteLine("//拍攝地點：座標換算地名的方式（Offline 離線 / Online 線上）")
            w.WriteLine("[Place]")
            w.WriteLine("Geocoder=" & If(m_enumPlaceSource = PlaceNames.enumPlaceSource.psOnline, "Online", "Offline"))
            w.WriteLine("//地圖（Online 線上 OpenStreetMap 街道地圖 / Offline 離線，只用簡易地圖）；MapCacheMB 線上地圖圖磚快取上限")
            w.WriteLine("MapMode=" & If(m_bolMapOffline, "Offline", "Online"))
            w.WriteLine("MapCacheMB=" & m_intMapCacheMB)
            w.WriteLine("//重建資料庫索引時是否補上空白的 GPS／地點（Y 補，會寫 .Exif / N 只重建索引）")
            w.WriteLine("RebuildFill=" & If(m_bolRebuildFill, "Y", "N"))
            w.WriteLine(" ")
            w.WriteLine("//輸入照片的畫面（New 新畫面 / Old 舊畫面）")
            w.WriteLine("[Import]")
            w.WriteLine("Style=" & If(m_bolImportNewStyle, "New", "Old"))
            w.WriteLine(" ")
            w.WriteLine("//全圖瀏覽換照片的翻頁（Alternate 依秒數單雙輪流 / Book 整頁翻 / Corner 翻頁角 / Off 關閉）；速度 0 快 1 中 2 慢")
            w.WriteLine("[Viewer]")
            w.WriteLine("PageTurn=" & {"Alternate", "Book", "Corner", "Off"}(CInt(m_enumPageTurn)))
            w.WriteLine("PageTurnSpeed=" & m_intPageTurnSpeed)
            w.WriteLine("StripDock=" & m_enumStripDock.ToString())
            w.WriteLine("StripOrder=" & If(m_bolStripNewestFirst, "Newest", "Oldest"))
            w.WriteLine("StripDockSingle=" & m_enumStripDockSingle.ToString())
            w.WriteLine(" ")
        End Using
        Return True
    End Function

    '==================================================================================================
    ' Album / photo-book roots
    '==================================================================================================
    Public Sub AddAlbumFolders(ByVal aszFolders() As String)
        m_lpAlbum = If(aszFolders Is Nothing, New List(Of String), New List(Of String)(aszFolders))
    End Sub

    Public ReadOnly Property AlbumCount As Integer
        Get
            Return m_lpAlbum.Count
        End Get
    End Property

    Public ReadOnly Property AlbumPath(ByVal Index As Integer) As String
        Get
            Return m_lpAlbum(Index)
        End Get
    End Property

    Public Sub AddFavoriteFolders(ByVal aszFolders() As String)
        m_lpFavorite = If(aszFolders Is Nothing, New List(Of String), New List(Of String)(aszFolders))
    End Sub

    Public ReadOnly Property FavoriteCount As Integer
        Get
            Return m_lpFavorite.Count
        End Get
    End Property

    Public ReadOnly Property FavoritePath(ByVal Index As Integer) As String
        Get
            Return m_lpFavorite(Index)
        End Get
    End Property

    '==================================================================================================
    ' Settings
    '==================================================================================================
    Public Property Slide(ByVal Mode As enumSlide) As String
        Get
            Return If(Mode = enumSlide.sliShowTimes, m_intSlideShowTimes.ToString(), m_strSlideMusicPath)
        End Get
        Set(value As String)
            If Mode = enumSlide.sliShowTimes Then m_intSlideShowTimes = CInt(Val(value)) Else m_strSlideMusicPath = value
        End Set
    End Property

    Public Property Attached(ByVal Mode As enumAttachedFile) As String
        Get
            Select Case Mode
                Case enumAttachedFile.filAddressBook : Return m_strAddressBook
                Case enumAttachedFile.filKeyWord : Return m_strKeyWord
                Case enumAttachedFile.filDatabase : Return m_strDataBase
                Case enumAttachedFile.filFaceCover : Return m_strFaceCover
            End Select
            Return ""
        End Get
        Set(value As String)
            Select Case Mode
                Case enumAttachedFile.filAddressBook : m_strAddressBook = value
                Case enumAttachedFile.filKeyWord : m_strKeyWord = value
                Case enumAttachedFile.filDatabase : m_strDataBase = value
                Case enumAttachedFile.filFaceCover : m_strFaceCover = value
            End Select
        End Set
    End Property

    Public Property Privilege(ByVal Mode As enumPrivilege) As Boolean
        Get
            Return Mode = enumPrivilege.privDeleteAlbumPhotos AndAlso m_bolPrivDeleteAlbumPhotos
        End Get
        Set(value As Boolean)
            If Mode = enumPrivilege.privDeleteAlbumPhotos Then m_bolPrivDeleteAlbumPhotos = value
        End Set
    End Property

    ''' <summary>Path of a plug-in application; setting a file that doesn't exist clears it (VB6).</summary>
    Public Property Application(ByVal app As enumApplication) As String
        Get
            Select Case app
                Case enumApplication.AppPrint : Return m_strAppPrint
                Case enumApplication.AppImageEdit : Return m_strAppImageEdit
                Case enumApplication.AppVideoEdit : Return m_strAppVideoEdit
                Case enumApplication.AppMail : Return m_strAppMail
                Case enumApplication.AppHomePage : Return m_strAppHomePage
            End Select
            Return ""
        End Get
        Set(value As String)
            Dim v As String = If(m_lpFileSystem.FileExists(value), value, "")
            Select Case app
                Case enumApplication.AppPrint : m_strAppPrint = v
                Case enumApplication.AppImageEdit : m_strAppImageEdit = v
                Case enumApplication.AppVideoEdit : m_strAppVideoEdit = v
                Case enumApplication.AppMail : m_strAppMail = v
                Case enumApplication.AppHomePage : m_strAppHomePage = v
            End Select
        End Set
    End Property

    Public Property Sound(ByVal Mode As enumSound) As String
        Get
            Select Case Mode
                Case enumSound.snButtonEnter : Return m_strButtonEnter
                Case enumSound.snButtonExit : Return m_strButtonExit
                Case enumSound.snButtonClick : Return m_strButtonClick
                Case enumSound.snOpenDialogBox : Return m_strOpenDialogBox   ' VB6 returned the enum value (20) here
                Case enumSound.snImportFinish : Return m_strImportFinish
                Case enumSound.snExportFinish : Return m_strExportFinish
            End Select
            Return ""
        End Get
        Set(value As String)
            Select Case Mode
                Case enumSound.snButtonEnter : m_strButtonEnter = value
                Case enumSound.snButtonExit : m_strButtonExit = value
                Case enumSound.snButtonClick : m_strButtonClick = value
                Case enumSound.snOpenDialogBox : m_strOpenDialogBox = value
                Case enumSound.snImportFinish : m_strImportFinish = value
                Case enumSound.snExportFinish : m_strExportFinish = value
            End Select
        End Set
    End Property

    ''' <summary>Plays the configured .wav/.mid asynchronously; anything else is ignored.</summary>
    Public Sub PlaySound(ByVal Mode As enumSound)
        Dim file As String = Sound(Mode)
        If Not m_lpFileSystem.FileExists(file) Then Return
        Select Case m_lpFileSystem.AnalyseFile(fsExtensionName, file).Trim().ToUpperInvariant()
            Case "WAV", "MID"
                PlaySoundApi(file, IntPtr.Zero, SND_ASYNC Or SND_FILENAME Or SND_NODEFAULT)
        End Select
    End Sub

    ''' <summary>Face recognition on (read when iPhoto starts).</summary>
    Public Property FaceEnabled As Boolean
        Get
            Return m_bolFaceEnabled
        End Get
        Set(value As Boolean)
            m_bolFaceEnabled = value
        End Set
    End Property

    ''' <summary>Analyse new / changed photos in the background when iPhoto starts.</summary>
    Public Property FaceAutoScan As Boolean
        Get
            Return m_bolFaceAutoScan
        End Get
        Set(value As Boolean)
            m_bolFaceAutoScan = value
        End Set
    End Property

    ''' <summary>Names the user confirms go into the photos' people fields.</summary>
    Public Property FaceWriteNames As Boolean
        Get
            Return m_bolFaceWriteNames
        End Get
        Set(value As Boolean)
            m_bolFaceWriteNames = value
        End Set
    End Property

    ''' <summary>How sure the program must be to put a name on a face by itself: 0 寬鬆, 1 平衡, 2 嚴格.</summary>
    Public Property FaceStrictness As Integer
        Get
            Return m_intFaceStrictness
        End Get
        Set(value As Integer)
            m_intFaceStrictness = Math.Max(0, Math.Min(2, value))
        End Set
    End Property

    ''' <summary>How 全圖瀏覽 turns to the next / prior photo.</summary>
    Public Property ViewerPageTurn As enumPageTurn
        Get
            Return m_enumPageTurn
        End Get
        Set(value As enumPageTurn)
            m_enumPageTurn = value
        End Set
    End Property

    ''' <summary>0 快 / 1 中 / 2 慢.</summary>
    Public Property ViewerPageTurnSpeed As Integer
        Get
            Return m_intPageTurnSpeed
        End Get
        Set(value As Integer)
            m_intPageTurnSpeed = Math.Max(0, Math.Min(2, value))
        End Set
    End Property

    ''' <summary>Milliseconds of one page turn (快 350 / 中 550 / 慢 900, as in the demo).</summary>
    Public ReadOnly Property ViewerPageTurnMs As Integer
        Get
            Return {350, 550, 900}(m_intPageTurnSpeed)
        End Get
    End Property

    ''' <summary>輸入 opens the new import window (frmImportStudio); False = the ported frmImport.</summary>
    Public Property ImportNewStyle As Boolean
        Get
            Return m_bolImportNewStyle
        End Get
        Set(value As Boolean)
            m_bolImportNewStyle = value
        End Set
    End Property

    ''' <summary>Where the 地點 filled from a photo's GPS comes from (PlaceNames).</summary>
    Public Property PlaceSource As PlaceNames.enumPlaceSource
        Get
            Return m_enumPlaceSource
        End Get
        Set(value As PlaceNames.enumPlaceSource)
            m_enumPlaceSource = value
        End Set
    End Property

    Public Property InterfaceFontName As String
        Get
            Return m_strFontName
        End Get
        Set(value As String)
            m_strFontName = value
        End Set
    End Property

    ''' <summary>設定 › 螢幕: one screen even when two are there (main window with the 全圖 inside it).
    ''' False = 雙螢幕 (the default); with only one screen iPhoto is single-screen whatever this says.
    ''' Takes effect at the next start.</summary>
    Public Property ScreenSingle As Boolean
        Get
            Return m_bolScreenSingle
        End Get
        Set(value As Boolean)
            m_bolScreenSingle = value
        End Set
    End Property

    Public Property Style As String
        Get
            Return m_strStyle
        End Get
        Set(value As String)
            m_strStyle = value
        End Set
    End Property

    Public ReadOnly Property SoundValue As Integer
        Get
            Return 0
        End Get
    End Property

    Public ReadOnly Property SoundMute As Integer
        Get
            Return -9640
        End Get
    End Property

    ''' <summary>The shared ImageLists on the hidden resource form (tree/list icons).</summary>
    Public ReadOnly Property ImageListSubject As ImageList
        Get
            Return frmResAlbum.imlSubject
        End Get
    End Property

    Public ReadOnly Property ImageListFileType As ImageList
        Get
            Return frmResAlbum.imlFileType
        End Get
    End Property

    Public ReadOnly Property ImageListStorage As ImageList
        Get
            Return frmResAlbum.imlStorage
        End Get
    End Property

End Class
