// iPhoto: the outline map under the map tiles (地點 map.html, 輸入地點 map_pick.html).
// Taiwan's counties and towns (lib/towns.json, taiwan-atlas: 內政部國土測繪中心 open data) drawn
// beneath the OpenStreetMap tiles: where a tile is there it covers the outline, where it isn't (no
// internet) the outline shows; 設定 › 地點 地圖「離線」(?mode=offline) shows no OSM tiles at all (their policy
// allows no offline use), just the outline: land, county and town borders, county names (towns' when zoomed in).
// The tiles come from tiles/{z}/{x}/{y}.png: Modules\MapHost.vb serves them from its cache or
// fetches them. A badge says 線上 / 離線, a pill at the top when online tiles can't be had.
window.iPhotoOutline = function (map) {
  var offline = /(^|[?&])mode=offline(&|$)/.test(location.search);
  map.getContainer().style.background = '#cfe2f3';   // the sea

  map.createPane('outline').style.zIndex = 150;      // under the tiles (200)
  var labelPane = map.createPane('outlineLabels');
  labelPane.style.zIndex = 160;
  labelPane.style.pointerEvents = 'none';
  var canvas = L.canvas({ pane: 'outline', padding: 0.5 });

  var style = document.createElement('style');
  style.textContent =
    '.ol-label{white-space:nowrap;transform:translate(-50%,-50%);color:#6b7480;font-size:13px;' +
    'text-shadow:0 0 3px #f6f3ec,0 0 3px #f6f3ec,0 0 3px #f6f3ec;pointer-events:none}' +
    '.ol-label.town{font-size:12px;color:#8a929b}' +
    '#olpill{position:absolute;top:10px;left:50%;transform:translateX(-50%);z-index:1000;display:none;' +
    'padding:4px 14px;border-radius:12px;font-size:14px;color:#6b4e00;background:rgba(255,243,205,.95);' +
    'border:1px solid #e0c46c;box-shadow:0 1px 4px rgba(0,0,0,.18);white-space:nowrap}' +
    '#olmode{display:inline-flex;align-items:center;gap:6px;height:26px;padding:0 12px;border-radius:13px;font-size:14px;' +
    'white-space:nowrap;cursor:pointer;border:1px solid #b9bcc1;background:linear-gradient(#ffffff,#eceef1);color:#333}' +
    '#olmode i{width:9px;height:9px;border-radius:50%;display:inline-block}' +
    '#olmode.on i{background:#2fa84f;box-shadow:0 0 0 2px rgba(47,168,79,.2)}' +
    '#olmode.off i{background:#e0a020;box-shadow:0 0 0 2px rgba(224,160,32,.25)}' +
    '#olmode.lost i{background:#d9534f;box-shadow:0 0 0 2px rgba(217,83,79,.2)}' +
    '#olmode.alone{position:absolute;top:10px;right:10px;z-index:1000;box-shadow:0 1px 4px rgba(0,0,0,.18)}';
  document.head.appendChild(style);
  var pill = document.createElement('div');
  pill.id = 'olpill';
  document.body.appendChild(pill);
  function showPill(text) { pill.textContent = text; pill.style.display = text ? 'block' : 'none'; }

  // the mode: 線上地圖 / 離線地圖, first in the page's tool bar (#bar), else on its own at the top right
  var badge = document.createElement('span');
  badge.id = 'olmode';
  var bar = document.getElementById('bar');
  if (bar) bar.insertBefore(badge, bar.firstChild); else { badge.className = 'alone'; document.body.appendChild(badge); }
  badge.style.cursor = 'pointer';   // a click: 地圖說明與流程 (frmMapHelp)
  badge.addEventListener('click', function () {
    var host = window.chrome && window.chrome.webview;
    if (host) host.postMessage({ type: 'help' });
  });
  var hasVector = offline && /(?:^|[?&])offlinemap=/.test(location.search) && !!window.protomapsL;
  function showMode(kind) {
    badge.className = kind + (bar ? '' : ' alone');
    badge.innerHTML = '<i></i>' + (kind === 'off' ? (hasVector ? '離線地圖' : '離線地圖（簡易）') : kind === 'lost' ? '線上地圖（連不上）' : '線上地圖');
    badge.title = kind === 'off'
      ? (hasVector ? '離線：不連網路，用電腦裡的台灣離線地圖檔（OpenStreetMap 資料）。'
                   : '離線：不連網路，只顯示簡易地圖（縣市、鄉鎮界線與地名）；設定 › 地點 可以下載離線地圖檔。') +
        '設定 › 地點 可以改成線上。點一下看說明與流程。'
      : '線上：OpenStreetMap 街道地圖；連不上時顯示簡易地圖。設定 › 地點 可以改成離線。點一下看說明與流程。';
  }
  showMode(offline ? 'off' : 'on');

  // offline: no OSM tiles at all (OSM's tile policy doesn't allow offline use of them) -- the outline alone
  var tiles = L.tileLayer('tiles/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors　界線：內政部國土測繪中心'
  });
  if (offline) map.attributionControl.addAttribution('界線：內政部國土測繪中心'); else tiles.addTo(map);

  // offline with the 離線地圖檔 downloaded (設定 › 地點; ?offlinemap=offline/taiwan.pmtiles): Taiwan's
  // streets from Protomaps' build of OpenStreetMap data, drawn here (protomaps-leaflet) above the outline
  var m = /(?:^|[?&])offlinemap=([^&]+)/.exec(location.search);
  var vectorMap = offline && m && window.protomapsL ? decodeURIComponent(m[1]) : null;
  if (vectorMap) {
    map.createPane('vector').style.zIndex = 170;
    protomapsL.leafletLayer({
      url: vectorMap, flavor: 'light', lang: 'zh-Hant', pane: 'vector', maxDataZoom: 14,
      bounds: L.latLngBounds([21.6, 117.8], [26.6, 122.6]),   // the file's box (OfflineMap.vb): the outline beyond
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors　Protomaps'
    }).addTo(map);
  }

  if (!offline) {
    // online: the pill (and a red dot) only when the tiles of the view all failed (no internet)
    var ok = 0, failed = 0;
    tiles.on('loading', function () { ok = 0; failed = 0; });
    tiles.on('tileload', function () { ok++; showPill(''); showMode('on'); });
    tiles.on('tileerror', function () { failed++; });
    tiles.on('load', function () {
      var lost = failed > 0 && ok === 0;
      showPill(lost ? '連不上地圖伺服器：顯示簡易地圖' : '');
      showMode(lost ? 'lost' : 'on');
    });
  }

  function county(n) { return String(n || '').replace(/^台/, '臺'); }

  fetch('lib/towns.json').then(function (r) { return r.json(); }).then(function (topo) {
    var o = topo.objects;
    var land = topojson.feature(topo, o.counties);
    var coast = topojson.mesh(topo, o.counties, function (a, b) { return a === b; });
    var countyLines = topojson.mesh(topo, o.counties, function (a, b) { return a !== b; });
    var townLines = topojson.mesh(topo, o.towns, function (a, b) {
      return a !== b && a.properties.COUNTYNAME === b.properties.COUNTYNAME;
    });
    function layer(data, st) { return L.geoJSON(data, { pane: 'outline', renderer: canvas, interactive: false, style: st }); }

    layer(coast, { color: '#9cc3e6', weight: 7, opacity: 0.55 }).addTo(map);          // a soft glow along the coast
    layer(land, { stroke: false, fillColor: '#f6f3ec', fillOpacity: 1 }).addTo(map);
    var towns = layer(townLines, { color: '#cfd3d7', weight: 0.7, opacity: 1 });
    layer(countyLines, { color: '#97a1ab', weight: 1.1, opacity: 1 }).addTo(map);
    layer(coast, { color: '#7d8a97', weight: 1, opacity: 1 }).addTo(map);

    // names: counties up to zoom 9, towns from 10
    function labels(features, name, cls) {
      var g = L.layerGroup();
      features.forEach(function (f) {
        var b = L.geoJSON(f).getBounds();
        if (!b.isValid()) return;
        g.addLayer(L.marker(b.getCenter(), {
          pane: 'outlineLabels', interactive: false, keyboard: false,
          icon: L.divIcon({ className: '', iconSize: null, html: '<div class="ol-label ' + cls + '">' + name(f.properties) + '</div>' })
        }));
      });
      return g;
    }
    var countyNames = labels(land.features, function (p) { return county(p.COUNTYNAME); }, '');
    var townNames = labels(topojson.feature(topo, o.towns).features, function (p) { return p.TOWNNAME; }, 'town');
    function byZoom() {
      var z = map.getZoom();
      if (z >= 9) towns.addTo(map); else map.removeLayer(towns);
      if (z >= 7 && z <= 9) countyNames.addTo(map); else map.removeLayer(countyNames);
      if (z >= 10 && z <= 14) townNames.addTo(map); else map.removeLayer(townNames);
    }
    map.on('zoomend', byZoom);
    byZoom();
  }).catch(function () { /* no outline: the tiles alone, as before */ });

  return tiles;
};
