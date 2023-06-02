export interface EditModel {
 id: number;
 versionId: number;
 version: number;
 title: string;
 hooks: HookModel[];
 views: ViewModel[];
}

export function isEditModel(model: any): model is EditModel {
 return 'id' in model;
}

export interface HookModel {
 name: string;
 displayName: string;
 status: HookStatus;
 mode: HookMode;
 entity: string;
 module: string;
 place: string;
 start: string;
 description: string;
}

export type ViewModel = HookModel

export enum HookStatus {
 Disabled = 0,
 AccessDenied = 1,
 Open = 2,
 Ready = 3,
 Exist = 4,
 Inactive = 5
}

export enum HookMode {
 view = 0,
 edit = 1
}