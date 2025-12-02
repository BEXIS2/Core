export interface schemaNode {
	title: string;
	description: string;
	type: string;
	properties: scheemaNode[];
}

export interface validationStoretype {
	allSimpleRequiredValid: boolean;
	simpleTypeValidationItems: SimpleComponentData[];
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
	isValid: boolean;
}

export interface ComplexComponentData {
	complexComponent: any;
	label: string;
}
