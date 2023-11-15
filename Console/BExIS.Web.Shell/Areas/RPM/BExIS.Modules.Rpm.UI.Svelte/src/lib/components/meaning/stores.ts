import { writable } from "svelte/store";
import type { MeaningModel, externalLinkType, prefixCategoryType } from "./types";

export const meaningsStore = writable<MeaningModel[]>([]);
export const externalLinksStore = writable<externalLinkType[]>([]);
export const prefixCategoryStore = writable<prefixCategoryType[]>([]);