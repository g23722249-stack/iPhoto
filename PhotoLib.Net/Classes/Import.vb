' Port of 電子相簿\Lib\Class\Import.cls: the list of files being imported and where they go.
' Fixed from VB6: the DateTime getter assigned the date to Title and returned "".
Public Class Import

    Private ReadOnly m_strImportFiles As New List(Of String)

    Public Property AlbumPath As String = ""
    Public Property ClassPath As String = ""
    Public Property Title As String = ""
    Public Property DateTime As String = ""
    Public Property ImportPath As String = ""

    Public Sub Clear()
        m_strImportFiles.Clear()
        ImportPath = ""
        AlbumPath = ""
        ClassPath = ""
    End Sub

    Public ReadOnly Property Count As Integer
        Get
            Return m_strImportFiles.Count
        End Get
    End Property

    Public ReadOnly Property Item(ByVal Index As Integer) As String
        Get
            Return m_strImportFiles(Index)
        End Get
    End Property

    Public Sub AddItem(ByVal FileName As String)
        m_strImportFiles.Add(FileName)
    End Sub

End Class
