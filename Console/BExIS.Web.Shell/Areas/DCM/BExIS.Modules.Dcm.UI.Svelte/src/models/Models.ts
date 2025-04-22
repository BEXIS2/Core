export interface KvP {
	id: number;
	text: string;
}

export interface ListItem {
	id: number;
	text: string;
	group: string;
	description: string;
}

export interface sortedError {
	name: string;
	count: number;
	issue: string;
	type: errorType;
	errors: string[];
}

export interface sortedWarning {
	name: string;
	count: number;
	issue: string;
	type: errorType;
	warnings: string[];
}

export enum errorType {
	Dataset,
	Datastructure,
	Value,
	MetadataAttribute,
	Other,
	FileReader,
	PrimaryKey,
	File
}
