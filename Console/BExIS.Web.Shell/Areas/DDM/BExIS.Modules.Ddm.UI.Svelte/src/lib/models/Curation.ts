import {
	CurationEntryClass,
	CurationEntryStatusDetails,
	CurationEntryType,
	CurationEntryTypeNames,
	type CurationEntryCreationModel,
	type CurationEntryModel
} from './CurationEntry';
import type { CurationLabelModel } from './CurationStatusEntry';
import {
	CurationUserClass,
	CurationUserType,
	fixedCurationUserId,
	type CurationUserModel
} from './CurationUser';

export interface CurationTemplateModel {
	name: string;
	content: string;
}

export interface CurationModel {
	datasetId: number;
	datasetTitle: string;
	datasetVersionDate: string;
	curationEntryTypes: CurationEntryType[];
	curationEntries: CurationEntryModel[];
	curationUsers: CurationUserModel[];
	curationLabels: CurationLabelModel[];
	greetingTemplates: CurationTemplateModel[];
	taskListTemplates: CurationTemplateModel[];
}

export enum CurationFilterType {
	status,
	type,
	search
}

export type CurationFilterModel<TData> = {
	type: CurationFilterType;
	data: TData;
	fn: (entry: CurationEntryClass, data: TData) => boolean;
	isClearedFn: (data: TData) => boolean;
};

export class CurationClass implements CurationModel {
	// Properties from CurationModel
	public readonly datasetId: number;
	public readonly datasetTitle: string;
	public readonly datasetVersionDate: string;
	public readonly curationEntryTypes: CurationEntryType[];
	public readonly curationEntries: CurationEntryClass[];
	public readonly curationStatusEntry: CurationEntryClass | null = null;
	public readonly curationUsers: CurationUserClass[];
	public readonly curationLabels: CurationLabelModel[];
	public readonly greetingTemplates: CurationTemplateModel[];
	public readonly taskListTemplates: CurationTemplateModel[];

	// Additional properties
	public readonly userMap: Map<number, CurationUserModel>;

	public readonly datasetVersionDateObj: Date | undefined;
	public readonly creationDate: Date | undefined;
	public readonly lastUserChangedDate: Date | undefined;
	public readonly lastCuratorChangedDate: Date | undefined;

	public readonly curationProgressTotal: number[];
	public readonly curationProgressPerType: number[][];

	public readonly currentUserType: CurationUserType;
	public readonly isCurator: boolean;

	public readonly draftCount: number;

	public readonly highestPositionPerType: number[];

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
		this.isCurator = this.currentUserType === CurationUserType.Curator;

		// dataset information
		this.datasetId = curation.datasetId || 0;
		this.datasetTitle = curation.datasetTitle || '';
		this.datasetVersionDate = curation.datasetVersionDate || '';

		this.greetingTemplates = curation.greetingTemplates || [];
		this.taskListTemplates = curation.taskListTemplates || [];

		const removeTypes = new Set<CurationEntryType>([
			CurationEntryType.None,
			CurationEntryType.StatusEntryItem
		]);
		this.curationEntryTypes = (curation.curationEntryTypes || []).filter(
			(type) => !removeTypes.has(type)
		);

		// curation entries
		const allEntries = (curation.curationEntries || []).map((entry) => {
			if (entry instanceof CurationEntryClass) return entry;
			return new CurationEntryClass(entry, this.currentUserType);
		});

		this.curationStatusEntry =
			allEntries.find((entry) => entry.type === CurationEntryType.StatusEntryItem) || null;

		if (doNotSort) this.curationEntries = allEntries;
		else this.curationEntries = CurationClass.applyPositioning(allEntries, changedEntryPos);

		this.highestPositionPerType = CurationEntryTypeNames.map((_) => 0);
		this.curationEntries.forEach((entry) => {
			if (entry.type !== CurationEntryType.StatusEntryItem) {
				this.highestPositionPerType[entry.type] = Math.max(
					this.highestPositionPerType[entry.type],
					entry.position
				);
			}
		});

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
			updatedPosition =
				newEntries[index].position !== entry.position || newEntries[index].type !== entry.type;
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

	private getCalculatedDates(): [Date | undefined, Date | undefined, Date | undefined] {
		let creationDate: number | undefined = undefined;
		let lastUserChangedDate: number | undefined = undefined;
		let lastCuratorChangedDate: number | undefined = undefined;

		this.curationEntries.forEach((entry) => {
			if (
				!creationDate ||
				(entry.creationDateObj && entry.creationDateObj.getTime() > creationDate)
			)
				creationDate = entry.creationDateObj?.getTime();
			if (
				!lastUserChangedDate ||
				(entry.lastChangeDatetime_UserObj &&
					entry.lastChangeDatetime_UserObj.getTime() > lastUserChangedDate)
			)
				lastUserChangedDate = entry.lastChangeDatetime_UserObj?.getTime();
			if (
				!lastCuratorChangedDate ||
				(entry.lastChangeDatetime_CuratorObj &&
					entry.lastChangeDatetime_CuratorObj.getTime() > lastCuratorChangedDate)
			)
				lastCuratorChangedDate = entry.lastChangeDatetime_CuratorObj?.getTime();
		});

		const cutoff = new Date();
		cutoff.setFullYear(cutoff.getFullYear() - 2000);
		const twoThousandYearsAgoTime = cutoff.getTime();

		return [
			// if creationDate is older than 2000 years set undefined
			!creationDate || creationDate < twoThousandYearsAgoTime ? undefined : new Date(creationDate),
			!lastUserChangedDate || lastUserChangedDate < twoThousandYearsAgoTime
				? undefined
				: new Date(lastUserChangedDate),
			!lastCuratorChangedDate || lastCuratorChangedDate < twoThousandYearsAgoTime
				? undefined
				: new Date(lastCuratorChangedDate)
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

	public updateEntry(entryId: number, updates: Partial<CurationEntryCreationModel>): CurationClass {
		const entry = this.getEntryById(entryId);
		if (!entry) return this;
		const comment = updates.comment;
		delete updates.comment;
		const { userIsDone, isApproved } = CurationEntryClass.getStatusBoolean(
			updates.status ?? entry.status
		);
		delete updates.status;
		let newEntry = new CurationEntryClass(
			{
				...entry,
				...updates,
				userIsDone: userIsDone,
				isApproved: isApproved
			},
			this.currentUserType
		);
		if (entry.isDraft()) {
			if (comment && comment.trim().length > 0) {
				newEntry = newEntry.clearNotes().addNote(comment);
			}
		}
		return this.addEntry(newEntry);
	}

	public addEntryModel(entryModel: CurationEntryCreationModel) {
		const isStatusEntry = entryModel.type === CurationEntryType.StatusEntryItem;
		const invalidPosition =
			(isStatusEntry && (entryModel.position !== 0 || this.curationStatusEntry)) ||
			(!isStatusEntry && entryModel.position < 1);
		if (invalidPosition) return { curation: this, newEntryId: 0 };
		let newEntry = CurationEntryClass.emptyEntry(this.datasetId, -this.draftCount - 1, entryModel);
		if (entryModel.status !== undefined) newEntry = newEntry.setStatus(entryModel.status);
		return {
			curation: new CurationClass(
				{
					...this,
					curationEntries: [...this.curationEntries, newEntry]
				},
				false,
				newEntry
			),
			newEntryId: newEntry.id
		};
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
