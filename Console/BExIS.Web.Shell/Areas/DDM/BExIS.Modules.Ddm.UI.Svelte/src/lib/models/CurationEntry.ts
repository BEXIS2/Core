import {
	faCircleCheck,
	faCircleDot,
	faCircleExclamation,
	faCirclePause
} from '@fortawesome/free-solid-svg-icons';
import { CurationUserType, fixedCurationUserId } from './CurationUser';
import {
	commandTypeMapping,
	CurationNoteClass,
	CurationNoteCommandType,
	type CurationNoteModel
} from './CurationNote';

export interface CurationEntryModel {
	id: number;
	topic: string;
	type: CurationEntryType;
	datasetId: number;
	name: string;
	description: string;
	solution: string;
	position: number;
	source: string;
	notes: CurationNoteModel[];
	creationDate: string;
	creatorId: number;
	userIsDone: boolean;
	isApproved: boolean;
	lastChangeDatetime_User: string;
	lastChangeDatetime_Curator: string;
}

export interface CurationEntryCreationModel {
	topic: string;
	type: CurationEntryType;
	name: string;
	description: string;
	solution: string;
	position: number;
	source: string;
	comment: string;
	status: CurationEntryStatus;
}

export interface CurationEntryHelperModel extends CurationEntryModel {
	isDraft: boolean;
	status: CurationEntryStatus;
	statusName: string;
	statusColor: string;
}

export enum CurationEntryType {
	None = 0,
	StatusEntryItem = 1,
	GeneralEntryItem = 2,
	MetadataEntryItem = 3,
	PrimaryDataEntryItem = 4,
	DatastructureEntryItem = 5,
	LinkEntryItem = 6,
	AttachmentEntryItem = 7
}

export const CurationEntryTypeNames: string[] = [
	'Hidden',
	'Status',
	'General',
	'Metadata',
	'Primary Data',
	'Datastructure',
	'Links',
	'Attachments'
];

export enum CurationEntryStatus {
	Open = 0,
	Fixed = 1,
	Ok = 2,
	Closed = 3
}

export const CurationEntryStatusDetails = [
	{
		status: CurationEntryStatus.Open,
		name: 'Open',
		icon: faCircleExclamation
	},
	{
		status: CurationEntryStatus.Fixed,
		name: 'Changed',
		icon: faCircleDot
	},
	{
		status: CurationEntryStatus.Ok,
		name: 'Paused',
		icon: faCirclePause
	},
	{
		status: CurationEntryStatus.Closed,
		name: 'Approved',
		icon: faCircleCheck
	}
];

export const CurationEntryStatusColorPalettes = [
	{
		name: 'Default',
		colors: [
			'hsl(330deg 100% 30%)',
			'hsl(150deg 100% 35%)',
			'hsl(150deg 100% 25%)',
			'hsl(150deg 100% 15%)'
		]
	},
	{
		name: 'Gray',
		colors: ['#555555', '#888888', '#aaaaaa', '#cccccc']
	},
	{
		name: 'Colorful',
		colors: ['#D55E00', '#56B4E9', '#CC79A7', '#004D40']
	}
];

export const DefaultCurationEntryCreationModel: CurationEntryCreationModel = {
	topic: '',
	type: CurationEntryType.None,
	name: '',
	description: '',
	solution: '',
	position: 1,
	source: '',
	comment: '',
	status: CurationEntryStatus.Open
};

export class CurationEntryClass implements CurationEntryModel {
	// Properties from CurationEntryModel
	public readonly id: number;
	public readonly topic: string;
	public readonly type: CurationEntryType;
	public readonly datasetId: number;
	public readonly name: string;
	public readonly description: string;
	public readonly solution: string;
	public readonly position: number;
	public readonly source: string;
	public readonly notes: CurationNoteClass[];
	public readonly creationDate: string;
	public readonly creatorId: number;
	public readonly userIsDone: boolean;
	public readonly isApproved: boolean;
	public readonly lastChangeDatetime_User: string;
	public readonly lastChangeDatetime_Curator: string;
	// Converted Date Object properties
	public readonly creationDateObj: Date | undefined;
	public readonly lastChangeDatetime_UserObj: Date | undefined;
	public readonly lastChangeDatetime_CuratorObj: Date | undefined;
	// Additional derived properties
	public readonly lastChangedDate: Date | undefined;
	public readonly visibleNotes: CurationNoteClass[];
	public readonly noteUsers: Set<number>;
	public readonly hasUnreadNotes: boolean;
	public readonly status: CurationEntryStatus;
	public readonly currentUserType: CurationUserType;

	constructor(
		curationEntry: CurationEntryModel | CurationEntryClass,
		currentUserType: CurationUserType
	) {
		this.id = curationEntry.id || 0;
		this.topic = curationEntry.topic || '';
		this.type = curationEntry.type || CurationEntryType.None;
		this.datasetId = curationEntry.datasetId || 0;
		this.name = curationEntry.name || '';
		this.description = curationEntry.description || '';
		this.solution = curationEntry.solution || '';
		this.position = curationEntry.position || 0;
		this.source = curationEntry.source || '';
		this.notes = (curationEntry.notes || [])
			.map((note) => new CurationNoteClass(note, false))
			.sort((a, b) => a.creationDateObj.getTime() - b.creationDateObj.getTime());
		this.creationDate = curationEntry.creationDate || '';
		this.creatorId = curationEntry.creatorId || 0;
		this.userIsDone = curationEntry.userIsDone || false;
		this.isApproved = curationEntry.isApproved || false;
		this.lastChangeDatetime_User = curationEntry.lastChangeDatetime_User || '';
		this.lastChangeDatetime_Curator = curationEntry.lastChangeDatetime_Curator || '';
		// Converted Date Object properties
		this.creationDateObj = new Date(curationEntry.creationDate);
		this.lastChangeDatetime_UserObj = new Date(curationEntry.lastChangeDatetime_User);
		this.lastChangeDatetime_CuratorObj = new Date(curationEntry.lastChangeDatetime_Curator);
		// Additional derived properties
		this.visibleNotes = this.notes.filter((note) => !note.hidden);
		this.lastChangedDate = this.getCalculatedLastChangedDate();
		this.noteUsers = new Set(this.visibleNotes.map((note) => note.userId));
		this.hasUnreadNotes = this.getHasUnreadNotes();
		this.status = this._getStatus();
		this.currentUserType = currentUserType;
	}

	private getHasUnreadNotes(): boolean {
		for (let i = this.notes.length - 1; i >= 0; i--) {
			const note = this.notes[i];
			if (note.userId !== fixedCurationUserId) {
				if (!note.hidden) return true;
			} else if (!note.hidden) {
				return false;
			} else {
				if (note.command === CurationNoteCommandType.Read) return false;
				if (note.command === CurationNoteCommandType.Unread) return true;
			}
		}
		return false;
	}

	private getCalculatedLastChangedDate() {
		let lastChangedDate = Math.max(
			this.creationDateObj?.getTime() ?? 0,
			this.lastChangeDatetime_UserObj?.getTime() ?? 0,
			this.lastChangeDatetime_CuratorObj?.getTime() ?? 0,
			...this.visibleNotes.map((note) => note.creationDateObj?.getTime() ?? 0)
		);
		return lastChangedDate ? new Date(lastChangedDate) : undefined;
	}

	/**
	 * Adds the note to the entry and returns a new CurationEntryClass object. This behavior should be necessary to update stores
	 *
	 * @param noteId
	 * @param comment
	 * @returns CurationEntryClass
	 * @throws Error if the comment is empty
	 */
	public updateNote(noteId: number, comment: string, escape: boolean = true): CurationEntryClass {
		if (!comment.trim().length) {
			console.warn('Comment cannot be empty');
			return this;
		}
		if (this.type === CurationEntryType.StatusEntryItem) {
			// note is a label and should be checked
			if (!/^\S*\s#[0-9a-fA-F]+$/.test(comment)) {
				console.warn(
					'Note for MetadataEntryItem should correspond to the following regex: /^\\S*\\s#[0-9a-fA-F]+$/'
				);
				return this;
			}
			if (
				this.visibleNotes
					.map((n) => /^\S*\s/.exec(n.comment)?.toString().trim())
					.includes(/^\S*\s/.exec(comment)?.toString().trim())
			) {
				console.warn('A note that contains this label already exists on this MetadataEntryItem.');
				return this;
			}
		}
		const note = new CurationNoteClass(
			{
				id: noteId,
				userType: this.currentUserType,
				creationDate: new Date().toISOString(),
				comment: comment,
				userId: fixedCurationUserId
			},
			escape
		);
		let newNotes = this.notes.filter((note) => note.id !== noteId);
		// remove the last note of the current user if it is a read / unread command
		let myLastNoteIndex = newNotes.findLastIndex((note) => note.userId === fixedCurationUserId);
		if (
			myLastNoteIndex !== -1 &&
			newNotes[myLastNoteIndex].hidden &&
			(newNotes[myLastNoteIndex].command === CurationNoteCommandType.Read ||
				newNotes[myLastNoteIndex].command === CurationNoteCommandType.Unread)
		) {
			newNotes = newNotes.filter((note) => note.id !== newNotes[myLastNoteIndex].id);
		}
		newNotes.push(note);
		if (this.currentUserType === CurationUserType.Curator) {
			return new CurationEntryClass(
				{
					...this,
					notes: newNotes,
					lastChangeDatetime_Curator: note.creationDate
				},
				this.currentUserType
			);
		}
		return new CurationEntryClass(
			{
				...this,
				notes: newNotes,
				lastChangeDatetime_User: note.creationDate
			},
			this.currentUserType
		);
	}

	public clearNotes(): CurationEntryClass {
		return new CurationEntryClass(
			{
				...this,
				notes: []
			},
			this.currentUserType
		);
	}

	public addNote(comment: string, escape: boolean = true): CurationEntryClass {
		return this.updateNote(0, comment, escape);
	}

	public deleteNote(noteId: number): CurationEntryClass {
		const newNotes = this.notes.filter((note) => note.id !== noteId);
		return new CurationEntryClass({ ...this, notes: newNotes }, this.currentUserType);
	}

	public deleteNotes(noteIds: number[]): CurationEntryClass {
		const newNotes = this.notes.filter((note) => !noteIds.includes(note.id));
		return new CurationEntryClass({ ...this, notes: newNotes }, this.currentUserType);
	}

	public setUnread(unread: boolean = true): CurationEntryClass {
		if (unread === this.hasUnreadNotes) return this;

		const command = unread ? CurationNoteCommandType.Unread : CurationNoteCommandType.Read;
		const oppositeCommand = unread ? CurationNoteCommandType.Read : CurationNoteCommandType.Unread;
		const commandComment = `\\${commandTypeMapping[command]}`;

		for (let i = this.notes.length - 1; i >= 0; i--) {
			const note = this.notes[i];
			if (note.hidden) {
				if (note.userId === fixedCurationUserId && note.command === oppositeCommand) {
					return this.updateNote(note.id, commandComment, false);
				}
			} else if (note.userId === fixedCurationUserId && unread) {
				break;
			} else if (note.userId !== fixedCurationUserId && !unread) {
				break;
			}
		}
		return this.addNote(commandComment, false);
	}

	public static getStatus(userIsDone: boolean, isApproved: boolean): CurationEntryStatus {
		if (userIsDone && !isApproved) return CurationEntryStatus.Fixed;
		if (!userIsDone && isApproved) return CurationEntryStatus.Ok;
		if (userIsDone && isApproved) return CurationEntryStatus.Closed;
		return CurationEntryStatus.Open;
	}

	public static getStatusBoolean(status: CurationEntryStatus): {
		userIsDone: boolean;
		isApproved: boolean;
	} {
		if (status === CurationEntryStatus.Fixed) return { userIsDone: true, isApproved: false };
		if (status === CurationEntryStatus.Ok) return { userIsDone: false, isApproved: true };
		if (status === CurationEntryStatus.Closed) return { userIsDone: true, isApproved: true };
		return { userIsDone: false, isApproved: false };
	}

	private _getStatus(): CurationEntryStatus {
		return CurationEntryClass.getStatus(this.userIsDone, this.isApproved);
	}

	public setStatusBoolean(newuserIsDone: boolean, newIsApproved: boolean): CurationEntryClass {
		return new CurationEntryClass(
			{
				...this,
				userIsDone: newuserIsDone,
				isApproved: newIsApproved
			},
			this.currentUserType
		);
	}

	public setStatus(status: CurationEntryStatus): CurationEntryClass {
		const { userIsDone, isApproved } = CurationEntryClass.getStatusBoolean(status);
		return this.setStatusBoolean(userIsDone, isApproved);
	}

	public setName(name: string): CurationEntryClass {
		if (this.name === name) return this;
		return new CurationEntryClass(
			{
				...this,
				name: name
			},
			this.currentUserType
		);
	}

	public setTopic(topic: string): CurationEntryClass {
		if (this.topic === topic) return this;
		return new CurationEntryClass(
			{
				...this,
				topic: topic
			},
			this.currentUserType
		);
	}

	public setDescription(description: string): CurationEntryClass {
		if (this.description === description) return this;
		return new CurationEntryClass(
			{
				...this,
				description: description
			},
			this.currentUserType
		);
	}

	public setPosition(position: number): CurationEntryClass {
		if (position < 1) return this;
		if (this.position === position) return this;

		return new CurationEntryClass(
			{
				...this,
				position: position
			},
			this.currentUserType
		);
	}

	public getNextStatus(): CurationEntryStatus {
		if (this.currentUserType === CurationUserType.User)
			return CurationEntryClass.getStatus(!this.userIsDone, false);
		if (this.currentUserType === CurationUserType.Curator) {
			if (this.status === CurationEntryStatus.Ok) return CurationEntryStatus.Closed;
			if (this.status === CurationEntryStatus.Closed) return CurationEntryStatus.Open;
			if (this.status === CurationEntryStatus.Fixed) return CurationEntryStatus.Ok;
			if (this.status === CurationEntryStatus.Open) return CurationEntryStatus.Ok;
		}
		return this.status;
	}

	public toggleStatus(): CurationEntryClass {
		return this.setStatus(this.getNextStatus());
	}

	public static emptyEntry(
		datasetId: number,
		id: number,
		entryModel: CurationEntryCreationModel
	): CurationEntryClass {
		let notes: CurationNoteModel[] = [];
		if (entryModel.comment && entryModel.comment.trim().length > 0) {
			notes.push({
				id: 0,
				comment: entryModel.comment,
				userType: CurationUserType.Curator,
				userId: 0,
				creationDate: ''
			});
		}
		return new CurationEntryClass(
			{
				...entryModel,
				id,
				datasetId,
				notes,
				creationDate: '',
				creatorId: fixedCurationUserId,
				userIsDone: false,
				isApproved: false,
				lastChangeDatetime_User: '',
				lastChangeDatetime_Curator: ''
			},
			CurationUserType.User
		);
	}

	public isHidden(): boolean {
		return this.type === CurationEntryType.None;
	}

	public isDraft(): boolean {
		return this.id < 0;
	}

	public isNoDraft() {
		return !this.isDraft();
	}

	public isVisible(): boolean {
		return !this.isHidden() && !this.isDraft() && this.type !== CurationEntryType.StatusEntryItem;
	}

	public isEditable(): boolean {
		return this.type !== CurationEntryType.StatusEntryItem;
	}

	public getHelperModel(
		currentColorPalette: { name: string; colors: string[] } | undefined = undefined
	): CurationEntryHelperModel {
		const m = this as CurationEntryModel;
		return {
			...m,
			isDraft: this.isDraft(),
			status: this.status,
			statusName: CurationEntryStatusDetails[this.status].name,
			statusColor: currentColorPalette
				? currentColorPalette.colors[this.status]
				: CurationEntryStatusColorPalettes[0].colors[this.status]
		};
	}
}
