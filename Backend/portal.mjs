import { randomBytes, randomUUID, scrypt as scryptCallback, timingSafeEqual } from 'node:crypto';
import { promisify } from 'node:util';
import { mkdir, readFile, writeFile, rename } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const scrypt = promisify(scryptCallback);
const here = path.dirname(fileURLToPath(import.meta.url));
const requireValid = (ok, message, status = 400) => { if (!ok) throw Object.assign(new Error(message), { status }); };
const publicUser = ({ id, name, nickname, age, gender, role }) => ({ id, name, nickname, age, gender, role });
const pressureStats = new Set(['risk', 'anxiety', 'fomoIndex', 'socialMediaDependencyTendency']);
const titles = ['Tekanan teman & zat berisiko','Bullying & cyberbullying','Hubungan & keamanan pribadi','Kesejahteraan emosional','Keamanan finansial','Gaya hidup sehat','Keseimbangan digital','Komunikasi & dukungan keluarga'];
async function passwordHash(password, salt) { return Buffer.from(await scrypt(password, salt, 64)).toString('hex'); }

// One local school/demo per server. No public counselor registration.
export async function createPortal(config = {}) {
  const directory = path.resolve(config.portalDirectory ?? path.join(here, 'private-portal'));
  await mkdir(directory, { recursive: true });
  const filename = path.join(directory, 'database.json');
  let db;
  try { db = JSON.parse(await readFile(filename, 'utf8')); }
  catch (error) { if (error.code !== 'ENOENT') throw error; db = { users: [], results: {}, reports: [] }; }
  const chapters = await Promise.all(Array.from({ length: 8 }, (_, i) => readFile(path.join(here, `../Assets/YouthRise/Resources/YouthRise/chapter${i + 1}.json`), 'utf8').then(JSON.parse)));
  let queue = Promise.resolve();
  function mutate(action) {
    const job = queue.then(async () => {
      const next = structuredClone(db);
      const result = await action(next);
      await writeFile(filename + '.tmp', JSON.stringify(next), { mode: 0o600 });
      await rename(filename + '.tmp', filename); db = next;
      return result;
    });
    queue = job.catch(() => {}); return job;
  }
  if (config.counselorUsername && config.counselorPassword) {
    requireValid(config.counselorPassword.length >= 12, 'Counselor password needs at least 12 characters.');
    const nickname = config.counselorUsername.toLowerCase();
    requireValid(/^[a-z0-9_.-]{3,30}$/.test(nickname), 'Invalid counselor username.');
    requireValid(!db.users.some(u => u.nickname === nickname && u.role !== 'counselor'), 'Counselor username already belongs to a player.');
    if (!db.users.some(u => u.nickname === nickname)) {
      const salt = randomBytes(16).toString('hex');
      const hash = await passwordHash(config.counselorPassword, salt);
      await mutate(next => next.users.push({ id: randomUUID(), name: 'Konselor', nickname, role: 'counselor', salt, hash }));
    }
  }
  const sessions = new Map(), limits = new Map();
  const headers = { 'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff', 'Referrer-Policy': 'no-referrer', 'Content-Security-Policy': "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'" };
  function summarize(choices) {
    requireValid(Array.isArray(choices) && choices.length <= 100, 'Data pilihan tidak valid.');
    const rows = chapters.map((c, i) => ({ title: titles[i], total: c.nodes.filter(n => n.choices?.length).length, observed: 0, supportive: 0, pressure: 0, mixed: 0 }));
    const seen = new Set();
    for (const recorded of choices) {
      requireValid(recorded && Number.isInteger(recorded.chapter), 'Referensi pilihan tidak valid.');
      const chapter = chapters[recorded?.chapter - 1];
      const choice = chapter?.nodes.find(n => n.id === recorded.nodeId)?.choices?.find(c => c.id === recorded.choiceId);
      const key = `${recorded.chapter}:${recorded.nodeId}`;
      requireValid(choice && !seen.has(key), 'Referensi pilihan tidak valid atau duplikat.'); seen.add(key);
      const row = rows[recorded.chapter - 1]; row.observed++;
      let protective = false, pressure = false;
      for (const effect of choice.effects ?? []) {
        if (!effect.amount || effect.stat === 'xp') continue;
        const positive = pressureStats.has(effect.stat) ? effect.amount < 0 : effect.amount > 0;
        protective ||= positive; pressure ||= !positive;
      }
      row[protective && !pressure ? 'supportive' : pressure && !protective ? 'pressure' : 'mixed']++;
    }
    return rows;
  }
  return async function handle(req, res) {
    if (!req.url.startsWith('/api/') && !['/','/dashboard','/dashboard.js','/dashboard.css'].includes(req.url)) return false;
    const send = (status, body, extra = {}) => { res.writeHead(status, { ...headers, 'Content-Type': 'application/json; charset=utf-8', ...extra }); res.end(JSON.stringify(body)); };
    try {
      if (req.method === 'GET' && !req.url.startsWith('/api/')) {
        const name = req.url.endsWith('.js') ? 'dashboard.js' : req.url.endsWith('.css') ? 'dashboard.css' : 'dashboard.html';
        res.writeHead(200, { ...headers, 'Content-Type': name.endsWith('.js') ? 'text/javascript' : name.endsWith('.css') ? 'text/css' : 'text/html; charset=utf-8' });
        res.end(await readFile(path.join(here, 'public', name))); return true;
      }
      const now = Date.now();
      for (const [key, value] of sessions) if (value.expires <= now) sessions.delete(key);
      for (const [key, value] of limits) if (value.until <= now) limits.delete(key);
      const loginRoute = ['/api/login','/api/register','/api/staff/login'].includes(req.url);
      const rateKey = `${req.socket.remoteAddress}:${loginRoute ? 'auth' : 'api'}`;
      const limit = limits.get(rateKey) ?? { count: 0, until: now + 60000 };
      limits.set(rateKey, limit);
      requireValid(++limit.count <= (loginRoute ? 20 : 240), 'Terlalu banyak permintaan. Coba satu menit lagi.', 429);
      let body = {};
      if (req.method === 'POST') {
        requireValid((req.headers['content-type'] ?? '').startsWith('application/json'), 'JSON required.', 415);
        // Custom header forces a same-origin browser request; no CORS allowance.
        requireValid(req.headers['x-youthrise-client'] === '1', 'Client header required.', 403);
        if (req.headers.origin) requireValid(req.headers.origin === `${config.secureCookies ? 'https' : 'http'}://${req.headers.host}`, 'Origin rejected.', 403);
        let size = 0; const chunks = [];
        for await (const chunk of req) { size += chunk.length; requireValid(size <= 40000, 'Request too large.', 413); chunks.push(chunk); }
        try { body = JSON.parse(Buffer.concat(chunks).toString('utf8')); } catch { requireValid(false, 'JSON tidak valid.'); }
        requireValid(body && typeof body === 'object' && !Array.isArray(body), 'Body tidak valid.');
      }
      const cookieToken = /(?:^|;\s*)yr_session=([a-f0-9]{64})(?:;|$)/.exec(req.headers.cookie ?? '')?.[1];
      const token = req.headers.authorization?.replace(/^Bearer /, '') ?? cookieToken;
      const session = sessions.get(token);
      const user = session && db.users.find(u => u.id === session.userId);
      const cookie = value => `yr_session=${value}; HttpOnly; SameSite=Strict; Path=/; Max-Age=${value ? 28800 : 0}${config.secureCookies ? '; Secure' : ''}`;
      if (loginRoute && req.method === 'POST') {
        requireValid(typeof body.nickname === 'string' && /^[a-zA-Z0-9_.-]{3,30}$/.test(body.nickname), 'Nickname harus 3–30 huruf, angka, titik, garis atau underscore.');
        requireValid(typeof body.password === 'string' && body.password.length >= 8 && body.password.length <= 128, 'Password harus 8–128 karakter.');
        const nickname = body.nickname.toLowerCase(); let account;
        if (req.url === '/api/register') {
          requireValid(body.consent === true, 'Setujui penyimpanan profil dan hasil game untuk konselor.');
          requireValid(typeof body.name === 'string' && body.name.trim().length >= 2 && body.name.trim().length <= 80, 'Nama harus 2–80 karakter.');
          requireValid(Number.isInteger(body.age) && body.age >= 11 && body.age <= 18, 'Usia harus 11–18 tahun.');
          requireValid(['male','female'].includes(body.gender), 'Pilih gender.');
          const salt = randomBytes(16).toString('hex'), hash = await passwordHash(body.password, salt);
          account = await mutate(next => {
            requireValid(!next.users.some(u => u.nickname === nickname), 'Nickname sudah digunakan.', 409);
            const value = { id: randomUUID(), nickname, name: body.name.trim(), age: body.age, gender: body.gender, role: 'player', salt, hash, consentUtc: new Date().toISOString() };
            next.users.push(value); return value;
          });
        } else {
          account = db.users.find(u => u.nickname === nickname);
          const hash = await passwordHash(body.password, account?.salt ?? 'youthrise-dummy-salt');
          requireValid(account && timingSafeEqual(Buffer.from(hash, 'hex'), Buffer.from(account.hash, 'hex')), 'Nickname atau password salah.', 401);
          requireValid(account.role === (req.url === '/api/staff/login' ? 'counselor' : 'player'), 'Akun tidak memiliki akses ini.', 403);
        }
      const newToken = randomBytes(32).toString('hex'); sessions.set(newToken, { userId: account.id, expires: now + 8 * 3600000 });
        send(200, { success: true, user: publicUser(account), ...(account.role === 'player' ? { token: newToken } : {}) }, account.role === 'counselor' ? { 'Set-Cookie': cookie(newToken) } : {});
        return true;
      }
      requireValid(user, 'Sesi berakhir. Silakan login lagi.', 401);
      if (req.url === '/api/logout' && req.method === 'POST') { sessions.delete(token); send(200, { success: true }, { 'Set-Cookie': cookie('') }); return true; }
      if (req.url === '/api/results' && req.method === 'POST') {
        requireValid(user.role === 'player', 'Akses ditolak.', 403);
        const rows = summarize(body.choices);
        await mutate(next => { next.results[user.id] = { rows, updatedUtc: new Date().toISOString() }; });
        send(200, { success: true }); return true;
      }
      if (req.url === '/api/incidents' && req.method === 'POST') {
        requireValid(user.role === 'player', 'Akses ditolak.', 403);
        requireValid(body.consent === true, 'Konfirmasi pengiriman diperlukan.', 403);
        requireValid(/^YR-[a-f0-9]{32}$/.test(body.reportId ?? ''), 'ID laporan tidak valid.');
        requireValid(typeof body.text === 'string' && body.text.trim().length > 0 && body.text.length <= 3500, 'Isi laporan harus 1–3500 karakter.');
        const report = await mutate(next => {
          const existing = next.reports.find(r => r.id === body.reportId);
          if (existing) { requireValid(existing.userId === user.id && existing.text === body.text, 'ID sudah digunakan.', 409); return existing; }
          const value = { id: body.reportId, userId: user.id, text: body.text, status: 'new', createdUtc: new Date().toISOString(), reviewedUtc: null };
          next.reports.push(value); return value;
        });
        send(200, { success: true, reportId: report.id, status: 'received_by_dashboard' }); return true;
      }
      requireValid(user.role === 'counselor', 'Hanya sekolah/konselor yang memiliki akses.', 403);
      if (req.url === '/api/dashboard' && req.method === 'GET') {
        const players = db.users.filter(u => u.role === 'player').map(u => ({ ...publicUser(u), result: db.results[u.id] ?? null }));
        const totals = chapters.map((_, i) => ({ title: titles[i], observed: 0, supportive: 0, pressure: 0, mixed: 0, players: 0 }));
        for (const p of players) p.result?.rows.forEach((r, i) => { for (const key of ['observed','supportive','pressure','mixed']) totals[i][key] += r[key]; if (r.observed) totals[i].players++; });
        send(200, { success: true, user: publicUser(user), players, totals, reports: db.reports.map(r => ({ ...r, player: publicUser(db.users.find(u => u.id === r.userId)) })) }); return true;
      }
      if (req.url === '/api/incidents/review' && req.method === 'POST') {
        requireValid(['new','reviewed','closed'].includes(body.status), 'Status tidak valid.');
        await mutate(next => { const report = next.reports.find(r => r.id === body.reportId); requireValid(report, 'Laporan tidak ditemukan.', 404); report.status = body.status; report.reviewedUtc = new Date().toISOString(); report.reviewedBy = user.id; });
        send(200, { success: true }); return true;
      }
      send(404, { success: false, error: 'Not found.' });
    } catch (error) { send(error.status ?? 500, { success: false, error: error.status ? error.message : 'Layanan lokal tidak tersedia.' }); }
    return true;
  };
}
