' The album tree (frmClass / tvList) and the thumbnails (mlList) share their width: dragging the
' invisible handle between them (pnlSplitter, only the cursor shows it) moves mlList's left edge, its
' right edge stays. The album fields under the tree (txtTitle, txtDate, txtSpot, txtRemark) keep their
' right edge on the tree's. Nothing else moves: butMode stays centred where the designer put mlList
' (ListHomeLeft, see OnLayout), the selection count (imgSelected / lblSelCount) stays where it is.
' ListMinWidth keeps mlList wide enough for the bottom row under it (butMode .. the size slider).
' New in the .NET port.
'
' The width is saved with WindowPlacement (HKCU\...\iPhoto\Window\ClassWidth). A saved value that is
' missing, not a number, below the minimum or wider than all the screens gives the designer width;
' any width is kept between ClassMinWidth and what leaves mlList ListMinWidth, also when the window
' gets smaller.
'
' While dragging, the window doesn't draw until every control has moved, then it is redrawn once
' (WM_SETREDRAW + RedrawWindow, as Aqua's live window resize): moved one by one, each control repainted
' on its own and the window flickered.
Imports System.Runtime.InteropServices

Partial Class frmMain

    Private Const ClassWidthKey As String = "ClassWidth"
    Private Const ClassMinWidth As Integer = 300
    Private Const ListMinWidth As Integer = 830

    Private m_classWidth As Integer          ' the width asked for; 0 until Load
    Private m_splitGap As Integer            ' frmClass.Right .. mlList.Left (the handle)
    Private m_splitDragging As Boolean
    Private m_splitStartX As Integer         ' screen x where the drag began
    Private m_splitStartWidth As Integer
    Private m_splitApplying As Boolean
    Private m_infoFields() As Control        ' the album fields under the tree
    Private m_infoRightGaps() As Integer     ' frmClass.Right - field.Right (designer)
    Private m_listHomeLeft As Integer        ' mlList.Left in the designer; 0 until Load

    ''' <summary>mlList's left edge before any dragging (the designer's), for laying out what must not
    ''' follow the handle.</summary>
    Private ReadOnly Property ListHomeLeft As Integer
        Get
            Return If(m_listHomeLeft > 0, m_listHomeLeft, mlList.Left)
        End Get
    End Property

    Private Sub Splitter_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        m_listHomeLeft = mlList.Left
        m_splitGap = mlList.Left - frmClass.Right
        m_infoFields = {txtTitle, txtDate, txtSpot, txtRemark}
        m_infoRightGaps = m_infoFields.Select(Function(c) frmClass.Right - c.Right).ToArray()
        Dim saved As Integer
        If WindowPlacement.SavedInteger(ClassWidthKey, saved) AndAlso saved >= ClassMinWidth AndAlso
           saved <= SystemInformation.VirtualScreen.Width Then
            m_classWidth = saved
        Else
            m_classWidth = frmClass.Width
        End If
        ApplyClassWidth()
    End Sub

    Private Sub Splitter_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If m_classWidth >= ClassMinWidth Then WindowPlacement.SaveInteger(ClassWidthKey, m_classWidth)
    End Sub

    ''' <summary>Lays frmClass, the handle, mlList and the album fields out for m_classWidth, kept
    ''' within the limits for the current window size. Called from OnLayout and while dragging.</summary>
    Private Sub ApplyClassWidth()
        If m_splitApplying OrElse m_classWidth <= 0 Then Return
        If WindowState = FormWindowState.Minimized OrElse ClientSize.Width <= 0 Then Return
        Dim listRight As Integer = mlList.Right
        Dim maxWidth As Integer = listRight - ListMinWidth - m_splitGap - frmClass.Left
        Dim w As Integer = Math.Max(ClassMinWidth, Math.Min(m_classWidth, maxWidth))
        Dim listLeft As Integer = frmClass.Left + w + m_splitGap
        If frmClass.Width = w AndAlso mlList.Left = listLeft Then Return
        m_splitApplying = True
        Dim freeze As Boolean = m_splitDragging AndAlso IsHandleCreated
        If freeze Then SendMessage(Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero)
        Try
            frmClass.Width = w
            pnlSplitter.Left = frmClass.Right
            mlList.SetBounds(listLeft, 0, Math.Max(1, listRight - listLeft), 0, BoundsSpecified.X Or BoundsSpecified.Width)
            For i As Integer = 0 To m_infoFields.Length - 1
                Dim c As Control = m_infoFields(i)
                c.Width = Math.Max(1, frmClass.Right - m_infoRightGaps(i) - c.Left)
            Next
        Finally
            If freeze Then
                SendMessage(Handle, WM_SETREDRAW, New IntPtr(1), IntPtr.Zero)
                RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE Or RDW_ERASE Or RDW_ALLCHILDREN Or RDW_UPDATENOW)
            End If
            m_splitApplying = False
        End Try
    End Sub

    Private Const WM_SETREDRAW As Integer = &HB
    Private Const RDW_INVALIDATE As Integer = &H1
    Private Const RDW_ERASE As Integer = &H4
    Private Const RDW_ALLCHILDREN As Integer = &H80
    Private Const RDW_UPDATENOW As Integer = &H100

    <DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function RedrawWindow(ByVal hWnd As IntPtr, ByVal lprcUpdate As IntPtr, ByVal hrgnUpdate As IntPtr, ByVal flags As Integer) As Boolean
    End Function

    Private Sub pnlSplitter_MouseDown(sender As Object, e As MouseEventArgs) Handles pnlSplitter.MouseDown
        If e.Button <> MouseButtons.Left OrElse m_classWidth <= 0 Then Return
        m_splitDragging = True
        m_splitStartX = Cursor.Position.X   ' screen coordinates: the handle moves while dragging
        m_splitStartWidth = frmClass.Width
    End Sub

    Private Sub pnlSplitter_MouseMove(sender As Object, e As MouseEventArgs) Handles pnlSplitter.MouseMove
        If Not m_splitDragging Then Return
        m_classWidth = m_splitStartWidth + (Cursor.Position.X - m_splitStartX)
        ApplyClassWidth()
        m_classWidth = frmClass.Width   ' remember what fitted, not how far the mouse went
    End Sub

    Private Sub pnlSplitter_MouseUp(sender As Object, e As MouseEventArgs) Handles pnlSplitter.MouseUp
        EndSplitterDrag()
    End Sub

    Private Sub pnlSplitter_MouseCaptureChanged(sender As Object, e As EventArgs) Handles pnlSplitter.MouseCaptureChanged
        EndSplitterDrag()   ' capture lost (Alt+Tab, a dialog ...)
    End Sub

    Private Sub EndSplitterDrag()
        If Not m_splitDragging Then Return
        m_splitDragging = False
        WindowPlacement.SaveInteger(ClassWidthKey, m_classWidth)
    End Sub

End Class
