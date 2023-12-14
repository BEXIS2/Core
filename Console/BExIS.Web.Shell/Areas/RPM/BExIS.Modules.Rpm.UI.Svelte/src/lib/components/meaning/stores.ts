import { writable } from "svelte/store";
import type { MeaningModel, externalLinkType, prefixCategoryType, prefixListItemType } from "./types";
import type { listItemType } from "@bexis2/bexis2-core-ui";

export const meaningsStore = writable<MeaningModel[]>([]);
export const externalLinksStore = writable<externalLinkType[]>([]);
export const prefixCategoryStore = writable<prefixCategoryType[]>([]);
export const externalLinkTypesStore = writable<listItemType[]>([]);
export const prefixesStore = writable<prefixListItemType[]>([]);