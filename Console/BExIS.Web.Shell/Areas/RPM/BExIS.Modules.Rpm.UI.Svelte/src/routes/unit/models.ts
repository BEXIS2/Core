import type { ValidationResult } from '../../models';

export interface UnitListItem {
	id: number;
	name: string;
	description: string;
	abbreviation: string;
	dimension?: DimensionListItem;
	datatypes: DataTypeListItem[];
	link?: LinkItem;
	measurementSystem: string;
	inUse: boolean;
}

export interface DataTypeListItem {
	id: number;
	name: string;
	description: string;
	systemType: string;
}

export interface DimensionListItem {
	id: number;
	name: string;
	description: string;
	specification: string;
}

export interface LinkItem {
	id: number;
	uri: string;
	name: string;
}

export interface UnitValidationResult {
	validationResult: ValidationResult;
	unitListItem: UnitListItem;
}
