import { writable } from "svelte/store";
import type { MeaningModel } from "./types";

export const meaningsStore = writable<MeaningModel[]>([]);