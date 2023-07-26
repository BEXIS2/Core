import { writable } from 'svelte/store';
import type { EntityTemplateModel } from '../../models/EntityTemplate';

export const entityTemplatesStore = writable<EntityTemplateModel[]>([]);
export const x = writable(1);
