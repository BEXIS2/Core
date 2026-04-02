import { writable } from 'svelte/store';

export type BasicDatasetInfo = {
    id: number,
    dataStructureId: number,
    title: string,
    abstract: string,
    isTabular: boolean,
    metadataComplete: boolean,
    hasMatchingProgress: boolean
}


let datasetRows: BasicDatasetInfo[] = [

]


export let datasetsStore = writable<BasicDatasetInfo[]>(datasetRows);