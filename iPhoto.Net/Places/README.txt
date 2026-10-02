Places.txt — PlaceNames.vb 的離線地名清單（UTF-8，每行：縣市鄉鎮區 TAB 緯度 TAB 經度）

來源：內政部國土測繪中心「鄉(鎮、市、區)界線(TWD97經緯度)」1140318 版
      https://data.gov.tw/dataset/7441
授權：政府資料開放授權條款－第1版

轉換：368 個鄉鎮市區，每個區內每 0.01 度一點，加上各區塊的中心點（共 33,129 點）。

Attractions.txt — Attractions.vb 的景點清單（UTF-8；# 開頭為說明；每行：名稱 TAB 緯度 TAB 經度 TAB 縣市 TAB 鄉鎮市區 TAB 景點代碼）

來源：交通部觀光署 觀光資訊資料庫「景點」 Attraction-json.zip（AttractionList.json）
      https://media.taiwan.net.tw/XMLReleaseAll_public/v2.0/Zh_tw/Attraction-json.zip
授權：政府資料開放授權條款－第1版
轉換：2026-09-29 版，6,225 個景點中採用 6,167 個；58 個名稱含 Big5（.Exif 的編碼）沒有的字，未採用。
更新：設定 › 地點 ›「更新景點資料」會重新下載並覆蓋這個檔案。
