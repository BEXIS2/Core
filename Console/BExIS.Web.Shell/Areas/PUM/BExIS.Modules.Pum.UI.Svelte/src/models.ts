export interface ValidationResult {
	isValid: boolean;
	validationItems: ValidationItem[];
}

export interface ValidationItem {
	name: string;
	message: string;
}
