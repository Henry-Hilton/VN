const $ = id => document.getElementById(id);
let data;
const element = (tag, text, className) => { const el = document.createElement(tag); if (text !== undefined) el.textContent = text; if (className) el.className = className; return el; };
async function api(route, body) {
  const response = await fetch(route, { method: body === undefined ? 'GET' : 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json', 'X-YouthRise-Client': '1' }, body: body === undefined ? undefined : JSON.stringify(body) });
  const result = await response.json();
  if (!response.ok) { if (response.status === 401) loggedOut(); throw new Error(result.error ?? 'Koneksi gagal.'); }
  return result;
}
function loggedOut() { data = null; $('workspace').hidden = true; $('login').hidden = false; $('logout').hidden = true; for (const id of ['player-rows','player-detail','report-list','trend-list','stats']) $(id).replaceChildren(); }
function trend(row) {
  const root = element('div', undefined, 'trend'), title = element('div');
  title.append(element('h3', row.title), element('small', `${row.observed} pilihan${row.players !== undefined ? ` · ${row.players} pemain` : ` / ${row.total} keputusan tersedia`}`));
  const bar = element('div', undefined, 'bar');
  const ns = 'http://www.w3.org/2000/svg', svg = document.createElementNS(ns, 'svg');
  svg.setAttribute('viewBox', '0 0 100 15'); svg.setAttribute('preserveAspectRatio', 'none'); svg.setAttribute('role', 'img');
  svg.setAttribute('aria-label', `Mendukung ${row.supportive}, tekanan ${row.pressure}, campuran ${row.mixed}`);
  let offset = 0;
  for (const key of ['supportive','pressure','mixed']) { const rect = document.createElementNS(ns, 'rect'), width = row.observed ? row[key] / row.observed * 100 : 0; rect.setAttribute('x', offset); rect.setAttribute('y', 0); rect.setAttribute('width', width); rect.setAttribute('height', 15); rect.setAttribute('class', key); svg.append(rect); offset += width; }
  bar.append(svg);
  root.append(title, bar, element('div', `${row.supportive} / ${row.pressure} / ${row.mixed}`, 'counts')); return root;
}
function renderPlayers() {
  $('player-rows').replaceChildren();
  const search = $('search').value.toLowerCase();
  for (const p of data.players.filter(p => `${p.name} ${p.nickname}`.toLowerCase().includes(search))) {
    const row = element('tr'), identity = element('td'); identity.append(element('strong', p.name), element('small', `@${p.nickname}`));
    const actions = element('td'), button = element('button', 'Lihat hasil');
    button.onclick = () => { $('player-detail').replaceChildren(element('h2', `Pilihan ${p.nickname}`)); if (!p.result) $('player-detail').append(element('p', 'Belum ada hasil tersinkron.')); else p.result.rows.forEach(r => $('player-detail').append(trend(r))); };
    actions.append(button); row.append(identity, element('td', `${p.age} / ${p.gender === 'female' ? 'Perempuan · Anita' : 'Laki-laki · Alex'}`), element('td', p.result ? p.result.rows.reduce((n, r) => n + r.observed, 0) : 'Belum ada data'), element('td', p.result ? new Date(p.result.updatedUtc).toLocaleString('id-ID') : '—'), actions); $('player-rows').append(row);
  }
  if (!$('player-rows').children.length) { const row = element('tr'), cell = element('td', 'Belum ada pemain yang cocok.', 'empty'); cell.colSpan = 5; row.append(cell); $('player-rows').append(row); }
}
async function refresh() {
  try {
    data = await api('/api/dashboard'); $('login').hidden = true; $('workspace').hidden = false; $('logout').hidden = false;
    $('status').textContent = `Diperbarui ${new Date().toLocaleTimeString('id-ID')} · Akun ${data.user.nickname}`;
    $('stats').replaceChildren();
    for (const [label, value] of [['Pemain terdaftar', data.players.length], ['Keputusan tercatat', data.totals.reduce((n, r) => n + r.observed, 0)], ['Laporan baru', data.reports.filter(r => r.status === 'new').length]]) { const card = element('div', undefined, 'stat'); card.append(element('span', label), element('strong', value)); $('stats').append(card); }
    $('trend-list').replaceChildren(...data.totals.map(trend)); renderPlayers(); $('report-list').replaceChildren();
    for (const r of [...data.reports].reverse()) {
      const card = element('article', undefined, 'report');
      card.append(element('h3', `${r.player.name} · @${r.player.nickname}`), element('small', `${new Date(r.createdUtc).toLocaleString('id-ID')} · ${r.id}`), element('p', `Status: ${{new:'Baru',reviewed:'Sudah ditinjau',closed:'Ditutup'}[r.status]}`), element('pre', r.text));
      for (const [status, label] of [['reviewed','Tandai ditinjau'],['closed','Tutup laporan'],['new','Buka kembali']]) { if (status === r.status) continue; const button = element('button', label); button.onclick = async () => { button.disabled = true; try { await api('/api/incidents/review', { reportId: r.id, status }); await refresh(); } catch (e) { $('status').textContent = e.message; button.disabled = false; } }; card.append(button); }
      $('report-list').append(card);
    }
    if (!data.reports.length) $('report-list').append(element('p', 'Belum ada laporan yang dikirim pemain.', 'empty'));
  } catch (e) { $('status').textContent = e.message; if (!data) $('login-message').textContent = e.message; }
}
$('login-form').onsubmit = async e => { e.preventDefault(); const button = e.target.querySelector('button'); button.disabled = true; $('login-message').textContent = 'Memeriksa akun…'; try { await api('/api/staff/login', Object.fromEntries(new FormData(e.target))); e.target.reset(); await refresh(); } catch (error) { $('login-message').textContent = error.message; } finally { button.disabled = false; } };
$('logout').onclick = async () => { try { await api('/api/logout', {}); loggedOut(); $('login-message').textContent = 'Anda sudah keluar.'; } catch (e) { $('status').textContent = e.message; } };
$('refresh').onclick = refresh; $('search').oninput = renderPlayers;
document.querySelectorAll('[data-tab]').forEach(button => button.onclick = () => { document.querySelectorAll('.tab').forEach(tab => tab.hidden = tab.id !== button.dataset.tab); document.querySelectorAll('[data-tab]').forEach(b => b.classList.toggle('selected', b === button)); });
refresh();
