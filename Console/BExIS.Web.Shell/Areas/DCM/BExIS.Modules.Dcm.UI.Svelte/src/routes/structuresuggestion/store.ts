import { writable } from 'svelte/store';
import type { ListItem } from '@bexis2/bexis2-core-ui';

export const structureStore = writable<ListItem[]>([]);
export const displayPatternStore = writable<ListItem[]>([]);
export const x = writable(1);
