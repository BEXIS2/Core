export interface errorItem {
	column: string;
	errorMsg: string;
}

export interface tableErrorItem {
	errorType: string;
	errorMsg: string;
	value?: any;
}

export interface errorArray {
	rowIndex: number;
	cellErrors: errorItem[];
}

export interface Mapping {
	publication?: MappingEntry[];
	Resource?: MappingEntry[];
}

// Anpassung von MappingEntry, sodass entweder Source/Target oder eine verschachtelte Struktur existieren kann
export interface MappingEntry {
	Source?: string;
	Target?: string;
	Curation?: MappingEntry[];
	Identifiers?: MappingEntry[];
	Availability?: MappingEntry[];
	File?: MappingEntry[];
	Code?: MappingEntry[];
}

export interface Dataset {
	Title?: string;
	Description?: string;
	DataStructureId?: number;
	MetadataStructureId?: number;
	EntityTemplateId?: number;
}

export interface dataSetType  {
	Title?: string;
	Description?: string;
	DataStructureId?: number;
	MetadataStructureId?: number;
	EntityTemplateId?: number;
}

export interface ValidationType {
	valid: boolean;
	cellError: errorItem[];
}

export interface ValidationReturn {
	validData: any[];
	invalidData: any[];
	invalidDataCounter: number;
	errors: errorArray[];
}

export interface createDatasetReturn {
	uploadedCount: number;
	idMapping: number[][];
	tempTitle: string | null;
}