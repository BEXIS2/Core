import { fileUploaderType, asciiFileReaderInfoType } from '@bexis2/bexis2-core-ui';

export interface FileUploadModel {
	fileUploader: fileUploaderType;
	allFilesReadable: boolean;
	hasStructure: boolean;
	asciiFileReaderInfo: asciiFileReaderInfoType;
	lastModification: string | null;
}
