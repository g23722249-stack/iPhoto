Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json

' 加密隱藏 (new in the .NET port): photos taken out of their album into the library's vault folder
' (<library>\System\Vault), encrypted, and shown again only while the 金鑰密碼 has been given.
'
' Keys
'   A random 256-bit master key encrypts everything. The database (VaultKey, Database.Vault.vb) keeps
'   it wrapped twice -- by a key derived from the 金鑰密碼 (PBKDF2-SHA256) and by one derived from the
'   救援碼 -- with AES-GCM: a wrong password simply fails to unwrap it. Changing the password only wraps
'   the same master key again; nothing is encrypted anew. The first password is 123456 (the key is made
'   the first time it is given).
'
' One encrypted item (one photo or video as the album showed it)
'   <id>.ipx   the photo / video itself      (chunked AES-GCM, below) -- the list item's file name
'   <id>.ipm   what it was (VaultMeta, JSON, one AES-GCM block): its album, its .Exif entry, its side
'              files, the photo books it was in, its rating
'   <id>.<n>.ipb  each side file (Restore\ copy, PhotoEdit's .pedit.json / .pedx, a video's Thumb\
'              picture, an edited picture's original and earlier versions), chunked as .ipx
'
' Chunked format: "IPX1" | chunk size (Int32) | nonce prefix (8 random bytes) | plain length (Int64),
'   then per chunk the cipher text and its 16-byte tag. Chunk i's nonce is the prefix and i (big-endian
'   Int32); the 24-byte header is the associated data of every chunk, so a cut or swapped file fails.
Public Module PhotoVault

    Public Const DefaultPassword As String = "123456"
    Public Const ItemExt As String = ".ipx"
    Private Const MetaExt As String = ".ipm"
    Private Const SideExt As String = ".ipb"
    Private Const Iterations As Integer = 200000
    Private Const ChunkSize As Integer = 1 << 20
    Private Const TagSize As Integer = 16
    Private Const NonceSize As Integer = 12
    Private ReadOnly FileMagic As Byte() = Encoding.ASCII.GetBytes("IPX1")
    Private ReadOnly MetaMagic As Byte() = Encoding.ASCII.GetBytes("IPM1")

    Private ReadOnly s_lock As New Object
    Private s_key As Byte()            ' the master key while unlocked
    Private s_defaultPassword As Boolean
    Private s_folder As String = ""
    Private ReadOnly s_meta As New Dictionary(Of String, VaultMeta)(StringComparer.OrdinalIgnoreCase)   ' by .ipx path, while unlocked

    ''' <summary>Unlocked or locked (on the UI thread of whoever called Unlock / Lock).</summary>
    Public Event StateChanged As EventHandler

    ''' <summary>The vault folder (made when the first photo goes in).</summary>
    Public Property Folder As String
        Get
            Return s_folder
        End Get
        Set(value As String)
            s_folder = If(value, "")
        End Set
    End Property

    Public ReadOnly Property IsUnlocked As Boolean
        Get
            SyncLock s_lock
                Return s_key IsNot Nothing
            End SyncLock
        End Get
    End Property

    ''' <summary>Unlocked with 123456: it should be changed before photos go in.</summary>
    Public ReadOnly Property UsesDefaultPassword As Boolean
        Get
            Return s_defaultPassword
        End Get
    End Property

    ''' <summary>True when a 救援碼 has been made.</summary>
    Public ReadOnly Property HasRecoveryCode As Boolean
        Get
            Return Values().ContainsKey("RcWrap")
        End Get
    End Property

    '==================================================================================================
    ' Password, recovery code
    '==================================================================================================
    Private Function Values() As Dictionary(Of String, String)
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then Return New Dictionary(Of String, String)
        Return g_lpDatabase.VaultValues()
    End Function

    ''' <summary>False for a wrong password. The first time, 123456 makes the master key.</summary>
    Public Function Unlock(ByVal password As String) As Boolean
        If String.IsNullOrEmpty(password) OrElse g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then Return False
        Dim v As Dictionary(Of String, String) = Values()
        Dim key As Byte()
        If Not v.ContainsKey("PwWrap") Then
            If password <> DefaultPassword Then Return False
            key = RandomNumberGenerator.GetBytes(32)
            g_lpDatabase.SetVaultValues(Wrapped("Pw", key, password))
        Else
            key = Unwrap(v, "Pw", password)
            If key Is Nothing Then Return False
        End If
        SetKey(key, password = DefaultPassword)
        Return True
    End Function

    ''' <summary>Unlocks with the 救援碼 (any case, dashes and spaces left out); False when it is wrong.</summary>
    Public Function UnlockWithRecovery(ByVal code As String) As Boolean
        Dim key As Byte() = Unwrap(Values(), "Rc", NormalizeCode(code))
        If key Is Nothing Then Return False
        SetKey(key, False)
        Return True
    End Function

    ''' <summary>The master key wrapped by a new password (unlocked only).</summary>
    Public Function ChangePassword(ByVal password As String) As Boolean
        If String.IsNullOrEmpty(password) Then Return False
        Dim key As Byte() = CopyKey()
        If key Is Nothing Then Return False
        g_lpDatabase.SetVaultValues(Wrapped("Pw", key, password))
        s_defaultPassword = password = DefaultPassword
        Return True
    End Function

    ''' <summary>A new 救援碼 (unlocked only), shown once: only the key it wraps is kept. An earlier code
    ''' stops working.</summary>
    Public Function NewRecoveryCode() As String
        Dim key As Byte() = CopyKey()
        If key Is Nothing Then Return Nothing
        Const alphabet As String = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"   ' no 0/O, 1/I
        Dim raw As Byte() = RandomNumberGenerator.GetBytes(24)
        Dim sb As New StringBuilder
        For i = 0 To raw.Length - 1
            If i > 0 AndAlso i Mod 4 = 0 Then sb.Append("-"c)
            sb.Append(alphabet(raw(i) And 31))
        Next
        Dim code As String = sb.ToString()
        g_lpDatabase.SetVaultValues(Wrapped("Rc", key, NormalizeCode(code)))
        Return code
    End Function

    Private Function NormalizeCode(ByVal code As String) As String
        Return New String(If(code, "").ToUpperInvariant().Where(Function(c) Char.IsLetterOrDigit(c)).ToArray())
    End Function

    ''' <summary>Forgets the key and the decrypted descriptions; the caller drops what it shows.</summary>
    Public Sub Lock()
        SyncLock s_lock
            If s_key Is Nothing Then Return
            CryptographicOperations.ZeroMemory(s_key)
            s_key = Nothing
            s_meta.Clear()
        End SyncLock
        s_defaultPassword = False
        RaiseEvent StateChanged(Nothing, EventArgs.Empty)   ' first: the viewer lets go of a video playing
        DeleteTempFiles()
    End Sub

    Private Sub SetKey(ByVal key As Byte(), ByVal isDefault As Boolean)
        SyncLock s_lock
            If s_key IsNot Nothing Then CryptographicOperations.ZeroMemory(s_key)
            s_key = key
            s_meta.Clear()
        End SyncLock
        s_defaultPassword = isDefault
        RaiseEvent StateChanged(Nothing, EventArgs.Empty)
    End Sub

    Private Function CopyKey() As Byte()
        SyncLock s_lock
            Return If(s_key Is Nothing, Nothing, CType(s_key.Clone(), Byte()))
        End SyncLock
    End Function

    Private Function KeyOrThrow() As Byte()
        Dim k As Byte() = CopyKey()
        If k Is Nothing Then Throw New InvalidOperationException("加密照片尚未解鎖")
        Return k
    End Function

    ''' <summary>The rows <prefix>Salt / <prefix>Wrap (nonce, cipher text, tag) and Iter for the key wrapped by <paramref name="secret"/>.</summary>
    Private Function Wrapped(ByVal prefix As String, ByVal key As Byte(), ByVal secret As String) As Dictionary(Of String, String)
        Dim salt As Byte() = RandomNumberGenerator.GetBytes(16)
        Dim kek As Byte() = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(secret), salt, Iterations, HashAlgorithmName.SHA256, 32)
        Dim nonce As Byte() = RandomNumberGenerator.GetBytes(NonceSize)
        Dim ct(key.Length - 1) As Byte, tag(TagSize - 1) As Byte
        Using g As New AesGcm(kek, TagSize)
            g.Encrypt(nonce, key, ct, tag, Encoding.ASCII.GetBytes(prefix))
        End Using
        CryptographicOperations.ZeroMemory(kek)
        Return New Dictionary(Of String, String) From {
            {"Version", "1"}, {prefix & "Iter", CStr(Iterations)},
            {prefix & "Salt", Convert.ToBase64String(salt)},
            {prefix & "Wrap", Convert.ToBase64String(nonce.Concat(ct).Concat(tag).ToArray())}}
    End Function

    Private Function Unwrap(ByVal v As Dictionary(Of String, String), ByVal prefix As String, ByVal secret As String) As Byte()
        Dim saltText As String = Nothing, wrapText As String = Nothing, iterText As String = Nothing
        If String.IsNullOrEmpty(secret) OrElse Not v.TryGetValue(prefix & "Salt", saltText) OrElse Not v.TryGetValue(prefix & "Wrap", wrapText) Then Return Nothing
        Dim iter As Integer = If(v.TryGetValue(prefix & "Iter", iterText), CInt(Val(iterText)), Iterations)
        Try
            Dim salt As Byte() = Convert.FromBase64String(saltText), wrap As Byte() = Convert.FromBase64String(wrapText)
            If wrap.Length <> NonceSize + 32 + TagSize Then Return Nothing
            Dim kek As Byte() = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(secret), salt, Math.Max(1000, iter), HashAlgorithmName.SHA256, 32)
            Dim key(31) As Byte
            Using g As New AesGcm(kek, TagSize)
                g.Decrypt(wrap.AsSpan(0, NonceSize), wrap.AsSpan(NonceSize, 32), wrap.AsSpan(NonceSize + 32, TagSize), key, Encoding.ASCII.GetBytes(prefix))
            End Using
            CryptographicOperations.ZeroMemory(kek)
            Return key
        Catch ex As Exception When TypeOf ex Is CryptographicException OrElse TypeOf ex Is FormatException
            Return Nothing
        End Try
    End Function

    '==================================================================================================
    ' Files
    '==================================================================================================
    ''' <summary>Encrypts <paramref name="source"/> into <paramref name="target"/> (written beside it first, then renamed).</summary>
    Public Sub EncryptFile(ByVal source As String, ByVal target As String)
        Dim key As Byte() = KeyOrThrow()
        Dim temp As String = target & ".tmp"
        Try
            Using input As New FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read),
                  output As New FileStream(temp, FileMode.Create, FileAccess.Write)
                Dim header(23) As Byte
                FileMagic.CopyTo(header, 0)
                BitConverter.GetBytes(ChunkSize).CopyTo(header, 4)
                RandomNumberGenerator.Fill(header.AsSpan(8, 8))
                BitConverter.GetBytes(input.Length).CopyTo(header, 16)
                output.Write(header, 0, header.Length)
                Dim plain(ChunkSize - 1) As Byte, ct(ChunkSize - 1) As Byte, tag(TagSize - 1) As Byte
                Dim index As Integer = 0
                Using g As New AesGcm(key, TagSize)
                    Do
                        Dim n As Integer = ReadFull(input, plain, ChunkSize)
                        If n = 0 AndAlso index > 0 Then Exit Do
                        g.Encrypt(ChunkNonce(header, index), plain.AsSpan(0, n), ct.AsSpan(0, n), tag, header)
                        output.Write(ct, 0, n)
                        output.Write(tag, 0, TagSize)
                        index += 1
                        If n < ChunkSize Then Exit Do
                    Loop
                End Using
            End Using
            If File.Exists(target) Then File.Delete(target)
            File.Move(temp, target)
        Finally
            CryptographicOperations.ZeroMemory(key)
            If File.Exists(temp) Then File.Delete(temp)
        End Try
    End Sub

    ''' <summary>Decrypts <paramref name="source"/> into <paramref name="output"/>; throws
    ''' CryptographicException when the file was changed or the key is another.</summary>
    Public Sub DecryptTo(ByVal source As String, ByVal output As Stream)
        Dim key As Byte() = KeyOrThrow()
        Try
            Using input As New FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim header(23) As Byte
                If ReadFull(input, header, 24) <> 24 OrElse Not header.AsSpan(0, 4).SequenceEqual(FileMagic) Then Throw New CryptographicException("not an encrypted file")
                Dim size As Integer = BitConverter.ToInt32(header, 4)
                Dim length As Long = BitConverter.ToInt64(header, 16)
                If size <= 0 OrElse size > 64 * ChunkSize OrElse length < 0 Then Throw New CryptographicException("bad header")
                Dim ct(size - 1) As Byte, plain(size - 1) As Byte, tag(TagSize - 1) As Byte
                Dim index As Integer = 0, left As Long = length
                Using g As New AesGcm(key, TagSize)
                    Do
                        Dim n As Integer = CInt(Math.Min(size, left))
                        If ReadFull(input, ct, n) <> n OrElse ReadFull(input, tag, TagSize) <> TagSize Then Throw New CryptographicException("cut short")
                        g.Decrypt(ChunkNonce(header, index), ct.AsSpan(0, n), tag, plain.AsSpan(0, n), header)
                        output.Write(plain, 0, n)
                        left -= n
                        index += 1
                    Loop While left > 0
                End Using
            End Using
        Finally
            CryptographicOperations.ZeroMemory(key)
        End Try
    End Sub

    ''' <summary>The decrypted content in memory.</summary>
    Public Function DecryptToMemory(ByVal source As String) As MemoryStream
        Dim ms As New MemoryStream
        DecryptTo(source, ms)
        ms.Position = 0
        Return ms
    End Function

    ''' <summary>Decrypts into <paramref name="target"/> (written beside it first, then renamed).</summary>
    Public Sub DecryptFile(ByVal source As String, ByVal target As String)
        Dim temp As String = target & ".tmp"
        Try
            Using fs As New FileStream(temp, FileMode.Create, FileAccess.Write)
                DecryptTo(source, fs)
            End Using
            If File.Exists(target) Then File.Delete(target)
            File.Move(temp, target)
        Finally
            If File.Exists(temp) Then File.Delete(temp)
        End Try
    End Sub

    Private Function ChunkNonce(ByVal header As Byte(), ByVal index As Integer) As Byte()
        Dim nonce(NonceSize - 1) As Byte
        Array.Copy(header, 8, nonce, 0, 8)
        nonce(8) = CByte((index >> 24) And &HFF) : nonce(9) = CByte((index >> 16) And &HFF)
        nonce(10) = CByte((index >> 8) And &HFF) : nonce(11) = CByte(index And &HFF)
        Return nonce
    End Function

    Private Function ReadFull(ByVal s As Stream, ByVal buffer As Byte(), ByVal count As Integer) As Integer
        Dim total As Integer = 0
        While total < count
            Dim n As Integer = s.Read(buffer, total, count - total)
            If n = 0 Then Exit While
            total += n
        End While
        Return total
    End Function

    '==================================================================================================
    ' Items
    '==================================================================================================
    ''' <summary>What an encrypted item was. Paths are relative to the vault folder (LibraryPath).</summary>
    Public Class VaultMeta
        Public Property Name As String = ""            ' the file the album showed (IMG_1_edited.jpg)
        Public Property Album As String = ""           ' its folder
        Public Property Video As Boolean
        Public Property Ranking As Integer
        Public Property ShotDate As String = ""        ' yyyyMMdd, for the order of 加密照片
        Public Property Title As String = ""
        Public Property Encrypted As DateTime
        Public Property Members As New List(Of VaultMember)   ' the first is Name itself
        Public Property Books As New List(Of String)
        Public Property Docked As Boolean
    End Class

    ''' <summary>One file of the item with its .Exif entry and side files.</summary>
    Public Class VaultMember
        Public Property Name As String = ""
        Public Property Blob As String = ""            ' file name in the vault folder
        Public Property Exif As String                 ' the entry's XML, Nothing when it had none
        Public Property Sides As New List(Of VaultSide)
    End Class

    Public Class VaultSide
        Public Property Path As String = ""            ' relative to the album folder: Restore\IMG_1.JPG
        Public Property Blob As String = ""
    End Class

    ''' <summary>A new item's .ipx path (the id is random).</summary>
    Public Function NewItemPath() As String
        Directory.CreateDirectory(s_folder)
        Return Path.Combine(s_folder, Guid.NewGuid().ToString("N") & ItemExt)
    End Function

    ''' <summary>The file name of side blob <paramref name="n"/> of an item.</summary>
    Public Function SideBlobName(ByVal item As String, ByVal n As Integer) As String
        Return Path.GetFileNameWithoutExtension(item) & "." & n & SideExt
    End Function

    ''' <summary>True for an encrypted item's file (the list item's file name).</summary>
    Public Function IsVaultItem(ByVal file As String) As Boolean
        Return Not String.IsNullOrEmpty(file) AndAlso file.EndsWith(ItemExt, StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function BlobPath(ByVal name As String) As String
        Return Path.Combine(s_folder, name)
    End Function

    ''' <summary>A path kept in a VaultMeta, as a full path again.</summary>
    Public Function FullPath(ByVal stored As String) As String
        Return LibraryPath.FromRelative(stored, s_folder)
    End Function

    Public Function StoredPath(ByVal full As String) As String
        Return LibraryPath.ToRelative(full, s_folder)
    End Function

    Public Sub SaveMeta(ByVal item As String, ByVal meta As VaultMeta)
        Dim key As Byte() = KeyOrThrow()
        Try
            Dim plain As Byte() = JsonSerializer.SerializeToUtf8Bytes(meta)
            Dim nonce As Byte() = RandomNumberGenerator.GetBytes(NonceSize)
            Dim ct(plain.Length - 1) As Byte, tag(TagSize - 1) As Byte
            Using g As New AesGcm(key, TagSize)
                g.Encrypt(nonce, plain, ct, tag, MetaAad(item))
            End Using
            Dim file As String = Path.ChangeExtension(item, MetaExt), temp As String = file & ".tmp"
            IO.File.WriteAllBytes(temp, MetaMagic.Concat(nonce).Concat(tag).Concat(ct).ToArray())
            If IO.File.Exists(file) Then IO.File.Delete(file)
            IO.File.Move(temp, file)
            SyncLock s_lock
                s_meta(item) = meta
            End SyncLock
        Finally
            CryptographicOperations.ZeroMemory(key)
        End Try
    End Sub

    ''' <summary>The item's description; Nothing when it can't be read (locked, missing, changed).</summary>
    Public Function LoadMeta(ByVal item As String) As VaultMeta
        SyncLock s_lock
            Dim m As VaultMeta = Nothing
            If s_meta.TryGetValue(item, m) Then Return m
        End SyncLock
        Dim key As Byte() = CopyKey()
        If key Is Nothing Then Return Nothing
        Try
            Dim bytes As Byte() = File.ReadAllBytes(Path.ChangeExtension(item, MetaExt))
            Dim head As Integer = MetaMagic.Length + NonceSize + TagSize
            If bytes.Length < head OrElse Not bytes.AsSpan(0, 4).SequenceEqual(MetaMagic) Then Return Nothing
            Dim plain(bytes.Length - head - 1) As Byte
            Using g As New AesGcm(key, TagSize)
                g.Decrypt(bytes.AsSpan(4, NonceSize), bytes.AsSpan(head), bytes.AsSpan(4 + NonceSize, TagSize), plain, MetaAad(item))
            End Using
            Dim meta As VaultMeta = JsonSerializer.Deserialize(Of VaultMeta)(plain)
            If meta Is Nothing Then Return Nothing
            SyncLock s_lock
                If s_key IsNot Nothing Then s_meta(item) = meta
            End SyncLock
            Return meta
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is CryptographicException OrElse TypeOf ex Is JsonException
            Return Nothing
        Finally
            CryptographicOperations.ZeroMemory(key)
        End Try
    End Function

    ''' <summary>The id is the associated data: a description can't be swapped onto another item.</summary>
    Private Function MetaAad(ByVal item As String) As Byte()
        Return Encoding.ASCII.GetBytes(Path.GetFileNameWithoutExtension(item).ToLowerInvariant())
    End Function

    ''' <summary>Every item's .ipx (also while locked: only the count is told then).</summary>
    Public Function ItemFiles() As List(Of String)
        If s_folder = "" OrElse Not Directory.Exists(s_folder) Then Return New List(Of String)
        Try
            Return Directory.GetFiles(s_folder, "*" & ItemExt).Where(Function(f) File.Exists(Path.ChangeExtension(f, MetaExt))).ToList()
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Return New List(Of String)
        End Try
    End Function

    ''' <summary>The items (unlocked only) with their descriptions, oldest shot first.</summary>
    Public Function Items() As List(Of KeyValuePair(Of String, VaultMeta))
        Dim result As New List(Of KeyValuePair(Of String, VaultMeta))
        If Not IsUnlocked Then Return result
        For Each f In ItemFiles()
            Dim m As VaultMeta = LoadMeta(f)
            If m IsNot Nothing Then result.Add(New KeyValuePair(Of String, VaultMeta)(f, m))
        Next
        Return result.OrderBy(Function(kv) kv.Value.ShotDate).ThenBy(Function(kv) kv.Value.Name, StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    ''' <summary>The items whose album is <paramref name="albumFolder"/> (unlocked only).</summary>
    Public Function ItemsIn(ByVal albumFolder As String) As List(Of KeyValuePair(Of String, VaultMeta))
        Dim folder As String = Path.GetFullPath(albumFolder).TrimEnd("\"c)
        Return Items().Where(Function(kv) String.Equals(FullPath(kv.Value.Album).TrimEnd("\"c), folder, StringComparison.OrdinalIgnoreCase)).ToList()
    End Function

    ''' <summary>Removes an item's files (.ipx, .ipm, side blobs).</summary>
    Public Sub DeleteItemFiles(ByVal item As String)
        Dim id As String = Path.GetFileNameWithoutExtension(item)
        Try
            For Each f In Directory.GetFiles(Path.GetDirectoryName(item), id & ".*")
                File.Delete(f)
            Next
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
        SyncLock s_lock
            s_meta.Remove(item)
        End SyncLock
    End Sub

    '==================================================================================================
    ' Decrypted for a moment: a video to play
    '==================================================================================================
    Private ReadOnly s_temps As New List(Of String)

    Private ReadOnly Property TempFolder As String
        Get
            Return Path.Combine(Path.GetTempPath(), "iPhoto.Vault")
        End Get
    End Property

    ''' <summary>The item decrypted into a temporary file with its own extension (a video to play);
    ''' deleted by DeleteTempFiles (Lock, the next one, the end of iPhoto).</summary>
    Public Function DecryptToTemp(ByVal item As String) As String
        Dim meta As VaultMeta = LoadMeta(item)
        If meta Is Nothing Then Return Nothing
        DeleteTempFiles()
        Directory.CreateDirectory(TempFolder)
        Dim target As String = Path.Combine(TempFolder, Guid.NewGuid().ToString("N") & Path.GetExtension(meta.Name))
        DecryptFile(item, target)
        SyncLock s_temps
            s_temps.Add(target)
        End SyncLock
        Return target
    End Function

    ''' <summary>Deletes the temporary files (also ones an earlier run left behind).</summary>
    Public Sub DeleteTempFiles()
        SyncLock s_temps
            s_temps.Clear()
        End SyncLock
        Try
            If Directory.Exists(TempFolder) Then
                For Each f In Directory.GetFiles(TempFolder)
                    Try
                        File.Delete(f)
                    Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                        ' still playing: the next time
                    End Try
                Next
            End If
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
        End Try
    End Sub

End Module
