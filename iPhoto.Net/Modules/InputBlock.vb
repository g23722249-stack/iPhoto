' While a list is being filled (with Application.DoEvents in between, so the window paints), clicks and
' keys meant for the main window are dropped -- what VB6 got with "Enabled = False". Turning the
' form's Enabled off and on did the same but cost about 2.5 s each way in iPhoto (every control of the
' tool bar answering the change; measured on 依日期排序\2009年\11月19日: 5.3 of 7 s to open the album).
' Use: Using New InputBlock(Me) ... End Using
Friend NotInheritable Class InputBlock
    Implements IMessageFilter, IDisposable

    Private Const WM_KEYFIRST As Integer = &H100, WM_KEYLAST As Integer = &H109
    Private Const WM_MOUSEFIRST As Integer = &H200, WM_MOUSELAST As Integer = &H20E
    Private Const WM_NCMOUSEFIRST As Integer = &HA0, WM_NCMOUSELAST As Integer = &HAD

    Private ReadOnly m_form As Form
    Private m_bolDone As Boolean

    Public Sub New(ByVal form As Form)
        m_form = form
        Application.AddMessageFilter(Me)
    End Sub

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        Dim input As Boolean = (m.Msg >= WM_KEYFIRST AndAlso m.Msg <= WM_KEYLAST) OrElse
                               (m.Msg >= WM_MOUSEFIRST AndAlso m.Msg <= WM_MOUSELAST) OrElse
                               (m.Msg >= WM_NCMOUSEFIRST AndAlso m.Msg <= WM_NCMOUSELAST)
        If Not input Then Return False
        If m.HWnd = m_form.Handle Then Return True
        Dim c As Control = Control.FromChildHandle(m.HWnd)
        Return c IsNot Nothing AndAlso c.FindForm() Is m_form
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If m_bolDone Then Return
        m_bolDone = True
        Application.RemoveMessageFilter(Me)
    End Sub

End Class
