import type { SimpleComponentData, validationStoretype } from './models';
import { metadataStore, hideStore, validationStore } from './stores';
// Utility functions for metadata handling
// Get and set values in the metadata store based on a dot-separated path
// Get value from an object based on a dot-separated path
export function getValueByPath(path: string) {
	let obj: any;
	metadataStore.subscribe((v) => {
		obj = v;
	});
	return path.split('.').reduce((acc, part) => acc && acc[part], obj);
}
// Set value in an object based on a dot-separated path
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
// Update metadata store with a new value at the specified path
export function updateMetadataStore(path: string, value: any):any {
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
	return obj;
}
// Convert a schema node to a JSON object with default values
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
// Toggle visibility of a metadata component based on its path
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
// Validation Store Functions
// Get current values from the validation store
// If undefined, initialize with default values
// and return the validation store values
export function getValidationStore(): validationStoretype {
	let validationStoreValues: validationStoretype = { allSimpleRequiredValid: false, simpleTypeValidationItems: [] };
			validationStore.subscribe(n => {
				validationStoreValues = n;
			});
		if(validationStoreValues == undefined) {
			validationStoreValues = { allSimpleRequiredValid: false, simpleTypeValidationItems: [] };
			validationStore.set(validationStoreValues);
		}
	return validationStoreValues;
	}
// Add a simple component's validation data to the validation store
// if it doesn't already exist
// and has relevant validation criteria
// Returns the updated validation store values
export function ValidationStoreAddSimpleComponent(item: SimpleComponentData): validationStoretype {
	let validationStoreValues: validationStoretype = getValidationStore();
		if( !validationStoreValues.simpleTypeValidationItems.includes(item) && (item.required || item.regex !== undefined || item.lowerBound !== undefined || item.upperBound !== undefined || (item.domainList && item.domainList.length > 0)) ) {
			validationStoreValues.simpleTypeValidationItems.push(item);
			validationStore.set(validationStoreValues);
		}
	return validationStoreValues;
	}
// Set overall validity for all simple required components in the validation store
// based on the validity of an individual component identified by its path
// Returns the updated validity of the specified component
export function ValidationStoreSetSimpleTypeValid(path:string, isValid: boolean): boolean {
		let valid :boolean = false;
		let validationStoreValues: validationStoretype = getValidationStore();
		if(isValid && isValid != null && isValid != undefined){
			validationStoreValues.simpleTypeValidationItems.find(item => item.path === path)!.isValid = isValid;
			valid = validationStoreValues.simpleTypeValidationItems.find(item => item.path === path)!.isValid;
		}
		validationStoreValues.allSimpleRequiredValid = true;
		for (const item of validationStoreValues.simpleTypeValidationItems) {
			if (!item.isValid && item.required) {
				validationStoreValues.allSimpleRequiredValid = false;
				break;
			}
		}
		validationStore.set(validationStoreValues);
		return valid;
	}
// Create a SimpleComponentData validation item
// based on the provided parameters and simple component properties
export function createSimpleComponentValidationItem(label: string, path: string, required: boolean , isValid: boolean, simpleComponent: any): SimpleComponentData {
	let simpleComponentValidationItem: SimpleComponentData = {label: label,path: path, required: required , isValid: false};

	// set regex if defined
	if(simpleComponent.properties['#text'].pattern && simpleComponent.properties['#text'].pattern != undefined && simpleComponent.properties['#text'].pattern != null && simpleComponent.properties['#text'].pattern != '') {
		simpleComponentValidationItem.regex = simpleComponent.properties['#text'].pattern;				
	}
	// set minLength if defined
	if(simpleComponent.properties['#text'].minLength && simpleComponent.properties['#text'].minLength != undefined && simpleComponent.properties['#text'].minLength != null && simpleComponent.properties['#text'].minLength != '') {
		simpleComponentValidationItem.minLength = simpleComponent.properties['#text'].minLength;				
	}
	// set maxLength if defined
	if(simpleComponent.properties['#text'].maxLength && simpleComponent.properties['#text'].maxLength != undefined && simpleComponent.properties['#text'].maxLength != null && simpleComponent.properties['#text'].maxLength != '') {
		simpleComponentValidationItem.maxLength = simpleComponent.properties['#text'].maxLength;				
	}
	// set domainList if defined
	if(simpleComponent.properties['#text'].domainList && simpleComponent.properties['#text'].domainList != undefined && simpleComponent.properties['#text'].domainList != null && simpleComponent.properties['#text'].domainList.length > 0) {
		simpleComponentValidationItem.domainList = simpleComponent.properties['#text'].domainList;				
	}
	// set lowerBound if defined
	if(simpleComponent.properties['#text'].lowerBound && simpleComponent.properties['#text'].lowerBound != undefined && simpleComponent.properties['#text'].lowerBound != null && simpleComponent.properties['#text'].lowerBound.length != '') {
		simpleComponentValidationItem.lowerBound = simpleComponent.properties['#text'].lowerBound;				
	}
	// set upperBound if defined
	if(simpleComponent.properties['#text'].upperBound && simpleComponent.properties['#text'].upperBound != undefined && simpleComponent.properties['#text'].upperBound != null && simpleComponent.properties['#text'].upperBound.length != '') {
		simpleComponentValidationItem.upperBound = simpleComponent.properties['#text'].upperBound;				
	}

	return simpleComponentValidationItem;
}