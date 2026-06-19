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
    matchSource: string,
    timeStamp: string,
    done: boolean
}

export interface MatchingProgress {
    datasetId: number,
    numRowsGlobal: number,
    steps: StepEntry[],
}

// this is a helper for typing response content correctly
// success false indicates that either the response failed or the whole request failed
export type ServiceResult<T> = 
    | { success: true, data: T }
    | { success: false, error: string };

export interface MatchingFileStatus {
    directoryExists: boolean,
    fileExists: boolean,
    markerExists: boolean,
    markerStale: boolean,
    markerStart: string,
    matchingProgressExists: boolean,
    stepExists: boolean,
    stepCompleted: boolean,
    downloadLinkPresent: boolean,
    jobKeyPresent: boolean,
}

export type SpeciesMatchingRow = {
    // database row id
    id: number,
    // original unchanged name (used for matching if EditedName is empty, and for display purposes)
    originalName: string,
    // helper to apply data cleaning and better overview (field itself does not exist in db)
    cleanedName: string,
    // edited name after data cleaning + manual corrections (used for matching)
    editedName: string,
    // indicates whether the match has been confirmed by the user
    confirmedByUser: boolean,
    // unique identifier of the matched name in the external source (e.g. GBIF taxon ID)
    matchId: string,
    // matched name from the external source (the result)
    matchedName: string,
    // authorship of the matched name
    matchAuthorship: string,
    // taxonomic rank of the matched name (e.g. species, genus, etc.)
    matchRank: string,
    // type of the match (e.g. exact, fuzzy, etc.)
    matchType: string,
    // taxonomic status of the matched name (e.g. accepted, synonym, etc.)
    status: string,
    // accepted name if (for example) the matched name is a synonym
    acceptedScientificName: string,
    // unique identifier of the accepted name in the external source (e.g. GBIF taxon ID)
    acceptedId: string,
    // authorship of the accepted name
    acceptedAuthorship: string,
    // higher classification of the matched name (e.g. kingdom, phylum, class, order, family, genus)
    taxonKingdom: string,
    taxonPhylum: string,
    taxonClass: string,
    taxonOrder: string,
    taxonFamily: string,
    taxonGenus: string,
    // source of the match (e.g. Catalogue of Life, GBIF, etc.)
    matchSource: string,
    // version of the source used for matching
    matchSourceVersion: string,
    // timestamp of the match (can vary by hours due to processing and queue times on different APIs)
    timeStampMatch: string
}

export interface GenericMatchingResult {
    original_ID: string,
    original_scientificName: string,
    scientificName: string,
    original_rank?: string,
    original_kingdom?: string,
    original_authorship?: string,
    matchType?: string,
    matchIssues?: string,
    id: string,
    rank?: string,
    authorship?: string,
    status?: string,
    acceptedID?: string,
    acceptedScientificName?: string,
    acceptedAuthorship?: string,
    kingdom?: string,
    phylum?: string,
    class?: string,
    order?: string,
    family?: string,
    genus?: string,
    classification?: string,
}

export interface CLBMatchingResult {
    original_ID: string,
    original_scientificName: string,
    original_rank: string,
    original_kingdom: string,
    original_authorship: string,
    matchType: string,
    matchIssues: string,
    id: string,
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
    matchIds: (string | undefined)[]
}


// Selectable api metadata/options provided by the backend
export interface SourceKeyInfoItem {
  sourceKey: string;
  title: string;
  alias: string;
}

export interface ExternalApiSource {
  sourceKeyInfo: SourceKeyInfoItem[];
}

export interface ExternalApiMetadata {
  clb: ExternalApiSource;
}

// apiOptions types that are actually send as a payload together with a file Matching request
export interface ClbOptions {
    type: 'clb'; // Discriminator (optional, but highly recommended)
    sourceKey: string;
    synonyms: boolean;
}

export interface GbifOptions {
    type: 'gbif';
    parameter1: string;
    parameter2: string;
}

// Representing the IApiOptions interface as a Union type
export type IApiOptions = ClbOptions | GbifOptions