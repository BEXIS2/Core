import { writable } from 'svelte/store';
import type { validationStoretype } from './models';
import type { SystemMappingEditModel } from '../../../../routes/m/edit/types';

export const metadataStore = writable<any>();

export const configStore = writable<any>();

export const systemMappingsStore = writable<SystemMappingEditModel>();

export const hideStore = writable<string[]>([]);

export const activeStore = writable<string[]>([]);

export const validationStore = writable<validationStoretype>();
