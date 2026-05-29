import type { SimpleComponentData, validationStoretype } from './models';
import { metadataStore,systemMappingsStore, hideStore, validationStore, configStore, activeStore } from './stores';
import { get } from 'svelte/store';
// Utility functions for metadata handling
// Get and set values in the metadata store based on a dot-separated path
export function setMetadataStore(metadata: any) {
	metadataStore.set(metadata);
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

	const obj	= getNodeByPath(path);
	const	partyId = obj ? obj['@partyid'] : null;
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
export function updateMetadataStore(path: string, value: any, isMulti?: boolean, ref?: any, partyid?:number): any {
 
	let obj: any = {};
	if (path !== undefined && path !== null && path !== '') {
		metadataStore.subscribe((v) => {
			obj = v;
		});
		{
			if (value !== undefined && value !== null && value !== getValueByPath(path)) {

				if (isMulti) {
						obj = setValueByPath(obj, path, value);
				} else {
					obj = setValueByPath(obj, path + '.#text', value);
					if (ref !== undefined && ref !== null) {
						obj = setValueByPath(obj, path + '.@ref', ref);
					}
					if (partyid !== undefined && partyid !== null) {
						obj = setValueByPath(obj, path + '.@partyid', partyid);
					}
					if (
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
			else if((value === undefined || value === null) && partyid	!== undefined && partyid !== null &&partyid>0)
			{
				const parent = getByPath(path);
			 console.log("🚀 ~ updateMetadataStore ~ parent:", parent)
				parent["@partyid"] = partyid;
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
	}
	console.log('remove metadata store:', obj);
	return obj;
}
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
		for (const component of config.components) {
			if (component.meta.component_name.toLowerCase() === componentName.toLowerCase() && component.globalSettings.anchorpoint === anchor) {
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
		for (const component of config.components) {
			if (component.meta.component_name.toLowerCase() === componentName.toLowerCase() && component.globalSettings.anchorpoint === anchor) {
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
		for (const variable of variables) {
			if (variable.target_variable === targetVariableName) {
				console.log('Found variable:', variable.JSONPath);
				return variable.JSONPath;
			}
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


// utils.js or inside <script>
export function hasValue(node) {
  if (node === null || node === undefined) return false;

  // If it's an array, check if any element has a value
  if (Array.isArray(node)) {
    return node.some(hasValue);
  }

  // If it's an object, check if any property has a value
  if (typeof node === 'object') {
    return Object.values(node).some(hasValue);
  }

  // If it's a string, trim it and check length; otherwise, check truthiness (for numbers/bools)
  return typeof node === 'string' ? node.trim().length > 0 : true;
}

// p = path:string & r = required: boolean
export function isActive(p:string, r:boolean):boolean {
  // logic to determine if the component is active
  const node = getNodeByPath(p);
  const hasData = hasValue(node); // replace with actual check for data presence

  if(r) {
    return true; // if required, it's always active
  } else if (hasData)
  {    return true; // if it has data, it's active
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

			if(node.length >	0){ 
					empty(node[0]); // clear the first element to preserve structure
			}

			// remove all	elements but only first one  stay to preserve structure
			return node.length = 1;
		}

		if(node.hasOwnProperty('#text')) {
				return node['#text'] = '';
		}
  
		if (typeof node === 'object') {
		
				Object.keys(node).forEach(key => {
					const value = node[key];
					return empty(value);
				});
		}

  if(node.hasOwnProperty('#text')) {
				return node['#text'] = '';
		}

		return node;

}

// Validation Store Functions
// Get current values from the validation store
// If undefined, initialize with default values
// and return the validation store values
export function getValidationStore(): validationStoretype {
	let validationStoreValues: validationStoretype = { allSimpleRequiredValid: false, simpleTypeValidationItems: [], complexTypeValidationItems: []	};
	validationStore.subscribe(n => {
		validationStoreValues = n;
	});
	if (validationStoreValues == undefined) {
		clearValidationStore();
	}
	return validationStoreValues;
}

export function clearValidationStore(): void {
	validationStore.set({ allSimpleRequiredValid: false, simpleTypeValidationItems: [],complexTypeValidationItems : [] });
}
// Add a simple component's validation data to the validation store
// if it doesn't already exist
// and has relevant validation criteria
// Returns the updated validation store values
export function ValidationStoreAddSimpleComponent(item: SimpleComponentData): validationStoretype {
	let validationStoreValues: validationStoretype = getValidationStore();
	if (validationStoreValues.simpleTypeValidationItems.find(i => i.path === item.path) === undefined && (item.required || item.regex !== undefined || item.lowerBound !== undefined || item.upperBound !== undefined || (item.domainList && item.domainList.length > 0))) {
		validationStoreValues.simpleTypeValidationItems.push(item);
		validationStore.set(validationStoreValues);
	}
	return validationStoreValues;
}
// Set overall validity for all simple required components in the validation store
// based on the validity of an individual component identified by its path
// Returns the updated validity of the specified component
export function ValidationStoreSetSimpleTypeValid(path: string, isValid: boolean, errorMessage:string = ''): boolean {
	let valid: boolean = false;
	let validationStoreValues: validationStoretype = getValidationStore(); 


	if (isValid != null && isValid != undefined) {
		const	item = validationStoreValues.simpleTypeValidationItems.find(item => {
			return item.path === path;
		});
		if(item)
		{
				item!.isValid = isValid;

				valid = item!.isValid;

				if(item && errorMessage){
					item.errorMessage = errorMessage;
				}

				if(valid == true && item) // reset errors if item is valid
				{	
						item.errorMessage	= '';
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
	validationStore.set(validationStoreValues);
	//console.log("🚀 ~ ValidationStoreSetSimpleTypeValid ~ validationStore:", get(validationStore))
	return valid;
}
// Create a SimpleComponentData validation item
// based on the provided parameters and simple component properties
export function createSimpleComponentValidationItem(path: string, label: string, required: boolean, simpleComponent: any): SimpleComponentData {

	let simpleComponentValidationItem: SimpleComponentData = { label: label, path: path, required: required, isValid: false,	errorMessage: '' };

 let item = simpleComponent.properties['#text'];

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

// type secific	validation criteria
// set minium if if defined
if ((item.minimum && item.minimum != undefined && item.minimum != null && item.minimum != '') || item.minimum == 0) {
	simpleComponentValidationItem.minimum = item.minimum;
}

if (item.maximum && item.maximum != undefined && item.maximum != null && item.maximum != '') {
	simpleComponentValidationItem.maximum = item.maximum;
}


	return simpleComponentValidationItem;
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
			
  // get party id from parent path, which is the last number in the path

}