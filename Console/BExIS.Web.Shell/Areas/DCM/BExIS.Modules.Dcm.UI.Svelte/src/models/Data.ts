import { FileInfo } from '@bexis2/bexis2-core-ui';

export interface DataModel {
	id: number;
	version: number;
	hasStrutcure: boolean;
	existingFiles: FileInfo[];
	deleteFiles: FileInfo[];
	descriptionType: number;
}
