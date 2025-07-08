import {
	faCircleCheck,
	faCircleDot,
	faCircleExclamation,
	faCirclePause
} from '@fortawesome/free-solid-svg-icons';
import type { CurationEntryClass } from './CurationEntries';

export interface CurationsOverviewModel {
	datasets: CurationDetailModel[];
	curationLabels: CurationLabel[];
}

export interface CurationDetailModel {
	datasetId: number;
	datasetName: string;
	notesComments: string[];
	curationStarted: boolean;
	userIsDone: boolean;
	isApproved: boolean;
	lastChangeDatetime_Curator: string;
	lastChangeDatetime_User: string;
	count_UserIsDone_True_IsApproved_True: number;
	count_UserIsDone_True_IsApproved_False: number;
	count_UserIsDone_False_IsApproved_True: number;
	count_UserIsDone_False_IsApproved_False: number;
}

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
	userIsDone: boolean;
	isApproved: boolean;
	lastChangeDatetime_User: string;
	lastChangeDatetime_Curator: string;
}

export interface CurationEntryHelperModel extends CurationEntryModel {
	isDraft: boolean;
	status: CurationEntryStatus;
	statusName: string;
	statusColor: string;
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

export const CurationEntryTypeViewOrders = {
	default: [
		CurationEntryType.MetadataEntryItem,
		CurationEntryType.DatastructureEntryItem,
		CurationEntryType.PrimaryDataEntryItem
	],
	editMode: [
		CurationEntryType.MetadataEntryItem,
		CurationEntryType.DatastructureEntryItem,
		CurationEntryType.PrimaryDataEntryItem,
		CurationEntryType.None
	]
};

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

export enum CurationFilterType {
	status,
	type,
	search
}

export type CurationFilterModel<TData> = {
	type: CurationFilterType;
	data: TData;
	fn: (entry: CurationEntryClass, data: TData) => boolean;
	isClearedFn: (data: TData) => boolean;
};

export type CurationLabel = {
	name: string;
	color: string;
};

// Curation Status (for full curation not for entries)
export enum CurationStatus {
	Check = 0,
	Back = 1,
	Changes = 2,
	Finished = 3
}

export const CurationStatusLabels = [
	{ name: 'check', bgColor: '#D55E00', fontColor: 'white' },
	{ name: 'back-to-author', bgColor: '#56B4E9', fontColor: 'white' },
	{ name: 'changes', bgColor: '#CC79A7', fontColor: 'white' },
	{ name: 'finished', bgColor: '#004D40', fontColor: 'white' }
];

export function getCurationStatusFromBoolean(
	userIsDone: boolean,
	isApproved: boolean
): CurationStatus {
	if (userIsDone && isApproved) return 3;
	if (userIsDone && !isApproved) return 2;
	if (!userIsDone && isApproved) return 1;
	return 0;
}

export function getBooleanFromCurationStatus(statusIndex: CurationStatus) {
	if (statusIndex === 0) {
		return { userIsDone: false, isApproved: false };
	} else if (statusIndex === 1) {
		return { userIsDone: false, isApproved: true };
	} else if (statusIndex === 2) {
		return { userIsDone: true, isApproved: false };
	} else if (statusIndex === 3) {
		return { userIsDone: true, isApproved: true };
	}
}

export type taskLine = {
	id: string;
	fullString: string;
	text: string;
	indentation: number;
	isListItem: boolean;
	isBold: boolean;
	isCheckbox: boolean;
	isChecked: boolean | undefined;
	linkString: string | undefined;
};

export enum helpType {
	empty = 0,
	mainResearcher = 1,
	mainCurator = 2,
	tasks = 3
}

export enum CurationStatusEntryTab {
	Introduction,
	Tasks,
	Hide
}
