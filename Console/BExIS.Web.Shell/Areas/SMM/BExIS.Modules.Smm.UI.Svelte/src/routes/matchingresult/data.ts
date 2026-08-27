import { writable } from 'svelte/store';
import type { GenericMatchingResult, SpeciesMatchingRow } from '$lib/types/types';



let acceptedRows: GenericMatchingResult[] = [

]

export let acceptedStore = writable<GenericMatchingResult[]>(acceptedRows)



let resultRows: GenericMatchingResult[] = [

]

export let resultStore = writable<GenericMatchingResult[]>(resultRows)


let mismatchRows: GenericMatchingResult[] = [

]

export let mismatchStore = writable<GenericMatchingResult[]>(mismatchRows)


let doneRows: SpeciesMatchingRow[] = [

]

export let doneStore = writable<SpeciesMatchingRow[]>(doneRows)