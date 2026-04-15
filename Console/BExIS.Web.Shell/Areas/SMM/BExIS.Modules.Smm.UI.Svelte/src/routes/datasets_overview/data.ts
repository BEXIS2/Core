import { writable } from 'svelte/store';

export type VersionInfo = {
    // title: string,
    // description: string,
    // changeDescription: string,
    timestamp: string,
    versionType: string,
    versionName: string,
    // versionDescription: string,
    // publicAccess: boolean,
    // publicAccessDate: string,
    hasMatchingProgress: boolean,
    versionId: number,
    versionNr: number,
}

export type BasicDatasetInfo = {
    id: number,
    dataStructureId: number,
    title: string,
    abstract: string,
    isTabular: boolean,
    metadataComplete: boolean,
    hasMatchingProgress: boolean,
    versions: VersionInfo[]
}

export type DisplayDatasetVersion = {
    id: number,
    dataStructureId: number,
    title: string,
    abstract: string,
    isTabular: boolean,
    metadataComplete: boolean,
    timestamp: string,
    versionType: string,
    versionName: string,
    hasMatchingProgress: boolean
    versionId: number,
    versionNr: number,
}

let datasetRows: DisplayDatasetVersion[] = [

]


export let datasetsStore = writable<DisplayDatasetVersion[]>(datasetRows);