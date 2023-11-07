import type { ValidationResult } from '../../models';

export interface ConstraintListItem {
	id: number;
	version: number;
	name: string;
	description: string;
	formalDescription: string;
	type:string;
	inUse: boolean;
}

export interface ConstraintValidationResult {
	validationResult: ValidationResult;
	dimensionListItem: ConstraintListItem;
}
