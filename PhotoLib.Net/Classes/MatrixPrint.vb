' Port of Lib\Class\MatrixPrint.cls: lays photos out in a grid on a sheet of paper (all sizes in mm):
' the paper, margins (Borderland), printer's unprintable edge (KeepWidth / KeepHeight), photos per
' row (Limit), 4x3 landscape or 3x4 portrait cells (Orientation) and header / footer text height.
' Count = cells per page; ItemLeft / ItemTop(i) = where cell i goes; ItemWidth / ItemHeight = cell size.
' The arithmetic is VB6's, Integer mm throughout (VB6 rounded to even when a Double landed in an
' Integer, as CInt does).
' Fixed from VB6: every column was placed one gap too far left ((Col - 1) * gap instead of Col * gap),
' so the first photo started left of the margin.

Public Enum enumBorderland
    arrLeft = 0
    arrRight = 1
    arrHeader = 2
    arrFooter = 3
End Enum

Public Enum enumText
    txtHeader = 0
    txtFooter = 1
End Enum

Public Enum enumOrientationMode
    水平排列 = 0
    垂直排列 = 1
End Enum

Public Class MatrixPrint

    Private Structure strPicture
        Public Col As Integer
        Public Row As Integer
        Public Left As Integer
        Public Top As Integer
    End Structure

    Private m_intLeftBorder As Integer = 0
    Private m_intRightBorder As Integer = 0
    Private m_intHeaderBorder As Integer = 0
    Private m_intFooterBorder As Integer = 0

    Private m_intWidth As Integer = 210        ' A4
    Private m_intHeight As Integer = 297

    Private m_intLimit As Integer = 1
    Private m_enumOrientation As enumOrientationMode = enumOrientationMode.水平排列

    Private m_intPictureWidth As Integer
    Private m_intPictureHeight As Integer

    Private m_intCount As Integer
    Private m_lpPicture() As strPicture = {}

    Private m_intRelWidth As Integer          ' 真實的列印面積
    Private m_intRelHeight As Integer
    Private m_intHorInterval As Integer = 4   ' 水平間距
    Private m_intVerInterval As Integer       ' 垂直間距
    Private m_intOneColumnCount As Integer    ' 一行印幾個
    Private m_intKeepWidth As Integer = 0     ' 印表機的保留寬度
    Private m_intKeepHeight As Integer = 0
    Private m_intHeaderTextHeight As Integer = 0
    Private m_intBottomTextHeight As Integer = 0
    Private m_intFixLeft As Integer
    Private m_intFixTop As Integer
    Private m_intInterval As Integer = 0

    Public Sub New()
        CalcMatrixPicture()
    End Sub

    Public Property TextHeight(ByVal Arrow As enumText) As Integer
        Get
            Return If(Arrow = enumText.txtHeader, m_intHeaderTextHeight, m_intBottomTextHeight)
        End Get
        Set(value As Integer)
            If Arrow = enumText.txtHeader Then m_intHeaderTextHeight = value Else m_intBottomTextHeight = value
        End Set
    End Property

    Public ReadOnly Property ItemCol(ByVal Index As Integer) As Integer
        Get
            Return m_lpPicture(Index).Col
        End Get
    End Property

    Public ReadOnly Property ItemRow(ByVal Index As Integer) As Integer
        Get
            Return m_lpPicture(Index).Row
        End Get
    End Property

    Public ReadOnly Property ItemLeft(ByVal Index As Integer) As Integer
        Get
            Return m_lpPicture(Index).Left
        End Get
    End Property

    Public ReadOnly Property ItemTop(ByVal Index As Integer) As Integer
        Get
            Return m_lpPicture(Index).Top
        End Get
    End Property

    '圖片寬度 / 高度
    Public ReadOnly Property ItemWidth As Integer
        Get
            Return m_intPictureWidth
        End Get
    End Property

    Public ReadOnly Property ItemHeight As Integer
        Get
            Return m_intPictureHeight
        End Get
    End Property

    ''' <summary>Cells per page.</summary>
    Public ReadOnly Property Count As Integer
        Get
            Return m_intCount
        End Get
    End Property

    Public Property Orientation As enumOrientationMode
        Get
            Return m_enumOrientation
        End Get
        Set(value As enumOrientationMode)
            If m_enumOrientation = value Then Return
            m_enumOrientation = value
            CalcMatrixPicture()
        End Set
    End Property

    '每欄照片數
    Public Property Limit As Integer
        Get
            Return m_intLimit
        End Get
        Set(value As Integer)
            m_intLimit = If(value <= 0, 1, value)
            CalcMatrixPicture()
        End Set
    End Property

    '邊界
    Public Property Borderland(ByVal Arrow As enumBorderland) As Integer
        Get
            Select Case Arrow
                Case enumBorderland.arrLeft : Return m_intLeftBorder
                Case enumBorderland.arrRight : Return m_intRightBorder
                Case enumBorderland.arrHeader : Return m_intHeaderBorder
                Case Else : Return m_intFooterBorder
            End Select
        End Get
        Set(value As Integer)
            Select Case Arrow
                Case enumBorderland.arrLeft : m_intLeftBorder = value
                Case enumBorderland.arrRight : m_intRightBorder = value
                Case enumBorderland.arrHeader : m_intHeaderBorder = value
                Case Else : m_intFooterBorder = value
            End Select
        End Set
    End Property

    Public Property Width As Integer
        Get
            Return m_intWidth
        End Get
        Set(value As Integer)
            m_intWidth = value
        End Set
    End Property

    Public Property Height As Integer
        Get
            Return m_intHeight
        End Get
        Set(value As Integer)
            m_intHeight = value
        End Set
    End Property

    '印表機保留寬度 / 高度
    Public Property KeepWidth As Integer
        Get
            Return m_intKeepWidth
        End Get
        Set(value As Integer)
            m_intKeepWidth = value
        End Set
    End Property

    Public Property KeepHeight As Integer
        Get
            Return m_intKeepHeight
        End Get
        Set(value As Integer)
            m_intKeepHeight = value
        End Set
    End Property

    '照片間的間距
    Public Property Interval As Integer
        Get
            Return m_intInterval
        End Get
        Set(value As Integer)
            m_intInterval = value
        End Set
    End Property

    Private Sub CalcMatrixPicture()
        Clear()
        CalcRealParameter()
        CalcPictureCoordinate()
    End Sub

    Private Sub Clear()
        m_intCount = 0
        m_lpPicture = {}
    End Sub

    Private Sub CalcRealParameter()
        '預先設定水平/垂直間距
        m_intHorInterval = m_intInterval
        m_intVerInterval = m_intInterval
        '取得實際列印面積
        m_intRelWidth = m_intWidth - m_intLeftBorder - m_intRightBorder - (2 * m_intKeepWidth)
        m_intRelHeight = m_intHeight - m_intHeaderBorder - m_intHeaderTextHeight - m_intBottomTextHeight - m_intFooterBorder - (2 * m_intKeepHeight)

        '取得列印的圖形尺寸
        m_intPictureWidth = (m_intRelWidth - (m_intHorInterval * (m_intLimit - 1))) \ m_intLimit
        If m_enumOrientation = enumOrientationMode.水平排列 Then
            m_intPictureHeight = CInt(m_intPictureWidth * (3 / 4))   ' 4x3 照片
        Else
            m_intPictureHeight = CInt(m_intPictureWidth * (4 / 3))   ' 3x4 照片
        End If

        '計算每頁可列印數量
        Dim iCount As Integer = 0
        For I As Integer = 1 To 100
            If (m_intVerInterval * (I - 1)) + (I * m_intPictureHeight) > m_intRelHeight Then
                iCount = I - 1
                Exit For
            End If
        Next
        m_intOneColumnCount = iCount

        '重新計算水平間距
        Dim intWidth As Integer = (m_intPictureWidth * m_intLimit) + (m_intHorInterval * (m_intLimit - 1))
        If intWidth <= m_intRelWidth Then
            Dim intSize As Integer
            If (m_intLimit - 1) <= 0 Then
                intSize = m_intRelWidth - (m_intPictureWidth * m_intLimit)
            Else
                intSize = (m_intRelWidth - (m_intPictureWidth * m_intLimit)) \ (m_intLimit - 1)
            End If
            If intSize > 0 Then m_intHorInterval = intSize
        End If

        '重新計算垂直間距
        If m_intOneColumnCount > 1 Then
            Dim intHeight As Integer = (m_intPictureHeight * m_intOneColumnCount) + (m_intVerInterval * (m_intOneColumnCount - 1))
            If intHeight <= m_intRelHeight Then m_intVerInterval = m_intInterval
        End If

        '每張紙可列印數量
        m_intCount = m_intOneColumnCount * m_intLimit
        If m_intCount <= 0 Then m_intCount = 1

        '重新修正開始位置
        intWidth = (m_intPictureWidth * m_intLimit) + ((m_intLimit - 1) * m_intHorInterval)
        m_intFixLeft = CInt((m_intRelWidth - intWidth) / 2)
        Dim totalHeight As Integer = (m_intPictureHeight * m_intOneColumnCount) + ((m_intOneColumnCount - 1) * m_intVerInterval)
        m_intFixTop = CInt((m_intRelHeight - totalHeight) / 2)
    End Sub

    Private Sub CalcPictureCoordinate()
        ReDim m_lpPicture(m_intCount - 1)
        Dim intLeft As Integer = m_intKeepWidth + m_intLeftBorder + m_intFixLeft
        Dim intTop As Integer = m_intKeepHeight + m_intHeaderBorder + m_intHeaderTextHeight + m_intFixTop

        For I As Integer = 0 To m_intCount - 1
            m_lpPicture(I).Col = I Mod m_intLimit
            m_lpPicture(I).Row = I \ m_intLimit
            m_lpPicture(I).Left = intLeft + (m_lpPicture(I).Col * m_intPictureWidth) + (m_lpPicture(I).Col * m_intHorInterval)
            m_lpPicture(I).Top = intTop + (m_lpPicture(I).Row * m_intPictureHeight) + (m_lpPicture(I).Row * m_intVerInterval)
        Next
    End Sub

End Class
