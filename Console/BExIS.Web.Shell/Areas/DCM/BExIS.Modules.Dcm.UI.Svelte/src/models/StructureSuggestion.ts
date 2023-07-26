import { ListItem } from './Models';

export interface StructureSuggestionModel {
	id: number;
	title: string;
	description: string;
	file: string;
	delimeter: number;
	decimal: number;
	textMarker: number;
	delimeters: ListItem[];
	decimals: ListItem[];
	textMarkers: ListItem[];
	preview: string[];
	total: number;
	skipped: number;
	markers: Marker[];
	variables: VariableModel[];
	missingValues: missingValueType[];
}

export interface missingValueType {
	displayName: string;
	description: string;
}

export interface Marker {
	type: string;
	row: number;
	cells: boolean[];
}

export interface VariableModel {
	id: number;
	name: string;
	description: string;
	systemType: string;
	isKey: boolean;
	isOptional: boolean;
	dataType: ListItem;
	unit: ListItem;
	template: ListItem;
	displayPattern: ListItem | undefined;
	possibleUnits: ListItem[];
	possibleTemplates: ListItem[];
	possibleDisplayPattern: ListItem[];
}
