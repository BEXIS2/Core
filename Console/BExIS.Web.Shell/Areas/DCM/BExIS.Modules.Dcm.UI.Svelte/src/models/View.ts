
import type { HookModel } from "./Hook";

export interface ViewModel extends ApiDatasetModel {
				settings: ViewSettings;
				hasData: boolean;
				count: number;
				isValid: boolean;
				downloadAccess: boolean;
				requestExist: boolean;
				requestAble: boolean;
				hasRequestRight: boolean;
				hasEditRight: boolean;
				labels: { [key: string]: string; };
}

export interface ViewSettings {
				useTags: boolean;
				useMinor: boolean;
				dataAggrement: string;
				hooks: HookModel[];
}

export interface ApiDatasetModel {
				id: number;
				version: number;
				versionId: number;
				tag:number;
				title: string;
				description: string;
				dataStructureId: number;
				metadataStructureId: number;
				entityTemplateId: number;
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

export interface ApiDatasetAttachmentsModel {
				datasetId: number;
				attachments: ApiSimpleAttachmentModel[];
}

export interface Citator {
				firstName: string;
				lastName: string;
}

export interface ApiSimpleAttachmentModel {
				id: number;
				name: string;
				mimeType: string;
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


export enum ReadCitationFormat {
    APA,
    Text,
				Default
}

export interface CitationModel {
				format: ReadCitationFormat;
				data: CitationDataModel;
}

export enum CitationFormat {
    APA,
    RIS,
    Text,
    Bibtex
}

export interface CitationDataModel {
    title: string;
    version: string;
    tag: string;
    projects: string[];
    year: string;
    doi: string;
    url: string;
    authors: string[];
    entryType: string;
    entityName: string;
    publisher: string;
    keyword: string;
    note: string;
}
