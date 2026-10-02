Imports System.Data.OleDb

' 輸入照片 (frmImportStudio, ImportRunner): writing an imported photo from the import thread, which has
' its own connection. Unlike AddItem (UI thread) nothing here shows a message box: errors are thrown
' to the caller, which logs them and goes on with the next photo. Lock errors are retried (WithRetry).
Partial Public Class Database

    ''' <summary>Replaces the photo's PhotoIndex row (delete + insert).</summary>
    Public Sub WriteImportedPhoto(ByVal lpPhoto As Photo)
        If Not m_bolImplement Then Return
        WithRetry(Sub()
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          Using cmd As New OleDbCommand("Delete From " & mc_strTableName & " Where FileName = ?", m_lpConnection, tx)
                              cmd.Parameters.Add(Txt(lpPhoto.FileDesc))
                              cmd.ExecuteNonQuery()
                          End Using
                          InsertPhoto(lpPhoto, tx)
                          tx.Commit()
                      End Using
                  End Sub)
    End Sub

    ''' <summary>True when PhotoIndex has a row for the file.</summary>
    Public Function PhotoIndexed(ByVal strFileDesc As String) As Boolean
        If Not m_bolImplement Then Return False
        Using cmd As New OleDbCommand("Select Count(*) From " & mc_strTableName & " Where FileName = ?", m_lpConnection)
            cmd.Parameters.Add(Txt(strFileDesc))
            Return CInt(cmd.ExecuteScalar()) > 0
        End Using
    End Function

    ''' <summary>Saves the faces found before the import as the photo's analysis (FacePhoto + FaceRegion),
    ''' so the background scan doesn't analyse it again. Sure matches and the user's names are saved as
    ''' confirmed (they go into the people field), suggestions as suggestions, "not a face" as such;
    ''' the people the user said a face is not go into FaceReject. <paramref name="lngSize"/> /
    ''' <paramref name="dtFileTime"/> are the copied file's.</summary>
    Public Sub SaveImportedFaces(ByVal strFileDesc As String, ByVal lngSize As Long, ByVal dtFileTime As DateTime,
                                 ByVal intShotYear As Integer, ByVal faces As IList(Of ImportFace))
        If Not FaceTablesReady OrElse faces Is Nothing Then Return
        ' people first: EnsureFacePerson works outside a transaction
        Dim ids As New Dictionary(Of ImportFace, Integer)
        For Each f In faces
            Dim id As Integer = 0
            Select Case f.State
                Case ImportFace.enumImportFaceState.ifAuto, ImportFace.enumImportFaceState.ifConfirmed
                    id = EnsureFacePerson(f.Name)
                Case ImportFace.enumImportFaceState.ifSuggested
                    ' the person may have gone meanwhile (清除面孔辨識資料): then just a face
                    id = If(FacePersonExists(f.PersonID), f.PersonID, 0)
            End Select
            ids(f) = id
        Next

        Dim faceIDs As New Dictionary(Of ImportFace, Integer)
        WithRetry(Sub()
                      faceIDs.Clear()
                      Using tx As OleDbTransaction = m_lpConnection.BeginTransaction()
                          Exec(tx, "Delete From FacePhoto Where FileName = ?", Txt(strFileDesc))
                          Exec(tx, "Insert Into FacePhoto (FileName, FileSize, FileTime, ShotYear, Status, FaceCount, AnalyzeTime, EngineVer) Values (?, ?, ?, ?, ?, ?, ?, ?)",
                               Txt(strFileDesc), P(OleDbType.Integer, CInt(Math.Min(lngSize, Integer.MaxValue))), P(OleDbType.Date, dtFileTime),
                               P(OleDbType.SmallInt, CShort(intShotYear)), P(OleDbType.UnsignedTinyInt, CByte(1)),
                               P(OleDbType.SmallInt, CShort(faces.Count)), P(OleDbType.Date, DateTime.Now), Txt(Quartz.FaceEngine.Version))
                          For Each f In faces
                              Dim state As FaceRegion.enumFaceState
                              Select Case f.State
                                  Case ImportFace.enumImportFaceState.ifAuto, ImportFace.enumImportFaceState.ifConfirmed
                                      state = If(ids(f) <> 0, FaceRegion.enumFaceState.fsConfirmed, FaceRegion.enumFaceState.fsUnnamed)
                                  Case ImportFace.enumImportFaceState.ifSuggested
                                      ' a hand-boxed face stays manual until named (as FaceOrganizer keeps it)
                                      If f.Manual Then
                                          state = FaceRegion.enumFaceState.fsManual
                                      Else
                                          state = If(ids(f) <> 0, FaceRegion.enumFaceState.fsSuggested, FaceRegion.enumFaceState.fsUnnamed)
                                      End If
                                  Case ImportFace.enumImportFaceState.ifNotFace
                                      state = FaceRegion.enumFaceState.fsNotFace
                                  Case Else
                                      state = If(f.Manual, FaceRegion.enumFaceState.fsManual, FaceRegion.enumFaceState.fsUnnamed)
                              End Select
                              Dim id As Integer = InsertFaceRegion(tx, strFileDesc, f.Face, state, intShotYear)
                              faceIDs(f) = id
                              Dim person As Integer = If(state = FaceRegion.enumFaceState.fsConfirmed OrElse state = FaceRegion.enumFaceState.fsSuggested, ids(f), 0)
                              If person <> 0 Then
                                  Exec(tx, "Update FaceRegion Set PersonID = ?, Similarity = ? Where FaceID = ?",
                                       P(OleDbType.Integer, person), P(OleDbType.Single, If(state = FaceRegion.enumFaceState.fsConfirmed, 0.0F, f.Similarity)),
                                       P(OleDbType.Integer, id))
                              End If
                          Next
                          tx.Commit()
                      End Using
                  End Sub)
        For Each f In faces
            For Each pid In f.Rejected.Where(Function(x) FacePersonExists(x))
                AddFaceReject(faceIDs(f), pid)
            Next
        Next
    End Sub

    Private Function FacePersonExists(ByVal intPersonID As Integer) As Boolean
        If intPersonID = 0 Then Return False
        Using cmd As New OleDbCommand("Select Count(*) From FacePerson Where PersonID = ?", m_lpConnection)
            cmd.Parameters.Add(P(OleDbType.Integer, intPersonID))
            Return CInt(cmd.ExecuteScalar()) > 0
        End Using
    End Function

End Class
