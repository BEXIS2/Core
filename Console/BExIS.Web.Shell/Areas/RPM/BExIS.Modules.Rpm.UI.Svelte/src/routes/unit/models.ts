import type { ValidationResult } from '../../models';

export interface UnitListItem {
	id: number;
	name: string;
	description: string;
	abbreviation: string;
	dimension?: DimensionListItem;
	datatypes: DataTypeListItem[];
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

export interface UnitValidationResult {
	validationResult: ValidationResult;
	unitListItem: UnitListItem;
}
