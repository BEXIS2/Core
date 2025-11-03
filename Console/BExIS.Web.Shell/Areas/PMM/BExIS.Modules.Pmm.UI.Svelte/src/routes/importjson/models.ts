export interface validatedData {
    data: Record<string, any>[];
    MissingFields?: string[];
    TypeMessage: string;
}

export interface datasetType  {
    Title?: string;
    Description?: string;
    DataStructureId?: number;
    MetadataStructureId?: number;
    EntityTemplateId?: number;
}