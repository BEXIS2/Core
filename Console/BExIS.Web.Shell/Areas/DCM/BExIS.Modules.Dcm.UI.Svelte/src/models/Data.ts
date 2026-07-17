import { FileInfo } from '@bexis2/bexis2-core-ui';

export interface DataModel {
	id: number;
	version: number;
	hasStructure: boolean;
	existingFiles: FileInfo[];
	deleteFiles: FileInfo[];
	descriptionType: number;
}
