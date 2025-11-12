export interface schemaNode {
	title: string;
	description: string;
	type: string;
	properties: scheemaNode[];
}

export interface validationStoretype {
	allSimpleTypesValid: boolean;
	simpleTypeValidationItems: SimpleComponentData[];
}

export interface SimpleComponentData {
	path: string;
	label: string;
	required: boolean;
	value: any;
}

export interface ComplexComponentData {
	complexComponent: any;
	label: string;
}
