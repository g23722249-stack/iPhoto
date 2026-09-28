' Port of Lib\Module\LibString.bas: fixed-width strings measured in ANSI (Big5) bytes, as VB6's
' StrConv(..., vbFromUnicode) did -- a Chinese character counts 2. (Not called by iPhoto itself.)
Public Module LibString

    ''' <summary>Length in ANSI bytes: ASCII 1, anything else 2.</summary>
    Public Function StringLength(ByVal szValue As String) As Integer
        Dim n As Integer = 0
        For Each c As Char In If(szValue, "")
            n += If(AscW(c) >= 0 AndAlso AscW(c) <= 127, 1, 2)
        Next
        Return n
    End Function

    ''' <summary>The first iLength ANSI bytes of the value, padded with spaces.</summary>
    Public Function FixedString(ByVal szValue As String, ByVal iLength As Integer) As String
        If iLength <= 0 Then Return ""
        Dim bytes() As Byte = AnsiText.Encoding.GetBytes(If(szValue, "") & Space(iLength))
        Return AnsiText.Encoding.GetString(bytes, 0, Math.Min(iLength, bytes.Length))
    End Function

    ''' <summary>FixedString without a half double-byte character at the end.</summary>
    Public Function FixedLawfulnessString(ByVal szValue As String, ByVal iLength As Integer) As String
        Dim s As String = FixedString(szValue, iLength)
        If s.Length = 0 Then Return s
        Dim last As Char = s(s.Length - 1)
        If last = ChrW(0) OrElse (AscW(last) > 127 AndAlso AnsiText.Encoding.GetByteCount(last.ToString()) = 1) Then
            s = s.Substring(0, s.Length - 1)
        End If
        Return s
    End Function

    ''' <summary>The number with iFixRound integer digits (zero-padded, cut from the left) and iFixScale
    ''' decimals (cut, not rounded); bSigned adds "+" / "-".</summary>
    Public Function FixedNumber(ByVal szValue As String, ByVal bSigned As Boolean, ByVal iFixRound As Integer, ByVal iFixScale As Integer) As String
        Dim v As Double = Val(szValue)
        Dim iSgn As Integer = If(bSigned, Math.Sign(v), 1)
        Dim s As String = Math.Abs(v).ToString("0.##########", Globalization.CultureInfo.InvariantCulture)
        Dim dot As Integer = s.IndexOf("."c)
        Dim whole As String = If(dot >= 0, s.Substring(0, dot), s)
        Dim frac As String = If(dot >= 0, s.Substring(dot + 1), "")
        Dim szRound As String = whole.PadLeft(iFixRound, "0"c)
        szRound = szRound.Substring(szRound.Length - iFixRound)
        Dim szScale As String = If(iFixScale > 0, (frac & New String("0"c, iFixScale)).Substring(0, iFixScale), "")
        Dim result As String = szRound & If(iFixScale > 0, "." & szScale, "")
        If bSigned Then result = If(iSgn < 0, "-", "+") & result
        Return result
    End Function

End Module
