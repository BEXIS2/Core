import { writable } from 'svelte/store';
import type { validationStoretype } from './models';

export const metadataStore = writable<any>();

export const configStore = writable<any>();

export const hideStore = writable<string[]>([]);

export const activeStore = writable<string[]>([]);

export const validationStore = writable<validationStoretype>();
