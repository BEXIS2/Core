import type { ValidationResult } from '../../models';

export interface DimensionListItem {
	id: number;
	name: string;
	description: string;
	specification: string;
	inUse: boolean;
}

export interface DimensionValidationResult {
	validationResult: ValidationResult;
	dimensionListItem: DimensionListItem;
}
