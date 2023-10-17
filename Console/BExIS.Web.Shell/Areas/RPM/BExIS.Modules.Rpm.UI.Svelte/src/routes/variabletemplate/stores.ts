import type { VariableTemplateModel } from "$lib/components/datastructure/types";
import { writable } from "svelte/store";

export const variableTemplatesStore = writable<VariableTemplateModel[]>([]);