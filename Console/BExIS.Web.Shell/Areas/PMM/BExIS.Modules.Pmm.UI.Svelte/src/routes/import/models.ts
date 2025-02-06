export interface errorItem {
	column: string;
	errorMsg: string;
}

export interface tableErrorItem{
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