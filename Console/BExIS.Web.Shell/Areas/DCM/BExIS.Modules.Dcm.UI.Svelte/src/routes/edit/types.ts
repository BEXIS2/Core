import type { HookModel } from "$models/Hook";

export interface EditModel {
	id: number;
	versionId: number;
	version: number;
	title: string;
	hooks: HookModel[];
	views: ViewModel[];
}

export function isEditModel(model: any): model is EditModel {
	return model.id !== undefined;
}

export type ViewModel = HookModel;

export interface ExtensionType{
	id: number;
	version: number;
	title: string;
};
