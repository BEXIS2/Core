import type { ListItem } from "./Models";


export interface EntityTemplateModel {
 id: number;
 name: string;
 description: string;
 entityType: ListItem;
 metadataStructure: ListItem;
 metadataFields: number[];
 metadataInvalidSaveMode: boolean;
 hasDatastructure: boolean;
 datastructureList: number[];
 allowedFileTypes: string[];
 disabledHooks: string[];
 notificationGroups: number[];
 permissionGroups: number[];
}
