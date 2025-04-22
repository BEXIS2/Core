import type { errorType, sortedError, sortedWarning } from './Models';

export interface ValidationModel {
	isValid: boolean;
	fileResults: FileValidationResult[];
}

export interface FileValidationResult {
	file: string;
	errors: string[];
	sortedErrors: sortedError[];
	sortedWarnings: sortedWarning[];
}

export interface Check {
	name: string;
	type: errorType;
	errors: sortedError[];
	warnings: sortedWarning[];
	style: string;
}
