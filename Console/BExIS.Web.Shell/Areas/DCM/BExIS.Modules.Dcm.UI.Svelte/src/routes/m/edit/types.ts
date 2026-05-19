export interface SystemMappingEditModel {
 partyMappings: PartyMappingModel[];
 keyMappings: KeyMappingModel[];
}

export interface PartyMappingModel {
 path: string;
 parentPath: string;
 linkElementId: number;
 selector: boolean;
 complexity:boolean;
 list:PartyMappingResultElementModel[];
} 

export interface KeyMappingModel {
 path: string;
 systemKeyName: string;
}

export interface PartyMappingResultElementModel {
 value: string;
 partyId: number;
}