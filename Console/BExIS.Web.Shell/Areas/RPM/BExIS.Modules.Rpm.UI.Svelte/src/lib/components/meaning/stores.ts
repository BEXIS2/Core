import { writable } from 'svelte/store';
import type { MeaningModel, externalLinkType } from './types';

export const meaningsStore = writable<MeaningModel[]>([]);
export const externalLinksStore = writable<externalLinkType[]>([]);
