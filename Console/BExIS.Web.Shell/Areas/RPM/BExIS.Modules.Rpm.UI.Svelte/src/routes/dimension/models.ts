import type {ValidationResult} from "../../models";

export interface DimensionListItem{
    id: number;
    name: string;
}

export interface DimensionListItem {
    id: number;
    name: string;
    description: string;
    specification: string;
}

export interface DimensionValidationResult {
    validationResult:ValidationResult;
    unitListItem:DimensionListItem;
}

