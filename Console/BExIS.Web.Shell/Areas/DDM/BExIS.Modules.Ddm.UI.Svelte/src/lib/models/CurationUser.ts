export interface CurationUserModel {
	id: number;
	displayName: string;
	curationUserType: CurationUserType;
}

export enum CurationUserType {
	User,
	Curator
}

export const fixedCurationUserId = 1; // This is set by the backend when requesting the curation entries

export class CurationUserClass implements CurationUserModel {
	public readonly id: number;
	public readonly displayName: string;
	public readonly curationUserType: CurationUserType;

	constructor(curationUser: CurationUserModel) {
		this.id = curationUser.id || 0;
		this.displayName = curationUser.displayName || 'None';
		this.curationUserType = curationUser.curationUserType || CurationUserType.User;
	}
}
