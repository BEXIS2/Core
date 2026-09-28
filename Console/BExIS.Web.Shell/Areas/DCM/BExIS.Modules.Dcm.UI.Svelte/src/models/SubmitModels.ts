import type { asciiFileReaderInfoType, fileInfoType } from '@bexis2/bexis2-core-ui';
import type { sortedError } from './Models';

export interface SubmitModel {
	deletedFiles: fileInfoType[];
	id: number;
	title: string;
	isDataValid: boolean;
	allFilesReadable: boolean;
	files: fileInfoType[];
	deleteFiles: fileInfoType[];
	modifiedFiles: fileInfoType[];
	asciiFileReaderInfo: asciiFileReaderInfoType;
	hasStructrue: boolean;
	structureId: number;
	structureTitle: string;
}

export interface submitResponceType {
	success: boolean;
	asyncUpload: boolean;
	asyncUploadMessage: string;
	errors: sortedError[];
}
