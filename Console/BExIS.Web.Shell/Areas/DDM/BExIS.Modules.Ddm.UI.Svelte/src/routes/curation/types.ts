export interface CurationModel {
	datasetId: number;
	datasetTitle: string;
	datasetVersionDate: string;
	curationEntries: CurationEntryModel[];
	curationUsers: CurationUserModel[];
}

export interface CurationUserModel {
	id: number;
	displayName: string;
	curationUserType: CurationUserType;
}

export interface CurationEntryModel {
	id: number;
	topic: string;
	type: CurationEntryType;
	datasetId: number;
	name: string;
	description: string;
	solution: string;
	position: number;
	source: string;
	notes: CurationNoteModel[];
	creationDate: string;
	creatorId: number;
	userlsDone: boolean;
	isApproved: boolean;
	lastChangeDatetime_User: string;
	lastChangeDatetime_Curator: string;
}

export interface CurationNoteModel {
	id: number;
	userType: CurationUserType;
	creationDate: string;
	comment: string;
	userId: number;
}

export enum CurationEntryType {
	None,
	StatusEntryItem,
	MetadataEntryItem,
	PrimaryDataEntryItem,
	DatastrutcureEntryItem
}

export const CurationEntryTypeNames: string[] = [
	'None',
	'Status Entry Item',
	'Metadata Entry Item',
	'Primary Data Entry Item',
	'Datastructure Entry Item'
];

export enum CurationUserType {
	User,
	Curator
}

export enum CurationEntryStatus {
	Open,
	Fixed,
	Ok,
	Closed
}

export const CurationEntryStatusNames: string[] = ['Open', 'Fixed', 'Ok', 'Closed'];

export const CurationEntryStatusColors: string[] = ['#D55E00', '#56B4E9', '#CC79A7', '#004D40'];
