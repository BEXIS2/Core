import { writable } from 'svelte/store';

export const validTableStore = writable<any[]>([]);
export const invalidTableStore = writable<any[]>([]);
export const DataCounterStore = writable<number>(0);