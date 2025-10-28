import { CurationUserType } from './CurationUser';

export interface CurationNoteModel {
	id: number;
	userType: CurationUserType;
	creationDate: string;
	comment: string;
	userId: number;
}

export enum CurationNoteCommandType {
	None,
	Read,
	Unread
}

export const commandTypeMapping = {
	[CurationNoteCommandType.None]: 'none',
	[CurationNoteCommandType.Read]: 'read',
	[CurationNoteCommandType.Unread]: 'unread'
};

export class CurationNoteClass implements CurationNoteModel {
	// Properties from CurationNoteModel
	public readonly id: number;
	public readonly userType: CurationUserType;
	public readonly creationDate: string;
	public readonly comment: string;
	public readonly userId: number;

	// Additional properties
	public readonly creationDateObj: Date;
	public readonly hidden: boolean;
	public readonly readableComment: string;
	public readonly command: CurationNoteCommandType;

	constructor(curationNote: CurationNoteModel, escape: boolean = false) {
		this.id = curationNote.id || 0;
		this.userType = curationNote.userType || CurationUserType.User;
		this.creationDate = curationNote.creationDate || '';
		if (escape) {
			this.comment = curationNote.comment.replace(/\\/g, '\\\\') || '';
		} else {
			this.comment = curationNote.comment || '';
		}
		this.userId = curationNote.userId || 0;
		// derive additional properties
		this.creationDateObj = new Date(curationNote.creationDate);
		this.hidden = this.isHidden();
		this.readableComment = this.getReadableComment();
		this.command = this.getCommandType();
	}

	private isHidden(): boolean {
		if (this.comment.length > 1) {
			return this.comment.startsWith('\\') && this.comment[1] !== '\\';
		}
		return this.comment.startsWith('\\');
	}

	private getReadableComment(): string {
		return this.comment.replace(/\\/, '');
	}

	private getCommandType(): CurationNoteCommandType {
		if (this.hidden) {
			if (this.comment.startsWith(`\\${commandTypeMapping[CurationNoteCommandType.Read]}`)) {
				return CurationNoteCommandType.Read;
			} else if (
				this.comment.startsWith(`\\${commandTypeMapping[CurationNoteCommandType.Unread]}`)
			) {
				return CurationNoteCommandType.Unread;
			}
		}
		return CurationNoteCommandType.None;
	}
}
