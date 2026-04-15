// global types

export interface MappingEntry {
    variableId: number,
    variableName: string,
    headerMapping: string
}

export interface HeaderMappings {
    datastructureId: number,
    datasetId: number,
    mappings: MappingEntry[],
}

export interface StepEntry {
    id: number,
    numRows: number,
    inputFileName: string,
    resultFileName: string,
    jobKey: string,
    downloadLink: string,
    done: boolean
}

export interface MappingProgress {
    datasetId: number,
    numRowsGlobal: number,
    steps: StepEntry[],
}

// this is a helper for typing response content correctly
// success false indicates that either the response failed or the whole request failed
export type ServiceResult<T> = 
    | { success: true, data: T }
    | { success: false, error: string };

export interface CLBMatchingResult {
    original_ID: string,
    original_scientificName: string,
    original_rank: string,
    original_kingdom: string,
    original_authorship: string,
    matchType: string,
    matchIssues: string,
    iD: string,
    rank: string,
    scientificName: string,
    authorship: string,
    status: string,
    acceptedID: string,
    acceptedScientificName: string,
    acceptedAuthorship: string,
    kingdom: string,
    phylum: string,
    class: string,
    order: string,
    family: string,
    genus: string,
    classification: string,
}

export type AcceptMatchesRequest = {
    datasetId: number,
    versionId: number,
    stepId: number,
    matchIds: string[]
}