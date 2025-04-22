import { asciiFileReaderInfoType, fileInfoType } from '@bexis2/bexis2-core-ui';
import { sortedError } from './Models';

export interface SubmitModel {
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
