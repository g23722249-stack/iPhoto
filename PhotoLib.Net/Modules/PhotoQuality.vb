Imports System.IO

' 整理清單's picture checks (new in the .NET port): photos that are probably not worth keeping --
'   模糊       Quartz.ImageQuality.Sharpness below BlurLimit (the sharpest ninth of the picture; a sharp
'              subject on a soft background passes)
'   太暗       mostly black: Mean below DarkMean, or DarkShare above DarkLimit
'   過曝       mostly white: Mean above BrightMean, or BrightShare above BrightLimit
' A too dark / washed-out picture is reported as that, not also as blurry (it has little detail anyway).
' The limits were set on the test library: every photo the eye calls sharp measured above 300, but one
' (motion blur, 102); copies blurred by 0.4% of their width measured 6..66.
' The measures are cached (CacheFile) by path, size and time, as Duplicates' picture hashes are.
' Only reads the photos; may run on a worker thread.
Public Module PhotoQuality

    Public Const BlurLimit As Double = 150
    Public Const DarkMean As Double = 35, DarkLimit As Double = 0.7
    Public Const BrightMean As Double = 228, BrightLimit As Double = 0.75

    Public Enum Problem
        None
        Blurry
        Dark
        Bright
    End Enum

    Public ReadOnly Property CacheFile As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "Quality.txt")
        End Get
    End Property

    Public Function ProblemOf(ByVal q As Quartz.ImageQuality) As Problem
        If q Is Nothing Then Return Problem.None
        If q.Mean < DarkMean OrElse q.DarkShare > DarkLimit Then Return Problem.Dark
        If q.Mean > BrightMean OrElse q.BrightShare > BrightLimit Then Return Problem.Bright
        If q.Sharpness < BlurLimit Then Return Problem.Blurry
        Return Problem.None
    End Function

    Public Function ProblemText(ByVal p As Problem) As String
        Select Case p
            Case Problem.Blurry : Return "模糊"
            Case Problem.Dark : Return "太暗"
            Case Problem.Bright : Return "過曝"
            Case Else : Return ""
        End Select
    End Function

    ''' <summary>The pictures among <paramref name="files"/> with one of the <paramref name="wanted"/> problems,
    ''' as groups of one problem in one album ("模糊 5 張：相簿"), every photo suggested to keep (nothing ticked):
    ''' the eye decides. <paramref name="progress"/> gets (step text, done, total).</summary>
    Public Function Find(ByVal files As List(Of String), ByVal wanted As ICollection(Of Problem),
                         Optional ByVal progress As Action(Of String, Integer, Integer) = Nothing,
                         Optional ByVal cancel As Threading.CancellationToken = Nothing) As List(Of Duplicates.DupGroup)
        Dim cache As Dictionary(Of String, String) = Duplicates.LoadCache(CacheFile)
        Dim pics As List(Of String) = files.Where(Function(f) GetMediaType(Nothing, f) = enumPhotoMediaType.mdImage).ToList()
        Dim found As New List(Of (File As String, Problem As Problem, Q As Quartz.ImageQuality))
        For i = 0 To pics.Count - 1
            If cancel.IsCancellationRequested Then Exit For
            Dim f As String = pics(i)
            Dim q As Quartz.ImageQuality = Nothing
            Try
                Dim fi As New FileInfo(f)
                Dim key As String = "1|" & f & "|" & fi.Length & "|" & fi.LastWriteTime.Ticks
                Dim cached As String = Nothing
                If cache.TryGetValue(key, cached) Then
                    q = Parse(cached)
                Else
                    q = Quartz.ImageQuality.Measure(f)
                    If q IsNot Nothing Then cache(key) = Format(q)
                End If
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
            Dim p As Problem = ProblemOf(q)
            If p <> Problem.None AndAlso wanted.Contains(p) Then found.Add((f, p, q))
            If progress IsNot Nothing AndAlso (i Mod 25 = 0 OrElse i = pics.Count - 1) Then progress("檢查畫面", i + 1, pics.Count)
        Next
        Duplicates.SaveCache(cache, CacheFile)

        Dim groups As New List(Of Duplicates.DupGroup)
        For Each g In found.GroupBy(Function(x) (x.Problem, Path.GetDirectoryName(x.File).ToLowerInvariant())).
                            OrderBy(Function(x) x.Key.Problem).ThenBy(Function(x) x.Key.Item2)
            Dim dg As New Duplicates.DupGroup With {
                .Title = ProblemText(g.Key.Problem) & " " & g.Count() & " 張：" & Path.GetFileName(Path.GetDirectoryName(g.First().File))}
            For Each x In g
                Dim df As New Duplicates.DupFile With {.File = x.File, .Keep = True, .Note = Describe(x.Problem, x.Q)}
                Try
                    Dim fi As New FileInfo(x.File)
                    df.Size = fi.Length
                    df.Time = fi.LastWriteTime
                Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                End Try
                Duplicates.FillInfo(df)
                dg.Files.Add(df)
            Next
            groups.Add(dg)
        Next
        Return groups
    End Function

    Private Function Describe(ByVal p As Problem, ByVal q As Quartz.ImageQuality) As String
        Select Case p
            Case Problem.Blurry : Return $"模糊（清晰度 {q.Sharpness:0}）"
            Case Problem.Dark : Return $"太暗（平均亮度 {q.Mean:0}）"
            Case Problem.Bright : Return $"過曝（平均亮度 {q.Mean:0}）"
            Case Else : Return ""
        End Select
    End Function

    Private Function Format(ByVal q As Quartz.ImageQuality) As String
        Dim inv As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
        Return String.Join(";", {q.Sharpness, q.Mean, q.DarkShare, q.BrightShare}.Select(Function(v) v.ToString("0.###", inv)))
    End Function

    Private Function Parse(ByVal text As String) As Quartz.ImageQuality
        Dim v() As String = text.Split(";"c)
        If v.Length <> 4 Then Return Nothing
        Dim d(3) As Double
        For i = 0 To 3
            If Not Double.TryParse(v(i), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, d(i)) Then Return Nothing
        Next
        Return New Quartz.ImageQuality With {.Sharpness = d(0), .Mean = d(1), .DarkShare = d(2), .BrightShare = d(3)}
    End Function

End Module
