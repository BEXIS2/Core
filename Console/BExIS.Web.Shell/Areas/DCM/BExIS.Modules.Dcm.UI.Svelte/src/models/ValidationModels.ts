export interface ValidationModel {
    isValid: boolean;
    fileResults: FileValidationResult[];
}

export interface FileValidationResult {
    file: string;
    errors: string[];
    sortedErrors: SortedError[];
}

export interface SortedError {
    name: string;
    count: number;
    issue: string;
    type: ErrorType;
    errors: string[];
}

export enum ErrorType {
  Dataset,
  Datastructure,
  Value,
  MetadataAttribute,
  Other,
  FileReader,
  PrimaryKey,
  File
}

export interface Check{
 name: string;
 type: ErrorType;
 errors: SortedError[];
 style: string;
}
