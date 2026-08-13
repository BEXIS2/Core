import { convertDisplayName } from './metadataShared';
import type { ComplexComponentData, SimpleComponentData, validationStoretype } from './models';
import { metadataStore, schemaStore, systemMappingsStore, hideStore, validationStore, configStore, activeStore, descriptionStore } from './stores';
import { get } from 'svelte/store';
// Utility functions for metadata handling
// Get and set values in the metadata store based on a dot-separated path
export function setMetadataStore(metadata: any) {
	metadataStore.set(metadata);
}

export function setSchemaStore(schema: any) {
	schemaStore.set(schema);
}

export function getSchemaStore(): any {
	let schema: any;
	schemaStore.subscribe((v) => {
		schema = v;
	});
	return schema;
}

// returns a node, that can be complex	or simple, based on the given path in the metadata store
export function getNodeByPath(path: string) {
	let obj: any;
	metadataStore.subscribe((v) => {
		obj = v;
	});
	return path.split('.').reduce((acc, part) => acc && acc[part], obj);
}

export function getByPath(path: string) {
	console.log("🚀 ~ getByPath ~ path:", path)
	let obj: any;
	metadataStore.subscribe((v) => {
		obj = v;
	});
	return path.split('.').reduce((acc, part) => acc && acc[part], obj);
};

export function getValueByPath(path: string) {
	path = path + '.#text';
	return getNodeByPath(path);
}

export function getRefByPath(path: string) {
	path = path + '.@ref';
	return getNodeByPath(path);
}

export function getPartyIdByPath(path: string) {

	const obj = getNodeByPath(path);
	const partyId = obj ? obj['@partyid'] : null;
	return partyId;
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
export function updateMetadataStore(path: string, value: any, isMulti?: boolean, ref?: any, partyid?: number): any {
	console.log('Updating metadata store at path:', path, 'with value:', value, 'isMulti:', isMulti, 'ref:', ref, 'partyid:', partyid);
	let obj: any = {};
	if (path !== undefined && path !== null && path !== '') {
		metadataStore.subscribe((v) => {
			obj = v;
		});
		{
			if (value !== getValueByPath(path) || ref !== getRefByPath(path)) {

				if (isMulti) {
					obj = setValueByPath(obj, path, value);
				} else {
					// Keep party-id-only updates untouched for complex parent nodes.
					if ((value === undefined || value === null) && partyid !== undefined && partyid !== null) {
						const parent = getByPath(path);
						parent["@partyid"] = partyid;
					} else {
						obj = setValueByPath(obj, path + '.#text', value ?? '');
					}
					if (ref !== undefined && ref !== null) {
						obj = setValueByPath(obj, path + '.@ref', ref);
					}
					if (partyid !== undefined && partyid !== null) {
						obj = setValueByPath(obj, path + '.@partyid', partyid);
					}
				}
				if (obj !== undefined && obj !== null) {
					metadataStore.set(obj);
				}
			}
			else if ((value === undefined || value === null) && partyid !== undefined && partyid !== null) {
				const parent = getByPath(path);
				parent["@partyid"] = partyid;
				if (obj !== undefined && obj !== null) {
					metadataStore.set(obj);
				}
				//console.log("🚀 ~ updateMetadataStore ~ parent:", parent)
			}

		}
	}
	//console.log('Updated metadata store:', obj);
	return obj;
}

export function removeFromMetadataStore(path: string): any {

	let obj: any = {};
	if (path !== undefined && path !== null && path !== '') {
		metadataStore.subscribe((v) => {
			obj = v;
		});
		{
			removeByPath(obj, path);
			metadataStore.set(obj);
		}
	}
	console.log('remove metadata store:', obj);
	return obj;
}

function removeByPath(obj, path) {
	const parts = path.split('.');
	const lastKey = parts.pop(); // The property to delete

	// Reach the parent of the last key
	const parent = parts.reduce((current, part) => {
		return (current && current[part] !== undefined) ? current[part] : undefined;
	}, obj);

	if (parent && parent.hasOwnProperty(lastKey)) {
		delete parent[lastKey];
		return true; // Success
	}
	return false; // Path not found
}

// Config Store Functions
// Set configuration data in the config store
export function setConfigStore(config: any) {
	configStore.set(config.data);
}

// Get configuration data from the config store
export function getConfigStore(): any {
	let config: any;
	configStore.subscribe((v) => {
		config = v;
	});
	return config;
}

// SystemMappings Store Functions
// Set system mappings data in the systemMappings store
export function setSystemMappingsStore(systemMappings: any) {
	systemMappingsStore.set(systemMappings);
}

// Get system mappings data from the systemMappings store
export function getSystemMappingsStore(): any {
	let systemMappings: any;
	systemMappingsStore.subscribe((v) => {
		systemMappings = v;
	});
	return systemMappings;
}

// Get anchor point for a given component name from the config store
// export function getAnchorFromConfig(componentName: string): string {	
// 	if(componentName != null && componentName != undefined && componentName != ''){
// 		let config: any = getConfigStore();
// 		for (const component of config.components) {
// 			if (component.meta.component_name.toLowerCase() === componentName.toLowerCase() && component.globalSettings.anchorpoint === anchor) {
// 				return component.globalSettings.anchorpoint;
// 			}
// 		}
// 	}
// 	return '';
// }

export function getVariablesFromConfig(componentName: string, anchor: string): any[] {
	let variables: any[] = [];
	if (componentName != null && componentName != undefined && componentName != '') {
		let config: any = getConfigStore();
		let cleanAnchor = removeJsonPathIndices(anchor);
		for (const component of config.components) {
			let cleanAnchorPoint = removeJsonPathIndices(component.globalSettings.anchorpoint);
			if (component.meta.component_name.toLowerCase() === componentName.toLowerCase() && cleanAnchorPoint === cleanAnchor) {
				variables = component.mode.variables.variable;
			}
		}
	}
	return variables;
}

export function getFullConfig(componentName: string, anchor: string): any[] {
	let fullConfig: any[] = [];
	if (componentName != null && componentName != undefined && componentName != '') {
		let config: any = getConfigStore();
		// console.log(config, 'config');
		let cleanAnchor = removeJsonPathIndices(anchor);
		// console.log('Searching for component:', componentName, 'with anchor:', cleanAnchor);
		for (const component of config.components) {
			let cleanAnchorPoint = removeJsonPathIndices(component.globalSettings.anchorpoint);
			if (component.meta.component_name.toLowerCase() === componentName.toLowerCase() && cleanAnchorPoint === cleanAnchor) {
				fullConfig = component;
			}
		}
	}
	return fullConfig;
}

type TargetVar = { target_variable: string; value: string };

export function getTargetVariablesWithValues(config: any): TargetVar[] {
	const result: TargetVar[] = [];
	let component: any = config;

	const globals = component?.globalSettings?.globalsetting ?? [];
	const settings = component?.mode?.settings?.setting ?? [];
	const variables = component?.mode?.variables?.variable ?? [];

	for (const item of globals) {
		if (item?.target_variable) result.push({ target_variable: item.target_variable, value: item.value ?? '' });
	}

	for (const item of settings) {
		if (item?.target_variable) result.push({ target_variable: item.target_variable, value: item.value ?? '' });
	}

	for (const item of variables) {
		if (item?.target_variable) result.push({ target_variable: item.target_variable, value: item.value ?? item.JSONPath ?? '' });
	}


	return result;
}

export function getVariableSoursePathFromConfig(componentName: string, anchor: string, targetVariableName: string): string {
	if (componentName != null && componentName != undefined && componentName != '') {
		let variables = getVariablesFromConfig(componentName, anchor);
		console.log('Searching for target variable:', targetVariableName, 'in variables:', variables);
		for (const variable of variables) {
			if (variable.target_variable === targetVariableName) {
				console.log('Found variable:', variable.JSONPath);
				return variable.JSONPath;
			}
		}
		if (targetVariableName === 'value' && variables.length > 0) {
			console.log('Found variable (fallback):', variables[0].JSONPath);
			return variables[0].JSONPath;
		}
	}
	return '';
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

export function activateShow(path: string) {
	let hideStoreValue: string[] = [];
	hideStore.subscribe((v) => {
		hideStoreValue = [...v];
	})();

	if (hideStoreValue.includes(path)) {
		let idx = hideStoreValue.findIndex((x) => x == path);
		if (idx > -1) hideStoreValue.splice(idx, 1);
	}
	hideStore.set(hideStoreValue);
}

export function hasValue(node) {

	if (node == null) return false;

	if (typeof node === 'object') {
		return Object.entries(node).some(([key, value]) => {
			if (key === '@ref' || key === '@partyid' || key.charAt(0) === '@') return false;
			return hasValue(value);
		});
	}

	if (typeof node === 'string') {
		//console.log("🚀 ~ hasValue ~ node:", node, node.trim().length)
		return node.trim().length > 0;
	}

	//return Boolean(node);

	return false;
}

// p = path:string & r = required: boolean
export function isActive(p: string, r: boolean): boolean {
	// logic to determine if the component is active

	const node = getNodeByPath(p);
	const hasData = hasValue(node); // replace with actual check for data presence
	if (r) {
		return true; // if required, it's always active
	} else if (hasData) {
		return true; // if it has data, it's active
	} else {
		return false; // otherwise, it's not active
	}
}

export function setActive(path: string): void {
	let activeStoreValue: string[] = get(activeStore);
	if (!activeStoreValue.includes(path)) {
		activeStoreValue.push(path);
		activeStore.set(activeStoreValue);
	}
}

export function setInactive(path: string): void {
	let activeStoreValue: string[] = get(activeStore);
	if (activeStoreValue.includes(path)) {
		let idx = activeStoreValue.findIndex((x) => x == path);
		if (idx > -1) activeStoreValue.splice(idx, 1);
		activeStore.set(activeStoreValue);
	}
}

// element at this node should be cleaned
// #t should be ''
// arrays should have one empty element	to preserve structure
export function empty(node) {
	console.log('emptying node:', node);
	if (node === null || node === undefined) return node;

	if (Array.isArray(node)) {
		console.log('array node:', node);

		if (node.length > 0) {
			empty(node[0]); // clear the first element to preserve structure
		}

		// remove all	elements but only first one  stay to preserve structure
		return node.length = 1;
	}

	if (node.hasOwnProperty('#text')) {
		return node['#text'] = '';
	}

	if (typeof node === 'object') {

		Object.keys(node).forEach(key => {
			const value = node[key];
			return empty(value);
		});
	}

	if (node.hasOwnProperty('#text')) {
		return node['#text'] = '';
	}

	return node;

}

// Validation Store Functions
// Get current values from the validation store
// If undefined, initialize with default values
// and return the validation store values
export function getValidationStore(): validationStoretype {
	let validationStoreValues: validationStoretype = { allSimpleRequiredValid: false, allComplexTypesValid: false, simpleTypeValidationItems: [], complexTypeValidationItems: [] };
	validationStore.subscribe(n => {
		validationStoreValues = n;
	});
	if (validationStoreValues == undefined) {
		clearValidationStore();
	}
	return validationStoreValues;
}

export function clearValidationStore(): void {
	validationStore.set({ allSimpleRequiredValid: false, allComplexTypesValid: false, simpleTypeValidationItems: [], complexTypeValidationItems: [] });
}
// Add a simple component's validation data to the validation store
// if it doesn't already exist
// and has relevant validation criteria
// Returns the updated validation store values
export function ValidationStoreAddSimpleComponent(
	item: SimpleComponentData,
	forceRegistration: boolean = false
): validationStoretype {
	let validationStoreValues: validationStoretype = getValidationStore();
	if (validationStoreValues.simpleTypeValidationItems.find(i => i.path === item.path) === undefined && (forceRegistration || item.required || item.regex !== undefined || item.lowerBound !== undefined || item.upperBound !== undefined || (item.domainList && item.domainList.length > 0))) {
		validationStoreValues.simpleTypeValidationItems.push(item);
		validationStore.set(validationStoreValues);
	}
	return validationStoreValues;
}

// Add a complex component's validation data to the validation store
// if it doesn't already exist
// and has relevant validation criteria
// Returns the updated validation store values
export function ValidationStoreAddComplexComponent(
	item: ComplexComponentData,
	forceRegistration: boolean = false
): validationStoretype {
	let validationStoreValues: validationStoretype = getValidationStore();
	if (validationStoreValues.complexTypeValidationItems.find(i => i.path === item.path) === undefined && (forceRegistration || item.required || item.maxItems !== undefined || item.minItems !== undefined)) {
		validationStoreValues.complexTypeValidationItems.push(item);
		validationStore.set(validationStoreValues);
	}
	return validationStoreValues;
}

// Set overall validity for all simple required components in the validation store
// based on the validity of an individual component identified by its path
// Returns the updated validity of the specified component
export function ValidationStoreSetSimpleTypeValid(
	path: string,
	isValid: boolean,
	errorMessage: string = '',
	overwriteErrorMessage: boolean = true
): boolean {
	let valid: boolean = false;
	let validationStoreValues: validationStoretype = getValidationStore();

	if (isValid != null && isValid != undefined) {
		const item = validationStoreValues.simpleTypeValidationItems.find(item => {
			return item.path === path;
		});
		if (item) {
			item!.isValid = isValid;
			valid = item!.isValid;

			if (item && errorMessage) {
				if (overwriteErrorMessage || !item.errorMessage) {
					item.errorMessage = errorMessage;
				} else {
					item.errorMessage = `${item.errorMessage}\n${errorMessage}`;
				}
			}

			if (valid == true && item) {
				item.errorMessage = '';
			}
		}
		validationStoreValues.allSimpleRequiredValid = true;
		for (const item of validationStoreValues.simpleTypeValidationItems) {
			if (!item.isValid && item.required) {
				validationStoreValues.allSimpleRequiredValid = false;
				break;
			}
		}
	}
	validationStore.set({
		...validationStoreValues,
		simpleTypeValidationItems: [...validationStoreValues.simpleTypeValidationItems]
	});
	//console.log("🚀 ~ ValidationStoreSetSimpleTypeValid ~ validationStore:", get(validationStore))
	return valid;
}

// Set overall validity for all simple required components in the validation store
// based on the validity of an individual component identified by its path
// Returns the updated validity of the specified component
export function ValidationStoreSetComplexTypeValid(
	path: string,
	isValid: boolean,
	errorMessage: string = '',
	overwriteErrorMessage: boolean = true
): boolean {
	let valid: boolean = false;
	let validationStoreValues: validationStoretype = getValidationStore();

	if (isValid != null && isValid != undefined) {
		const item = validationStoreValues.complexTypeValidationItems.find(item => {
			return item.path === path;
		});
		if (item) {
			item!.isValid = isValid;
			valid = item!.isValid;

			if (item && errorMessage) {
				if (overwriteErrorMessage || !item.errorMessage) {
					item.errorMessage = errorMessage;
				} else {
					item.errorMessage = `${item.errorMessage}\n${errorMessage}`;
				}
			}

			if (valid == true && item) {
				item.errorMessage = '';
			}
		}
		validationStoreValues.allComplexTypesValid = true;
		for (const item of validationStoreValues.complexTypeValidationItems) {
			if (!item.isValid && item.required) {
				validationStoreValues.allComplexTypesValid = false;
				break;
			}
		}
	}
	validationStore.set({
		...validationStoreValues,
		complexTypeValidationItems: [...validationStoreValues.complexTypeValidationItems]
	});
	//console.log("🚀 ~ ValidationStoreSetSimpleTypeValid ~ validationStore:", get(validationStore))
	return valid;
}

export function setValidationErrorMessage(
	path: string,
	errorMessage: string,
	overwriteErrorMessage: boolean = true
): string {
	const validationStoreValues = getValidationStore();
	const item = validationStoreValues.simpleTypeValidationItems.find((entry) => entry.path === path);

	if (!item) {
		return errorMessage;
	}

	if (overwriteErrorMessage || !item.errorMessage) {
		item.errorMessage = errorMessage;
	} else {
		item.errorMessage = `${item.errorMessage}\n${errorMessage}`;
	}

	validationStore.set(validationStoreValues);
	return item.errorMessage;
}

export function setValidationLengthConstraints(
	path: string,
	minLength?: number,
	maxLength?: number
): void {
	const store = getValidationStore();
	const item = store.simpleTypeValidationItems.find((item) => item.path === path);

	if (!item) return;

	if (minLength != null) {
		item.minLength = Math.max(item.minLength ?? 0, minLength);
	}

	if (maxLength != null) {
		item.maxLength = Math.min(item.maxLength ?? Number.POSITIVE_INFINITY, maxLength);
	}

	validationStore.set({
		...store,
		simpleTypeValidationItems: [...store.simpleTypeValidationItems]
	});
}

export function validateCustomCondition(
	path: string,
	isValid: boolean,
	errorMessage: string,
	overwriteErrorMessage: boolean = true
): boolean {
	ValidationStoreSetSimpleTypeValid(path, isValid, errorMessage, overwriteErrorMessage);
	return isValid;
}
// Create a SimpleComponentData validation item
// based on the provided parameters and simple component properties
export function createSimpleComponentValidationItem(path: string, label: string, required: boolean, simpleComponent: any): SimpleComponentData {

	let simpleComponentValidationItem: SimpleComponentData = { label: label, path: path, required: required, isValid: false, errorMessage: '' };

	let item = simpleComponent.properties['#text'];

	 console.log('simpleComponentValidationItem',label,item, simpleComponent	);


	// set regex if defined
	if (item.pattern && item.pattern != undefined && item.pattern != null && item.pattern != '') {
		simpleComponentValidationItem.regex = item.pattern;
	}
	// set minLength if defined
	if (item.minLength && item.minLength != undefined && item.minLength != null && item.minLength != '') {
		simpleComponentValidationItem.minLength = item.minLength;
	}
	// set maxLength if defined
	if (item.maxLength && item.maxLength != undefined && item.maxLength != null && item.maxLength != '') {
		simpleComponentValidationItem.maxLength = item.maxLength;
	}
	// set domainList if defined
	if (item.enum && item.enum != undefined && item.enum != null && item.enum.length > 0) {
		simpleComponentValidationItem.enum = item.enum;
	}
	// set lowerBound if defined
	if (item.lowerBound && item.lowerBound != undefined && item.lowerBound != null && item.lowerBound.length != '') {
		simpleComponentValidationItem.lowerBound = item.lowerBound;
	}
	// set upperBound if defined
	if (item.upperBound && item.upperBound != undefined && item.upperBound != null && item.upperBound.length != '') {
		simpleComponentValidationItem.upperBound = item.upperBound;
	}

	// type specific	validation criteria
	// set minium if if defined
	if ((item.minimum && item.minimum != undefined && item.minimum != null && item.minimum != '') || item.minimum == 0) {
		simpleComponentValidationItem.minimum = item.minimum;
	}

	if (item.maximum && item.maximum != undefined && item.maximum != null && item.maximum != '') {
		simpleComponentValidationItem.maximum = item.maximum;
	}


	return simpleComponentValidationItem;
}


export function createComplexComponentValidationItem(path: string, label: string, required: boolean, complexComponent: any): ComplexComponentData {

	let complexComponentValidationItem: ComplexComponentData = { 
		label: label, 
		path: path, 
		required: required, 
		isValid: false, 
		errorMessage: '' };

	let item = complexComponent;

 console.log('complexComponentValidationItem',label,item, complexComponent	);


	// set max items in array if defined
	if (item.maxItems && item.maxItems != undefined && item.maxItems != null && item.maxItems != '') {
		complexComponentValidationItem.maxItems = item.maxItems;
	}

 // set min items in array if defined
	if (item.minItems && item.minItems != undefined && item.minItems != null && item.minItems != '') {
		complexComponentValidationItem.minItems = item.minItems;
	}

	// // set minLength if defined
	// if (item.minLength && item.minLength != undefined && item.minLength != null && item.minLength != '') {
	// 	simpleComponentValidationItem.minLength = item.minLength;
	// }
	// // set maxLength if defined
	// if (item.maxLength && item.maxLength != undefined && item.maxLength != null && item.maxLength != '') {
	// 	simpleComponentValidationItem.maxLength = item.maxLength;
	// }
	// // set domainList if defined
	// if (item.enum && item.enum != undefined && item.enum != null && item.enum.length > 0) {
	// 	simpleComponentValidationItem.enum = item.enum;
	// }
	// // set lowerBound if defined
	// if (item.lowerBound && item.lowerBound != undefined && item.lowerBound != null && item.lowerBound.length != '') {
	// 	simpleComponentValidationItem.lowerBound = item.lowerBound;
	// }
	// // set upperBound if defined
	// if (item.upperBound && item.upperBound != undefined && item.upperBound != null && item.upperBound.length != '') {
	// 	simpleComponentValidationItem.upperBound = item.upperBound;
	// }

	// // type specific	validation criteria
	// // set minium if if defined
	// if ((item.minimum && item.minimum != undefined && item.minimum != null && item.minimum != '') || item.minimum == 0) {
	// 	simpleComponentValidationItem.minimum = item.minimum;
	// }

	// if (item.maximum && item.maximum != undefined && item.maximum != null && item.maximum != '') {
	// 	simpleComponentValidationItem.maximum = item.maximum;
	// }


	return complexComponentValidationItem;
}


export function removeJsonPathIndices(path) {
	// Matches a dot followed by one or more digits
	// The '\b' ensures we only match whole numbers, not numbers embedded in words
	return path.replace(/\.\d+\b/g, '');
}

export function getParentPath(path) {
	if (typeof path !== 'string' || !path.includes('.')) {
		return ''; // Return empty if there's no dot to remove
	}

	// Find the position of the very last dot
	const lastDotIndex = path.lastIndexOf('.');

	// Slice the string from the start up to that last dot
	return path.substring(0, lastDotIndex);
}

export function getPartyIdFromParent(path) {

	if (typeof path !== 'string' || path.trim() === '') {
		return null;
	}

	const parentPath = getParentPath(path);
	if (!parentPath) {
		return null;
	}

	return getPartyIdByPath(parentPath);

}



export function showDescriptionHandler(e: any, type: 'simple' | 'complex') {
	let descriptionTimeout: any;

	// unset current timeout if any
	if (descriptionTimeout) {
		clearTimeout(descriptionTimeout);
	}

	// add a small delay
	descriptionTimeout = setTimeout(() => {
		descriptionStore.set({ type, content: e.detail.description, path: e.detail.id });
		// console.log('🚀 ~ showDescriptionHandler ~ e.detail.description:', e.detail);
	}, 500);
}

export function hideDescriptionHandler(e: any, type: 'simple' | 'complex') {
	let descriptionTimeout: any;
	// unset current timeout if any
	if (descriptionTimeout) {
		clearTimeout(descriptionTimeout);
	}

	// add a small delay
	descriptionTimeout = setTimeout(() => {
		descriptionStore.set({ type, content: '', path: '' });
	}, 500);
}


function isArrayIndex(segment) {
	return /^\d+$/.test(segment);
}

export function resolveNode(path) {
	const schema = getSchemaStore();
	const parts = path.split('.').filter(Boolean);
	let node = schema;
	let parentNode = null; // tracks the object whose "required" array we'd check
	let lastKey = null;

	for (let i = 0; i < parts.length; i++) {
		const part = parts[i];
		if (!node) return { node: undefined, parentNode: undefined, lastKey: undefined };

		if (isArrayIndex(part)) {
			// Numeric index: just descend into items, don't treat as a property name
			if (node.type === 'array' && node.items) {
				node = node.items;
			}
			continue;
		}

		// Auto-unwrap array wrapper before checking properties
		if (node.type === 'array' && node.items) {
			node = node.items;
		}

		if (!node.properties || !Object.prototype.hasOwnProperty.call(node.properties, part)) {
			return { node: undefined, parentNode: undefined, lastKey: undefined };
		}

		parentNode = node;      // remember parent for "required" check
		lastKey = part;
		node = node.properties[part];
	}

	return { node, parentNode, lastKey };
}
/* @param {object} schema - the root JSON schema object
 * @param {string} path   - dot-separated path, e.g. "Basic.alternateIdentifier"
 * @returns {string|undefined} the description text, or undefined if not found
 */
export function getDescriptionBySchemaAndPath(path) {
	let { node } = resolveNode(path);
	if (!node) return undefined;

	if (node.type === 'array' && node.items) {
		return node.items.description ?? node.description;
	}
	// console.log('🚀 ~ getDescriptionBySchemaAndPath ~ node.description:', node.description, 'node:', node);
	return node.description;

}

/**
 * Check whether a dot-separated path points to a field that is "required"
 * in its immediate parent object.
 *
 * @param {object} schema - the root JSON schema object
 * @param {string} path   - dot-separated path, e.g. "Basic.title" or "creator.individualName.surName"
 * @returns {boolean|undefined} true/false if resolvable, undefined if the path doesn't exist
 */
export function getIsRequiredBySchemaAndPath(path) {
	const resolved: any = resolveNode(path);
	const { parentNode, lastKey } = resolved;
	if (!parentNode || !lastKey) return undefined;

	const requiredList: string[] = parentNode.required || [];
	return requiredList.includes(lastKey);
}

export function getLabelByPath(path: string): string {
	// check if path is array and extract position
	let index = -1;
	let label = '';
	if (path.split('.').length > 1 && !isNaN(Number(path.split('.')[path.split('.').length - 1]))) {
		//path = path.split('.').slice(0, -1).join('.');

		index = Number(path.split('.')[path.split('.').length - 1]);
		label = `${index + 1}. ${convertDisplayName(path.split('.')[path.split('.').length - 2])}`;
	}
	else {
		label = convertDisplayName(path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path);
		console.log('Path is not an array:', path, label);
	}
	return label;
}

export function getMetadata(path: string): { value: any, ref: any, label: string, description: string, required: boolean } {
	console.log('🚀 ~ getMetadata ~ path:', path);
	let value: any = getValueByPath(path);
	let ref: any = getRefByPath(path);
	console.log('🚀 ~ getMetadata ~ path: label', path);
	let label: string = getLabelByPath(path);
	let description = getDescriptionBySchemaAndPath(path);
	let required = !!getIsRequiredBySchemaAndPath(path);
	return { value, ref, label, description, required };
}

export function updateValidationState(path: string, res: any): void {
	let errorMessage = '';
	if (res && res.hasErrors(path)) {
		errorMessage = res.getErrors(path).join('.  ');
	}

	console.log('🚀 ~ updateValidationState ~ path:', path, 'res:', res, 'errorMessage:', errorMessage, get(validationStore));

	if(isSimpleComponent(getNodeByPath(path), path)){
		ValidationStoreSetSimpleTypeValid(path, res ? res.isValid(path) : true, errorMessage);
	}
	else
	{
		ValidationStoreSetComplexTypeValid(path, res ? res.isValid(path) : true, errorMessage);
	}

	
}

export function registerValidationItem(
	path: string,
	label: string,
	required: boolean,
	schemaNode: any,
	forceRegistration: boolean = false
): void {

	if (schemaNode) {

			console.log('🚀 ~ registerValidationItem ~ path:', path);

			//	check if the schemaNode is a complex component and add the specific validation item to the validation store
			if(isSimpleComponent(schemaNode, path)){
					console.log('🚀 ~ registerValidationItem ~ simple:', path);
				let validationItem = createSimpleComponentValidationItem(
					path,
					label,
					required,
					schemaNode
				);
				ValidationStoreAddSimpleComponent(validationItem, forceRegistration);
		}else{

			console.log('🚀 ~ registerValidationItem ~ complex:', path);


			let validationItem = createComplexComponentValidationItem(
				path,
				label,
				required,
				schemaNode
			);
			ValidationStoreAddComplexComponent(validationItem, forceRegistration);
		}

	}
}

export function getAtPath(path) {
  const segments = path.split('.');

  let current = metadataStore ? get(metadataStore) : null; // Start with the metadata store if available
  for (const segment of segments) {
    if (current == null) return undefined;
    current = Array.isArray(current)
      ? current[Number(segment)]
      : current[segment];
  }
  return current;
}

/** Recursively checks for any non-empty "#text" under a node, ignoring @-attributes */
export function hasAnyValue(node) {
  if (node == null) return false;

  if (Array.isArray(node)) {
    return node.some(hasAnyValue);
  }

  if (typeof node === 'object') {
    for (const [key, value] of Object.entries(node)) {
      if (key.startsWith('@')) continue;       // skip @ref, @partyid, @function...
      if (key === '#text') {
        if (typeof value === 'string' && value.trim() !== '') return true;
        continue;
      }
      if (hasAnyValue(value)) return true;
    }
    return false;
  }

  // bare primitive (rare in this schema, but handle it)
  return typeof node === 'string' ? node.trim() !== '' : Boolean(node);
}

/** Public helper: does the given dot-path contain any real value? */
export function hasValueAtPath( path) {
  return hasAnyValue(getAtPath( path));
}

// checks if a component is "simple" by looking for a "#text" property in its properties
export function isSimpleComponent(component: any, path:string): boolean {

		if(component	&& component.properties && component.properties['#text'] !== undefined){
			return true;
		}else	if(component && component['#text'] !== undefined){
			return true;
		}
		else{
			return false;
		}
}