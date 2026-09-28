Namespace Quartz

    ''' <summary>
    ''' Port of Quartz\CoClass\FixSize.cls: shrinks a picture size to fit a canvas, keeping its aspect
    ''' ratio (it never enlarges). Steps as in VB6: SetCanvasSize, SetPictureSize, Resize, then read
    ''' Width / Height.
    ''' </summary>
    Public Class FixSize

        Private m_lngOutWidth, m_lngOutHeight As Integer
        Private m_lngInWidth, m_lngInHeight As Integer

        Public Sub SetCanvasRect(ByVal Left As Integer, ByVal Right As Integer, ByVal Top As Integer, ByVal Bottom As Integer)
            m_lngOutWidth = Right - Left
            m_lngOutHeight = Bottom - Top
        End Sub

        Public Sub SetCanvasSize(ByVal Width As Integer, ByVal Height As Integer)
            m_lngOutWidth = Width
            m_lngOutHeight = Height
        End Sub

        Public Sub SetPictureSize(ByVal Width As Integer, ByVal Height As Integer)
            m_lngInWidth = Width
            m_lngInHeight = Height
        End Sub

        Public ReadOnly Property Width As Integer
            Get
                Return m_lngInWidth
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            Get
                Return m_lngInHeight
            End Get
        End Property

        Public Sub Resize()
            If m_lngInHeight <= 0 OrElse m_lngInWidth <= 0 Then Return
            If m_lngOutWidth <= 0 OrElse m_lngOutHeight <= 0 Then Return   ' VB6 could loop forever here
            Dim dblRatio As Double = m_lngInHeight / m_lngInWidth
            While m_lngInWidth > m_lngOutWidth OrElse m_lngInHeight > m_lngOutHeight
                If m_lngInWidth <= m_lngOutWidth AndAlso m_lngInHeight >= m_lngOutHeight Then
                    m_lngInWidth = CInt(Math.Floor(m_lngOutHeight / dblRatio))
                    m_lngInHeight = m_lngOutHeight
                ElseIf m_lngInWidth >= m_lngOutWidth AndAlso m_lngInHeight <= m_lngOutHeight Then
                    m_lngInWidth = m_lngOutWidth
                    m_lngInHeight = CInt(Math.Floor(m_lngOutWidth * dblRatio))
                Else
                    m_lngInWidth = m_lngOutWidth
                    m_lngInHeight = CInt(m_lngOutWidth * dblRatio)
                End If
            End While
        End Sub

    End Class

End Namespace
