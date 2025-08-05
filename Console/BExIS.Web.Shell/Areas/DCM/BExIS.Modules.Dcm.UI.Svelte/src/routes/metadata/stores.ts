import { writable } from 'svelte/store';

export const metadataStore = writable<any>();

export const hideStore = writable<string[]>([]);
