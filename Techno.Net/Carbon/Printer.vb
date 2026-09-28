Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Carbon

    Public Enum PrintAlignment
        alLeft = 0
        alRight = 1
        alCenter = 2
    End Enum

    ''' <summary>
    ''' Port of Carbon\CoClass\Printer.cls: draws on a page in millimetres. VB6 wrapped the VB Printer
    ''' object or a PictureBox (ScaleMode = vbMillimeters); here it wraps any Graphics -- a
    ''' PrintDocument page, a preview bitmap or a page saved as JPEG -- whose PageUnit it sets to
    ''' millimetres. Pages and copies belong to PrintDocument, so NewPage / EndDoc are gone.
    ''' Like the VB Printer object, the text font and size carry over from one PrintText to the next.
    ''' </summary>
    Public Class Printer

        Private ReadOnly m_g As Graphics
        Private m_iResidualX As Single
        Private m_iResidualY As Single
        Private m_dblScale As Double = 1
        Private m_fontName As String = "標楷體"
        Private m_fontSize As Single = 12

        Public Sub New(ByVal canvas As Graphics)
            m_g = canvas
            m_g.PageUnit = GraphicsUnit.Millimeter
            m_g.InterpolationMode = InterpolationMode.HighQualityBicubic
            m_g.SmoothingMode = SmoothingMode.AntiAlias
            m_g.TextRenderingHint = Text.TextRenderingHint.AntiAliasGridFit
        End Sub

        Public ReadOnly Property Canvas As Graphics
            Get
                Return m_g
            End Get
        End Property

        ''' <summary>Print everything in black (VB6 Mono).</summary>
        Public Property Mono As Boolean

        ''' <summary>VB6 SetScale: every coordinate and font size is multiplied by it (a preview drawn smaller).</summary>
        Public Property SetScale As Double
            Get
                Return m_dblScale
            End Get
            Set(value As Double)
                m_dblScale = If(value > 0, value, 1)
            End Set
        End Property

        ''' <summary>X / Y offset (mm) added to every position (VB6 Residual).</summary>
        Public Sub Residual(ByVal x As Single, ByVal y As Single)
            m_iResidualX = x
            m_iResidualY = y
        End Sub

        Private Function Px(ByVal v As Single, ByVal residual As Single) As Single
            Return CSng((v + residual) * m_dblScale)
        End Function

        ''' <summary>Draws the picture at (x, y) mm. With width and height and Midway it is fitted into that
        ''' box and centred -- never larger than its own size (96 dpi) unless Fill; without Midway it is
        ''' stretched to the box.</summary>
        Public Sub PaintPicture(ByVal picture As Image, ByVal x As Single, ByVal y As Single,
                                Optional ByVal width As Single = 0, Optional ByVal height As Single = 0,
                                Optional ByVal midway As Boolean = True, Optional ByVal fill As Boolean = False)
            If picture Is Nothing Then Return
            Dim picW As Single = CSng(picture.Width * 25.4 / 96), picH As Single = CSng(picture.Height * 25.4 / 96)
            Dim w As Single = width, h As Single = height
            If width <= 0 AndAlso height <= 0 Then
                w = picW : h = picH
            ElseIf width <= 0 Then
                w = picW * height / picH
            ElseIf height <= 0 Then
                h = picH * width / picW
            ElseIf midway Then
                Dim ratio As Double = Math.Min(width / picW, height / picH)
                w = CSng(picW * ratio) : h = CSng(picH * ratio)
                If Not fill AndAlso (w > picW OrElse h > picH) Then
                    w = picW : h = picH
                End If
                x += (width - w) / 2
                y += (height - h) / 2
            End If
            m_g.DrawImage(picture, New RectangleF(Px(x, m_iResidualX), Px(y, m_iResidualY), CSng(w * m_dblScale), CSng(h * m_dblScale)))
        End Sub

        Private Function MakeFont(ByVal font As String, ByVal fontSize As Single, ByVal bold As Boolean, ByVal italic As Boolean,
                                  ByVal underline As Boolean, ByVal strike As Boolean) As Font
            If font IsNot Nothing AndAlso font.Trim() <> "" Then m_fontName = font
            If fontSize > 0 Then m_fontSize = fontSize
            Dim style As FontStyle = FontStyle.Regular
            If bold Then style = style Or FontStyle.Bold
            If italic Then style = style Or FontStyle.Italic
            If underline Then style = style Or FontStyle.Underline
            If strike Then style = style Or FontStyle.Strikeout
            Return New Font(m_fontName, CSng(m_fontSize * m_dblScale), style, GraphicsUnit.Point)
        End Function

        ''' <summary>Text at (x, y) mm; a Width &gt; 0 cuts the text to fit and is the box for
        ''' alRight / alCenter.</summary>
        Public Sub PrintText(ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal text As String,
                             Optional ByVal color As Color = Nothing, Optional ByVal alignment As PrintAlignment = PrintAlignment.alLeft,
                             Optional ByVal font As String = "標楷體", Optional ByVal fontSize As Single = 0,
                             Optional ByVal bold As Boolean = False, Optional ByVal italic As Boolean = False,
                             Optional ByVal underline As Boolean = False, Optional ByVal strike As Boolean = False)
            If text Is Nothing OrElse text.Trim() = "" Then Return
            If color.IsEmpty OrElse Mono Then color = Color.Black
            Using f As Font = MakeFont(font, fontSize, bold, italic, underline, strike)
                Dim fmt As StringFormat = StringFormat.GenericTypographic
                If width > 0 Then
                    ' VB6 cut the text one character at a time until it fitted
                    While text.Length > 0 AndAlso m_g.MeasureString(text, f, PointF.Empty, fmt).Width / m_dblScale > width
                        text = text.Substring(0, text.Length - 1)
                    End While
                End If
                Dim textW As Single = CSng(m_g.MeasureString(text, f, PointF.Empty, fmt).Width / m_dblScale)
                Dim left As Single = x
                Select Case alignment
                    Case PrintAlignment.alRight : left = x + (width - textW)
                    Case PrintAlignment.alCenter : left = x + (width - textW) / 2
                End Select
                Using b As New SolidBrush(color)
                    m_g.DrawString(text, f, b, Px(left, m_iResidualX), Px(y, m_iResidualY), fmt)
                End Using
            End Using
        End Sub

        ''' <summary>Width of the text in mm (and makes font / size current, as VB6 did).</summary>
        Public Function TextWidth(ByVal text As String, Optional ByVal font As String = "", Optional ByVal fontSize As Single = 0,
                                  Optional ByVal bold As Boolean = False, Optional ByVal italic As Boolean = False) As Single
            Using f As Font = MakeFont(font, fontSize, bold, italic, False, False)
                Return CSng(m_g.MeasureString(If(text, ""), f, PointF.Empty, StringFormat.GenericTypographic).Width / m_dblScale)
            End Using
        End Function

        ''' <summary>Height of a line of the text in mm, at the current font unless one is given.</summary>
        Public Function TextHeight(ByVal text As String, Optional ByVal font As String = "", Optional ByVal fontSize As Single = 0,
                                   Optional ByVal bold As Boolean = False, Optional ByVal italic As Boolean = False) As Single
            Using f As Font = MakeFont(font, fontSize, bold, italic, False, False)
                Return CSng(m_g.MeasureString(If(String.IsNullOrEmpty(text), "1", text), f).Height / m_dblScale)
            End Using
        End Function

        Public Sub PrintLine(ByVal x1 As Single, ByVal y1 As Single, ByVal x2 As Single, ByVal y2 As Single, ByVal drawWidth As Integer, Optional ByVal color As Color = Nothing)
            If color.IsEmpty OrElse Mono Then color = Color.Black
            Using p As New Pen(color, PenWidth(drawWidth))
                m_g.DrawLine(p, Px(x1, m_iResidualX), Px(y1, m_iResidualY), Px(x2, m_iResidualX), Px(y2, m_iResidualY))
            End Using
        End Sub

        Public Sub PrintRectangle(ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal drawWidth As Integer,
                                  Optional ByVal color As Color = Nothing, Optional ByVal fill As Boolean = False)
            If width <= 0 OrElse height <= 0 Then Return
            If color.IsEmpty Then color = Color.Black
            Dim r As New RectangleF(Px(x, m_iResidualX), Px(y, m_iResidualY), CSng(width * m_dblScale), CSng(height * m_dblScale))
            If fill AndAlso Not Mono Then
                Using b As New SolidBrush(color)
                    m_g.FillRectangle(b, r)
                End Using
            Else
                Using p As New Pen(If(Mono, Color.Black, color), PenWidth(drawWidth))
                    m_g.DrawRectangle(p, r.X, r.Y, r.Width, r.Height)
                End Using
            End If
        End Sub

        ''' <summary>Ellipse centred on (x, y) with the larger of width / height as radius (VB6 Circle).</summary>
        Public Sub PrintCircle(ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal drawWidth As Integer,
                               Optional ByVal color As Color = Nothing, Optional ByVal fill As Boolean = False)
            If width <= 0 OrElse height <= 0 Then Return
            If color.IsEmpty OrElse Mono Then color = Color.Black
            Dim radius As Single = CSng(Math.Max(width, height) * m_dblScale)
            Dim aspect As Single = height / width
            Dim rx As Single = If(aspect > 1, radius / aspect, radius), ry As Single = If(aspect > 1, radius, radius * aspect)
            Dim r As New RectangleF(Px(x, m_iResidualX) - rx, Px(y, m_iResidualY) - ry, rx * 2, ry * 2)
            If fill Then
                Using b As New SolidBrush(color)
                    m_g.FillEllipse(b, r)
                End Using
            End If
            Using p As New Pen(color, PenWidth(drawWidth))
                m_g.DrawEllipse(p, r)
            End Using
        End Sub

        ''' <summary>VB6 DrawWidth is in device pixels; 1 px of a 96 dpi screen = 0.26 mm.</summary>
        Private Shared Function PenWidth(ByVal drawWidth As Integer) As Single
            Return CSng(Math.Max(1, drawWidth) * 25.4 / 96)
        End Function

    End Class

End Namespace
