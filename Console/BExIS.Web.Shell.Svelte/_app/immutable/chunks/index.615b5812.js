function $() {}
function H(t, n) {
	for (const e in n) t[e] = n[e];
	return t;
}
function P(t) {
	return t();
}
function O() {
	return Object.create(null);
}
function g(t) {
	t.forEach(P);
}
function T(t) {
	return typeof t == 'function';
}
function ut(t, n) {
	return t != t ? n == n : t !== n || (t && typeof t == 'object') || typeof t == 'function';
}
function I(t) {
	return Object.keys(t).length === 0;
}
function L(t, ...n) {
	if (t == null) return $;
	const e = t.subscribe(...n);
	return e.unsubscribe ? () => e.unsubscribe() : e;
}
function lt(t) {
	let n;
	return L(t, (e) => (n = e))(), n;
}
function ft(t, n, e) {
	t.$$.on_destroy.push(L(n, e));
}
function at(t, n, e, i) {
	if (t) {
		const r = B(t, n, e, i);
		return t[0](r);
	}
}
function B(t, n, e, i) {
	return t[1] && i ? H(e.ctx.slice(), t[1](i(n))) : e.ctx;
}
function _t(t, n, e, i) {
	if (t[2] && i) {
		const r = t[2](i(e));
		if (n.dirty === void 0) return r;
		if (typeof r == 'object') {
			const l = [],
				c = Math.max(n.dirty.length, r.length);
			for (let o = 0; o < c; o += 1) l[o] = n.dirty[o] | r[o];
			return l;
		}
		return n.dirty | r;
	}
	return n.dirty;
}
function dt(t, n, e, i, r, l) {
	if (r) {
		const c = B(n, e, i, l);
		t.p(c, r);
	}
}
function ht(t) {
	if (t.ctx.length > 32) {
		const n = [],
			e = t.ctx.length / 32;
		for (let i = 0; i < e; i++) n[i] = -1;
		return n;
	}
	return -1;
}
function mt(t) {
	const n = {};
	for (const e in t) e[0] !== '$' && (n[e] = t[e]);
	return n;
}
function pt(t, n) {
	const e = {};
	n = new Set(n);
	for (const i in t) !n.has(i) && i[0] !== '$' && (e[i] = t[i]);
	return e;
}
function yt(t) {
	const n = {};
	for (const e in t) n[e] = !0;
	return n;
}
let v = !1;
function W() {
	v = !0;
}
function G() {
	v = !1;
}
function J(t, n, e, i) {
	for (; t < n; ) {
		const r = t + ((n - t) >> 1);
		e(r) <= i ? (t = r + 1) : (n = r);
	}
	return t;
}
function K(t) {
	if (t.hydrate_init) return;
	t.hydrate_init = !0;
	let n = t.childNodes;
	if (t.nodeName === 'HEAD') {
		const s = [];
		for (let u = 0; u < n.length; u++) {
			const a = n[u];
			a.claim_order !== void 0 && s.push(a);
		}
		n = s;
	}
	const e = new Int32Array(n.length + 1),
		i = new Int32Array(n.length);
	e[0] = -1;
	let r = 0;
	for (let s = 0; s < n.length; s++) {
		const u = n[s].claim_order,
			a = (r > 0 && n[e[r]].claim_order <= u ? r + 1 : J(1, r, (x) => n[e[x]].claim_order, u)) - 1;
		i[s] = e[a] + 1;
		const f = a + 1;
		(e[f] = s), (r = Math.max(f, r));
	}
	const l = [],
		c = [];
	let o = n.length - 1;
	for (let s = e[r] + 1; s != 0; s = i[s - 1]) {
		for (l.push(n[s - 1]); o >= s; o--) c.push(n[o]);
		o--;
	}
	for (; o >= 0; o--) c.push(n[o]);
	l.reverse(), c.sort((s, u) => s.claim_order - u.claim_order);
	for (let s = 0, u = 0; s < c.length; s++) {
		for (; u < l.length && c[s].claim_order >= l[u].claim_order; ) u++;
		const a = u < l.length ? l[u] : null;
		t.insertBefore(c[s], a);
	}
}
function Q(t, n) {
	if (v) {
		for (
			K(t),
				(t.actual_end_child === void 0 ||
					(t.actual_end_child !== null && t.actual_end_child.parentNode !== t)) &&
					(t.actual_end_child = t.firstChild);
			t.actual_end_child !== null && t.actual_end_child.claim_order === void 0;

		)
			t.actual_end_child = t.actual_end_child.nextSibling;
		n !== t.actual_end_child
			? (n.claim_order !== void 0 || n.parentNode !== t) && t.insertBefore(n, t.actual_end_child)
			: (t.actual_end_child = n.nextSibling);
	} else (n.parentNode !== t || n.nextSibling !== null) && t.appendChild(n);
}
function gt(t, n, e) {
	v && !e ? Q(t, n) : (n.parentNode !== t || n.nextSibling != e) && t.insertBefore(n, e || null);
}
function R(t) {
	t.parentNode && t.parentNode.removeChild(t);
}
function U(t) {
	return document.createElement(t);
}
function A(t) {
	return document.createTextNode(t);
}
function xt() {
	return A(' ');
}
function bt() {
	return A('');
}
function $t(t, n, e, i) {
	return t.addEventListener(n, e, i), () => t.removeEventListener(n, e, i);
}
function V(t, n, e) {
	e == null ? t.removeAttribute(n) : t.getAttribute(n) !== e && t.setAttribute(n, e);
}
function vt(t, n) {
	const e = Object.getOwnPropertyDescriptors(t.__proto__);
	for (const i in n)
		n[i] == null
			? t.removeAttribute(i)
			: i === 'style'
				? (t.style.cssText = n[i])
				: i === '__value'
					? (t.value = t[i] = n[i])
					: e[i] && e[i].set
						? (t[i] = n[i])
						: V(t, i, n[i]);
}
function X(t) {
	return Array.from(t.childNodes);
}
function Y(t) {
	t.claim_info === void 0 && (t.claim_info = { last_index: 0, total_claimed: 0 });
}
function D(t, n, e, i, r = !1) {
	Y(t);
	const l = (() => {
		for (let c = t.claim_info.last_index; c < t.length; c++) {
			const o = t[c];
			if (n(o)) {
				const s = e(o);
				return s === void 0 ? t.splice(c, 1) : (t[c] = s), r || (t.claim_info.last_index = c), o;
			}
		}
		for (let c = t.claim_info.last_index - 1; c >= 0; c--) {
			const o = t[c];
			if (n(o)) {
				const s = e(o);
				return (
					s === void 0 ? t.splice(c, 1) : (t[c] = s),
					r ? s === void 0 && t.claim_info.last_index-- : (t.claim_info.last_index = c),
					o
				);
			}
		}
		return i();
	})();
	return (l.claim_order = t.claim_info.total_claimed), (t.claim_info.total_claimed += 1), l;
}
function Z(t, n, e, i) {
	return D(
		t,
		(r) => r.nodeName === n,
		(r) => {
			const l = [];
			for (let c = 0; c < r.attributes.length; c++) {
				const o = r.attributes[c];
				e[o.name] || l.push(o.name);
			}
			l.forEach((c) => r.removeAttribute(c));
		},
		() => i(n)
	);
}
function Et(t, n, e) {
	return Z(t, n, e, U);
}
function tt(t, n) {
	return D(
		t,
		(e) => e.nodeType === 3,
		(e) => {
			const i = '' + n;
			if (e.data.startsWith(i)) {
				if (e.data.length !== i.length) return e.splitText(i.length);
			} else e.data = i;
		},
		() => A(n),
		!0
	);
}
function wt(t) {
	return tt(t, ' ');
}
function kt(t, n) {
	(n = '' + n), t.wholeText !== n && (t.data = n);
}
function Nt(t, n, e, i) {
	e === null ? t.style.removeProperty(n) : t.style.setProperty(n, e, i ? 'important' : '');
}
function At(t, n, e) {
	t.classList[e ? 'add' : 'remove'](n);
}
function nt(t, n, { bubbles: e = !1, cancelable: i = !1 } = {}) {
	const r = document.createEvent('CustomEvent');
	return r.initCustomEvent(t, e, i, n), r;
}
function St(t, n) {
	return new t(n);
}
let y;
function p(t) {
	y = t;
}
function S() {
	if (!y) throw new Error('Function called outside component initialization');
	return y;
}
function jt(t) {
	S().$$.on_mount.push(t);
}
function Ct(t) {
	S().$$.after_update.push(t);
}
function Ot() {
	const t = S();
	return (n, e, { cancelable: i = !1 } = {}) => {
		const r = t.$$.callbacks[n];
		if (r) {
			const l = nt(n, e, { cancelable: i });
			return (
				r.slice().forEach((c) => {
					c.call(t, l);
				}),
				!l.defaultPrevented
			);
		}
		return !0;
	};
}
function Mt(t, n) {
	const e = t.$$.callbacks[n.type];
	e && e.slice().forEach((i) => i.call(this, n));
}
const h = [],
	M = [];
let m = [];
const w = [],
	q = Promise.resolve();
let k = !1;
function z() {
	k || ((k = !0), q.then(F));
}
function Pt() {
	return z(), q;
}
function N(t) {
	m.push(t);
}
function Tt(t) {
	w.push(t);
}
const E = new Set();
let d = 0;
function F() {
	if (d !== 0) return;
	const t = y;
	do {
		try {
			for (; d < h.length; ) {
				const n = h[d];
				d++, p(n), et(n.$$);
			}
		} catch (n) {
			throw ((h.length = 0), (d = 0), n);
		}
		for (p(null), h.length = 0, d = 0; M.length; ) M.pop()();
		for (let n = 0; n < m.length; n += 1) {
			const e = m[n];
			E.has(e) || (E.add(e), e());
		}
		m.length = 0;
	} while (h.length);
	for (; w.length; ) w.pop()();
	(k = !1), E.clear(), p(t);
}
function et(t) {
	if (t.fragment !== null) {
		t.update(), g(t.before_update);
		const n = t.dirty;
		(t.dirty = [-1]), t.fragment && t.fragment.p(t.ctx, n), t.after_update.forEach(N);
	}
}
function it(t) {
	const n = [],
		e = [];
	m.forEach((i) => (t.indexOf(i) === -1 ? n.push(i) : e.push(i))), e.forEach((i) => i()), (m = n);
}
const b = new Set();
let _;
function Lt() {
	_ = { r: 0, c: [], p: _ };
}
function Bt() {
	_.r || g(_.c), (_ = _.p);
}
function rt(t, n) {
	t && t.i && (b.delete(t), t.i(n));
}
function Dt(t, n, e, i) {
	if (t && t.o) {
		if (b.has(t)) return;
		b.add(t),
			_.c.push(() => {
				b.delete(t), i && (e && t.d(1), i());
			}),
			t.o(n);
	} else i && i();
}
function qt(t, n) {
	const e = {},
		i = {},
		r = { $$scope: 1 };
	let l = t.length;
	for (; l--; ) {
		const c = t[l],
			o = n[l];
		if (o) {
			for (const s in c) s in o || (i[s] = 1);
			for (const s in o) r[s] || ((e[s] = o[s]), (r[s] = 1));
			t[l] = o;
		} else for (const s in c) r[s] = 1;
	}
	for (const c in i) c in e || (e[c] = void 0);
	return e;
}
function zt(t, n, e) {
	const i = t.$$.props[n];
	i !== void 0 && ((t.$$.bound[i] = e), e(t.$$.ctx[i]));
}
function Ft(t) {
	t && t.c();
}
function Ht(t, n) {
	t && t.l(n);
}
function ct(t, n, e, i) {
	const { fragment: r, after_update: l } = t.$$;
	r && r.m(n, e),
		i ||
			N(() => {
				const c = t.$$.on_mount.map(P).filter(T);
				t.$$.on_destroy ? t.$$.on_destroy.push(...c) : g(c), (t.$$.on_mount = []);
			}),
		l.forEach(N);
}
function st(t, n) {
	const e = t.$$;
	e.fragment !== null &&
		(it(e.after_update),
		g(e.on_destroy),
		e.fragment && e.fragment.d(n),
		(e.on_destroy = e.fragment = null),
		(e.ctx = []));
}
function ot(t, n) {
	t.$$.dirty[0] === -1 && (h.push(t), z(), t.$$.dirty.fill(0)),
		(t.$$.dirty[(n / 31) | 0] |= 1 << n % 31);
}
function It(t, n, e, i, r, l, c, o = [-1]) {
	const s = y;
	p(t);
	const u = (t.$$ = {
		fragment: null,
		ctx: [],
		props: l,
		update: $,
		not_equal: r,
		bound: O(),
		on_mount: [],
		on_destroy: [],
		on_disconnect: [],
		before_update: [],
		after_update: [],
		context: new Map(n.context || (s ? s.$$.context : [])),
		callbacks: O(),
		dirty: o,
		skip_bound: !1,
		root: n.target || s.$$.root
	});
	c && c(u.root);
	let a = !1;
	if (
		((u.ctx = e
			? e(t, n.props || {}, (f, x, ...j) => {
					const C = j.length ? j[0] : x;
					return (
						u.ctx &&
							r(u.ctx[f], (u.ctx[f] = C)) &&
							(!u.skip_bound && u.bound[f] && u.bound[f](C), a && ot(t, f)),
						x
					);
				})
			: []),
		u.update(),
		(a = !0),
		g(u.before_update),
		(u.fragment = i ? i(u.ctx) : !1),
		n.target)
	) {
		if (n.hydrate) {
			W();
			const f = X(n.target);
			u.fragment && u.fragment.l(f), f.forEach(R);
		} else u.fragment && u.fragment.c();
		n.intro && rt(t.$$.fragment), ct(t, n.target, n.anchor, n.customElement), G(), F();
	}
	p(s);
}
class Wt {
	$destroy() {
		st(this, 1), (this.$destroy = $);
	}
	$on(n, e) {
		if (!T(e)) return $;
		const i = this.$$.callbacks[n] || (this.$$.callbacks[n] = []);
		return (
			i.push(e),
			() => {
				const r = i.indexOf(e);
				r !== -1 && i.splice(r, 1);
			}
		);
	}
	$set(n) {
		this.$$set && !I(n) && ((this.$$.skip_bound = !0), this.$$set(n), (this.$$.skip_bound = !1));
	}
}
export {
	ct as A,
	st as B,
	at as C,
	dt as D,
	ht as E,
	_t as F,
	Q as G,
	$ as H,
	ft as I,
	lt as J,
	H as K,
	vt as L,
	At as M,
	$t as N,
	qt as O,
	g as P,
	pt as Q,
	yt as R,
	Wt as S,
	Ot as T,
	mt as U,
	Mt as V,
	zt as W,
	Tt as X,
	xt as a,
	gt as b,
	wt as c,
	Dt as d,
	bt as e,
	Bt as f,
	rt as g,
	R as h,
	It as i,
	Ct as j,
	U as k,
	Et as l,
	X as m,
	V as n,
	jt as o,
	Nt as p,
	A as q,
	tt as r,
	ut as s,
	Pt as t,
	kt as u,
	Lt as v,
	M as w,
	St as x,
	Ft as y,
	Ht as z
};
