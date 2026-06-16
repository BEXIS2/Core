export interface validatedData {
    data: any;
    MissingFields?: string[];
    TypeMessage: string;
    imported: boolean;
}

export interface datasetType  {
    Title?: string;
    Description?: string;
    DataStructureId?: number;
    MetadataStructureId?: number;
    EntityTemplateId?: number;
}

export  interface issueType {
    Index: number;
    errorType: string;
    msg: string;
}

export interface fillDatasetType {
    id: number;
    data: any;
}