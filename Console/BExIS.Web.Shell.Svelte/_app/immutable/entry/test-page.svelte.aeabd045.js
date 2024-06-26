import {
	J as ue,
	S as Y,
	i as Z,
	s as x,
	K as j,
	k as P,
	a as A,
	l as p,
	m as V,
	c as N,
	h as E,
	L as H,
	n as k,
	M as R,
	b as L,
	G as D,
	N as y,
	O as de,
	g as O,
	v as fe,
	d as U,
	f as he,
	P as ge,
	Q as M,
	R as be,
	T as _e,
	U as Q,
	C as me,
	D as ke,
	E as ve,
	F as we,
	V as I,
	w as Ee,
	W as Se,
	q,
	y as Te,
	r as J,
	z as ye,
	A as ze,
	X as De,
	u as Pe,
	B as pe
} from '../chunks/index.615b5812.js';
import { w as Be } from '../chunks/index.84e7447a.js';
const K = {};
function W(e) {
	return e === 'local' ? localStorage : sessionStorage;
}
function F(e, l, s) {
	const t = (s == null ? void 0 : s.serializer) ?? JSON,
		i = (s == null ? void 0 : s.storage) ?? 'local',
		n = typeof window < 'u' && typeof document < 'u';
	function o(h, w) {
		n && W(i).setItem(h, t.stringify(w));
	}
	if (!K[e]) {
		const h = Be(l, (_) => {
				const m = n ? W(i).getItem(e) : null;
				if ((m && _(t.parse(m)), n)) {
					const v = (d) => {
						d.key === e && _(d.newValue ? t.parse(d.newValue) : null);
					};
					return (
						window.addEventListener('storage', v), () => window.removeEventListener('storage', v)
					);
				}
			}),
			{ subscribe: w, set: b } = h;
		K[e] = {
			set(_) {
				o(e, _), b(_);
			},
			update(_) {
				const m = _(ue(h));
				o(e, m), b(m);
			},
			subscribe: w
		};
	}
	return K[e];
}
F('modeOsPrefers', !1);
F('modeUserPrefers', void 0);
F('modeCurrent', !1);
function X(e) {
	let l, s;
	const t = e[21].default,
		i = me(t, e, e[20], null);
	return {
		c() {
			(l = P('div')), i && i.c(), this.h();
		},
		l(n) {
			l = p(n, 'DIV', { class: !0 });
			var o = V(l);
			i && i.l(o), o.forEach(E), this.h();
		},
		h() {
			k(l, 'class', 'slide-toggle-text ml-3');
		},
		m(n, o) {
			L(n, l, o), i && i.m(l, null), (s = !0);
		},
		p(n, o) {
			i &&
				i.p &&
				(!s || o[0] & 1048576) &&
				ke(i, t, n, n[20], s ? we(t, n[20], o, null) : ve(n[20]), null);
		},
		i(n) {
			s || (O(i, n), (s = !0));
		},
		o(n) {
			U(i, n), (s = !1);
		},
		d(n) {
			n && E(l), i && i.d(n);
		}
	};
}
function Ie(e) {
	let l,
		s,
		t,
		i,
		n,
		o,
		h,
		w,
		b,
		_,
		m,
		v,
		d,
		z,
		r,
		g = [
			{ type: 'checkbox' },
			{ class: 'slide-toggle-input hidden' },
			{ name: e[1] },
			e[8](),
			{ disabled: (i = e[9].disabled) }
		],
		S = {};
	for (let c = 0; c < g.length; c += 1) S = j(S, g[c]);
	let u = e[10].default && X(e);
	return {
		c() {
			(l = P('div')),
				(s = P('label')),
				(t = P('input')),
				(n = A()),
				(o = P('div')),
				(h = P('div')),
				(_ = A()),
				u && u.c(),
				this.h();
		},
		l(c) {
			l = p(c, 'DIV', {
				id: !0,
				class: !0,
				'data-testid': !0,
				role: !0,
				'aria-label': !0,
				'aria-checked': !0,
				tabindex: !0
			});
			var f = V(l);
			s = p(f, 'LABEL', { class: !0 });
			var T = V(s);
			(t = p(T, 'INPUT', { type: !0, class: !0, name: !0 })),
				(n = N(T)),
				(o = p(T, 'DIV', { class: !0 }));
			var B = V(o);
			(h = p(B, 'DIV', { class: !0 })),
				V(h).forEach(E),
				B.forEach(E),
				(_ = N(T)),
				u && u.l(T),
				T.forEach(E),
				f.forEach(E),
				this.h();
		},
		h() {
			H(t, S),
				k(h, 'class', (w = 'slide-toggle-thumb ' + e[3])),
				R(h, 'cursor-not-allowed', e[9].disabled),
				k(o, 'class', (b = 'slide-toggle-track ' + e[4])),
				R(o, 'cursor-not-allowed', e[9].disabled),
				k(s, 'class', (m = 'slide-toggle-label ' + e[5])),
				k(l, 'id', e[2]),
				k(l, 'class', (v = 'slide-toggle ' + e[6])),
				k(l, 'data-testid', 'slide-toggle'),
				k(l, 'role', 'switch'),
				k(l, 'aria-label', e[2]),
				k(l, 'aria-checked', e[0]),
				k(l, 'tabindex', '0');
		},
		m(c, f) {
			L(c, l, f),
				D(l, s),
				D(s, t),
				t.autofocus && t.focus(),
				(t.checked = e[0]),
				D(s, n),
				D(s, o),
				D(o, h),
				D(s, _),
				u && u.m(s, null),
				(d = !0),
				z ||
					((r = [
						y(t, 'change', e[30]),
						y(t, 'click', e[22]),
						y(t, 'keydown', e[23]),
						y(t, 'keyup', e[24]),
						y(t, 'keypress', e[25]),
						y(t, 'mouseover', e[26]),
						y(t, 'change', e[27]),
						y(t, 'focus', e[28]),
						y(t, 'blur', e[29]),
						y(l, 'keydown', e[7])
					]),
					(z = !0));
		},
		p(c, f) {
			H(
				t,
				(S = de(g, [
					{ type: 'checkbox' },
					{ class: 'slide-toggle-input hidden' },
					(!d || f[0] & 2) && { name: c[1] },
					c[8](),
					(!d || (f[0] & 512 && i !== (i = c[9].disabled))) && { disabled: i }
				]))
			),
				f[0] & 1 && (t.checked = c[0]),
				(!d || (f[0] & 8 && w !== (w = 'slide-toggle-thumb ' + c[3]))) && k(h, 'class', w),
				(!d || f[0] & 520) && R(h, 'cursor-not-allowed', c[9].disabled),
				(!d || (f[0] & 16 && b !== (b = 'slide-toggle-track ' + c[4]))) && k(o, 'class', b),
				(!d || f[0] & 528) && R(o, 'cursor-not-allowed', c[9].disabled),
				c[10].default
					? u
						? (u.p(c, f), f[0] & 1024 && O(u, 1))
						: ((u = X(c)), u.c(), O(u, 1), u.m(s, null))
					: u &&
						(fe(),
						U(u, 1, 1, () => {
							u = null;
						}),
						he()),
				(!d || (f[0] & 32 && m !== (m = 'slide-toggle-label ' + c[5]))) && k(s, 'class', m),
				(!d || f[0] & 4) && k(l, 'id', c[2]),
				(!d || (f[0] & 64 && v !== (v = 'slide-toggle ' + c[6]))) && k(l, 'class', v),
				(!d || f[0] & 4) && k(l, 'aria-label', c[2]),
				(!d || f[0] & 1) && k(l, 'aria-checked', c[0]);
		},
		i(c) {
			d || (O(u), (d = !0));
		},
		o(c) {
			U(u), (d = !1);
		},
		d(c) {
			c && E(l), u && u.d(), (z = !1), ge(r);
		}
	};
}
const Le = 'inline-block',
	Ve = 'unstyled flex items-center',
	Ce = 'flex transition-all duration-[200ms] cursor-pointer',
	Ae = 'w-[50%] h-full scale-[0.8] transition-all duration-[200ms] shadow';
function Ne(e, l, s) {
	let t, i, n, o, h, w, b, _;
	const m = ['name', 'checked', 'size', 'active', 'border', 'rounded', 'label'];
	let v = M(l, m),
		{ $$slots: d = {}, $$scope: z } = l;
	const r = be(d),
		g = _e();
	let { name: S } = l,
		{ checked: u = !1 } = l,
		{ size: c = 'md' } = l,
		{ active: f = 'bg-surface-900 dark:bg-surface-300' } = l,
		{ border: T = '' } = l,
		{ rounded: B = 'rounded-full' } = l,
		{ label: G = '' } = l,
		C;
	switch (c) {
		case 'sm':
			C = 'w-12 h-6';
			break;
		case 'lg':
			C = 'w-20 h-10';
			break;
		default:
			C = 'w-16 h-8';
	}
	function $(a) {
		['Enter', 'Space'].includes(a.code) &&
			(a.preventDefault(), g('keyup', a), a.target.firstChild.click());
	}
	function ee() {
		return delete v.class, v;
	}
	function le(a) {
		I.call(this, e, a);
	}
	function se(a) {
		I.call(this, e, a);
	}
	function te(a) {
		I.call(this, e, a);
	}
	function ae(a) {
		I.call(this, e, a);
	}
	function ie(a) {
		I.call(this, e, a);
	}
	function re(a) {
		I.call(this, e, a);
	}
	function ne(a) {
		I.call(this, e, a);
	}
	function ce(a) {
		I.call(this, e, a);
	}
	function oe() {
		(u = this.checked), s(0, u);
	}
	return (
		(e.$$set = (a) => {
			s(9, (l = j(j({}, l), Q(a)))),
				s(32, (v = M(l, m))),
				'name' in a && s(1, (S = a.name)),
				'checked' in a && s(0, (u = a.checked)),
				'size' in a && s(11, (c = a.size)),
				'active' in a && s(12, (f = a.active)),
				'border' in a && s(13, (T = a.border)),
				'rounded' in a && s(14, (B = a.rounded)),
				'label' in a && s(2, (G = a.label)),
				'$$scope' in a && s(20, (z = a.$$scope));
		}),
		(e.$$.update = () => {
			e.$$.dirty[0] & 4097 &&
				s(18, (t = u ? f : 'bg-surface-400 dark:bg-surface-700 cursor-pointer')),
				e.$$.dirty[0] & 1 && s(17, (i = u ? 'bg-white/75' : 'bg-white')),
				e.$$.dirty[0] & 1 && s(16, (n = u ? 'translate-x-full' : '')),
				s(
					19,
					(o =
						l.disabled === !0
							? 'opacity-50'
							: 'hover:brightness-[105%] dark:hover:brightness-110 cursor-pointer')
				),
				s(6, (h = `${Le} ${B} ${o} ${l.class ?? ''}`)),
				e.$$.dirty[0] & 319488 && s(4, (b = `${Ce} ${T} ${B} ${C} ${t}`)),
				e.$$.dirty[0] & 212992 && s(3, (_ = `${Ae} ${B} ${i} ${n}`));
		}),
		s(5, (w = `${Ve}`)),
		(l = Q(l)),
		[
			u,
			S,
			G,
			_,
			b,
			w,
			h,
			$,
			ee,
			l,
			r,
			c,
			f,
			T,
			B,
			C,
			n,
			i,
			t,
			o,
			z,
			d,
			le,
			se,
			te,
			ae,
			ie,
			re,
			ne,
			ce,
			oe
		]
	);
}
class Oe extends Y {
	constructor(l) {
		super(),
			Z(
				this,
				l,
				Ne,
				Ie,
				x,
				{ name: 1, checked: 0, size: 11, active: 12, border: 13, rounded: 14, label: 2 },
				null,
				[-1, -1]
			);
	}
}
function Re(e) {
	let l, s, t, i, n, o, h, w, b, _, m, v;
	function d(r) {
		e[1](r);
	}
	let z = { name: 'david' };
	return (
		e[0] !== void 0 && (z.checked = e[0]),
		(i = new Oe({ props: z })),
		Ee.push(() => Se(i, 'checked', d)),
		{
			c() {
				(l = P('h1')),
					(s = q('Test with layout')),
					(t = A()),
					Te(i.$$.fragment),
					(o = A()),
					(h = P('br')),
					(w = A()),
					(b = P('b')),
					(_ = q('toggle : ')),
					(m = q(e[0]));
			},
			l(r) {
				l = p(r, 'H1', {});
				var g = V(l);
				(s = J(g, 'Test with layout')),
					g.forEach(E),
					(t = N(r)),
					ye(i.$$.fragment, r),
					(o = N(r)),
					(h = p(r, 'BR', {})),
					(w = N(r)),
					(b = p(r, 'B', {}));
				var S = V(b);
				(_ = J(S, 'toggle : ')), (m = J(S, e[0])), S.forEach(E);
			},
			m(r, g) {
				L(r, l, g),
					D(l, s),
					L(r, t, g),
					ze(i, r, g),
					L(r, o, g),
					L(r, h, g),
					L(r, w, g),
					L(r, b, g),
					D(b, _),
					D(b, m),
					(v = !0);
			},
			p(r, [g]) {
				const S = {};
				!n && g & 1 && ((n = !0), (S.checked = r[0]), De(() => (n = !1))),
					i.$set(S),
					(!v || g & 1) && Pe(m, r[0]);
			},
			i(r) {
				v || (O(i.$$.fragment, r), (v = !0));
			},
			o(r) {
				U(i.$$.fragment, r), (v = !1);
			},
			d(r) {
				r && E(l), r && E(t), pe(i, r), r && E(o), r && E(h), r && E(w), r && E(b);
			}
		}
	);
}
function Ue(e, l, s) {
	let t;
	function i(n) {
		(t = n), s(0, t);
	}
	return s(0, (t = !1)), [t, i];
}
class Ke extends Y {
	constructor(l) {
		super(), Z(this, l, Ue, Re, x, {});
	}
}
export { Ke as default };
