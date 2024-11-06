export interface errorItem {
	column: string;
	errorMsg: string;
}

export interface tableErrorItem {
	errorType: string;
	errorMsg: string;
	value: any;
}

export interface errorArray {
	rowIndex: number;
	cellErrors: errorItem[];
}



export interface MappingType {
    Mappings: MappingEntry[];
}

export interface MappingEntry {
    publication?: MappingField[];
    Resource?: MappingField[];
}

export interface MappingField {
    Source?: string;
    Target?: string;
    [key: string]: MappingField[] | string | undefined;
}