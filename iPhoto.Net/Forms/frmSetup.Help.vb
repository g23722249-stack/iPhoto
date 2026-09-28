' Help windows for 設定 (HelpTip; texts in PhotoLib HelpTexts, keys "setup.*"). Registered once the
' window is up: the 面孔 page is made in Load (frmSetup.Faces.vb). A field's caption and its box share
' the help; the browse icons show it too.
Partial Class frmSetup

    Private ReadOnly m_lpHelp As New HelpTip

    Private Sub Help_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        With m_lpHelp
            ' 一般設定
            .SetHelp("setup.slide", Label18, sliSlide)
            .SetHelp("setup.music", Label19, txtMusic, imgMusic)
            .SetHelp("setup.database", Label22, txtAttached(0), imgAttached(0))
            .SetHelp("setup.keyword", Label17, txtAttached(1), imgAttached(1))
            .SetHelp("setup.address", Label21, txtAttached(2), imgAttached(2))
            .SetHelp("setup.facecover", Label25, txtAttached(3), imgAttached(3))
            .SetHelp("setup.rebuild", imgRebuild)
            ' 相簿位置
            .SetHelp("setup.albums", Label2, lstAlbum)
            For i = 0 To imgAlbums.Length - 1
                .SetHelp("setup.album." & i, imgAlbums(i))
            Next
            .SetHelp("setup.favorite", Label1, txtFavorite, imgFavorites)
            .SetHelp("setup.privilege", chkPrivilege)
            ' 外觀 / 音效
            .SetHelp("setup.font", Label15, txtFont, imgFontName)
            .SetHelp("setup.switch", chkSwitchScreen)
            Dim soundCaptions As Control() = {Label4, Label3, Label6, Label5, Label7}   ' 匯入完成, 匯出完成, 滑鼠移出, 滑鼠移入, 滑鼠按下
            For i = 0 To txtSound.Length - 1
                .SetHelp("setup.sound." & i, soundCaptions(i), txtSound(i), imgSound(i))
            Next
            ' 外掛應用程式
            Dim appCaptions As Control() = {Label8, Label9, Label10, Label11, Label12, Label13}
            For i = 0 To txtApp.Length - 1
                .SetHelp("setup.app." & i, appCaptions(i), txtApp(i), imgApp(i))
            Next
            ' 面孔 (frmSetup.Faces.vb)
            .SetHelp("setup.face.enabled", chkFaceEnabled)
            .SetHelp("setup.face.autoscan", chkFaceAutoScan)
            .SetHelp("setup.face.write", chkFaceWriteNames)
            .SetHelp("setup.face.strict", rbFaceStrict)
            .SetHelp("setup.face.clear", lblFaceClear)
            .SetHelp("setup.face.guide", lblFaceGuide)
            .SetHelp("setup.face.strangers", lblFaceStrangers)

            .SetHelp("setup.save", butSave)
            .SetHelp("setup.cancel", butExit)
        End With
    End Sub

    Private Sub Help_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        m_lpHelp.Dispose()
    End Sub

End Class
