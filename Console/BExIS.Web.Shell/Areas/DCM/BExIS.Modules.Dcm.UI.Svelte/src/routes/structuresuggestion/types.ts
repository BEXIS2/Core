import type { listItemType } from '@bexis2/bexis2-core-ui';

export interface StructureSuggestionModel {
	id: number;
	title: string;
	description: string;
	file: string;
	delimeter: number;
	decimal: number;
	textMarker: number;
	delimeters: listItemType[];
	decimals: listItemType[];
	textMarkers: listItemType[];
	preview: string[];
	total: number;
	skipped: number;
	markers: Marker[];
	variables: VariableModel[];
	missingValues: MissingValueModel[];
}

export interface MissingValueModel {
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
	dataType: listItemType;
	unit: listItemType;
	template: listItemType;
	displayPattern: listItemType | undefined;
	possibleUnits: listItemType[];
	possibleTemplates: listItemType[];
	possibleDisplayPattern: listItemType[];
}
