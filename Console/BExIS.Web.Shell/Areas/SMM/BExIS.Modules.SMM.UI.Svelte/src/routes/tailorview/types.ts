// one row-change of a SpeciesMatchingResult in bexis
// used to submit/apply changes to backend database
export interface TailorEdit {
    id: number,
    originalName: string,
    editedName: string,
    cleanedName: string
}
