// Curation Status (for full curation not for entries)
export enum CurationStatus {
	Check = 0,
	Back = 1,
	Changes = 2,
	Finished = 3
}

export type CurationLabelModel = {
	name: string;
	color: string;
};

export function noteCommentToLabel(noteComment: string) {
	return {
		name: /^\S*\s/.exec(noteComment)?.toString().trim(),
		color: RegExp(/\s#[0-9a-fA-F]+$/)
			.exec(noteComment)
			?.toString()
			.slice(1, 8)
	} as CurationLabelModel;
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
	entryTemplateMD: string | undefined;
};

export enum CurationStatusEntryTab {
	Greeting,
	Tasks,
	Hide
}
