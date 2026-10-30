' 年度回顧 (the tool bar's 16): frmYearReview asks which year and what to make; YearPicks picks that
' year's photos from the index (rating, people from the face data, 地點), and PhotoMontage's editor opens
' with them (frmMain.Montage.vb) -- more can be added there, and the work goes into the album picked.
Partial Class frmMain

    Private Sub ShowYearReview()
        If g_lpDatabase Is Nothing OrElse Not g_lpDatabase.Implement Then
            frmMsgBox.ShowExclamationMessage("年度回顧要用到照片索引（設定 › 維護 › 重建索引）。", "年度回顧")
            Return
        End If
        Dim years As List(Of KeyValuePair(Of Integer, Integer)) = g_lpDatabase.TimelineYears()
        If years.Count = 0 Then
            frmMsgBox.ShowExclamationMessage("照片索引裡沒有記錄拍攝日期的照片。", "年度回顧")
            Return
        End If
        Dim year As Integer, count As Integer, mode As PhotoMontage.Core.MontageMode
        Using f As New frmYearReview(years)
            If f.ShowDialog(Me) <> DialogResult.OK Then Return
            year = f.Year : count = f.Count : mode = f.Mode
        End Using

        Dim picks As List(Of String)
        SetBusy(True)
        Try
            picks = YearReviewPicks(year, count)
        Finally
            SetBusy(False)
        End Try
        If picks.Count = 0 Then
            frmMsgBox.ShowExclamationMessage(year & " 年沒有可以用的照片。", "年度回顧")
            Return
        End If
        OpenMontage(picks, year & " 年度回顧", mode)
    End Sub

    ''' <summary>The year's pictures that are still there, scored and picked (YearPicks).</summary>
    Private Shared Function YearReviewPicks(ByVal year As Integer, ByVal count As Integer) As List(Of String)
        Dim faces = g_lpDatabase.FaceCountsByFile()
        Dim candidates As New List(Of YearPicks.Candidate)
        For Each r In g_lpDatabase.TimelineRows(year)
            If r.FileName = "" OrElse GetMediaType(g_lpFileSystem, r.FileName) <> enumPhotoMediaType.mdImage OrElse
               Not IO.File.Exists(r.FileName) Then Continue For
            Dim fc As (Faces As Integer, Named As Integer) = Nothing
            faces.TryGetValue(r.FileName, fc)
            candidates.Add(New YearPicks.Candidate With {
                .File = r.FileName, .DateText = r.DateText,
                .Score = YearPicks.Score(r.Ranking, fc.Faces, fc.Named, r.Spot <> "" OrElse r.City <> "")})
        Next
        Return YearPicks.Pick(candidates, count).Select(Function(c) c.File).ToList()
    End Function

End Class
