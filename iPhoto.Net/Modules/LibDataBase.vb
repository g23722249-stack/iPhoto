' Port of iPhoto\Module\LibDataBase.bas.
Friend Module LibDataBase

    ''' <summary>Replaces the photo's row in the index (delete, then add).</summary>
    Public Sub WritePhotoToDatabase(ByVal lpDatabase As Database, ByVal lpPhoto As Photo)
        lpDatabase.Delete(lpPhoto.FileDesc)
        lpDatabase.AddItem(lpPhoto)
    End Sub

End Module
