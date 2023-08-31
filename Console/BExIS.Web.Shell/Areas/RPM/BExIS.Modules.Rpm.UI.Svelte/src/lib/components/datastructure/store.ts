import { writable } from 'svelte/store';
import type { listItemType } from '@bexis2/bexis2-core-ui';

export const structureStore = writable<listItemType[]>([]);
export const displayPatternStore = writable<listItemType[]>([]);
export const x = writable(1);
