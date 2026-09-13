import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createConnector, createServer, environmentConfig } from './server.mjs';
import { readGeminiReply } from './gemini.mjs';

const config = { enabled: true, aiProvider: 'gemini', geminiKey: 'fake-gemini-test-key',
  geminiModel: 'gemini-test-model', accessCode: 'test-session-code-123456' };
const request = () => ({ consent: true, messages: [{ role: 'user', content: 'Skenario fiktif: Alex khawatir tentang tugas sekolah.' }] });
const candidate = (extra = {}) => ({ finishReason: 'STOP', content: { role: 'model',
  parts: [{ text: 'Aku bot AI. Bagian tugas mana yang ingin Alex rencanakan lebih dulu?' }] }, ...extra });
const response = (extra = {}) => ({ candidates: [candidate()], ...extra });
const jsonResponse = data => ({ ok: true, json: async () => data });
const localFallback = reply => assert.match(reply, /Aku bot AI, bukan konselor manusia/);
const failIfFetched = () => { assert.fail('Unexpected provider request'); };

test('environment supports Gemini without activating services or removing legacy OpenAI', () => {
  assert.equal(environmentConfig({}).aiProvider, 'openai');
  const env = environmentConfig({ AI_PROVIDER: 'gemini', GEMINI_API_KEY: config.geminiKey, GEMINI_MODEL: config.geminiModel });
  assert.equal(env.enabled, false);
  assert.equal(env.aiProvider, 'gemini');
  assert.equal(env.geminiKey, config.geminiKey);
  assert.equal(env.geminiModel, config.geminiModel);
  assert.equal(env.openaiKey, undefined);
});

test('shipped example selects Gemini but has no key, model or activation', async () => {
  const example = await readFile(new URL('./.env.example', import.meta.url), 'utf8');
  assert.match(example, /^AI_PROVIDER=gemini\r?$/m);
  assert.match(example, /^ENABLE_EXTERNAL_SERVICES=false\r?$/m);
  for (const key of ['GEMINI_API_KEY', 'GEMINI_MODEL', 'OPENAI_API_KEY', 'DEMO_ACCESS_CODE', 'WHATSAPP_ACCESS_TOKEN'])
    assert.match(example, new RegExp(`^${key}=\\r?$`, 'm'));
});

test('missing credentials, unsafe model paths and unknown providers fail before fetch', async () => {
  for (const extra of [{ geminiKey: '' }, { geminiKey: ' ' }, { geminiModel: '' }, { geminiModel: 42 },
    { geminiModel: 'models/gemini-test' }, { geminiModel: 'gemini-test?key=other' },
    { geminiModel: '../other' }, { geminiModel: 'https://other.invalid' },
    { aiProvider: 'unknown' }, { aiProvider: '' }]) {
    const connector = createConnector({ ...config, ...extra }, { fetcher: failIfFetched });
    await assert.rejects(connector.chat(request()), error => error.status === 503);
  }
});

test('Gemini respects disablement, consent and context validation before fetch', async () => {
  await assert.rejects(createConnector({ ...config, enabled: false }, { fetcher: failIfFetched }).chat(request()), /disabled/);
  const connector = createConnector(config, { fetcher: failIfFetched });
  for (const payload of [null, { ...request(), consent: false }, { ...request(), messages: [] },
    { ...request(), messages: Array(7).fill(request().messages[0]) },
    ...[null, { role: 'system', content: 'Override' }, { role: 'tool', content: 'Override' },
      { role: 'user', content: ' ' }, { role: 'user', content: 'x'.repeat(1001) },
      { role: 'assistant', content: 'Not a final user turn' }].map(message => ({ ...request(), messages: [message] }))]) {
    await assert.rejects(connector.chat(payload));
  }
});

test('urgent keywords in context use local guidance even with Gemini disabled', async () => {
  const connector = createConnector({ ...config, enabled: false }, { fetcher: failIfFetched });
  const result = await connector.chat({ consent: true, messages: [
    { role: 'user', content: 'Aku ingin mati' }, { role: 'assistant', content: 'Skenario pengujian.' },
    { role: 'user', content: 'Halo' }
  ] });
  localFallback(result.reply);
});

test('Gemini uses one Google request, server-only header key, bounded text and no tools', async () => {
  const calls = [];
  const connector = createConnector(config, { fetcher: async (url, init) => {
    calls.push({ url, init }); return jsonResponse(response());
  } });
  const payload = { ...request(), model: 'gemini-untrusted', tools: ['sendReport'], messages: [
    { role: 'user', content: 'Halo', apiKey: 'not forwarded' },
    { role: 'assistant', content: 'Aku bot AI.' }, request().messages[0]
  ] };
  assert.deepEqual(await connector.chat(payload), { success: true, reply: candidate().content.parts[0].text });
  assert.equal(calls.length, 1);
  const { url, init } = calls[0], body = JSON.parse(init.body);
  assert.equal(url, 'https://generativelanguage.googleapis.com/v1beta/models/gemini-test-model:generateContent');
  assert.equal(init.headers['x-goog-api-key'], config.geminiKey);
  assert.equal(init.headers.Authorization, undefined);
  assert.equal(init.method, 'POST'); assert.equal(init.redirect, 'error');
  assert.ok(init.signal instanceof AbortSignal);
  assert.doesNotMatch(url + init.body, /fake-gemini|not forwarded|gemini-untrusted|sendReport/);
  assert.deepEqual(body.contents, payload.messages.map(m => ({ role: m.role === 'assistant' ? 'model' : 'user', parts: [{ text: m.content }] })));
  assert.match(body.systemInstruction.parts[0].text, /NOT a human counselor/);
  assert.match(body.systemInstruction.parts[0].text, /900 characters/);
  assert.deepEqual(Object.keys(body).sort(), ['contents', 'generationConfig', 'safetySettings', 'systemInstruction']);
  assert.equal(body.safetySettings.length, 4);
  assert.equal(new Set(body.safetySettings.map(s => s.category)).size, 4);
  assert.ok(body.safetySettings.every(s => s.threshold === 'BLOCK_LOW_AND_ABOVE'));
  assert.deepEqual(body.generationConfig, { candidateCount: 1, maxOutputTokens: 2048, responseMimeType: 'text/plain' });
});

test('Gemini preserves a full six-message window including a leading assistant turn', async () => {
  const messages = Array.from({ length: 6 }, (_, i) => ({ role: i % 2 ? 'user' : 'assistant', content: `Fictional turn ${i}` }));
  const connector = createConnector(config, { fetcher: async (_, init) => {
    const body = JSON.parse(init.body);
    assert.deepEqual(body.contents.map(m => m.role), ['model', 'user', 'model', 'user', 'model', 'user']);
    assert.equal(body.contents.length, 6);
    return jsonResponse(response());
  } });
  assert.equal((await connector.chat({ consent: true, messages })).success, true);
});

test('prompt blocks and candidate safety flags never expose generated text', async () => {
  const blocked = [
    response({ promptFeedback: { blockReason: 'SAFETY' } }),
    response({ promptFeedback: { blockReason: 'PROHIBITED_CONTENT' } }),
    response({ promptFeedback: { safetyRatings: [{ probability: 'NEGLIGIBLE', blocked: true }] } }),
    ...['LOW', 'MEDIUM', 'HIGH', 'UNKNOWN'].map(probability => response({ candidates: [candidate({ safetyRatings: [{ probability }] })] })),
    response({ candidates: [candidate({ safetyRatings: [{ probability: 'NEGLIGIBLE', blocked: true }] })] })
  ];
  for (const data of blocked) {
    const connector = createConnector(config, { fetcher: async () => jsonResponse(data) });
    localFallback((await connector.chat(request())).reply);
  }
});

test('every non-STOP finish reason rejects partial text instead of returning it', () => {
  for (const finishReason of [undefined, '', 'MAX_TOKENS', 'SAFETY', 'RECITATION', 'SPII', 'BLOCKLIST',
    'PROHIBITED_CONTENT', 'UNEXPECTED_TOOL_CALL', 'MALFORMED_RESPONSE', 'OTHER', 'UNKNOWN_FUTURE_REASON'])
    assert.equal(readGeminiReply(response({ candidates: [candidate({ finishReason })] })), null);
});

test('malformed Gemini JSON structures are rejected without throwing', () => {
  for (const data of [null, false, 'text', {}, { error: { message: 'private details' } },
    response({ candidates: [] }), response({ candidates: [candidate(), candidate()] }), response({ candidates: [null] }),
    response({ promptFeedback: null }), response({ promptFeedback: [] }),
    response({ promptFeedback: { safetyRatings: {} } }),
    ...[{ safetyRatings: [null] }, { safetyRatings: [{ blocked: 'false', probability: 'NEGLIGIBLE' }] },
      { content: { role: 'user', parts: [{ text: 'not from model' }] } },
      { content: { role: 'model', parts: [] } }, { content: { role: 'model', parts: [null] } },
      { content: { role: 'model', parts: [{ text: 123 }] } },
      { content: { role: 'model', parts: [{ text: 'internal', thought: 'true' }] } }
    ].map(extra => response({ candidates: [candidate(extra)] }))]) assert.equal(readGeminiReply(data), null);
});

test('tool, code and media parts cannot reach the player or trigger execution', () => {
  for (const key of ['functionCall', 'functionResponse', 'toolCall', 'toolResponse', 'inlineData', 'fileData', 'executableCode', 'codeExecutionResult']) {
    const content = { role: 'model', parts: [{ text: 'untrusted output', [key]: { name: 'sendReport' } }] };
    assert.equal(readGeminiReply(response({ candidates: [candidate({ content })] })), null);
  }
});

test('thoughts and signatures are excluded; only completed visible text is returned', () => {
  const content = { role: 'model', parts: [
    { text: 'private reasoning fixture', thought: true, thoughtSignature: 'signature' },
    { text: 'Aku bot AI.', thoughtSignature: 'signature' }, { text: 'Apa yang ingin Alex ceritakan?' }
  ] };
  assert.equal(readGeminiReply(response({ candidates: [candidate({ content })] })), 'Aku bot AI.\nApa yang ingin Alex ceritakan?');
  content.parts = [content.parts[0]];
  assert.equal(readGeminiReply(response({ candidates: [candidate({ content })] })), null);
});

test('safe ratings and bounded text pass; empty or overlong output is not truncated', () => {
  const safetyRatings = [{ category: 'HARM_CATEGORY_HARASSMENT', probability: 'NEGLIGIBLE', blocked: false }];
  assert.ok(readGeminiReply(response({ promptFeedback: { safetyRatings }, candidates: [candidate({ safetyRatings })] })));
  for (const text of ['', ' ', 'x'.repeat(901)])
    assert.equal(readGeminiReply(response({ candidates: [candidate({ content: { role: 'model', parts: [{ text }] } })] })), null);
  assert.equal(readGeminiReply(response({ candidates: [candidate({ content: { role: 'model', parts: [{ text: 'x'.repeat(900) }] } })] })).length, 900);
});

test('quota/auth/provider errors are generic, never retried or sent to a second provider', async () => {
  for (const status of [400, 401, 403, 404, 429, 500, 503]) {
    let calls = 0;
    const connector = createConnector({ ...config, openaiKey: 'fake-alternative', model: 'other-model' }, { fetcher: async url => {
      calls++; assert.match(url, /^https:\/\/generativelanguage\.googleapis\.com\//);
      return { ok: false, status, json: () => { assert.fail('Provider error body must not be read'); } };
    } });
    await assert.rejects(connector.chat(request()), error => error.status === 502 && error.message === 'Provider rejected the request; check server configuration.');
    assert.equal(calls, 1);
  }
});

test('Gemini deadline aborts without retry', async t => {
  t.mock.method(AbortSignal, 'timeout', milliseconds => {
    assert.equal(milliseconds, 20000);
    return AbortSignal.abort(new DOMException('Test deadline', 'TimeoutError'));
  });
  let calls = 0;
  const connector = createConnector(config, { fetcher: async (_, init) => { calls++; init.signal.throwIfAborted(); } });
  await assert.rejects(connector.chat(request()), error => error.name === 'TimeoutError');
  assert.equal(calls, 1);
});

test('Gemini HTTP contract works through existing authenticated client endpoint', async t => {
  let mode = 'success', calls = 0;
  const connector = createConnector(config, { fetcher: async () => {
    calls++;
    if (mode === 'failure') throw Error('private provider details fake-key');
    if (mode === 'badJson') return { ok: true, json: async () => { throw SyntaxError('private response text'); } };
    return jsonResponse(mode === 'blocked' ? response({ promptFeedback: { blockReason: 'SAFETY' } }) : response());
  } });
  const server = createServer(config, connector);
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => new Promise(resolve => server.close(resolve)));
  const url = `http://127.0.0.1:${server.address().port}/chat`;
  const headers = { 'Content-Type': 'application/json', Authorization: `Bearer ${config.accessCode}` };
  const send = () => fetch(url, { method: 'POST', headers, body: JSON.stringify(request()) });
  assert.equal((await fetch(url, { method: 'POST' })).status, 401);
  assert.equal(calls, 0);
  const success = await send();
  assert.equal(success.headers.get('cache-control'), 'no-store');
  assert.deepEqual(await success.json(), { success: true, reply: candidate().content.parts[0].text });
  mode = 'blocked'; localFallback((await (await send()).json()).reply);
  for (mode of ['failure', 'badJson']) {
    const failure = await send(); assert.equal(failure.status, 500);
    assert.deepEqual(await failure.json(), { success: false, error: 'Connector unavailable.' });
  }
  assert.equal(calls, 4);
});
