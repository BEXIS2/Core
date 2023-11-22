import type { ValidationResult } from '../../models';

export interface DataTypeListItem {
	id: number;
	name: string;
	description: string;
	systemType: string;
	inUse: boolean;
}

export interface DataTypeValidationResult {
	validationResult: ValidationResult;
	dataTypeListItem: DataTypeListItem;
}
