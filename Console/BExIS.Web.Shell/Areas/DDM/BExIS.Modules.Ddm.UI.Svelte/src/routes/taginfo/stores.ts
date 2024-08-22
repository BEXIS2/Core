import type { TagInfoModel } from './types';
import { writable } from 'svelte/store';

export const tagInfoModelStore = writable<TagInfoModel[]>([]);

