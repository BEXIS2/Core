export interface TailorEdit {
    id: number,
    originalName: string,
    editedName: string,
    cleanedName: string
}

export type TailorEditsRequest = {
    datasetId: number,
    versionId: number,
    edits: TailorEdit[]
}