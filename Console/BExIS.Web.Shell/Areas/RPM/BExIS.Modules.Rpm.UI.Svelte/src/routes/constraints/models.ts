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

export interface DomainConstraintListItem extends ConstraintListItem {
    domain: string;
    provider: string;
    selectionPredicate?: ConstraintSelectionPredicate;
}

export interface RangeConstraintListItem extends ConstraintListItem {
	lowerbound: number;
	upperbound: number;
	lowerboundIncluded: boolean;
	upperboundIncluded: boolean;
}

export interface PatternConstraintListItem extends ConstraintListItem {
	pattern: string;
}

export interface ConstraintValidationResult {
	validationResult: ValidationResult;
	constraintListItem: ConstraintListItem;
}

export interface Info {
	id: number;
	name: string;
	description: string;
}

export interface DatasetInfo extends Info {
    datasetVersionId: number;
    datasetVersionNumber: number;
    datastructureId: number;
}

export interface DatasetImportInfo extends DatasetInfo{
	varId?: number;
}

export interface DatastructureInfo extends Info {
    columnInfos: ColumnInfo[];
}

export interface ColumnInfo extends Info {
    orderNr: number;
	unit: string;
    dataType: string;
}

export interface ConstraintSelectionPredicate {
    datasetId: number;
    datasetVersionId: number;
    datasetVersionNumber: number;
    tagId: number;
    variableId: number;
    url: string;
}