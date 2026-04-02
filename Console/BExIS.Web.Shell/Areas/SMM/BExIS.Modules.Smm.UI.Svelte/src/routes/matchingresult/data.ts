import { writable } from 'svelte/store';
import type { CLBMatchingResult } from '$lib/types/types';



let acceptedRows: CLBMatchingResult[] = [

]

export let acceptedStore = writable<CLBMatchingResult[]>(acceptedRows)



let resultRows: CLBMatchingResult[] = [

]

export let resultStore = writable<CLBMatchingResult[]>(resultRows)