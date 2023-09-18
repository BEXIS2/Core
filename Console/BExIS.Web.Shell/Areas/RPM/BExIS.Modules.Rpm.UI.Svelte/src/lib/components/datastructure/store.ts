import { writable } from 'svelte/store';
import type { listItemType } from '@bexis2/bexis2-core-ui';
import type { unitListItemType } from './types';

export const structureStore = writable<listItemType[]>([]);
export const displayPatternStore = writable<listItemType[]>([]);
export const dataTypeStore = writable<listItemType[]>([]);
export const unitStore = writable<unitListItemType[]>([]);


export const x = writable(1);
