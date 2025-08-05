import { metadataStore } from './stores';

export function getValueByPath(path) {
	let obj: any;
	metadataStore.subscribe((v) => {
		obj = v;
	});
	return path.split('.').reduce((acc, part) => acc && acc[part], obj);
}

export function setValueByPath(obj, path, value) {
	const parts = path.split('.');
	let current = obj;
	for (let i = 0; i < parts.length - 1; i++) {
		if (!(parts[i] in current) || typeof current[parts[i]] !== 'object') {
			current[parts[i]] = {};
		}
		current = current[parts[i]];
	}
	current[parts[parts.length - 1]] = value;
	return obj;
}

export function updateMetadataStore(path, value) {
	let obj: any;
	metadataStore.subscribe((v) => {
		obj = v;
	});
	{
		if (value !== undefined && value !== null && value !== getValueByPath(path + '.#text')) {
			obj = setValueByPath(obj, path + '.#text', value);
			if (
				obj &&
				obj !== undefined &&
				obj !== null &&
				obj !==
					metadataStore.subscribe((value) => {
						obj = value;
					})
			) {
				metadataStore.set(obj);
			}
		}
	}
}

export function schemaToJson(schema: any): any {
	if (!schema) return null;

	if (schema.type === 'object' && schema.properties) {
		const obj: any = {};
		for (const [key, value] of Object.entries(schema.properties)) {
			obj[key] = schemaToJson(value);
		}
		return obj;
	}
	if (schema.type === 'array' && schema.items) {
		return [schemaToJson(schema.items)];
	}
	// Standardwerte für primitive Typen
	switch (schema.type) {
		case 'string':
			return '';
		case 'boolean':
			return false;
		case 'number':
			return 0;
		case 'int':
			return 0;
		case 'date':
			return new Date().toISOString().split('T')[0];
		default:
			return null;
	}
}
