import type Variable from '$lib/components/datastructure/structure/variable/Variable.svelte';
import type { ValidationResult } from '../../models';

export interface ConstraintListItem {
	id: number;
	version: number;
	name: string;
	description: string;
	formalDescription: string;
	type: string;
	negated: boolean;
	inUse: boolean;
	variableIDs: number[];
	creationDate?: string;
	lastModified?: string;
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
	variableIDs: number[];
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
	variableIDs: number[];
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
	variableIDs: number[];
}

export interface ConstraintValidationResult {
	validationResult: ValidationResult;
	constraintListItem: ConstraintListItem;
}

export interface DatasetInfo {
	id?: number;
	name?: string;
	description?: string;
}
