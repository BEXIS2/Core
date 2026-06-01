import { writable } from 'svelte/store';
import type { GenericMatchingResult } from '$lib/types/types';



let acceptedRows: GenericMatchingResult[] = [

]

export let acceptedStore = writable<GenericMatchingResult[]>(acceptedRows)



let resultRows: GenericMatchingResult[] = [

]

export let resultStore = writable<GenericMatchingResult[]>(resultRows)