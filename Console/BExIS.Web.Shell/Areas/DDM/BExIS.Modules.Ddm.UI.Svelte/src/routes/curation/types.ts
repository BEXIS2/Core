import {
	faCircleCheck,
	faCircleDot,
	faCircleExclamation,
	faCirclePause
} from '@fortawesome/free-solid-svg-icons';
import type { CurationEntryClass } from './CurationEntries';

export interface CurationModel {
	datasetId: number;
	datasetTitle: string;
	datasetVersionDate: string;
	curationEntries: CurationEntryModel[];
	curationUsers: CurationUserModel[];
	curationLabels: CurationLabel[];
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
	None = 0,
	StatusEntryItem = 1,
	MetadataEntryItem = 2,
	PrimaryDataEntryItem = 3,
	DatastructureEntryItem = 4
}

export const CurationEntryTypeNames: string[] = [
	'Hidden',
	'Status',
	'Metadata',
	'Primary Data',
	'Datastructure'
];

export enum CurationUserType {
	User,
	Curator
}

export enum CurationEntryStatus {
	Open = 0,
	Fixed = 1,
	Ok = 2,
	Closed = 3
}

export const CurationEntryStatusDetails = [
	{
		status: CurationEntryStatus.Open,
		name: 'Open',
		icon: faCircleExclamation
	},
	{
		status: CurationEntryStatus.Fixed,
		name: 'Changed',
		icon: faCircleDot
	},
	{
		status: CurationEntryStatus.Ok,
		name: 'Paused',
		icon: faCirclePause
	},
	{
		status: CurationEntryStatus.Closed,
		name: 'Approved',
		icon: faCircleCheck
	}
];

export const CurationEntryStatusColorPalettes = [
	{
		name: 'Default',
		colors: [
			'hsl(330deg 100% 30%)',
			'hsl(150deg 100% 35%)',
			'hsl(150deg 100% 25%)',
			'hsl(150deg 100% 15%)'
		]
	},
	{
		name: 'Gray',
		colors: ['#555555', '#888888', '#aaaaaa', '#cccccc']
	},
	{
		name: 'Colorful',
		colors: ['#D55E00', '#56B4E9', '#CC79A7', '#004D40']
	}
];

export type FilterModel<TData> = {
	id: string;
	data: TData;
	fn: (entry: CurationEntryClass, data: TData) => boolean;
	isClearedFn: (data: TData) => boolean;
};

export type CurationLabel = {
	name: string;
	color: string;
};
