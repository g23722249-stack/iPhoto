Imports System.Runtime.InteropServices
Imports System.Text

Namespace CoreMedia

    ''' <summary>
    ''' Port of CoreMedia.Audio (the slide show's background music): plays one sound file through the
    ''' Windows MCI (winmm) -- mp3, wav, mid and wma all play without extra components. Use it from one
    ''' thread (the UI thread), as MCI requires.
    ''' </summary>
    Public Class Audio
        Implements IDisposable

        <DllImport("winmm.dll", CharSet:=CharSet.Unicode, EntryPoint:="mciSendStringW")>
        Private Shared Function mciSendString(ByVal command As String, ByVal returnValue As StringBuilder, ByVal returnLength As Integer, ByVal callback As IntPtr) As Integer
        End Function

        Private ReadOnly m_alias As String = "iphoto" & Guid.NewGuid().ToString("N")
        Private m_strFileName As String = ""
        Private m_bolOpen As Boolean

        ''' <summary>The file to play; setting it closes the previous one ("" = nothing open).</summary>
        Public Property FileName As String
            Get
                Return m_strFileName
            End Get
            Set(value As String)
                Close()
                m_strFileName = If(value, "")
                If m_strFileName = "" Then Return
                Dim type As String = If(IO.Path.GetExtension(m_strFileName).Equals(".mid", StringComparison.OrdinalIgnoreCase), "sequencer", "mpegvideo")
                m_bolOpen = Send("open """ & m_strFileName & """ type " & type & " alias " & m_alias) = 0
                If m_bolOpen Then Send("set " & m_alias & " time format milliseconds")
            End Set
        End Property

        Public Sub Play()
            If m_bolOpen Then Send("play " & m_alias)
        End Sub

        Public Sub Pause()
            If m_bolOpen Then Send("pause " & m_alias)
        End Sub

        ''' <summary>Stops and closes the file (VB6 Stopped); FileName becomes "".</summary>
        Public Sub Stopped()
            Close()
            m_strFileName = ""
        End Sub

        ''' <summary>Current position in milliseconds.</summary>
        Public ReadOnly Property Position As Integer
            Get
                Return StatusNumber("position")
            End Get
        End Property

        ''' <summary>Length in milliseconds.</summary>
        Public ReadOnly Property Duration As Integer
            Get
                Return StatusNumber("length")
            End Get
        End Property

        ''' <summary>True once the file has played to its end (or nothing is open).</summary>
        Public ReadOnly Property Finished As Boolean
            Get
                If Not m_bolOpen Then Return True
                Dim mode As String = Status("mode")
                Return mode = "stopped" AndAlso Position >= Duration - 50
            End Get
        End Property

        Private Function StatusNumber(ByVal item As String) As Integer
            Dim n As Integer
            Integer.TryParse(Status(item), n)
            Return n
        End Function

        Private Function Status(ByVal item As String) As String
            If Not m_bolOpen Then Return ""
            Dim sb As New StringBuilder(128)
            mciSendString("status " & m_alias & " " & item, sb, sb.Capacity, IntPtr.Zero)
            Return sb.ToString().Trim()
        End Function

        Private Shared Function Send(ByVal command As String) As Integer
            Return mciSendString(command, Nothing, 0, IntPtr.Zero)
        End Function

        Private Sub Close()
            If m_bolOpen Then
                Send("stop " & m_alias)
                Send("close " & m_alias)
                m_bolOpen = False
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Close()
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace
