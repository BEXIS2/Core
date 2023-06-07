import { FileInfo } from "@bexis2/bexis2-core-ui";

export interface DataDescriptionModel {
    id: number;
    structureId: number;
    title: string;
    description: string;
    lastModification: string | null;
    variables: VariableModel[];
    allFilesReadable: boolean;
    readableFiles: FileInfo[];
}

export interface VariableModel {
    id: number;
    name: string;
    unit: string;
    dataType: string;
}