' The text of the face-recognition guide (frmFaceGuide): what each part does, worked examples and
' questions. Kept apart from the window so the wording can change without touching the drawing code.
' Figures are drawn by GuideFigures (by key); links name another topic's key.
Public Module FaceGuideContent

    Public Const GroupGuide As String = "使用說明"
    Public Const GroupExamples As String = "操作範例"
    Public Const GroupFaq As String = "常見問題"

    Private s_topics As List(Of GuideTopic)

    ''' <summary>All topics in reading order.</summary>
    Public ReadOnly Property Topics As List(Of GuideTopic)
        Get
            If s_topics Is Nothing Then s_topics = Build()
            Return s_topics
        End Get
    End Property

    Public Function Topic(ByVal key As String) As GuideTopic
        Return Topics.FirstOrDefault(Function(x) String.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase))
    End Function

    '==================================================================================================
    ' Block helpers
    '==================================================================================================
    Private Function T(ByVal key As String, ByVal group As String, ByVal title As String, ByVal summary As String, ParamArray blocks As GuideBlock()) As GuideTopic
        Dim tp As New GuideTopic With {.Key = key, .Group = group, .Title = title, .Summary = summary}
        tp.Blocks.AddRange(blocks)
        Return tp
    End Function

    Private Function H(ByVal text As String) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Heading, .Text = text}
    End Function

    Private Function P(ByVal text As String) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Paragraph, .Text = text}
    End Function

    Private Function Steps(ParamArray items As String()) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Steps, .Items = items}
    End Function

    Private Function Bullets(ParamArray items As String()) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Bullets, .Items = items}
    End Function

    Private Function Tip(ByVal text As String) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Tip, .Text = text}
    End Function

    Private Function Note(ByVal text As String) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Note, .Text = text}
    End Function

    Private Function Fig(ByVal key As String, ByVal caption As String) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Figure, .Figure = key, .Text = caption}
    End Function

    Private Function Link(ByVal text As String, ByVal target As String) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Link, .Text = text, .Target = target}
    End Function

    Private Function QA(ByVal question As String, ByVal answer As String) As GuideBlock
        Return New GuideBlock With {.Kind = GuideBlockKind.Question, .Text = question, .Items = {answer}}
    End Function

    '==================================================================================================
    ' The guide
    '==================================================================================================
    Private Function Build() As List(Of GuideTopic)
        Dim list As New List(Of GuideTopic)

        '---------------------------------------------------------------------------------- 使用說明
        list.Add(T("intro", GroupGuide, "認識面孔功能", "iPhoto 幫你找出照片裡的人，你只要告訴它「這是誰」。",
            P("iPhoto 會在背景把每張照片裡的臉找出來，並把長得像的臉放在一起。你只要替幾張臉取名字，它就會幫你找出同一個人的其他照片。"),
            Fig("flow", "從分析照片到名字寫進照片，每一步都由你決定"),
            H("在三個地方使用"),
            Bullets("「面孔牆」：左邊清單最下面的「面孔」。每個人一張名片，還有等你命名的「未命名的臉」。",
                    "「全圖瀏覽」：看照片時按右上角的「顯示面孔」，照片上每張臉都會框起來，點一下就能輸入名字。",
                    "「設定 › 面孔」：開關這個功能、調整程式自動認人的嚴格程度。"),
            H("放心使用"),
            Bullets("程式自己認出的名字只是「猜的」，會帶著問號出現；要等你按 ✓ 確認，才會寫進照片的「人物」欄。",
                    "分析照片只是讀取，不會修改照片。",
                    "分析在背景進行，進行中照常使用 iPhoto 就好。")))

        list.Add(T("start", GroupGuide, "開始使用", "啟用功能、等待分析、看懂主畫面下方的進度。",
            Steps("開啟「設定」，切到「面孔」分頁，勾選「啟用人物辨識」，按「儲存」。第一次啟用要關掉 iPhoto 再開一次。",
                  "重新開啟後，主畫面下方「已選取 N 張照片」的右邊會顯示分析進度，例如「分析面孔 1,234 / 13,000」。",
                  "分析完成後，進度會變成「面孔：N 人 · M 群未命名」，左邊清單最下面出現「面孔」。"),
            Fig("status", "主畫面下方的面孔狀態：分析中按一下可以暫停"),
            Tip("分析時覺得電腦變慢，點一下進度文字就會暫停，再點一下繼續。每張照片約 0.15 秒，一萬張大約半小時；關掉 iPhoto 也沒關係，下次開啟會接著分析。"),
            H("之後新增的照片"),
            P("匯入照片後，新照片會自動在背景分析。如果在設定中取消了「開啟 iPhoto 時，在背景分析新增或修改過的照片」，狀態會顯示「面孔：按這裡分析新照片」，點一下就開始分析。")))

        list.Add(T("how", GroupGuide, "程式怎麼認人", "程式從哪裡學會認人，臉上的各種標籤代表什麼。",
            P("程式替每張臉算出一組「特徵」，再和已經知道名字的臉比較有多像（相似度 0～1，越大越像）。"),
            H("程式從哪裡學會認人"),
            Bullets("照片的「人物」欄只寫了一個名字、照片裡也只找到一張臉：程式就把這張臉當成那個人的樣本。你以前填的人物欄，一開始就派上用場。",
                    "你按 ✓ 確認過、或親手取名的臉，也會成為樣本。",
                    "程式自己猜的結果不會拿來當樣本，所以猜錯一次不會越錯越多。"),
            H("臉上的標籤"),
            Fig("labels", "全圖瀏覽中，框的顏色與文字告訴你程式有多確定"),
            Bullets("白色實線＋名字：確定是這個人（你確認過，或人物欄寫的）。",
                    "淺藍虛線＋「名字？」：程式很有把握認出來的，請按 ✓ 確認或 ✕ 否認。",
                    "黃色虛線＋「這是 名字 嗎？」：程式覺得可能是，但沒有把握。",
                    "黃色虛線＋「這是誰？」：還不知道是誰，點一下就能取名。",
                    "灰色虛線＋「不認識」：你標成「我不認識」的臉；點一下仍然可以取名。"),
            H("同一個人，不同年紀"),
            P("程式依拍照年份，把每個人的樣本分成不同年紀的組：小時候的臉跟小時候比，長大的臉跟長大比。替這個人設定出生年後分得更細：0～2 歲每年一組、3～12 歲每兩年、13～20 歲每三年，之後每十年一組。"),
            Fig("ages", "設定出生年後，每個年紀各有自己的樣本"),
            Link("範例五：小孩從小到大", "ex5")))

        list.Add(T("wall", GroupGuide, "面孔牆", "每個人一張名片，未命名的臉按群組排好。",
            P("點左邊清單最下面的「面孔」，右邊就會出現面孔牆。"),
            Fig("wall", "左邊是「面孔」清單，右邊是面孔牆"),
            Bullets("人物名片：一個人一張。滑鼠停在名片上會顯示照片張數與待確認的張數；按兩下看這個人的所有照片。",
                    "群組卡（四張小臉拼成一張）：程式覺得是同一個人、但還不知道名字的臉。按兩下逐張確認後命名。",
                    "「面孔」底下列出每個人，括號裡是照片張數；「未命名的臉 (N 群)」只顯示群組卡。"),
            P("點「面孔」底下的人名，會像一般相簿一樣列出他的照片，可以全圖瀏覽、評價，或放進 Dock 匯出、列印。"),
            H("人物名片的右鍵選單"),
            Fig("wallmenu", "在人物名片上按右鍵"),
            Bullets("看照片：同按兩下。",
                    "確認更多照片（N）…：程式替他找到的 N 張臉，請你一批一批確認。沒有待確認的臉時不能按。",
                    "改名／合併…：改名字；輸入另一個人的名字就合併成同一人。",
                    "設定出生年…：讓程式更會認出他不同年紀的樣子。",
                    "更換封面…：用「封面」視窗替他做一張名片。",
                    "找合照…：勾選其他人，列出你們同時出現的照片。",
                    "隱藏：不再顯示、也不再自動認出這個人（例如路人、明星海報）；在「已隱藏的人」可以取消隱藏。"),
            Tip("面孔牆一次最多顯示 60 個群組。先把大的群組命名，剩下的會陸續出現。")))

        list.Add(T("group", GroupGuide, "替未命名的臉取名字", "把程式分好的一群臉，一次取好名字。",
            Steps("在面孔牆按兩下群組卡（或按右鍵「逐張確認…」）。",
                  "畫面會列出這一群的每張臉，預設全部勾選。",
                  "不是同一個人的臉，按兩下（或點勾選框）取消勾選。",
                  "按右鍵「將勾選的臉命名為…」，輸入名字，按確定。"),
            Fig("group", "取消勾選不是同一人的臉，再按右鍵命名"),
            P("輸入已經有的名字，這些臉就加到那個人；輸入新名字就多一個人。命名完會回到面孔牆。"),
            Tip("整群一看就知道都是同一個人時，可以直接在面孔牆的群組卡按右鍵「全部命名為…」，不用一張張看。"),
            H("認不出是誰？看整張照片"),
            P("在小臉上按右鍵「全圖瀏覽」，會打開整張照片、框出這張臉，看場合和旁邊的人比較好認。上一張、下一張會依序看這一群的每張臉；在全圖瀏覽裡點臉也能直接輸入名字。"),
            H("我不認識"),
            P("路人、同學、海報上的人……不認識的臉，在面孔牆的群組卡按右鍵「我不認識…」，整群就不會再出現在「未命名的臉」，程式也不會再拿它們認人；照片和人物欄都不變。在逐張確認畫面也可以只把勾選的幾張標成「勾選的臉我不認識…」。"),
            Note("標錯了：在全圖瀏覽點那張灰色「不認識」的臉輸入名字就好；要全部恢復，到「設定 › 面孔」按「恢復標成我不認識的臉」。"),
            Note("命名就等於確認：名字會寫進這些照片的「人物」欄（除非在設定中取消了「確認過的名字寫進照片的人物欄」）。")))

        list.Add(T("confirm", GroupGuide, "確認更多照片", "程式替某個人找到的照片，由你一批一批確認。",
            P("程式替某個人找到可能的照片後，他的名片右鍵選單會出現「確認更多照片（N）…」，N 是等你確認的臉數。"),
            Steps("在面孔牆的人物名片上按右鍵 →「確認更多照片…」。",
                  "一次列出 40 張臉，預設全部勾選。滑鼠停在臉上，可以看到檔名、拍攝年份和相似度。",
                  "把不是這個人的臉取消勾選（按兩下也可以）。",
                  "按右鍵，選「勾選的是「名字」、其餘不是」：勾選的確認，沒勾的記成「不是這個人」，以後不會再猜他。",
                  "或選「只確認勾選的（其餘之後再說）」：沒勾的先保留，之後再問你。",
                  "自動換下一批，直到全部確認完。"),
            Fig("confirm", "一批 40 張，取消勾選認錯的，再按右鍵"),
            Tip("每確認一批，程式就多學到一些樣本，下一批通常會更準。看不清楚的臉，按右鍵「全圖瀏覽」看整張照片。"),
            Note("如果整批都取消勾選再選「其餘不是」，會先問你「這一批全部都不是 某某 嗎？」，避免按錯。")))

        list.Add(T("viewer", GroupGuide, "在全圖瀏覽中命名", "一邊看照片，一邊替臉取名字、確認或否認。",
            Steps("全圖瀏覽照片時，按右上角的「顯示面孔」。之後換照片也會一直顯示，按「隱藏面孔」關掉。",
                  "按鈕下方會顯示這張照片的狀況，例如「3 張臉 · 1 張待確認 · 1 張未命名」。",
                  "點一張臉（或它下面的標籤），會出現名字框；輸入時會列出已有的名字，按 Enter 儲存，按 Esc 取消。",
                  "儲存後會自動跳到下一張還沒有名字的臉，可以連續命名。"),
            Fig("namebox", "點臉輸入名字，會提示已有的名字"),
            H("✓ 和 ✕"),
            Bullets("✓：是這個人。確認後名字寫進照片的人物欄。",
                    "✕：不是這個人。這張臉變回「這是誰？」，以後不會再把它猜成這個人。"),
            H("在臉上按右鍵"),
            Bullets("不是 某某：同 ✕。",
                    "移除名字：這張臉變回「這是誰？」；你取的名字也會從照片的人物欄拿掉（照片裡還有別張臉是他時除外）。",
                    "這不是臉：程式把衣服圖案、海報、畫裡的人當成臉時使用，之後不再顯示。點選那張臉後按 Delete 也可以。"),
            Note("照片編輯後還沒存檔時，臉框會暫時隱藏；存檔後才會重新找臉。"),
            Link("範例四：程式沒找到的臉", "ex4")))

        list.Add(T("manage", GroupGuide, "改名、合併與隱藏", "整理人物：改名字、把兩個人合成一個、藏起不需要的人。",
            H("改名"),
            P("在人物名片上按右鍵 →「改名／合併…」，輸入新名字。已經寫進照片人物欄的名字會一起改。"),
            H("合併"),
            P("同一個人出現兩張名片（名字寫法不同、或被分成兩人）時，在其中一張按右鍵「改名／合併…」，輸入另一張的名字。確認後兩人合成一人，照片的人物欄也一起改。"),
            Fig("merge", "輸入另一個人的名字，兩張名片就合成一張"),
            H("設定出生年"),
            P("右鍵 →「設定出生年…」，輸入西元年（1900 到今年），不知道就輸入 0。有出生年，程式更會分辨同一個人不同年紀的樣子。"),
            H("更換封面"),
            P("右鍵 →「更換封面…」，用「封面」視窗替這個人做一張好看的名片，面孔牆就會用它。"),
            H("隱藏"),
            P("右鍵 →「隱藏」。這個人會從面孔牆和清單消失，程式也不再自動認出他；照片的人物欄不會改。適合路人、明星、照片裡的海報人物。"),
            P("隱藏的人會列在「面孔 › 已隱藏的人」，在名片上按右鍵「取消隱藏」就回來了。"),
            Link("範例二：同一個人有兩個名字", "ex2")))

        list.Add(T("setup", GroupGuide, "面孔設定", "設定 › 面孔 分頁裡每個選項的意思。",
            Fig("setup", "設定 › 面孔"),
            Bullets("啟用人物辨識：關掉後，面孔牆與全圖瀏覽的面孔功能都不會出現。下次開啟 iPhoto 生效。",
                    "開啟 iPhoto 時，在背景分析新增或修改過的照片：關掉後改成點主畫面的面孔狀態手動分析。",
                    "確認過的名字寫進照片的「人物」欄：關掉後，名字只記在面孔資料裡，不改照片資訊。",
                    "自動認人：程式要多有把握才自己把名字放到臉上（見下表）。改了馬上生效，不用重開。",
                    "清除面孔辨識資料…：刪掉找到的臉、人物、確認、「不是此人」與「我不認識」的紀錄，之後重新分析全部照片。照片、人物欄和面孔名片不受影響。",
                    "恢復標成「我不認識」的臉…：把標成我不認識的臉全部恢復成未命名，重新分群、重新認人。"),
            H("自動認人：寬鬆、平衡、嚴格"),
            Fig("strict", "程式自己認出的比例與其中認錯的比例"),
            Tip("認錯的太多 → 改成「嚴格」；很多照片都沒被認出來 → 改成「寬鬆」。不管哪一種，程式認出的名字都要你按 ✓ 才會寫進照片。")))

        '---------------------------------------------------------------------------------- 操作範例
        list.Add(T("ex1", GroupExamples, "範例一：第一次整理全家人", "從啟用到把家人都認出來的完整流程。",
            P("情況：相片庫有一萬多張照片，以前在部分照片的人物欄填過名字。"),
            Steps("「設定 › 面孔」勾選「啟用人物辨識」，儲存後重新開啟 iPhoto。",
                  "等主畫面下方顯示「面孔：N 人 · M 群未命名」（分析中照常使用就好）。",
                  "點左邊的「面孔」：以前在人物欄填過名字的人，已經有名片了。",
                  "從最前面（最大）的群組開始：按兩下群組卡，取消勾選不是的人，按右鍵「將勾選的臉命名為…」，輸入「媽媽」。",
                  "重複幾個大群組。名片的提示開始出現「N 張待確認」。",
                  "在名片上按右鍵「確認更多照片…」，一批一批確認。",
                  "每確認一批，程式就更準，更多照片會被自動認出。"),
            Fig("wall", "整理後的面孔牆"),
            Tip("先整理最常出現的家人。一次十幾分鐘，分幾天做完就好。")))

        list.Add(T("ex2", GroupExamples, "範例二：同一個人有兩個名字", "把寫法不同的兩個名字合併成一個。",
            P("情況：面孔牆上有「陳慈佑」和「陳慈祐」兩張名片，其實是同一個人，只是以前人物欄的字打得不一樣。"),
            Steps("在「陳慈佑」的名片上按右鍵 →「改名／合併…」。",
                  "把名字改成正確的「陳慈祐」，按確定。",
                  "出現「「陳慈佑」會合併到「陳慈祐」，照片的人物欄也會一起改，確定嗎？」，按確定。",
                  "稍等一下，兩張名片合成一張，所有照片的人物欄都變成「陳慈祐」。"),
            Fig("merge", "合併後只剩一張名片")))

        list.Add(T("ex3", GroupExamples, "範例三：程式認錯人", "把認錯的名字改回來，並讓程式記住。",
            P("情況：全圖瀏覽時，看到哥哥的臉被標成「弟弟？」。"),
            Steps("按標籤右邊的 ✕（不是弟弟）。",
                  "這張臉變成「這是誰？」。點它，輸入「哥哥」，按 Enter。"),
            Fig("reject", "按 ✕ 否認，再點臉輸入正確的名字"),
            P("如果已經確認過、框是白色實線寫著「弟弟」：直接點這張臉，在名字框改成「哥哥」按 Enter，照片人物欄會一起更正。"),
            Tip("兩個人（例如兄弟）常被認錯時，多替兩人各確認幾批照片，或在設定改成「嚴格」。")))

        list.Add(T("ex4", GroupExamples, "範例四：程式沒找到的臉", "自己框出側臉、戴口罩或太小的臉。",
            P("情況：側臉、戴口罩、太小或太暗的臉，程式可能找不到。"),
            Steps("在全圖瀏覽中，用滑鼠在照片上拖曳，框出那張臉（按住 Shift 會是正方形）。",
                  "按右上角的「新增面孔」。",
                  "臉下方出現名字框，輸入名字，按 Enter。"),
            Fig("addface", "先框出臉，再按「新增面孔」"),
            Note("照片編輯後要先存檔，才能新增面孔。")))

        list.Add(T("ex5", GroupExamples, "範例五：小孩從小到大", "讓程式認得同一個孩子各個年紀的樣子。",
            P("情況：孩子嬰兒時和上學後長得很不一樣，程式把他小時候的照片分成了另一群。"),
            Steps("在面孔牆孩子的名片上按右鍵 →「設定出生年…」，輸入例如 2012。",
                  "點「未命名的臉」，找到他小時候的群組，按兩下。",
                  "取消勾選不是他的臉，按右鍵「將勾選的臉命名為…」，輸入同一個名字。",
                  "每個年紀都命名或確認幾張，之後程式就能認出各個年紀的他。"),
            Fig("ages", "每個年紀有幾張確認過的臉就夠了"),
            Tip("出生年只影響怎麼分年紀組，不會寫進照片。")))

        '---------------------------------------------------------------------------------- 常見問題
        list.Add(T("faq", GroupFaq, "常見問題", "",
            QA("程式認錯了，會改到我的照片嗎？",
               "不會。程式猜的名字只顯示在面孔牆和全圖瀏覽；只有你按 ✓、取名字或確認之後，名字才會寫進照片的人物欄。"),
            QA("為什麼有些人一直沒被認出來？",
               "可能樣本太少，或臉太小、太暗、側臉。替他多確認幾批照片；或在設定中把「自動認人」改成「寬鬆」。"),
            QA("分析要多久？可以中途關掉 iPhoto 嗎？",
               "每張約 0.15 秒，一萬張大約半小時。可以隨時關掉，下次開啟會接著分析還沒分析的照片。"),
            QA("不想讓名字寫進照片怎麼辦？",
               "在「設定 › 面孔」取消「確認過的名字寫進照片的「人物」欄」。名字只記在面孔資料裡，照片資訊不會變。"),
            QA("「清除面孔辨識資料」會刪掉什麼？",
               "找到的臉、人物、確認與「不是此人」的紀錄。照片、人物欄和面孔名片都不受影響；之後會重新分析全部照片，人物欄裡的名字會再當成樣本。"),
            QA("隱藏的人可以找回來嗎？",
               "可以。點「面孔 › 已隱藏的人」，在名片上按右鍵「取消隱藏」。"),
            QA("怎麼找兩個人的合照？",
               "在其中一人的名片上按右鍵「找合照…」，勾選其他人，就會列出他們同時出現的照片（臉被認出或人物欄有寫的都算）。"),
            QA("同一個人的名字有好幾種寫法？",
               "到「設定 › 維護」按「人物欄名字檢查…」，會列出寫法相近的名字（例如 陳慈佑／陳慈祐），選要保留的寫法就會一次改好；修改前會先自動備份 .Exif。"),
            QA("「未命名的臉」裡都是不認識的人怎麼辦？",
               "在群組卡按右鍵「我不認識…」，這些臉就不會再出現，也不會被拿去認人。之後想恢復，到「設定 › 面孔」按「恢復標成我不認識的臉」。"),
            QA("寵物也能認嗎？",
               "不行，程式只找人的臉。寵物的名字可以照舊寫在人物欄。"),
            QA("編輯過的照片會怎樣？",
               "存檔後會重新找這張照片的臉。還沒存檔時，全圖瀏覽會先把臉框藏起來。")))

        Return list
    End Function

End Module

''' <summary>One page of the guide.</summary>
Public Class GuideTopic
    Public Key As String = ""
    Public Group As String = ""
    Public Title As String = ""
    ''' <summary>One line under the title.</summary>
    Public Summary As String = ""
    Public ReadOnly Blocks As New List(Of GuideBlock)
End Class

Public Enum GuideBlockKind
    Heading
    Paragraph
    Steps       ' numbered
    Bullets
    Tip         ' blue box
    Note        ' orange box
    Figure      ' drawn by GuideFigures
    Link        ' to another topic
    Question    ' Text = question, Items(0) = answer
End Enum

Public Class GuideBlock
    Public Kind As GuideBlockKind
    Public Text As String = ""
    Public Items As String()
    Public Figure As String = ""
    Public Target As String = ""
End Class
