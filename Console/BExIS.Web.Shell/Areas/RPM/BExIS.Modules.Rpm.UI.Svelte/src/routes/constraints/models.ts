import type { ValidationResult } from '../../models';

export interface ConstraintListItem {
	id: number;
	version: number;
	name: string;
	description: string;
	formalDescription: string;
	type: string;
	negated : boolean;
	inUse: boolean;
}

export interface DomainConstraintListItem {
	id: number;
	version: number;
	name: string;
	description: string;
	formalDescription: string;
	domain: string; 
	negated: boolean;
	inUse: boolean;
}

export interface RangeConstraintListItem {
	id: number;
	version: number;
	name: string;
	description: string;
	formalDescription: string;
	lowerbound: number;
	upperbound: number;
	lowerboundIncluded: boolean;
	upperboundIncluded: boolean;
	negated: boolean;
	inUse: boolean;
}

export interface PatternConstraintListItem {
	id: number;
	version: number;
	name: string;
	description: string;
	formalDescription: string;
	pattern: string;
	negated: boolean;
	inUse: boolean;
}

export interface ConstraintValidationResult {
	validationResult: ValidationResult;
	constraintListItem: ConstraintListItem;
}

export interface DomainConstraintValidationResult {
	validationResult: ValidationResult;
	dimensionListItem: DomainConstraintListItem;
}

export interface RangeConstraintValidationResult {
	validationResult: ValidationResult;
	dimensionListItem: RangeConstraintListItem;
}
export interface PatternConstraintValidationResult {
	validationResult: ValidationResult;
	dimensionListItem: PatternConstraintListItem;
}

