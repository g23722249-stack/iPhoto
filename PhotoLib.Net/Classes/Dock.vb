Imports System.Data
Imports System.IO

' Port of 電子相簿\Lib\Class\Dock.cls: the "Dock" -- photos the user picked for export / print /
' slide show, kept across runs in &lt;app&gt;\~Dock.dck (lines "file|yyyyMMdd|HHmmss", ANSI).
'
' Differences from VB6:
'   - Clear deleted App.Path & "~Dock.dck" (no backslash), i.e. a file next to the app folder; it
'     deletes &lt;app&gt;\~Dock.dck now.
'   - SortRecordSet returns a sorted DataTable (FileDesc, SortDate, SortTime) instead of an ADODB.Recordset.
'   - A docked file deleted meanwhile no longer raises an error when its size is subtracted.
Public Class Dock

    Private Class DockFile
        Public FileDesc As String
        Public ShortFileDesc As String
        Public FileName As String
        Public ExtensionName As String
        Public CreateDate As String
        Public CreateTime As String
        Public Length As Long
    End Class

    Private ReadOnly m_lpDockImage As New List(Of DockFile)
    Private ReadOnly m_lpFileSystem As New Carbon.FileSystem
    Private m_lngFilelengthByte As Long
    Private m_dblFileLength As Double
    Private m_strFileLengthUnit As String = ""

    Private Shared ReadOnly Property DefaultFile As String
        Get
            Return Path.Combine(Application.StartupPath, "~Dock.dck")
        End Get
    End Property

    Public Sub Save(Optional ByVal strFileDesc As String = "")
        If strFileDesc.Trim().Length = 0 Then strFileDesc = DefaultFile
        m_lpFileSystem.DeleteFile(strFileDesc)
        If m_lpDockImage.Count = 0 Then Return
        Using w As New StreamWriter(strFileDesc, False, AnsiText.Encoding)
            For Each d In m_lpDockImage
                w.WriteLine(d.FileDesc & "|" & d.CreateDate & "|" & d.CreateTime)
            Next
        End Using
    End Sub

    ''' <summary>Reloads the dock from the file, dropping photos that no longer exist.</summary>
    Public Sub Restore(Optional ByVal strFileDesc As String = "")
        If strFileDesc.Trim().Length = 0 Then strFileDesc = DefaultFile
        If Not m_lpFileSystem.FileExists(strFileDesc) Then Return
        m_lpDockImage.Clear()
        m_lngFilelengthByte = 0
        For Each line In File.ReadAllLines(strFileDesc, AnsiText.Encoding)
            If line.Trim().Length = 0 Then Continue For
            Dim parts As String() = line.Split("|"c)
            ReDim Preserve parts(2)
            If m_lpFileSystem.FileExists(parts(0)) Then AddItem(parts(0), If(parts(1), ""), If(parts(2), ""))
        Next
    End Sub

    ''' <summary>The docked files sorted by date/time (columns FileDesc, SortDate, SortTime).</summary>
    Public ReadOnly Property SortRecordSet As DataTable
        Get
            Dim t As New DataTable("Dock")
            t.Columns.Add("FileDesc", GetType(String))
            t.Columns.Add("SortDate", GetType(String))
            t.Columns.Add("SortTime", GetType(String))
            For Each d In m_lpDockImage
                t.Rows.Add(d.FileDesc, d.CreateDate, d.CreateTime)
            Next
            t.DefaultView.Sort = "SortDate ASC, SortTime ASC"
            Return t.DefaultView.ToTable()
        End Get
    End Property

    Public ReadOnly Property CreateDate(ByVal Index As Integer) As String
        Get
            Return m_lpDockImage(Index).CreateDate
        End Get
    End Property

    Public ReadOnly Property CreateTime(ByVal Index As Integer) As String
        Get
            Return m_lpDockImage(Index).CreateTime
        End Get
    End Property

    ''' <summary>Full path of the docked file at Index.</summary>
    Public ReadOnly Property FileName(ByVal Index As Integer) As String
        Get
            Return m_lpDockImage(Index).FileDesc
        End Get
    End Property

    Public ReadOnly Property FileLength As Double
        Get
            Return m_dblFileLength
        End Get
    End Property

    Public ReadOnly Property FileLengthUnit As String
        Get
            Return m_strFileLengthUnit
        End Get
    End Property

    Public ReadOnly Property Count As Integer
        Get
            Return m_lpDockImage.Count
        End Get
    End Property

    Public Sub Clear()
        m_lpDockImage.Clear()
        m_lngFilelengthByte = 0
        m_lpFileSystem.DeleteFile(DefaultFile)   ' VB6 dropped the "\" and deleted the wrong file
    End Sub

    Public Function Selected(ByVal FileName As String) As Boolean
        Return IndexOf(FileName) >= 0
    End Function

    Public Function Docking(ByVal FileName As String) As Boolean
        Return IndexOf(FileName) >= 0
    End Function

    ''' <summary>Adds a file (False if it's already docked).</summary>
    Public Function AddItem(ByVal FileName As String, Optional ByVal CreateDate As String = "00000000", Optional ByVal CreateTime As String = "000000") As Boolean
        If IndexOf(FileName) >= 0 Then Return False
        Dim len As Long = If(File.Exists(FileName), New FileInfo(FileName).Length, 0)
        m_lpDockImage.Add(New DockFile With {
            .FileDesc = FileName,
            .ShortFileDesc = m_lpFileSystem.AnalyseFile(fsShortFileDsce, FileName),
            .FileName = m_lpFileSystem.AnalyseFile(fsFileName, FileName),
            .ExtensionName = m_lpFileSystem.AnalyseFile(fsExtensionName, FileName),
            .CreateDate = CreateDate,
            .CreateTime = CreateTime,
            .Length = len})
        m_lngFilelengthByte += len
        CalcFileLength(m_lngFilelengthByte, m_dblFileLength, m_strFileLengthUnit)
        Return True
    End Function

    ''' <summary>Removes a file; always True (VB6).</summary>
    Public Function RemoveItem(ByVal FileName As String) As Boolean
        Dim i As Integer = IndexOf(FileName)
        If i < 0 Then Return True
        m_lngFilelengthByte -= m_lpDockImage(i).Length
        m_lpDockImage.RemoveAt(i)
        CalcFileLength(m_lngFilelengthByte, m_dblFileLength, m_strFileLengthUnit)
        Return True
    End Function

    ''' <summary>The docked files sorted by date/time.</summary>
    Public Function SortFiles(ByRef strFiles() As String) As Integer
        strFiles = m_lpDockImage.Where(Function(d) d.FileDesc.Trim().Length > 0).
            OrderBy(Function(d) d.CreateDate, StringComparer.Ordinal).ThenBy(Function(d) d.CreateTime, StringComparer.Ordinal).
            Select(Function(d) d.FileDesc.Trim()).ToArray()
        Return strFiles.Length
    End Function

    Private Function IndexOf(ByVal fileName As String) As Integer
        Dim target As String = If(fileName, "").Trim()
        For i = 0 To m_lpDockImage.Count - 1
            If String.Equals(m_lpDockImage(i).FileDesc.Trim(), target, StringComparison.OrdinalIgnoreCase) Then Return i
        Next
        Return -1
    End Function

End Class
