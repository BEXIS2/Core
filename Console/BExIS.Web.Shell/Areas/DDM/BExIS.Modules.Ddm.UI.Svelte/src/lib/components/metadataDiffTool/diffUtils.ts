export function isPrimitive(val: unknown): boolean {
	return (
		val === null ||
		typeof val === 'string' ||
		typeof val === 'number' ||
		typeof val === 'boolean' ||
		typeof val === 'undefined'
	);
}

export function isArray(val: unknown): val is any[] {
	return Array.isArray(val);
}

export function isObject(val: unknown): val is Record<string, any> {
	return !!val && typeof val === 'object' && !Array.isArray(val);
}

export function hasDiff(value1: unknown, value2: unknown): boolean {
	if (isPrimitive(value1) || isPrimitive(value2)) {
		return value1 !== value2;
	}

	if (isArray(value1) || isArray(value2)) {
		const arr1 = isArray(value1) ? value1 : [];
		const arr2 = isArray(value2) ? value2 : [];
		if (arr1.length !== arr2.length) return true;
		for (let i = 0; i < arr1.length; i++) {
			if (hasDiff(arr1[i], arr2[i])) return true;
		}
		return false;
	}

	const o1 = isObject(value1) ? value1 : {};
	const o2 = isObject(value2) ? value2 : {};
	const keys = new Set([...Object.keys(o1), ...Object.keys(o2)]);
	for (const k of keys) {
		if (hasDiff(o1[k], o2[k])) return true;
	}
	return false;
}
