export interface ViewSettingsModel {
    id: number;
    versionId: number;
    version: number;
    title: string;
    useTags: boolean;
    useMinor: boolean;
    hasData: boolean;
    dataAggrement: string;
    hooks: Hook[];
    labels: { [key: string]: string; };
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


export interface ApiDatasetModel {
	id: number;
	version: number;
	versionId: number;
	title: string;
	description: string;
	dataStructureId: number;
	metadataStructureId: number;
	isPublic: boolean;
	publicationDate: string;
	additionalInformations: { [key: string]: string; };
	parties: { [key: string]: { [key: string]: string; }; };
	versionDate: string;
	names: any;
	links: LinksOverview;
}

export interface LinksOverview {
	from: ReferenceModel[];
	to: ReferenceModel[];
}

export interface ReferenceModel {
	refId: number;
	target: ReferenceElementModel;
	source: ReferenceElementModel;
	context: string;
	referenceType: string;
	linkType: string;
	category: string;
}

export interface ReferenceElementModel {
	id: number;
	version: number;
	typeId: number;
	type: string;
	title: string;
	latestVersion: boolean;
}