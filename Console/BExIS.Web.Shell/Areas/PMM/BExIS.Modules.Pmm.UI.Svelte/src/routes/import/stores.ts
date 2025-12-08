import { writable } from 'svelte/store';
import type { createDatasetReturn } from './models';

export const validTableStore = writable<any[]>([]);
export const invalidTableStore = writable<any[]>([]);
export const createDatasetReturnStore = writable<createDatasetReturn>({
	uploadedCount: 0,
	idMapping: [],
	tempTitle: null
});