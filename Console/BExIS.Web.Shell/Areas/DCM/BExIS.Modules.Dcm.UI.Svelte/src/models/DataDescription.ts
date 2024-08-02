import type { fileInfoType } from '@bexis2/bexis2-core-ui';

export interface DataDescriptionModel {
	id: number;
	structureId: number;
	title: string;
	description: string;
	lastModification: string | null;
	variables: VariableModel[];
	allFilesReadable: boolean;
	fileReaderExist: boolean;
	readableFiles: fileInfoType[];
	isRestricted: boolean;
	isStructured: boolean;
	hasData: boolean;
	enableEdit: boolean;
}

export interface VariableModel {
	id: number;
	name: string;
	unit: string;
	dataType: string;
	isKey: boolean;
}
