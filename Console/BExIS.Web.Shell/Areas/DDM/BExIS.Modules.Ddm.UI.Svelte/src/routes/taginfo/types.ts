export interface TagInfoEditModel {
	versionId: number;
	versionNr: number;
	tagId: number;
	tagNr: number;
	show: boolean;
	publish: boolean;
	releaseNote: string;
	releaseDate: string;
	systemDescription: string;
	systemAuthor: string;
	systemDate: string;
	link: string;
}

export interface TagInfoViewModel {
	version: number;
	releaseNotes: string[];
	releaseDate: string;
}

export enum TagType {
	None = 0,
	Major = 1,
	Minor = 2,
	Copy = 3
}
