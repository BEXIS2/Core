import { writable } from 'svelte/store';

export type ResultRow = {
	inputID: string,
	inputRank?: string,
	inputName: string,
	matchType: string,
	id: number,
	rank: string,
	label?: string,
	scientificName: string,
	authorship: string,
	status: string,
	acceptedName?: string,
	classification?: string,
	issues?: string,
}

let acceptedRows: ResultRow[] = [

]

export let acceptedStore = writable<ResultRow[]>(acceptedRows)

let emptyTestRows: ResultRow[] = [

]

export let resultStore = writable<ResultRow[]>(emptyTestRows)
