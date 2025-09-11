import type { ListItem } from './Models';

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
	hasExtension: boolean;
	extentionsList: number[];
	allowedFileTypes: string[];
	disabledHooks: string[];
	notificationGroups: number[];
	permissionGroups: permissionsType;
	linkedSubjects: ListItem[];
	inUse: boolean;
	activated: boolean;
}

export interface permissionsType {
	full: number[];
	viewEditGrant: number[];
	viewEdit: number[];
	view: number[];
}
