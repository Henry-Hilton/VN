import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { createPortal } from './portal.mjs';
import { createServer } from './server.mjs';

async function fixture(t) {
  const directory = await mkdtemp(path.join(tmpdir(), 'youthrise-portal-'));
  const config = { portalDirectory: directory, counselorUsername: 'counselor', counselorPassword: 'test-staff-password-123' };
  const portal = await createPortal(config), server = createServer({}, undefined, portal);
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(async () => { await new Promise(resolve => server.close(resolve)); await rm(directory, { recursive: true, force: true }); });
  const base = `http://127.0.0.1:${server.address().port}`;
  const request = async (route, body, token, extra = {}) => {
    const response = await fetch(base + route, { method: body === undefined ? 'GET' : 'POST', headers: { 'Content-Type': 'application/json', 'X-YouthRise-Client': '1', ...(token ? { Authorization: `Bearer ${token}` } : {}), ...extra }, body: body === undefined ? undefined : JSON.stringify(body) });
    return { status: response.status, body: await response.json(), cookie: response.headers.get('set-cookie') };
  };
  const register = async (nickname = 'player', gender = 'female') => request('/api/register', { nickname, name: 'Demo Player', age: 16, gender, password: 'test-player-password', consent: true, role: 'counselor' });
  const staff = async () => (await request('/api/staff/login', { nickname: 'counselor', password: config.counselorPassword })).cookie.split(';')[0];
  return { request, register, staff, directory, config };
}
test('accounts validate consent and profile; passwords hashed, roles cannot be self-assigned', async t => {
  const f = await fixture(t);
  assert.equal((await f.request('/api/register', { nickname: 'none', password: 'long-password', name: 'Player', age: 16, gender: 'female' })).status, 400);
  const user = await f.register(); assert.equal(user.status, 200); assert.equal(user.body.user.role, 'player');
  assert.equal(user.body.user.gender, 'female'); assert.equal(user.body.user.hash, undefined);
  assert.equal((await f.register('PLAYER')).status, 409);
  assert.equal((await f.request('/api/login', { nickname: 'player', password: 'wrong-password' })).status, 401);
  assert.equal((await f.request('/api/staff/login', { nickname: 'player', password: 'test-player-password' })).status, 403);
  const text = await readFile(path.join(f.directory, 'database.json'), 'utf8'); assert.ok(!text.includes('test-player-password')); assert.ok(!text.includes('test-staff-password'));
});
test('dashboard data requires staff session and blocks players; logout revokes access', async t => {
  const f = await fixture(t), user = await f.register();
  assert.equal((await f.request('/api/dashboard')).status, 401);
  assert.equal((await f.request('/api/dashboard', undefined, user.body.token)).status, 403);
  const cookie = await f.staff();
  const dashboard = await f.request('/api/dashboard', undefined, null, { Cookie: cookie });
  assert.equal(dashboard.status, 200); assert.equal(dashboard.body.players.length, 1);
  assert.equal((await f.request('/api/logout', {}, null, { Cookie: cookie })).status, 200);
  assert.equal((await f.request('/api/dashboard', undefined, null, { Cookie: cookie })).status, 401);
});
test('results use authenticated identity and server-derived choice summaries; replay replaces data', async t => {
  const f = await fixture(t), one = await f.register('player-one'), two = await f.register('player-two', 'male');
  const chapter = JSON.parse(await readFile(new URL('../Assets/YouthRise/Resources/YouthRise/chapter1.json', import.meta.url)));
  const node = chapter.nodes.find(n => n.choices?.length);
  const choices = [{ chapter: 1, nodeId: node.id, choiceId: node.choices[0].id }];
  assert.equal((await f.request('/api/results', { userId: two.body.user.id, choices, supportive: 999 }, one.body.token)).status, 200);
  assert.equal((await f.request('/api/results', { choices: [...choices,...choices] }, one.body.token)).status, 400);
  assert.equal((await f.request('/api/results', { choices: [{ chapter: 9, nodeId: 'fake', choiceId: 'fake' }] }, one.body.token)).status, 400);
  const cookie = await f.staff(), dashboard = await f.request('/api/dashboard', undefined, null, { Cookie: cookie });
  assert.equal(dashboard.body.totals[0].observed, 1); assert.equal(dashboard.body.players.find(p => p.id === two.body.user.id).result, null);
  await f.request('/api/results', { choices: [] }, one.body.token);
  assert.equal((await f.request('/api/dashboard', undefined, null, { Cookie: cookie })).body.totals[0].observed, 0);
});
test('incidents require explicit consent, are idempotent and only staff can review', async t => {
  const f = await fixture(t), one = await f.register('player-one'), two = await f.register('player-two');
  const body = { reportId: 'YR-' + 'a'.repeat(32), text: 'Pesan bantuan demo', consent: false };
  assert.equal((await f.request('/api/incidents', body, one.body.token)).status, 403);
  body.consent = true;
  assert.equal((await f.request('/api/incidents', body, one.body.token)).body.status, 'received_by_dashboard');
  assert.equal((await f.request('/api/incidents', body, one.body.token)).status, 200);
  assert.equal((await f.request('/api/incidents', body, two.body.token)).status, 409);
  assert.equal((await f.request('/api/incidents', { ...body, text: 'Changed' }, one.body.token)).status, 409);
  assert.equal((await f.request('/api/incidents/review', { reportId: body.reportId, status: 'closed' }, one.body.token)).status, 403);
  const cookie = await f.staff();
  assert.equal((await f.request('/api/dashboard', undefined, null, { Cookie: cookie })).body.reports.length, 1);
  await f.request('/api/incidents/review', { reportId: body.reportId, status: 'reviewed' }, null, { Cookie: cookie });
  assert.equal((await f.request('/api/dashboard', undefined, null, { Cookie: cookie })).body.reports[0].status, 'reviewed');
});
test('browser writes reject cross-origin requests; malformed input cannot mutate database', async t => {
  const f = await fixture(t);
  assert.equal((await f.request('/api/login', {}, null, { Origin: 'https://evil.invalid' })).status, 403);
  assert.equal((await f.request('/api/register', null)).status, 400);
  assert.equal((await f.request('/api/login', {}, null, { 'X-YouthRise-Client': '' })).status, 403);
});
test('accounts, results and reports survive reopening storage', async t => {
  const f = await fixture(t), user = await f.register();
  await f.request('/api/incidents', { reportId: 'YR-' + 'b'.repeat(32), text: 'Persistent demo', consent: true }, user.body.token);
  const handler = await createPortal(f.config), server = createServer({}, undefined, handler);
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => new Promise(resolve => server.close(resolve)));
  const response = await fetch(`http://127.0.0.1:${server.address().port}/api/login`, { method: 'POST', headers: { 'Content-Type': 'application/json', 'X-YouthRise-Client': '1' }, body: JSON.stringify({ nickname: 'player', password: 'test-player-password' }) });
  assert.equal(response.status, 200);
  const db = JSON.parse(await readFile(path.join(f.directory, 'database.json'))); assert.equal(db.reports.length, 1);
});
