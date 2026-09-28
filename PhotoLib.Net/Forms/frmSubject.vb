' Port of Lib\Form\frmSubject.frm: the subject-icon picker, opened at a screen position; click an icon to
' choose it. The list is filled from Config.ImageListSubject (VB6 LibUserInterface.AddSubjectToListView,
' moved here since this form is its only user).
' Fixed from VB6: the answer is reset on every call, so closing without a choice returns "" (VB6 kept the
' previous choice).
Public Class frmSubject

    Private m_strIcon As String = ""

    ''' <summary>Opens at (lLeft, lTop) screen pixels; returns the chosen imlSubject key, "" if none.</summary>
    Public Function ShowSubject(ByVal lLeft As Integer, ByVal lTop As Integer) As String
        m_strIcon = ""
        Location = New Point(lLeft, lTop)
        ShowDialog()
        Return m_strIcon
    End Function

    ' VB6 Form_Load
    Protected Overrides Sub OnLoad(e As EventArgs)
        AddSubjectToListView(g_lpConfig, lvSubject)
        MyBase.OnLoad(e)
    End Sub

    ''' <summary>Every subject icon except the album / book / photo type icons.</summary>
    Public Shared Sub AddSubjectToListView(ByVal lpConfig As Config, ByVal lpListView As ListView)
        Dim images As ImageList = lpConfig.ImageListSubject
        lpListView.Items.Clear()
        lpListView.LargeImageList = images
        lpListView.SmallImageList = images
        For Each key As String In images.Images.Keys
            Select Case key.ToUpperInvariant()
                Case "ICOFILM_OLD", "ICOFILM", "ICOROOT", "ICOALBUM", "ICOFAVORITE", "ICOALBUMFOLDER", "ICOALBUMBOOK",
                     "ICOCLASS", "ICOBOOK", "ICOPHOTO", "ICOVIDEO"
                    ' 不列出
                Case Else
                    lpListView.Items.Add(key, frmResAlbum.SubjectName(key), key)
            End Select
        Next
    End Sub

    Private Sub lvSubject_Click(sender As Object, e As EventArgs) Handles lvSubject.Click
        If lvSubject.SelectedItems.Count = 0 Then Return
        g_lpConfig.PlaySound(Config.enumSound.snButtonClick)
        m_strIcon = lvSubject.SelectedItems(0).Name
        Close()
    End Sub

End Class
