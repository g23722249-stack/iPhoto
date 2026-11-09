Imports System.Data.Common

' VaultKey (new in the .NET port): what PhotoVault keeps in the database -- the salts, the iteration
' count and the master key wrapped by the 金鑰密碼 and by the 救援碼. Never the password itself, nor the
' key in the clear. Name / KeyValue rows; the table is made the first time a value is written.
Partial Class Database

    Private Const VaultTable As String = "VaultKey"
    Private m_intVaultTable As Integer = -1   ' -1 unknown, 0 missing, 1 present

    Private Function VaultTableReady(ByVal create As Boolean) As Boolean
        If Not m_bolImplement Then Return False
        If m_intVaultTable < 0 Then
            Try
                m_intVaultTable = If(TableExists(VaultTable), 1, 0)
            Catch ex As DbException
                m_intVaultTable = 0
            End Try
        End If
        If m_intVaultTable = 0 AndAlso create Then
            ' Text(255) in both: Jet's Text and SQLite (which takes the length as a type name only)
            Using cmd As DbCmd = NewCmd("Create Table " & VaultTable & " (Name Text(32) Not Null Primary Key, KeyValue Text(255))")
                cmd.ExecuteNonQuery()
            End Using
            m_intVaultTable = 1
        End If
        Return m_intVaultTable = 1
    End Function

    ''' <summary>Every VaultKey row (empty when there is no table yet).</summary>
    Public Function VaultValues() As Dictionary(Of String, String)
        Dim result As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        If Not VaultTableReady(False) Then Return result
        Using cmd As DbCmd = NewCmd("Select Name, KeyValue From " & VaultTable)
            Using r As DbDataReader = cmd.ExecuteReader()
                While r.Read()
                    result(AsText(r(0))) = AsText(r(1))
                End While
            End Using
        End Using
        Return result
    End Function

    ''' <summary>Writes the rows (all or none); a value Nothing removes its row.</summary>
    Public Sub SetVaultValues(ByVal values As IDictionary(Of String, String))
        VaultTableReady(True)
        Using tx As DbTransaction = m_lpConnection.BeginTransaction()
            For Each kv In values
                Using cmd As DbCmd = NewCmd("Delete From " & VaultTable & " Where Name = ?", tx)
                    cmd.Parameters.Add(Txt(kv.Key))
                    cmd.ExecuteNonQuery()
                End Using
                If kv.Value Is Nothing Then Continue For
                Using cmd As DbCmd = NewCmd("Insert Into " & VaultTable & " (Name, KeyValue) Values (?, ?)", tx)
                    cmd.Parameters.Add(Txt(kv.Key))
                    cmd.Parameters.Add(Txt(kv.Value))
                    cmd.ExecuteNonQuery()
                End Using
            Next
            tx.Commit()
        End Using
    End Sub

End Class
