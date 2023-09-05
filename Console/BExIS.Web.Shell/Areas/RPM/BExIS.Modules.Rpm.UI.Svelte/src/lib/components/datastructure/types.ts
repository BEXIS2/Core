import type { listItemType } from '@bexis2/bexis2-core-ui';

export interface DataStructureModel {
	id: number;
	title: string;
	description: string;
	linkedTo: number[];
}

export interface DataStructureCreationModel {
	entityId: number;
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
	markers: markerType[];
	variables: VariableModel[];
	missingValues: missingValueType[];
}

export interface missingValueType {
	displayName: string;
	description: string;
}

export interface markerType {
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
