import {
	CurationEntryStatus,
	CurationEntryStatusDetails,
	CurationEntryType,
	CurationEntryTypeNames,
	CurationUserType,
	type CurationEntryModel,
	type CurationLabel,
	type CurationModel,
	type CurationNoteModel,
	type CurationUserModel
} from './types';

export const fixedCurationUserId = 1; // This is set by the backend when requesting the curation entries

export class CurationClass {
	// Properties from CurationModel
	public readonly datasetId: number;
	public readonly datasetTitle: string;
	public readonly datasetVersionDate: string;
	public readonly curationEntries: CurationEntryClass[];
	public readonly curationStatusEntry: CurationEntryClass | null = null;
	public readonly curationUsers: CurationUserClass[];
	public readonly curationLabels: CurationLabel[];

	// Additional properties
	public readonly userMap: Map<number, CurationUserModel>;

	public readonly datasetVersionDateObj: Date;
	public readonly creationDate: Date;
	public readonly lastUserChangedDate: Date;
	public readonly lastCuratorChangedDate: Date;

	public readonly curationProgressTotal: number[];
	public readonly curationProgressPerType: number[][];

	public readonly currentUserType: CurationUserType;

	public readonly draftCount: number;

	constructor(
		curation: CurationModel | CurationClass,
		doNotSort: boolean | undefined = undefined,
		changedEntryPos: CurationEntryClass | undefined = undefined
	) {
		// create user map and set current user type
		this.curationUsers = (curation.curationUsers || []).map((user) => new CurationUserClass(user));
		this.userMap = new Map<number, CurationUserClass>();
		this.curationUsers.forEach((user) => {
			this.userMap.set(user.id, user);
		});
		this.currentUserType =
			this.userMap.get(fixedCurationUserId)?.curationUserType ?? CurationUserType.User;

		// dataset information
		this.datasetId = curation.datasetId || 0;
		this.datasetTitle = curation.datasetTitle || 'None';
		this.datasetVersionDate = curation.datasetVersionDate || 'None';

		// curation entries
		const allEntries = (curation.curationEntries || []).map((entry) => {
			if (entry instanceof CurationEntryClass) return entry;
			return new CurationEntryClass(entry, this.currentUserType);
		});

		this.curationStatusEntry =
			allEntries.find((entry) => entry.type === CurationEntryType.StatusEntryItem) || null;

		if (doNotSort) this.curationEntries = allEntries;
		else this.curationEntries = CurationClass.applyPositioning(allEntries, changedEntryPos);

		this.curationLabels = curation.curationLabels || [];

		// Additional properties
		this.datasetVersionDateObj = new Date(curation.datasetVersionDate);

		[this.creationDate, this.lastUserChangedDate, this.lastCuratorChangedDate] =
			this.getCalculatedDates();

		[this.curationProgressTotal, this.curationProgressPerType] = this.getCurationProgress();

		this.draftCount = this.curationEntries.filter((entry) => entry.isDraft()).length;
	}

	public addEntry(entry: CurationEntryClass): CurationClass {
		const index = this.curationEntries.findIndex((e) => e.id === entry.id);
		const newEntries = [...this.curationEntries];

		let updatedPosition = true;

		if (index !== -1) {
			updatedPosition = newEntries[index].position !== entry.position;
			newEntries[index] = entry;
		} else {
			newEntries.push(entry);
		}

		return new CurationClass(
			{
				...this,
				curationEntries: newEntries
			},
			!updatedPosition,
			updatedPosition ? entry : undefined
		);
	}

	private getCalculatedDates(): [Date, Date, Date] {
		let creationDate = this.datasetVersionDateObj.getTime();
		let lastUserChangedDate = this.datasetVersionDateObj.getTime();
		let lastCuratorChangedDate = this.datasetVersionDateObj.getTime();

		this.curationEntries.forEach((entry) => {
			if (entry.creationDateObj.getTime() > creationDate)
				creationDate = entry.creationDateObj.getTime();
			if (entry.lastChangeDatetime_UserObj.getTime() > lastUserChangedDate)
				lastUserChangedDate = entry.lastChangeDatetime_UserObj.getTime();
			if (entry.lastChangeDatetime_CuratorObj.getTime() > lastCuratorChangedDate)
				lastCuratorChangedDate = entry.lastChangeDatetime_CuratorObj.getTime();
		});

		return [
			new Date(creationDate),
			new Date(lastUserChangedDate),
			new Date(lastCuratorChangedDate)
		];
	}

	private getCurationProgress(): [number[], number[][]] {
		const progressTotal = new Array(CurationEntryStatusDetails.length).fill(0);
		const progressPerType = new Array(CurationEntryTypeNames.length)
			.fill(0)
			.map(() => new Array(CurationEntryStatusDetails.length).fill(0));

		this.curationEntries.forEach((entry) => {
			if (!entry.isVisible()) return;
			progressTotal[entry.status]++;
			progressPerType[entry.type][entry.status]++;
		});

		return [progressTotal, progressPerType];
	}

	public getEntryById(entryId: number): CurationEntryClass | null {
		const entry = this.curationEntries.find((entry) => entry.id === entryId);
		if (!entry) return null;
		return entry;
	}

	public updateEntry(
		entryId: number,
		updates: Partial<{
			position: number;
			topic: string;
			type: CurationEntryType;
			name: string;
			description: string;
			solution: string;
			source: string;
		}>
	): CurationClass {
		const entry = this.getEntryById(entryId);
		if (!entry) return this;
		const newEntry = new CurationEntryClass(
			{
				...entry,
				...updates
			},
			this.currentUserType
		);
		return this.addEntry(newEntry);
	}

	public addEmptyEntry(
		position: number,
		name = '',
		type = CurationEntryType.None,
		description = ''
	) {
		const isStatusEntry = type === CurationEntryType.StatusEntryItem;
		const invalidPosition =
			(isStatusEntry && (position !== 0 || this.curationStatusEntry)) ||
			(!isStatusEntry && position < 1);
		if (invalidPosition) return this;
		let newEntry = CurationEntryClass.emptyEntry(
			this.datasetId,
			-this.draftCount - 1,
			position,
			name,
			type,
			description
		);
		return new CurationClass(
			{
				...this,
				curationEntries: [...this.curationEntries, newEntry]
			},
			false,
			newEntry
		);
	}

	private static sortedCurationEntries(
		curationEntries: CurationEntryClass[]
	): CurationEntryClass[] {
		return curationEntries.toSorted(
			(a, b) =>
				a.position - b.position || // most important is the position
				a.id - b.id || // if position is the same, sort by id
				(a.topic + a.name + a.description).localeCompare(b.topic + b.name + b.description) // if position and id are the same, sort by topic, name and description
		);
	}

	/**
	 *
	 * @param curationEntries the full current list of curationEntries
	 * @param entry a specific entry, that already has its new wanted position (the previous position does not matter as well as if it is inside the curationEntries or not)
	 * @returns the new sorted list of entries
	 */
	private static applyPositioning(
		curationEntries: CurationEntryClass[],
		entry: CurationEntryClass | undefined = undefined
	): CurationEntryClass[] {
		let newEntries: CurationEntryClass[] = [];
		let nextPositions = CurationEntryTypeNames.map((_) => 1);
		let addedEntry = entry?.type === CurationEntryType.StatusEntryItem; // is false if entry is not a StatusEntryItem
		const sortedCurationEntries = this.sortedCurationEntries(curationEntries);
		for (const e of sortedCurationEntries) {
			if (!addedEntry && entry && entry.position === nextPositions[entry.type]) {
				// add entry add correct position
				newEntries.push(entry);
				addedEntry = true;
				if (entry.isNoDraft()) nextPositions[entry.type]++;
			}
			if (e.type === CurationEntryType.StatusEntryItem) {
				newEntries.push(
					new CurationEntryClass(
						{
							...e,
							position: 0
						},
						e.currentUserType
					)
				);
			} else if (e.id !== entry?.id) {
				newEntries.push(
					new CurationEntryClass(
						{
							...e,
							position: nextPositions[e.type]
						},
						e.currentUserType
					)
				);
				if (!e.isDraft()) {
					// if entry is a draft the counter should not be increased because they are not in the backend
					nextPositions[e.type]++;
				}
			}
		}
		if (!addedEntry && entry) {
			// push entry to end if not prviously added
			newEntries.push(
				new CurationEntryClass(
					{
						...entry,
						position: nextPositions[entry.type]
					},
					entry.currentUserType
				)
			);
		}
		return newEntries;
	}

	/**
	 * Only for removing the entry locally. An entry can not be removed from the backend, only updated to be hidden.
	 *
	 * @param entryId - The id of the entry to be removed
	 * @returns CurationClass - A new CurationClass object with the entry removed
	 */
	public removeEntry(entryId: number): CurationClass {
		const newEntries = this.curationEntries.filter((entry) => entry.id !== entryId);
		return new CurationClass({
			...this,
			curationEntries: newEntries
		});
	}
}

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
		this.creationDate = curationNote.creationDate || 'None';
		if (escape) {
			this.comment = curationNote.comment.replace(/\\/g, '\\\\') || 'None';
		} else {
			this.comment = curationNote.comment || 'None';
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
	public readonly creationDateObj: Date;
	public readonly lastChangeDatetime_UserObj: Date;
	public readonly lastChangeDatetime_CuratorObj: Date;
	// Additional derived properties
	public readonly lastChangedDate: Date;
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
		this.topic = curationEntry.topic || 'None';
		this.type = curationEntry.type || CurationEntryType.None;
		this.datasetId = curationEntry.datasetId || 0;
		this.name = curationEntry.name || 'None';
		this.description = curationEntry.description || 'None';
		this.solution = curationEntry.solution || 'None';
		this.position = curationEntry.position || 0;
		this.source = curationEntry.source || 'None';
		this.notes = (curationEntry.notes || [])
			.map((note) => new CurationNoteClass(note, false))
			.sort((a, b) => a.creationDateObj.getTime() - b.creationDateObj.getTime());
		this.creationDate = curationEntry.creationDate || 'None';
		this.creatorId = curationEntry.creatorId || 0;
		this.userIsDone = curationEntry.userIsDone || false;
		this.isApproved = curationEntry.isApproved || false;
		this.lastChangeDatetime_User = curationEntry.lastChangeDatetime_User || 'None';
		this.lastChangeDatetime_Curator = curationEntry.lastChangeDatetime_Curator || 'None';
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
			this.creationDateObj.getTime(),
			this.lastChangeDatetime_UserObj.getTime(),
			this.lastChangeDatetime_CuratorObj.getTime(),
			...this.visibleNotes.map((note) => note.creationDateObj.getTime())
		);
		return new Date(lastChangedDate);
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
		if (status === CurationEntryStatus.Fixed) return this.setStatusBoolean(true, false);
		if (status === CurationEntryStatus.Ok) return this.setStatusBoolean(false, true);
		if (status === CurationEntryStatus.Closed) return this.setStatusBoolean(true, true);
		return this.setStatusBoolean(false, false);
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
		position: number,
		name = '',
		type = CurationEntryType.None,
		description = ''
	): CurationEntryClass {
		return new CurationEntryClass(
			{
				id: id,
				datasetId: datasetId,
				topic: '',
				type: type,
				name: name,
				description: description,
				solution: '',
				position: position,
				source: '',
				notes: [],
				creationDate: new Date().toISOString(),
				creatorId: fixedCurationUserId,
				userIsDone: false,
				isApproved: false,
				lastChangeDatetime_User: new Date().toISOString(),
				lastChangeDatetime_Curator: new Date().toISOString()
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
}

export function noteCommentToLabel(noteComment: string) {
	return {
		name: /^\S*\s/.exec(noteComment)?.toString().trim(),
		color: RegExp(/\s#[0-9a-fA-F]+$/)
			.exec(noteComment)
			?.toString()
			.slice(1, 8)
	} as CurationLabel;
}

// -------------------- Copilot with GPT-4.1 --------------------
export function getContrastColor(hex: string | undefined) {
	if (!hex) return '#000000';
	// Remove hash if present
	hex = hex.replace('#', '');
	// Expand shorthand form (e.g. "03F") to full form ("0033FF")
	if (hex.length === 3) {
		hex = hex
			.split('')
			.map((x) => x + x)
			.join('');
	}
	const r = parseInt(hex.substring(0, 2), 16);
	const g = parseInt(hex.substring(2, 4), 16);
	const b = parseInt(hex.substring(4, 6), 16);
	// Calculate luminance
	const luminance = 0.299 * r + 0.587 * g + 0.114 * b;
	return luminance > 186 ? '#000000' : '#ffffff';
}
// --------------------------------------------------------------
