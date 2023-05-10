export interface ViewModel {
 id: number;
 versionId: number;
 version: number;
 title: string;
 hooks: Hook[];
}

export interface Hook {
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