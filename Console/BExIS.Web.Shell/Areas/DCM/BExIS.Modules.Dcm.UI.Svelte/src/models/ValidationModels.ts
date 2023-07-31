import type {errorType, sortedError} from './Models'

export interface ValidationModel {
	isValid: boolean;
	fileResults: FileValidationResult[];
}

export interface FileValidationResult {
	file: string;
	errors: string[];
	sortedErrors: sortedError[];
}



export interface Check {
	name: string;
	type: errorType;
	errors: sortedError[];
	style: string;
}
