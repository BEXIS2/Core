import { writable } from 'svelte/store';
import { get } from 'svelte/store';
import type { StepEntry } from '$lib/types/types';

let matchingJobRows: StepEntry[] = [

]

export let matchingJobStore = writable<StepEntry[]>(matchingJobRows);