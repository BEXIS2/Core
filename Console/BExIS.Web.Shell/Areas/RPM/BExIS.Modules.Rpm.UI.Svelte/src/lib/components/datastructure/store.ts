import { writable } from 'svelte/store';
import type { listItemType } from '@bexis2/bexis2-core-ui';
import type { meaningListItemType, templateListItemType, unitListItemType } from './types';

export const structureStore = writable<listItemType[]>([]);
export const displayPatternStore = writable<listItemType[]>([]);
export const dataTypeStore = writable<listItemType[]>([]);
export const unitStore = writable<unitListItemType[]>([]);
export const templateStore = writable<templateListItemType[]>([]);
export const isTemplateRequiredStore = writable<boolean>(false);
export const isMeaningRequiredStore = writable<boolean>(false);
export const setByTemplateStore = writable<boolean>(false);
export const updateDescriptionByTemplateStore = writable<boolean>(false);
export const enforcePrimaryKeyStore = writable<boolean>(false);
export const changeablePrimaryKeyStore = writable<boolean>(false);
export const meaningsStore = writable<meaningListItemType[]>([]);
export const constraintsStore = writable<listItemType[]>([]);

export const x = writable(1);
