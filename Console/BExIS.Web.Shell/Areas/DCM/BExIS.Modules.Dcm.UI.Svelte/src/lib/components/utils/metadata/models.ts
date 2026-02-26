export interface schemaNode {
	title: string;
	description: string;
	type: string;
	properties: schemaNode[];
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
