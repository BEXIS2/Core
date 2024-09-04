import type { TagInfoEditModel } from './types';
import { writable } from 'svelte/store';

export const tagInfoModelStore = writable<TagInfoEditModel[]>([]);
