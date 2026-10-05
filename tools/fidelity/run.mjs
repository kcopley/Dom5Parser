#!/usr/bin/env node
// Fidelity suite: checks Dom5Parser against the dom6inspector oracle in four stages, by
// comparing what the inspector *holds in memory* after parsing, not file text.
//
//   1. Inspector self-check: load a mod in the inspector, export what it read, reload the
//      export -> the inspector's data must be functionally the same (compared after its
//      full post-processing). Validates the oracle itself.
//   2. Vanilla data: (a) the exporter round-trips the CSV gamedata; (b) the committed
//      vanilla.dm matches what the pinned oracle generates; (c) audit of vanilla.dm values
//      that differ from the raw game data (e.g. display values instead of base values).
//   3. Save fidelity: Dom5Parser load -> save -> the inspector's parse of the saved file must
//      equal its parse of the original (strict: compared right after parsing).
//   4. Edits: Dom5Parser load -> scripted edits -> save -> must differ from an unedited save
//      by exactly the expected changes.
//
// Usage: node tools/fidelity/run.mjs [--oracle DIR] [--stages 1,2,3,4] [--only TEXT]
//                                    [--quick] [--update-baselines] [--json summary.json]
// --quick = stages 3-4 only (about 1-2 min); the default runs all stages (~15 min).
// Needs: Node 18+, a built Dom5Tests (Dom5Tests/bin/Debug/net8.0), and a checkout of the
// kcopley/dom6inspector fork (branch export-test). Exit 0 = no failures.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const SUITE = JSON.parse(fs.readFileSync(path.join(ROOT, 'tools/fidelity/suite.json'), 'utf8'));
const BASELINES = path.join(ROOT, 'tools/fidelity/baselines');
const WORK = path.join(ROOT, 'Dom5Tests/bin/fidelity');

// --- arguments & environment -------------------------------------------------
const opt = { stages: null, only: null, quick: false, update: false, json: null, oracle: process.env.DOM6INSPECTOR || null };
for (let i = 2; i < process.argv.length; i++) {
	const a = process.argv[i];
	if (a === '--oracle') opt.oracle = process.argv[++i];
	else if (a === '--stages') opt.stages = process.argv[++i].split(',').map(Number);
	else if (a === '--only') opt.only = process.argv[++i];
	else if (a === '--quick') opt.quick = true;
	else if (a === '--update-baselines') opt.update = true;
	else if (a === '--json') opt.json = process.argv[++i];
	else { console.error('unknown argument ' + a); process.exit(2); }
}
// --quick: the everyday loop. Skips the oracle self-checks (stages 1-2 need the inspector's
// full post-processing, ~50s per load); run those in CI or after oracle/vanilla.dm changes.
if (!opt.stages) opt.stages = opt.quick ? [3, 4] : [1, 2, 3, 4];
const ORACLE = [opt.oracle, path.join(ROOT, '..', 'dom6inspector'), '/mnt/c/Projects/dom6inspector', 'C:\\Projects\\dom6inspector']
	.filter(Boolean).map((p) => path.resolve(ROOT, p)).find((p) => fs.existsSync(path.join(p, 'scripts/headless/roundtrip_check.js')));
if (!ORACLE) { console.error('dom6inspector oracle not found; pass --oracle DIR or set DOM6INSPECTOR'); process.exit(2); }
fs.mkdirSync(WORK, { recursive: true });
fs.mkdirSync(BASELINES, { recursive: true });

const IS_WSL = process.platform === 'linux' && /microsoft/i.test(os.release());
const BIN = path.join(ROOT, 'Dom5Tests/bin/Debug/net8.0');
function winPath(p) { return spawnSync('wslpath', ['-w', p], { encoding: 'utf8' }).stdout.trim(); }

function dom5tests(argv) {
	const exe = path.join(BIN, 'Dom5Tests.exe');
	let cmd = 'dotnet', args = [path.join(BIN, 'Dom5Tests.dll'), ...argv];
	if (process.platform === 'win32') { cmd = exe; args = argv; }
	else if (IS_WSL && fs.existsSync(exe)) { cmd = exe; args = argv.map((a) => (a.startsWith('/') ? winPath(a) : a)); }
	const r = spawnSync(cmd, args, { encoding: 'utf8', maxBuffer: 1 << 26 });
	return { code: r.status, out: (r.stdout || '') + (r.stderr || '') + (r.error ? String(r.error) : '') };
}

function oracle(script, argv) {
	const r = spawnSync(process.execPath, [path.join(ORACLE, 'scripts/headless', script), ...argv], { encoding: 'utf8', maxBuffer: 1 << 28 });
	return { code: r.status, out: (r.stdout || '') + (r.stderr || '') };
}

// roundtrip_check.js -> parsed JSON report (throws on setup error)
function compare(a, b, at, tag) {
	const json = path.join(WORK, tag + '.' + at + '.json');
	const r = oracle('roundtrip_check.js', [a, b, '--at', at, '--json', json, '--show', '0']);
	if (r.code !== 0 && r.code !== 1) throw new Error('oracle failed: ' + r.out.trim().split('\n').slice(-3).join(' | '));
	return JSON.parse(fs.readFileSync(json, 'utf8'));
}

// --- judging ------------------------------------------------------------------
const results = [];
function record(stage, name, status, detail) {
	results.push({ stage, name, status, detail });
	const mark = { PASS: 'PASS ', FAIL: 'FAIL ', XFAIL: 'xfail', XPASS: 'XPASS', IMPROVED: 'PASS+', NEW: 'NEW  ' }[status] || status;
	console.log(`  [${mark}] ${name}${detail ? '  ' + detail : ''}`);
}

function summarize(rep) { return rep.dataDiffs + ' data / ' + rep.diagnosticDiffs + ' diagnostic diffs'; }
function topFields(counts, n = 4) { return Object.entries(counts).sort((x, y) => y[1] - x[1]).slice(0, n).map(([f, c]) => f + ' ' + c).join(', '); }

function judgePass(stage, name, rep, knownFailing) {
	const ok = rep.dataDiffs === 0 && rep.diagnosticDiffs === 0;
	if (knownFailing) record(stage, name, ok ? 'XPASS' : 'XFAIL', ok ? 'now passes; remove knownFailing' : knownFailing);
	else record(stage, name, ok ? 'PASS' : 'FAIL', ok ? '' : summarize(rep) + (rep.dataDiffs ? ' (' + topFields(rep.fieldCounts) + ')' : ''));
}

// Ratchet: fail only if a field count (or the diagnostic count) rises above the baseline.
function judgeBaseline(stage, name, current) {
	const file = path.join(BASELINES, `${name}.stage${stage}.json`);
	const label = current.dataDiffs + ' diffs';
	if (opt.update) {
		fs.writeFileSync(file, JSON.stringify(current, null, 1) + '\n');
		return record(stage, name, 'PASS', label + ' (baseline written)');
	}
	if (!fs.existsSync(file)) return record(stage, name, 'NEW', label + '; no baseline yet, run with --update-baselines');
	const base = JSON.parse(fs.readFileSync(file, 'utf8'));
	const worse = Object.entries(current.fieldCounts).filter(([f, n]) => n > (base.fieldCounts[f] || 0)).map(([f, n]) => `${f} ${base.fieldCounts[f] || 0}->${n}`);
	if (current.diagnosticDiffs > base.diagnosticDiffs) worse.push(`diagnostics ${base.diagnosticDiffs}->${current.diagnosticDiffs}`);
	if (worse.length) return record(stage, name, 'FAIL', `${label} (baseline ${base.dataDiffs}); regressed: ${worse.slice(0, 6).join(', ')}`);
	if (current.dataDiffs < base.dataDiffs) return record(stage, name, 'IMPROVED', `${label} (baseline ${base.dataDiffs}); run --update-baselines to lock in`);
	record(stage, name, 'PASS', label + ' (= baseline)');
}

function baselineOf(rep) { return { dataDiffs: rep.dataDiffs, diagnosticDiffs: rep.diagnosticDiffs, fieldCounts: rep.fieldCounts }; }

const stable = (v) => (v === null || typeof v !== 'object' ? JSON.stringify(v ?? null)
	: Array.isArray(v) ? '[' + v.map(stable).join(',') + ']'
		: '{' + Object.keys(v).sort().map((k) => JSON.stringify(k) + ':' + stable(v[k])).join(',') + '}');

// --- corpus selection ---------------------------------------------------------
const mods = SUITE.mods
	.filter((m) => !opt.only || m.name.includes(opt.only))
	.map((m) => ({ ...m, abs: path.join(ROOT, m.path) }));
const saveCache = new Map(); // base .dm -> Dom5Parser save of it

function dom5Save(abs, tag) {
	if (saveCache.has(abs)) return saveCache.get(abs);
	const out = path.join(WORK, tag + '.save.dm');
	const r = dom5tests(['roundtrip', abs, out]);
	if (r.code !== 0 || !fs.existsSync(out)) throw new Error('Dom5Tests roundtrip failed: ' + r.out.trim().split('\n').slice(-2).join(' | '));
	saveCache.set(abs, out);
	return out;
}

function run(stage, name, fn) {
	try { fn(); } catch (e) { record(stage, name, 'FAIL', 'error: ' + e.message); }
}

// --- stages -------------------------------------------------------------------
console.log(`oracle: ${ORACLE}\nwork:   ${WORK}\n`);

if (opt.stages.includes(1)) {
	console.log('Stage 1: inspector loads mod -> exports -> reloads (functional, after post-processing)');
	for (const m of mods) run(1, m.name, () => {
		const out = path.join(WORK, m.name + '.inspector.dm');
		const r = oracle('export-mod.js', [m.abs, out]);
		if (r.code !== 0) throw new Error('export-mod failed: ' + r.out.trim().split('\n').slice(-2).join(' | '));
		const rep = compare(m.abs, out, 'final', m.name + '.stage1');
		if (m.expect === 'baseline') judgeBaseline(1, m.name, baselineOf(rep));
		else judgePass(1, m.name, rep, m.knownFailing?.['1']);
	});
}

if (opt.stages.includes(2) && !opt.only) {
	console.log('\nStage 2: vanilla data');
	run(2, 'exporter-roundtrip', () => {
		const r = oracle('verify-export.js', []);
		const failing = [...r.out.matchAll(/^FAIL (\w+)/gm)].map((x) => x[1]);
		const known = SUITE.vanillaKnownFailing || {};
		const unexpected = failing.filter((t) => !known[t]);
		if (unexpected.length) record(2, 'exporter-roundtrip', 'FAIL', 'failing types: ' + unexpected.join(', '));
		else record(2, 'exporter-roundtrip', failing.length ? 'XFAIL' : 'PASS', failing.length ? 'known: ' + failing.map((t) => `${t} (${known[t]})`).join('; ') : '');
	});
	run(2, 'vanilla.dm-current', () => {
		const out = path.join(WORK, 'vanilla.generated.dm');
		const r = oracle('export-vanilla.js', [out]);
		if (r.code !== 0) throw new Error('export-vanilla failed');
		const norm = (p) => fs.readFileSync(p, 'utf8').replace(/\r\n/g, '\n');
		const same = norm(out) === norm(path.join(ROOT, 'vanilla.dm'));
		record(2, 'vanilla.dm-current', same ? 'PASS' : 'FAIL', same ? '' : 'committed vanilla.dm differs from what the oracle generates; regenerate with scripts/headless/export-vanilla.js');
	});
	run(2, 'vanilla-base-values', () => {
		// Apply vanilla.dm on top of the raw game data; single-valued fields whose value
		// changes are places where vanilla.dm does not carry the game's base value.
		const empty = path.join(WORK, 'empty.dm');
		fs.writeFileSync(empty, '#modname "empty"\n');
		const rep = compare(empty, path.join(ROOT, 'vanilla.dm'), 'parse', 'vanilla-base');
		const norm = (f, v) => {
			let s = String(v).replace(/%$/, '');
			if (f === 'type') s = { 4: 'shield', 5: 'armor', 6: 'helm', 8: 'misc' }[s] || s; // armor type spelling
			return s;
		};
		// Differences listed in suite.json "vanillaExpected" are representation or limits of the
		// mod command language (documented there); anything else is a real discrepancy.
		const expected = SUITE.vanillaExpected || {};
		const fieldCounts = {}, expectedCounts = {};
		let dataDiffs = 0;
		for (const e of rep.entities) {
			let differs = false;
			for (const [f, [a, b]] of Object.entries(e.fields)) {
				if (f === '(entity)' || a == null || typeof a === 'object' || typeof b === 'object' || b == null) continue;
				if (norm(f, a) === norm(f, b)) continue;
				const k = e.type + '.' + f;
				if (expected[k]) { expectedCounts[k] = (expectedCounts[k] || 0) + 1; continue; }
				fieldCounts[k] = (fieldCounts[k] || 0) + 1;
				differs = true;
			}
			if (differs) dataDiffs++;
		}
		const exp = Object.entries(expectedCounts).map(([k, n]) => k + ' ' + n).join(', ');
		if (exp) console.log('      expected (suite.json vanillaExpected): ' + exp);
		if (!dataDiffs) record(2, 'vanilla-base-values', 'PASS', 'only expected differences');
		else judgeBaseline(2, 'vanilla-base-values', { dataDiffs, diagnosticDiffs: 0, fieldCounts });
	});
}

if (opt.stages.includes(3)) {
	console.log('\nStage 3: Dom5Parser load -> save -> inspector sees the same data (strict, after parsing)');
	for (const m of mods) run(3, m.name, () => {
		const rep = compare(m.abs, dom5Save(m.abs, m.name), 'parse', m.name + '.stage3');
		if (m.expect === 'baseline') judgeBaseline(3, m.name, baselineOf(rep));
		else judgePass(3, m.name, rep, m.knownFailing?.['3']);
	});
}

if (opt.stages.includes(4)) {
	console.log('\nStage 4: Dom5Parser load -> edit -> save -> differs from an unedited save by exactly the edits');
	const dir = path.join(ROOT, SUITE.edits);
	for (const file of fs.readdirSync(dir).filter((f) => f.endsWith('.json')).sort()) {
		const name = path.basename(file, '.json');
		if (opt.only && !name.includes(opt.only)) continue;
		const spec = JSON.parse(fs.readFileSync(path.join(dir, file), 'utf8'));
		const base = path.resolve(dir, spec.base);
		run(4, name, () => {
			const save = dom5Save(base, 'edit-base-' + path.basename(base, '.dm'));
			const out = path.join(WORK, name + '.edit.dm');
			const r = dom5tests(['edit', base, path.join(dir, file), out]);
			if (r.code !== 0) throw new Error('Dom5Tests edit failed: ' + r.out.trim().split('\n').slice(-2).join(' | '));
			const rep = compare(save, out, 'parse', name + '.stage4');
			const actual = [];
			for (const e of rep.entities) for (const [f, [a, b]] of Object.entries(e.fields)) actual.push({ type: e.type, id: String(e.id), field: f, from: a, to: b });
			const same = (x, y) => x.type === y.type && String(x.id) === y.id && x.field === y.field
				&& (x.from === undefined || stable(x.from) === stable(y.from)) && (x.to === undefined || stable(x.to) === stable(y.to));
			const missing = spec.expect.filter((x) => !actual.some((y) => same(x, y)));
			const unexpected = actual.filter((y) => !spec.expect.some((x) => same(x, y)));
			const ok = !missing.length && !unexpected.length && rep.diagnosticDiffs === 0;
			const fmt = (d) => `${d.type} #${d.id} ${d.field}: ${stable(d.from)} -> ${stable(d.to)}`;
			const detail = [missing.length ? 'missing: ' + missing.slice(0, 3).map(fmt).join('; ') : '',
				unexpected.length ? 'unexpected: ' + unexpected.slice(0, 3).map(fmt).join('; ') + (unexpected.length > 3 ? ` (+${unexpected.length - 3})` : '') : '',
				rep.diagnosticDiffs ? rep.diagnosticDiffs + ' diagnostic diffs' : ''].filter(Boolean).join(' | ');
			if (spec.knownFailing) record(4, name, ok ? 'XPASS' : 'XFAIL', ok ? 'now passes; remove knownFailing' : spec.knownFailing + (detail ? ' [' + detail + ']' : ''));
			else record(4, name, ok ? 'PASS' : 'FAIL', detail);
		});
	}
}

// --- summary ------------------------------------------------------------------
const count = (s) => results.filter((r) => r.status === s).length;
const failed = results.filter((r) => r.status === 'FAIL' || r.status === 'NEW');
console.log(`\n${results.length} checks: ${count('PASS') + count('IMPROVED')} pass, ${count('XFAIL')} known-failing, ${failed.length} failing` + (count('XPASS') ? `, ${count('XPASS')} unexpectedly passing` : ''));
if (opt.json) fs.writeFileSync(opt.json, JSON.stringify({ oracle: ORACLE, results }, null, 1));
process.exit(failed.length ? 1 : 0);
