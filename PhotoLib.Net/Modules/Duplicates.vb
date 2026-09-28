Imports System.IO
Imports System.Security.Cryptography

' 尋找重複照片 (new in the .NET port). Only the album folders' own photos are looked at (the files a
' Class lists -- not covers, Restore\ copies or Thumb\ pictures):
'   exact     same size, then same SHA-256
'   similar   (optional) a 64-bit difference hash of the picture (Quartz.ImageHash, 9 x 8 grey): at most
'             SimilarBits bits apart -- bursts, a photo saved twice at
'             another size. The hashes are cached (CacheFile) by path, size and time, so a second scan is quick.
' Everything here only reads the photos and may run on a worker thread.
Public Module Duplicates

    Public Const SimilarBits As Integer = 4

    Public Class DupFile
        Public File As String = ""
        Public Size As Long
        Public Time As DateTime
        Public Width As Integer
        Public Height As Integer
        Public Rating As Integer
        ''' <summary>Suggested to keep (the best of the group).</summary>
        Public Keep As Boolean
    End Class

    Public Class DupGroup
        Public ReadOnly Files As New List(Of DupFile)
        ''' <summary>True: byte for byte the same; False: very similar pictures.</summary>
        Public Exact As Boolean
        Public Overrides Function ToString() As String
            Return If(Exact, "相同 ", "相似 ") & Files.Count & " 張：" & Path.GetFileName(Files(0).File) & "…"
        End Function
    End Class

    Public ReadOnly Property CacheFile As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iPhoto", "DupHash.txt")
        End Get
    End Property

    ''' <summary>The photos the albums hold: root \ folder \ album \ file (as Storage / Class see them).</summary>
    Public Function AlbumPhotos(ByVal roots As IEnumerable(Of String)) As List(Of String)
        Dim result As New List(Of String)
        For Each root In roots.Where(Function(r) Directory.Exists(r)).Distinct(StringComparer.OrdinalIgnoreCase)
            Dim sections() As String = Nothing
            ExactFolders(root, sections)
            For Each section In sections.Where(Function(f) Not IsSystemFolder(f))
                Dim albums() As String = Nothing
                ExactFolders(section, albums)
                For Each album In albums.Where(Function(f) Not IsSystemFolder(f))
                    Dim files() As String = Nothing
                    ExactFiles(album, GetPhotoPatterns(), files, True)
                    result.AddRange(files)
                Next
            Next
        Next
        Return result
    End Function

    ''' <summary>The groups of duplicates, biggest first. <paramref name="progress"/> gets (step text, done, total);
    ''' <paramref name="cancel"/> stops early (what was found so far is returned).</summary>
    Public Function Find(ByVal roots As IEnumerable(Of String), ByVal similar As Boolean,
                         Optional ByVal progress As Action(Of String, Integer, Integer) = Nothing,
                         Optional ByVal cancel As Threading.CancellationToken = Nothing) As List(Of DupGroup)
        Dim files As List(Of String) = AlbumPhotos(roots)
        Dim infos As New Dictionary(Of String, DupFile)(StringComparer.OrdinalIgnoreCase)
        For Each f In files
            Try
                Dim fi As New FileInfo(f)
                infos(f) = New DupFile With {.File = f, .Size = fi.Length, .Time = fi.LastWriteTime}
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
        Next
        Dim groups As New List(Of DupGroup)
        Dim inExact As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        ' exact: same size, then same content
        Dim sameSize = infos.Values.Where(Function(x) x.Size > 0).GroupBy(Function(x) x.Size).Where(Function(g) g.Count() > 1).ToList()
        Dim toHash As Integer = sameSize.Sum(Function(g) g.Count()), hashed As Integer = 0
        For Each g In sameSize
            If cancel.IsCancellationRequested Then Exit For
            Dim byHash As New Dictionary(Of String, List(Of DupFile))
            For Each x In g
                Dim h As String = Sha(x.File)
                hashed += 1
                If progress IsNot Nothing AndAlso hashed Mod 20 = 0 Then progress("比對檔案內容", hashed, toHash)
                If h Is Nothing Then Continue For
                If Not byHash.ContainsKey(h) Then byHash(h) = New List(Of DupFile)
                byHash(h).Add(x)
            Next
            For Each same In byHash.Values.Where(Function(l) l.Count > 1)
                Dim dg As New DupGroup With {.Exact = True}
                dg.Files.AddRange(same)
                groups.Add(dg)
                For Each x In same
                    inExact.Add(x.File)
                Next
            Next
        Next

        ' similar: close picture hashes (a file already in an exact group only once: its first copy)
        If similar AndAlso Not cancel.IsCancellationRequested Then
            Dim cache As Dictionary(Of String, String) = LoadCache()
            Dim pics = infos.Values.Where(Function(x) GetMediaType(Nothing, x.File) = enumPhotoMediaType.mdImage).ToList()
            Dim hashes As New List(Of (Info As DupFile, Hash As ULong))
            For i = 0 To pics.Count - 1
                If cancel.IsCancellationRequested Then Exit For
                Dim x As DupFile = pics(i)
                Dim key As String = "3|" & x.File & "|" & x.Size & "|" & x.Time.Ticks   ' "2": the hash method (a new one makes old entries unused)
                Dim hv As ULong
                Dim cached As String = Nothing
                If cache.TryGetValue(key, cached) AndAlso ULong.TryParse(cached, Globalization.NumberStyles.HexNumber, Nothing, hv) Then
                    If hv <> 0UL Then hashes.Add((x, hv))   ' 0: too dark / flat to compare
                Else
                    Dim got As ULong? = PictureHash(x.File)
                    If got.HasValue Then
                        If got.Value <> 0UL Then hashes.Add((x, got.Value))
                        cache(key) = got.Value.ToString("x16")
                    End If
                End If
                If progress IsNot Nothing AndAlso (i Mod 25 = 0 OrElse i = pics.Count - 1) Then progress("比對畫面", i + 1, pics.Count)
            Next
            SaveCache(cache)
            ' groups around one photo each: a photo and the ones close to IT (not close to one of those, which
            ' chained unrelated pictures together); pairs that are exact copies were grouped already
            Dim vals() As ULong = hashes.Select(Function(x) x.Hash).ToArray()   ' ~n²/2 comparisons: a plain array
            Dim taken(vals.Length - 1) As Boolean
            Dim clusters As New List(Of List(Of Integer))
            For i = 0 To vals.Length - 1
                If cancel.IsCancellationRequested Then Exit For
                If taken(i) Then Continue For
                Dim hi As ULong = vals(i)
                Dim near As New List(Of Integer) From {i}
                For j = i + 1 To vals.Length - 1
                    If Not taken(j) AndAlso BitCount(hi Xor vals(j)) <= SimilarBits Then near.Add(j)
                Next
                If near.Count < 2 Then Continue For
                For Each k In near
                    taken(k) = True
                Next
                clusters.Add(near)
            Next
            For Each g In clusters
                Dim members = g.Select(Function(i) hashes(i).Info).ToList()
                ' only exact copies of one another: already an exact group
                If members.All(Function(m) inExact.Contains(m.File)) AndAlso
                   groups.Any(Function(eg) eg.Exact AndAlso members.All(Function(m) eg.Files.Contains(m))) Then Continue For
                Dim dg As New DupGroup With {.Exact = False}
                dg.Files.AddRange(members)
                groups.Add(dg)
            Next
        End If

        For Each g In groups
            Describe(g)
        Next
        Return groups.OrderByDescending(Function(g) g.Files.Count).ThenBy(Function(g) g.Files(0).File).ToList()
    End Function

    ''' <summary>Size, rating and which one to keep: the best rated, then the biggest picture, then the
    ''' oldest file.</summary>
    Private Sub Describe(ByVal g As DupGroup)
        For Each f In g.Files
            Try
                Using fs As New FileStream(f.File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), img As Image = Image.FromStream(fs, False, False)
                    f.Width = img.Width
                    f.Height = img.Height
                End Using
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is UnauthorizedAccessException
            End Try
            Dim p As New Photo
            p.Construct(f.File)
            f.Rating = CInt(Val(p.Exif(enumPhotoExif.peRanking)))
        Next
        Dim best As DupFile = g.Files.OrderByDescending(Function(f) f.Rating).ThenByDescending(Function(f) CLng(f.Width) * f.Height).
                                     ThenByDescending(Function(f) f.Size).ThenBy(Function(f) f.Time).First()
        For Each f In g.Files
            f.Keep = f Is best
        Next
    End Sub

    Private Function Sha(ByVal file As String) As String
        Try
            Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), sha256 As SHA256 = SHA256.Create()
                Return Convert.ToHexString(sha256.ComputeHash(fs))
            End Using
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

    ''' <summary>The picture's difference hash (Quartz.ImageHash: decoded at 1/8 size, so a photo and a copy
    ''' saved at another size hash alike -- not from the JPEG's embedded thumbnail, which a copy may lack).</summary>
    Public Function PictureHash(ByVal file As String) As ULong?
        Return Quartz.ImageHash.DHash(file)
    End Function
    Private Function BitCount(ByVal v As ULong) As Integer
        Return Numerics.BitOperations.PopCount(v)
    End Function

    Private Function LoadCache() As Dictionary(Of String, String)
        Dim d As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Try
            If File.Exists(CacheFile) Then
                For Each line In File.ReadLines(CacheFile, Text.Encoding.UTF8)
                    Dim tab As Integer = line.LastIndexOf(ChrW(9))
                    If tab > 0 Then d(line.Substring(0, tab)) = line.Substring(tab + 1)
                Next
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        Return d
    End Function

    Private Sub SaveCache(ByVal d As Dictionary(Of String, String))
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(CacheFile))
            File.WriteAllLines(CacheFile, d.Select(Function(kv) kv.Key & ChrW(9) & kv.Value), Text.Encoding.UTF8)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
    End Sub

End Module
