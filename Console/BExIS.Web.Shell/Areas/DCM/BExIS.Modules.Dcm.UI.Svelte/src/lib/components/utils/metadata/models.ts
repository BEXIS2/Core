export interface schemaNode {
	title: string;
	description: string;
	type: string;
	properties: schemaNode[];
}

export interface validationStoretype {
	allSimpleRequiredValid: boolean;
	simpleTypeValidationItems: SimpleComponentData[];
	complexTypeValidationItems: ComplexComponentData[];
}

export interface SimpleComponentData {
	path: string;
	label: string;
	required: boolean;
	regex?: string;
	lowerBound?: number;
	upperBound?: number;
	domainList?: string[];
	minLength?: number;
	maxLength?: number;
	minimum?: number;
	maximum?: number;
	isValid: boolean;
	enum?: string[];
	errorMessage: string
}

export interface ComplexComponentData {
	complexComponent: any;
	label: string;
	required: boolean;
	errorMessage: string
}

/* PARTIES */
export interface SystemMappingEditModel {
 partyMappings: PartyMappingModel[];
 keyMappings: KeyMappingModel[];
}

export interface PartyMappingModel {
 path: string;
 parentPath: string;
 linkElementId: number;
 selector: boolean;
 complexity:boolean;
 list:PartyMappingResultElementModel[];
} 

export interface KeyMappingModel {
 path: string;
 systemKeyName: string;
}

export interface PartyMappingResultElementModel {
 value: string;
 partyId: number;
}

export class MappingComponentConfig {
	isMappedToParty: boolean;
	isSelector: boolean;
	partyMappingObject: any;
	isMappedToKey: boolean;
	pathWithoutIndices: string;
	selectorValue: any

	constructor(){
		this.isMappedToParty = false;
		this.isSelector = false;
		this.partyMappingObject = null;
		this.isMappedToKey = false;
		this.pathWithoutIndices = '';
		this.selectorValue = null;
	}
}
