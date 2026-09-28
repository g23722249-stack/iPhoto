' Port of Lib\Class\KeyWord.cls: the keyword book (iPhoto.Kwd) -- one "Section<TAB>Key" per line
' (ANSI), kept sorted by section then key. VB6 held it in a disconnected ADODB.Recordset.
' Fixed from VB6: a quote in a section or key no longer breaks AddItem (VB6 filtered the recordset
' with a string criteria).
Public Class KeyWord

    Private Const mc_strStepChar As String = vbTab

    Private m_strFileDesc As String = ""
    Private ReadOnly m_items As New List(Of KeyValuePair(Of String, String))   ' (Section, Key)

    Public Sub Construct(ByVal FileName As String)
        m_strFileDesc = If(FileName, "")
        Clear()
        LoadKeyWords()
    End Sub

    Public Sub Clear()
        m_items.Clear()
    End Sub

    Private Sub LoadKeyWords()
        If m_strFileDesc.Trim() = "" OrElse Not IO.File.Exists(m_strFileDesc) Then Return
        For Each line As String In IO.File.ReadAllLines(m_strFileDesc, AnsiText.Encoding)
            Dim parts() As String = line.Split(New String() {mc_strStepChar}, StringSplitOptions.None)
            If parts.Length <> 2 Then Continue For
            m_items.Add(New KeyValuePair(Of String, String)(parts(0).Trim(), parts(1).Trim()))
        Next
        Sort()
    End Sub

    Private Sub Sort()
        m_items.Sort(Function(a, b)
                         Dim c As Integer = String.Compare(a.Key, b.Key, StringComparison.CurrentCulture)
                         Return If(c <> 0, c, String.Compare(a.Value, b.Value, StringComparison.CurrentCulture))
                     End Function)
    End Sub

    ''' <summary>Adds the keyword unless the section already has it; True when added.</summary>
    Public Function AddItem(ByVal Section As String, ByVal Key As String) As Boolean
        Section = If(Section, "").Trim()
        Key = If(Key, "").Trim()
        If m_items.Any(Function(p) p.Key = Section AndAlso p.Value = Key) Then Return False
        m_items.Add(New KeyValuePair(Of String, String)(Section, Key))
        Sort()
        Return True
    End Function

    Public Function Save() As Boolean
        Try
            Sort()
            IO.File.WriteAllLines(m_strFileDesc, m_items.Select(Function(p) p.Key & vbTab & p.Value), AnsiText.Encoding)
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>The distinct sections, in order; returns their count.</summary>
    Public Function Sections(ByRef Section() As String) As Integer
        Section = m_items.Select(Function(p) p.Key).Distinct().ToArray()
        Return Section.Length
    End Function

    ''' <summary>The distinct keys of one section, in order; returns their count.</summary>
    Public Function Keys(ByVal Section As String, ByRef Key() As String) As Integer
        Key = m_items.Where(Function(p) p.Key = Section).Select(Function(p) p.Value).Distinct().ToArray()
        Return Key.Length
    End Function

End Class
