// Dumps the dom6inspector's derived unit values for every vanilla unit, with the inputs it
// worked them out from, as JSON: what MUnit.prepareForRender adds to a unit when it's shown
// (defence with gear, protection with armor, encumbrance and combat speed, resource cost, ages,
// old age, magic path bonuses), the per-weapon attack, damage and length of its weapon table,
// map move (prepareData_PostSiteData) and gold cost (MUnit.goldCost).
//
//   node tools/derived/inspector_totals.js <dom6inspector checkout> <out.json>
//
// Dom5Tests derived-check reads the file: it computes the same values with Dom5Edit.Derived,
// once from these inputs (the formulas) and once from vanilla.dm (formulas and data).
// The inspector runs headless (its scripts/headless/boot.js); nothing in it is changed.
'use strict';
const fs = require('fs');
const path = require('path');

const [root, out] = process.argv.slice(2);
if (!root || !out) {
	console.error('usage: node tools/derived/inspector_totals.js <dom6inspector checkout> <out.json>');
	process.exit(2);
}
const { boot } = require(path.join(path.resolve(root), 'scripts', 'headless', 'boot.js'));

const int = (v) => { const n = parseInt(v); return isNaN(n) ? 0 : n; };
const num = (v) => { const n = parseFloat(v); return isNaN(n) ? null : n; };
const PATHS = ['F', 'A', 'W', 'E', 'S', 'D', 'N', 'G', 'B', 'H'];

// MUnit.js's isCmdr (private there)
function isCmdr(u) {
	if (u.sorttype) return u.sorttype.indexOf('cmdr') != -1 && !u.createdby;
	return true;
}

// Map move gets the commander bonus from isCmdr as it is when prepareData_PostSiteData reaches the
// unit; a unit later in that loop can still change an earlier one's type (linked shapes) or mark it
// as summoned. So record isCmdr when map move is written.
const cmdrAtMove = new WeakMap();
function watchMapMove(DMI) {
	const original = DMI.MUnit.prepareData_PostSiteData;
	DMI.MUnit.prepareData_PostSiteData = function () {
		const units = DMI.modctx.unitdata.filter(Boolean);
		for (const u of units) {
			let value = u.mapmove;
			Object.defineProperty(u, 'mapmove', {
				configurable: true, enumerable: true,
				get() { return value; },
				set(v) { value = v; cmdrAtMove.set(u, isCmdr(u)); },
			});
		}
		const result = original.apply(this, arguments);
		for (const u of units) {
			const v = u.mapmove;
			delete u.mapmove;
			u.mapmove = v;
		}
		return result;
	};
}

const started = Date.now();
const { DMI, modctx } = boot({ onLoaded: watchMapMove });
const MUnit = DMI.MUnit;

// start and max age as prepareForRender reads them: 0 is "not set", a start age of -1 is 0
const startAge = (v) => (v == null || v === '' || String(v) == '0' ? null : String(v) == '-1' ? 0 : int(v));
const maxAge = (v) => (v == null || v === '' || String(v) == '0' ? null : int(v));

function weaponIn(w) {
	return {
		id: int(w.id), name: w.name, melee: w.wpnclass == 'melee',
		att: int(w.wpnclass == 'melee' ? w.att : w.prec), def: int(w.def), len: int(w.len), dmg: num(w.dmg),
		rcost: int(w.rcost), range: int(w.range), bonus: !!w.bonus && w.bonus != '0', twohanded: !!w.twohanded && w.twohanded != '0',
		nostr: !!w.nostr, halfstr: !!w.halfstr, bowstr: !!w.bowstr,
	};
}

function armorIn(a) {
	return {
		id: int(a.id), name: a.name, type: a.type, def: int(a.def), enc: int(a.enc), parry: int(a.parry),
		protbody: int(a.protbody), prothead: int(a.prothead), general: int(a.general), prot: int(a.prot),
		rcost: int(a.rcost), movepen: int(a.movepen),
	};
}

const units = [];
for (const u of modctx.unitdata) {
	if (!u || !u.unprep) continue;
	const base = (modctx.unitlookup[Math.floor(u.id)] || u)._base || {};
	const mount = int(u.mountmnr) > 0 ? modctx.unitlookup[u.mountmnr] : null;
	const input = {
		hp: int(u.hp), str: int(u.str), att: int(u.att), def: int(u.def), prec: int(u.prec), enc: int(u.enc), ap: int(u.ap),
		prot: int(u.prot), size: int(u.size), ressize: int(u.ressize), rcost: int(base.rcost || 1), mountrcost: mount ? int(mount.rcostsort) : 0,
		mapmove: base.mapmove == null ? null : int(base.mapmove), startage: startAge(u.startage),
		maxage: maxAge(u.maxage), undead: !!(u.undead && u.undead != '0'), demon: !!(u.demon && u.demon != '0'),
		inanimate: !!(u.inanimate && u.inanimate != '0'), mounted: int(u.mountmnr) > 0, ambidextrous: int(u.ambidextrous),
		flying: !!(u.flying && u.flying != '0'), slave: !!(u.slave && u.slave != '0'), nomovepen: !!(u.nomovepen && u.nomovepen != '0'),
		leader: int(u.leader), magicleader: int(u.magicleader), undeadleader: int(u.undeadleader),
		command: int(u.command), magiccommand: int(u.magiccommand), undcommand: int(u.undcommand),
		fear: int(u.fear), fireshield: int(u.fireshield), heat: int(u.heat), cold: int(u.cold),
		fireres: int(u.fireres), coldres: int(u.coldres), shockres: int(u.shockres), poisonres: int(u.poisonres), supplybonus: int(u.supplybonus),
		goldcost: u.goldcost == null ? null : int(u.goldcost), mpath: u.mpath || '',
		paths: PATHS.map((k) => int(u[k])),
		randompaths: (u.randompaths || []).map((r) => ({ paths: [...String(r.paths)].map((c) => PATHS.indexOf(c)).filter((i) => i >= 0), levels: int(r.levels), chance: int(r.chance) })),
		// what MUnit.monsterGoldCost reads
		basecost: int(u.basecost), inspirational: int(u.inspirational), sailingshipsize: int(u.sailingshipsize),
		forgebonus: int(u.forgebonus), researchbonus: int(u.researchbonus), assassin: int(u.assassin), spy: int(u.spy),
		seduce: int(u.seduce), autohealer: int(u.autohealer), autodishealer: int(u.autodishealer), stealthy: int(u.stealthy),
		mountedflag: int(u.mounted) > 0, holy: int(u.holy) > 0, slowrec: int(u.rt) == 2, rpcost: int(u.rpcost),
		shapechange: int(u.shapechange), nofriders: int(u.nofriders), coridermnr: int(u.coridermnr), mountmnr: int(u.mountmnr),
		pretender: u.typechar == 'Pretender',
		weapons: (u.weapons || []).map(weaponIn), armor: (u.armor || []).map(armorIn),
	};
	let output = null, error = null;
	try {
		MUnit.prepareForRender(u);
		output = {
			hp: num(u.hp), str: num(u.str), att: num(u.att), def: num(u.def), prec: num(u.prec), enc: num(u.enc), ap: num(u.ap),
			prot: num(u.prot), casting_enc: u.casting_enc == null ? null : num(u.casting_enc),
			rcost: u.rcost == null ? null : num(u.rcost), startage: num(u.startage), maxage: num(u.maxage), isold: !!u.isold,
			mapmove: num(u.mapmove), goldcost: u.goldcost == null ? null : num(u.goldcost),
			leader: num(u.leader), magicleader: num(u.magicleader), undeadleader: num(u.undeadleader),
			fireres: num(u.fireres), coldres: num(u.coldres), shockres: num(u.shockres), poisonres: num(u.poisonres),
			supplybonus: num(u.supplybonus), fear: num(u.fear), fireshield: num(u.fireshield), heat: num(u.heat), cold: num(u.cold),
			watt: num(u.watt), titles: u.titles,
			weapons: (u.weapons || []).map((w, i) => ({
				id: int(w.id), len: String(MUnit.getWpnLen(u, i)), att: num(MUnit.getWpnAtt(u, i)), dmg: num(MUnit.getWpnDmg(u, i)),
			})),
		};
	} catch (e) {
		error = String(e);
	}
	units.push({
		id: u.id, name: u.name, type: u.type || null, cmdr: isCmdr(u), cmdrAtMove: cmdrAtMove.has(u) ? cmdrAtMove.get(u) : null,
		typechar: u.typechar || '', in: input, out: output, error,
	});
}

// items: the gem cost to forge (MItem.prepareData_PostMod), "10F5W"
const items = (modctx.itemdata || []).filter(Boolean).map((o) => ({ id: int(o.id), name: o.name, gemcost: String(o.gemcost || '') }));

fs.writeFileSync(out, JSON.stringify({ inspector: path.resolve(root), units, items }));
console.log(`${units.length} units (${units.filter((u) => u.error).length} render errors), ${items.length} items -> ${out} (${((Date.now() - started) / 1000).toFixed(1)} s)`);
