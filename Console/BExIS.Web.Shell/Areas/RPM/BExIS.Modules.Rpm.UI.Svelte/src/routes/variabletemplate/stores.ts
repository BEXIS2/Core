import type { VariableTemplateModel } from "$lib/components/datastructure/types";
import type { listItemType } from "@bexis2/bexis2-core-ui";
import { writable } from "svelte/store";

export const variableTemplatesStore = writable<VariableTemplateModel[]>([]);
export const meaningsStore = writable<listItemType[]>([]);