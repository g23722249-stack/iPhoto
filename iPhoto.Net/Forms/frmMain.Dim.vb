' The main window goes dark while a feature dialog (設定, 搜尋, 匯出, Dock, 列印 ...) is open over it
' (PhotoLib ModalDimmer; message boxes, pickers and the viewers are left out there). The dialogs that used
' to hide the main window instead (Dock, 匯出, 照片目錄, 列印, 批次修改資訊) now leave it shown, dimmed.
Partial Class frmMain

    Private m_lpDimmer As ModalDimmer

    Private Sub Dim_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_lpDimmer = New ModalDimmer(Me)
    End Sub

    Private Sub Dim_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        m_lpDimmer?.Dispose()
        m_lpDimmer = Nothing
    End Sub

End Class
