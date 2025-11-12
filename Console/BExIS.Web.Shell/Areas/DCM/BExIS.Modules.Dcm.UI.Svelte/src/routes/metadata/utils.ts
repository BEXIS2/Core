import type { SimpleComponentData, validationStoretype } from './models';
import { metadataStore, hideStore, validationStore } from './stores';

export function getValueByPath(path: string) {
	let obj: any;
	metadataStore.subscribe((v) => {
		obj = v;
	});
	return path.split('.').reduce((acc, part) => acc && acc[part], obj);
}

export function setValueByPath(obj: any, path: string, value: any) {
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

export function updateMetadataStore(path: string, value: any) {
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

export function toggleShow(path: string) {
		let hideStoreValue: string[] = [];
		hideStore.subscribe((v) => {
			hideStoreValue = [...v];
		})();

		if (hideStoreValue.includes(path)) {
			let idx = hideStoreValue.findIndex((x) => x == path);
			if (idx > -1) hideStoreValue.splice(idx, 1);
		} else {
			hideStoreValue.push(path);
		}
		hideStore.set(hideStoreValue);
	}

export function getValidationStore(): validationStoretype {
	let validationStoreValues: validationStoretype = { allSimpleTypesValid: true, simpleTypeValidationItems: [] };
			validationStore.subscribe(n => {
				validationStoreValues = n;
			});
		if(validationStoreValues == undefined) {
			validationStoreValues = { allSimpleTypesValid: true, simpleTypeValidationItems: [] };
			validationStore.set(validationStoreValues);
		}
	return validationStoreValues;
	}

export function ValidationStoreAddSimpleComponent(item: SimpleComponentData): validationStoretype {
	let validationStoreValues: validationStoretype = getValidationStore();
		if( !validationStoreValues.simpleTypeValidationItems.includes(item) && item.required) {
			validationStoreValues.simpleTypeValidationItems.push(item);
			validationStore.set(validationStoreValues);
		}
	return validationStoreValues;
	}

export function ValidationStoreSetAllValid(isValid: boolean): boolean {
	let validationStoreValues: validationStoretype = getValidationStore();
		validationStoreValues.allSimpleTypesValid = isValid;
		validationStore.set(validationStoreValues);
	return validationStoreValues.allSimpleTypesValid;
	}


