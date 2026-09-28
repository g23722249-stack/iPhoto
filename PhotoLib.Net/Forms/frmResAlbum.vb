' Port of 電子相簿\Lib\Form\frmResAlbum.frm: a hidden form that holds shared resources -- the
' ImageLists imlSubject / imlFileType / imlStorage (Config.ImageList*) -- and, in VB6, the Dir/FileListBoxes
' used to list folders and files. Listing is plain System.IO now (LibAlbum.ExactFolders/ExactFiles);
' these Shared forwarders keep the existing "frmResAlbum.ExactFiles(...)" calls working without
' creating the form.
' VB6 kept each subject icon's display name in its ListImage.Tag, which a .NET ImageList has no room
' for: SubjectName gives it back.
Public Class frmResAlbum

    Public Shared Function ExactFolders(ByVal szFolderSpec As String, ByRef aszFolders() As String) As Integer
        Return LibAlbum.ExactFolders(szFolderSpec, aszFolders)
    End Function

    Public Shared Function ExactFiles(ByVal szFolderSpec As String, ByVal szPattern As String, ByRef aszFiles() As String, ByVal bMergePath As Boolean) As Integer
        Return LibAlbum.ExactFiles(szFolderSpec, szPattern, aszFiles, bMergePath)
    End Function

    ''' <summary>imlSubject's display names (VB6 ListImage.Tag), by key.</summary>
    Private Shared ReadOnly SubjectNames As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
        {"icoPeople", "人物"}, {"icoHome", "家庭"}, {"icoHouse", "住宅"}, {"icoTravel", "旅遊"},
        {"icoBirthday", "生日慶祝"}, {"icoWedding", "喜事"}, {"icoSport", "運動"}, {"icoBuddha", "佛"},
        {"icoVideo", "錄影留念"}, {"icoPeopleSign", "人型"}, {"icoWoman", "美女"}, {"icoAnimal", "動物"},
        {"icoBird", "鳥類"}, {"icoMacaw", "金剛鸚鵡"}, {"icoInsect", "昆蟲"}, {"icoFish", "魚"},
        {"icoFlower", "花草"}, {"icoGarden", "庭院"}, {"icoPotPlant", "盆栽"}, {"icoSunFlower", "向日葵"},
        {"icoSun", "日出"}, {"icoForest", "森林"}, {"icoTree", "樹木"}, {"icoWin", "獎盃"},
        {"icoBugle", "號角"}, {"icoBank", "打擊樂器"}, {"icoGiitar", "吉他"}, {"icoTea", "下午茶"},
        {"icoMail", "吃飯"}, {"icoFolder", "資料夾"}, {"icoJaguarFolder", "資料夾"}, {"icoOther", "未分類"},
        {"icoLocked", "鎖定"}, {"icoStar", "星號"}, {"icoSmile", "微笑"}, {"icoAlter", "警告"},
        {"icoFilm_old", "開啟中"}, {"icoFilm", "開啟中"}, {"icoAlbumFolder", "相簿"}, {"icoAlbumBook", "相簿"}, {"icoRoot", "Root"},
        {"icoFace", "面孔"}}     ' icoFace had no Tag in VB6 (a blank caption in frmSubject); 面孔 as in the tree

    Public Shared Function SubjectName(ByVal key As String) As String
        Dim name As String = Nothing
        Return If(SubjectNames.TryGetValue(If(key, ""), name), name, key)
    End Function

    ''' <summary>A keyword drawn white on blue, sized to the text (a drag image). VB6 also saved it to
    ''' C:\KEYWORD.BMP, which is dropped.</summary>
    Public Shared Function PaintKeywordDragImage(ByVal objFont As Font, ByVal strText As String) As Image
        Dim sz As Size = TextRenderer.MeasureText(If(strText, ""), objFont)
        Dim bmp As New Bitmap(Math.Max(1, sz.Width), Math.Max(1, sz.Height))
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.FromArgb(8, 73, 198))
            TextRenderer.DrawText(g, strText, objFont, Point.Empty, Color.White)
        End Using
        Return bmp
    End Function

End Class
