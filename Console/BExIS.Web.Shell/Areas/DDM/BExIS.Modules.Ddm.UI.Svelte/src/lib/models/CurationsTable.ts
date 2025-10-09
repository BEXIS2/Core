import type { CurationLabelModel } from './CurationStatusEntry';

export interface CurationsOverviewModel {
	datasets: CurationDetailModel[];
	curationLabels: CurationLabelModel[];
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
