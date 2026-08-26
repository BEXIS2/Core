import type { listItemType } from "@bexis2/bexis2-core-ui";
import type { HookModel } from "../edit/types";
import type { date } from "vest/enforce/date";

export interface ViewModel extends ApiDatasetModel {
    settings: ViewSettings;
    entityName: string;
    hasData: boolean;
    count: number;
    isValid: boolean;
    downloadAccess: boolean;
    requestExist: boolean;
    requestAble: boolean;
    hasRequestRight: boolean;
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

export interface versionListItemType extends listItemType {
	date: string;
	tagNr: number;
	changeDescription: string;
}

export interface TagInfoViewModel {
	version: number;
	releaseNotes: string[];
	releaseDate: Date;
}

export interface DeletedModel{
   id: number;
   title: string;
   links: LinksOverview;
}