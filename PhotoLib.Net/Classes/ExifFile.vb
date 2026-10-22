Imports System.IO
Imports System.Xml
Imports System.Xml.Linq

' A photo's or video's "<name>.Exif" side file (new in the .NET port): XML, one entry per file of that
' base name -- IMG_1.JPG and IMG_1.MP4 share IMG_1.Exif, each keeps its own data:
'
'   <?xml version="1.0" encoding="utf-8"?>
'   <Exif version="1">
'     <Image name="IMG_1.JPG" modified="2026-10-22T21:05:13">
'       <Create><Date>20240214</Date><Time>002721</Time></Create>
'       <Exif><Date>20240214</Date><Time>002721</Time><Title>...</Title><Character>...</Character>
'             <Spot/><KeyWord/><Remark/><Ranking/><GPS/><Country/><City/><Town/></Exif>
'     </Image>
'     <Video name="IMG_1.MP4" modified="...">...</Video>
'   </Exif>
'
' The VB6 format, an ini with [Create] and [Exif] for every file of the base name, is still read; the
' first write turns it into XML, giving each of those files a copy of it. The API is IniFile's
' (SimpleGetValue / SimpleSetValue / EnumerateKeys, by section and key), so the code that used an
' IniFile on the .Exif uses this instead, bound to the photo rather than to the side file.
' Writes go through a temporary file (a crash never leaves half a file) under one lock for the process
' (the UI and the face scan thread write side files too).
Public Class ExifFile

    Private Const RootName As String = "Exif"
    Private Const ImageName As String = "Image"
    Private Const VideoName As String = "Video"
    Private Shared ReadOnly s_lock As New Object

    Private ReadOnly m_strMedia As String
    Private ReadOnly m_strSidecar As String

    ''' <summary>The side file data of <paramref name="strMediaFile"/> (a photo or video, which needn't exist).</summary>
    Public Sub New(ByVal strMediaFile As String)
        m_strMedia = strMediaFile
        m_strSidecar = SidecarOf(strMediaFile)
    End Sub

    Public ReadOnly Property MediaFile As String
        Get
            Return m_strMedia
        End Get
    End Property

    Public ReadOnly Property SidecarFile As String
        Get
            Return m_strSidecar
        End Get
    End Property

    ''' <summary>folder\base.Exif of a photo or video.</summary>
    Public Shared Function SidecarOf(ByVal strMediaFile As String) As String
        Return Path.ChangeExtension(strMediaFile, gc_strExifPattern)
    End Function

    ''' <summary>The extensions an album lists (GetPhotoPatterns): the files a side file can belong to.</summary>
    Public Shared Function IsMediaFile(ByVal strFile As String) As Boolean
        Dim ext As String = Path.GetExtension(If(strFile, "")).TrimStart("."c).ToUpperInvariant()
        Return ext.Length > 0 AndAlso GetPhotoPatterns().Split(";"c).Any(Function(p) String.Equals(p.Substring(2), ext, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function KindOf(ByVal strFile As String) As String
        Return If(GetMediaType(New Carbon.FileSystem, strFile) = enumPhotoMediaType.mdVideo, VideoName, ImageName)
    End Function

    '==================================================================================================
    ' Reading
    '==================================================================================================
    ''' <summary>True when there is data for this file: its entry, or an old-format side file.</summary>
    Public ReadOnly Property Exists As Boolean
        Get
            SyncLock s_lock
                Dim doc As XDocument = Nothing
                Select Case Peek(m_strSidecar, doc)
                    Case Format.Missing : Return False
                    Case Format.Ini : Return True
                    Case Else : Return EntryOf(doc, m_strMedia) IsNot Nothing
                End Select
            End SyncLock
        End Get
    End Property

    ''' <summary>When this file's entry was last written (the side file's time for the old format);
    ''' Nothing when there is no data.</summary>
    Public ReadOnly Property Modified As DateTime?
        Get
            SyncLock s_lock
                Dim doc As XDocument = Nothing
                Select Case Peek(m_strSidecar, doc)
                    Case Format.Missing : Return Nothing
                    Case Format.Ini : Return File.GetLastWriteTime(m_strSidecar)
                    Case Else
                        Dim e As XElement = EntryOf(doc, m_strMedia)
                        If e Is Nothing Then Return Nothing
                        Dim t As DateTime
                        If DateTime.TryParse(CStr(e.Attribute("modified")), Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.RoundtripKind, t) Then Return t
                        Return File.GetLastWriteTime(m_strSidecar)
                End Select
            End SyncLock
        End Get
    End Property

    ''' <summary>A value ("" when it isn't there), as IniFile.SimpleGetValue.</summary>
    Public Function SimpleGetValue(ByVal section As String, ByVal key As String) As String
        SyncLock s_lock
            Dim doc As XDocument = Nothing
            Select Case Peek(m_strSidecar, doc)
                Case Format.Missing : Return ""
                Case Format.Ini : Return If(New Carbon.IniFile With {.FileName = m_strSidecar}.SimpleGetValue(section, key), "")
                Case Else
                    Dim v As XElement = EntryOf(doc, m_strMedia)?.Element(section)?.Element(XmlName(key))
                    Return If(v Is Nothing, "", v.Value)
            End Select
        End SyncLock
    End Function

    ''' <summary>The keys of a section (empty when there are none), as IniFile.EnumerateKeys.</summary>
    Public Function EnumerateKeys(ByVal section As String) As String()
        SyncLock s_lock
            Dim doc As XDocument = Nothing
            Select Case Peek(m_strSidecar, doc)
                Case Format.Missing : Return New String() {}
                Case Format.Ini : Return If(New Carbon.IniFile With {.FileName = m_strSidecar}.EnumerateKeys(section), New String() {})
                Case Else
                    Dim s As XElement = EntryOf(doc, m_strMedia)?.Element(section)
                    If s Is Nothing Then Return New String() {}
                    Return s.Elements().Select(Function(x) x.Name.LocalName).ToArray()
            End Select
        End SyncLock
    End Function

    '==================================================================================================
    ' Writing
    '==================================================================================================
    ''' <summary>Sets a value (and the entry's time), as IniFile.SimpleSetValue.</summary>
    Public Sub SimpleSetValue(ByVal section As String, ByVal key As String, ByVal value As String)
        SetValues({(section, key, value)})
    End Sub

    ''' <summary>Several values in one write.</summary>
    Public Sub SetValues(ByVal values As IEnumerable(Of (Section As String, Key As String, Value As String)))
        SyncLock s_lock
            Dim doc As XDocument = OpenForWrite(m_strSidecar)
            Dim e As XElement = EnsureEntry(doc, m_strMedia)
            For Each v In values
                Dim s As XElement = e.Element(v.Section)
                If s Is Nothing Then
                    s = New XElement(v.Section)
                    e.Add(s)
                End If
                Dim k As XName = XmlName(v.Key)
                Dim x As XElement = s.Element(k)
                If x Is Nothing Then
                    s.Add(New XElement(k, If(v.Value, "")))
                Else
                    x.Value = If(v.Value, "")
                End If
            Next
            Touch(e)
            Save(doc, m_strSidecar)
        End SyncLock
    End Sub

    ''' <summary>A copy of the entry's child element <paramref name="name"/> (e.g. "Faces"); Nothing when
    ''' there is none (also for the old format, which has no such elements).</summary>
    Public Function GetElement(ByVal name As String) As XElement
        SyncLock s_lock
            Dim doc As XDocument = Nothing
            If Peek(m_strSidecar, doc) <> Format.Xml Then Return Nothing
            Dim x As XElement = EntryOf(doc, m_strMedia)?.Element(name)
            Return If(x Is Nothing, Nothing, New XElement(x))
        End SyncLock
    End Function

    ''' <summary>Replaces the entry's child element named as <paramref name="value"/> (Nothing with
    ''' <paramref name="name"/>: removes it). The entry's time is left alone: it is the photo's
    ''' information that changed, not this. Unchanged: nothing is written.</summary>
    Public Sub SetElement(ByVal name As String, ByVal value As XElement)
        SyncLock s_lock
            Dim doc As XDocument = OpenForWrite(m_strSidecar)
            Dim e As XElement = If(value Is Nothing, EntryOf(doc, m_strMedia), EnsureEntry(doc, m_strMedia))
            If e Is Nothing Then Return
            Dim old As XElement = e.Element(name)
            If value Is Nothing Then
                If old Is Nothing Then Return
                old.Remove()
            Else
                If old IsNot Nothing AndAlso XNode.DeepEquals(old, value) Then Return
                old?.Remove()
                e.Add(New XElement(value))
            End If
            Save(doc, m_strSidecar)
        End SyncLock
    End Sub

    ''' <summary>The files that have data in <paramref name="strSidecar"/>: the entries' files (those
    ''' that exist), or for the old format every album file of that base name.</summary>
    Public Shared Function MediaFilesOf(ByVal strSidecar As String) As List(Of String)
        SyncLock s_lock
            Dim doc As XDocument = Nothing
            Select Case Peek(strSidecar, doc)
                Case Format.Missing : Return New List(Of String)
                Case Format.Ini : Return SiblingsOf(strSidecar)
                Case Else
                    Dim folder As String = Path.GetDirectoryName(strSidecar)
                    Return doc.Root.Elements().Select(Function(x) CStr(x.Attribute("name"))).
                               Where(Function(n) Not String.IsNullOrEmpty(n)).
                               Select(Function(n) Path.Combine(folder, n)).Where(Function(f) File.Exists(f)).ToList()
            End Select
        End SyncLock
    End Function

    ''' <summary>The data of <paramref name="strFrom"/> becomes that of <paramref name="strTo"/> (a photo
    ''' moved or renamed); <paramref name="strFrom"/>'s entry is removed. Nothing when it had none.</summary>
    Public Shared Sub MoveEntry(ByVal strFrom As String, ByVal strTo As String)
        SyncLock s_lock
            Dim srcSide As String = SidecarOf(strFrom)
            Dim srcDoc As XDocument = Nothing
            Dim f As Format = Load(srcSide, srcDoc)
            If f = Format.Missing Then Return
            If f = Format.Ini Then srcDoc = OpenForWrite(srcSide, strFrom)   ' the old format, with an entry for strFrom
            Dim e As XElement = EntryOf(srcDoc, strFrom)
            If e Is Nothing Then Return
            Dim dstSide As String = SidecarOf(strTo)
            Dim dstDoc As XDocument = OpenForWrite(dstSide)
            EntryOf(dstDoc, strTo)?.Remove()
            Dim copy As New XElement(e)
            copy.Name = KindOf(strTo)
            copy.SetAttributeValue("name", Path.GetFileName(strTo))
            dstDoc.Root.Add(copy)
            Save(dstDoc, dstSide)
            e.Remove()
            If srcDoc.Root.HasElements Then
                Save(srcDoc, srcSide)
            Else
                DeleteQuietly(srcSide)
            End If
        End SyncLock
    End Sub

    ''' <summary>Removes the file's entry. True when the side file has no entry left (the caller deletes
    ''' or recycles it then); an old-format side file still used by another file of the name is turned
    ''' into XML for that one.</summary>
    Public Shared Function RemoveEntry(ByVal strMediaFile As String) As Boolean
        SyncLock s_lock
            Dim side As String = SidecarOf(strMediaFile)
            Dim doc As XDocument = Nothing
            Dim f As Format = Load(side, doc)
            If f = Format.Missing Then Return False
            If f = Format.Ini Then
                If SiblingsOf(side).All(Function(s) String.Equals(s, strMediaFile, StringComparison.OrdinalIgnoreCase)) Then Return True
                doc = OpenForWrite(side)
            End If
            EntryOf(doc, strMediaFile)?.Remove()
            If Not doc.Root.HasElements Then Return True
            Save(doc, side)
            Return False
        End SyncLock
    End Function

    ''' <summary>True when the side file is in the XML format (False also when there is none).</summary>
    Public Shared Function IsXml(ByVal strSidecar As String) As Boolean
        SyncLock s_lock
            Dim doc As XDocument = Nothing
            Return Peek(strSidecar, doc) = Format.Xml
        End SyncLock
    End Function

    ''' <summary>An old-format side file as XML (nothing else changes); False when it already was or is missing.</summary>
    Public Shared Function ConvertToXml(ByVal strSidecar As String) As Boolean
        SyncLock s_lock
            Dim doc As XDocument = Nothing
            If Peek(strSidecar, doc) <> Format.Ini Then Return False
            Save(OpenForWrite(strSidecar), strSidecar)
            Return True
        End SyncLock
    End Function

    '==================================================================================================
    ' Inside
    '==================================================================================================
    Private Enum Format
        Missing
        Ini
        Xml
    End Enum

    ' Parsed side files for reading, by path, while the file's time and size stay the same (a sort or
    ' the index sync reads every field of thousands of photos); a write or delete here drops its entry.
    ' Never changed in place: the writers parse afresh (Load).
    Private Shared ReadOnly s_cache As New Dictionary(Of String, (Stamp As DateTime, Size As Long, Kind As Format, Doc As XDocument))(StringComparer.OrdinalIgnoreCase)
    Private Const CacheMax As Integer = 2000

    ''' <summary>Load for reading only (the document must not be changed): from the cache when the file is unchanged.</summary>
    Private Shared Function Peek(ByVal strSidecar As String, ByRef doc As XDocument) As Format
        doc = Nothing
        Dim fi As New FileInfo(strSidecar)
        If Not fi.Exists Then
            s_cache.Remove(strSidecar)
            Return Format.Missing
        End If
        Dim hit As (Stamp As DateTime, Size As Long, Kind As Format, Doc As XDocument) = Nothing
        If s_cache.TryGetValue(strSidecar, hit) AndAlso hit.Stamp = fi.LastWriteTimeUtc AndAlso hit.Size = fi.Length Then
            doc = hit.Doc
            Return hit.Kind
        End If
        Dim k As Format = Load(strSidecar, doc)
        If s_cache.Count >= CacheMax Then s_cache.Clear()
        s_cache(strSidecar) = (fi.LastWriteTimeUtc, fi.Length, k, doc)
        Return k
    End Function

    ''' <summary>Which format the side file is, with the document when XML (a broken XML file counts as missing).</summary>
    Private Shared Function Load(ByVal strSidecar As String, ByRef doc As XDocument) As Format
        doc = Nothing
        If Not File.Exists(strSidecar) Then Return Format.Missing
        Dim content As String
        Try
            content = File.ReadAllText(strSidecar, System.Text.Encoding.UTF8)   ' a BOM-less ANSI ini still shows its first "["
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Format.Missing
        End Try
        Dim t As String = content.TrimStart(ChrW(&HFEFF), " "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf)
        If Not t.StartsWith("<") Then Return Format.Ini
        Try
            doc = XDocument.Parse(t)
            If doc.Root Is Nothing OrElse doc.Root.Name.LocalName <> RootName Then doc = Nothing : Return Format.Missing
            Return Format.Xml
        Catch ex As XmlException
            doc = Nothing
            Return Format.Missing
        End Try
    End Function

    ''' <summary>The document to change: the XML one, a new one, or the old format turned into XML (an
    ''' entry with a copy of it for every album file of the name, and for <paramref name="strAlso"/>).</summary>
    Private Shared Function OpenForWrite(ByVal strSidecar As String, Optional ByVal strAlso As String = Nothing) As XDocument
        Dim doc As XDocument = Nothing
        Select Case Load(strSidecar, doc)
            Case Format.Xml : Return doc
            Case Format.Ini
                Dim ini As New Carbon.IniFile With {.FileName = strSidecar}
                Dim stamp As DateTime = File.GetLastWriteTime(strSidecar)
                doc = NewDocument()
                Dim owners As List(Of String) = SiblingsOf(strSidecar)
                If strAlso IsNot Nothing AndAlso Not owners.Contains(strAlso, StringComparer.OrdinalIgnoreCase) Then owners.Add(strAlso)
                For Each owner In owners
                    Dim e As XElement = EnsureEntry(doc, owner)
                    For Each section In {"Create", "Exif"}
                        Dim keys As String() = If(ini.EnumerateKeys(section), New String() {})
                        If keys.Length = 0 Then Continue For
                        Dim s As New XElement(section)
                        For Each k In keys
                            s.Add(New XElement(XmlName(k), If(ini.SimpleGetValue(section, k), "")))
                        Next
                        e.Add(s)
                    Next
                    e.SetAttributeValue("modified", stamp.ToString("s"))
                Next
                Return doc
            Case Else
                Return NewDocument()
        End Select
    End Function

    Private Shared Function NewDocument() As XDocument
        Return New XDocument(New XDeclaration("1.0", "utf-8", Nothing), New XElement(RootName, New XAttribute("version", "1")))
    End Function

    ''' <summary>The album files (GetPhotoPatterns) of the side file's base name.</summary>
    Private Shared Function SiblingsOf(ByVal strSidecar As String) As List(Of String)
        Dim folder As String = Path.GetDirectoryName(strSidecar), baseName As String = Path.GetFileNameWithoutExtension(strSidecar)
        Try
            Return Directory.GetFiles(folder, baseName & ".*").
                   Where(Function(f) IsMediaFile(f) AndAlso String.Equals(Path.GetFileNameWithoutExtension(f), baseName, StringComparison.OrdinalIgnoreCase)).
                   OrderBy(Function(f) If(KindOf(f) = ImageName, 0, 1)).ToList()
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return New List(Of String)
        End Try
    End Function

    Private Shared Function EntryOf(ByVal doc As XDocument, ByVal strMediaFile As String) As XElement
        Dim name As String = Path.GetFileName(strMediaFile)
        Return doc.Root.Elements().FirstOrDefault(Function(x) String.Equals(CStr(x.Attribute("name")), name, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function EnsureEntry(ByVal doc As XDocument, ByVal strMediaFile As String) As XElement
        Dim e As XElement = EntryOf(doc, strMediaFile)
        If e IsNot Nothing Then Return e
        e = New XElement(KindOf(strMediaFile), New XAttribute("name", Path.GetFileName(strMediaFile)))
        doc.Root.Add(e)
        Return e
    End Function

    Private Shared Sub Touch(ByVal e As XElement)
        e.SetAttributeValue("modified", DateTime.Now.ToString("s"))
    End Sub

    ''' <summary>An ini key as an element name (the keys are plain words; anything else is encoded).</summary>
    Private Shared Function XmlName(ByVal key As String) As XName
        Return XmlConvert.EncodeLocalName(If(key, "").Trim())
    End Function

    ''' <summary>Written to a temporary file next to it, then put in place.</summary>
    Private Shared Sub Save(ByVal doc As XDocument, ByVal strSidecar As String)
        Dim temp As String = strSidecar & ".tmp"
        Using w As XmlWriter = XmlWriter.Create(temp, New XmlWriterSettings With {.Indent = True, .Encoding = New Text.UTF8Encoding(False)})
            doc.Save(w)
        End Using
        Dim attrs As FileAttributes? = Nothing
        If File.Exists(strSidecar) Then
            attrs = File.GetAttributes(strSidecar)
            If (attrs.Value And FileAttributes.ReadOnly) <> 0 Then File.SetAttributes(strSidecar, attrs.Value And Not FileAttributes.ReadOnly)
        End If
        File.Move(temp, strSidecar, True)
        s_cache.Remove(strSidecar)
        If attrs.HasValue Then File.SetAttributes(strSidecar, attrs.Value)
    End Sub

    Private Shared Sub DeleteQuietly(ByVal strFile As String)
        Try
            If File.Exists(strFile) Then
                File.SetAttributes(strFile, FileAttributes.Normal)
                File.Delete(strFile)
                s_cache.Remove(strFile)
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
    End Sub

End Class
