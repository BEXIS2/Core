import { derived, writable, type Readable, type Writable } from 'svelte/store';
import {
	getCurationEntries,
	getCurationDataset,
	postCurationEntry,
	putCurationEntry
} from './services';
import {
	CurationEntryStatus,
	CurationEntryStatusColorPalettes,
	CurationEntryType,
	CurationFilterType,
	type CurationDetailModel,
	type CurationLabel,
	type CurationFilterModel,
	CurationStatusEntryTab,
	DefaultCurationEntryCreationModel,
	type CurationEntryHelperModel,
	type CurationEntryCreationModel
} from './types';
import { CurationClass, CurationEntryClass } from './CurationEntries';
import { tick } from 'svelte';
import { get } from 'svelte/store';

class CurationStore {
	public readonly datasetId = writable<number | null>(null);

	private readonly _curation = writable<CurationClass | null>(null);
	public get curation(): Readable<CurationClass | null> {
		return this._curation;
	}
	private readonly _loadingCuration = writable<boolean>(true);
	public get loadingCuration(): Readable<boolean> {
		return this._loadingCuration;
	}
	private readonly _loadingError = writable<string | null>(null);
	public get loadingError(): Readable<string | null> {
		return this._loadingError;
	}

	private readonly _uploadingEntries = writable<number[]>([]);
	public readonly uploadingEntries: Readable<number[]> = this._uploadingEntries;

	private readonly debounceList = writable<number[]>([]);
	private timer: NodeJS.Timeout | null = null;
	private readonly debounceTime = 1000; // 1 second

	private readonly _entryFilters = writable<CurationFilterModel<any>[]>([]);

	public readonly editMode = writable<boolean>(false);
	public readonly statusColorPalette = writable(CurationEntryStatusColorPalettes[0]);

	public readonly jumpToEntryWhere = writable<
		((entry: CurationEntryHelperModel) => boolean) | undefined
	>();
	private jumpToEntryWhereTimer: NodeJS.Timeout | null = null;

	private readonly _entryCardStates = new Map<
		number,
		Writable<{
			isExpanded: boolean;
			editEntryMode: boolean;
			inputData:
				| {
						type: CurationEntryType;
						position: number;
						name: string;
						description: string;
						comment: string;
						status: CurationEntryStatus;
				  }
				| undefined;
		}>
	>();

	public readonly currentStatusEntryTab = writable<CurationStatusEntryTab>(
		CurationStatusEntryTab.Tasks
	);

	public readonly progressInfoExpanded = writable(false);
	public readonly curationInfoExpanded = writable(true);

	// Jump to Data and Dispatch Jump
	public readonly jumpToDataEnabled = writable(false);
	private jumpToDataCallback:
		| ((curationEntryHelper: Partial<CurationEntryHelperModel>) => void)
		| undefined;
	public setJumpToDataCallback(
		callback: (curationEntryHelper: Partial<CurationEntryHelperModel>) => void
	) {
		this.jumpToDataCallback = callback;
	}
	public dispatchJumpToData(curationEntryHelper: Partial<CurationEntryHelperModel>) {
		this.jumpToDataCallback?.(curationEntryHelper);
	}

	constructor() {
		this.datasetId.subscribe((datasetId) => {
			if (!datasetId) {
				this.setState(null, true, null);
				return;
			}
			this.fetch();
		});
		this.jumpToEntryWhere.subscribe((fn) => {
			if (this.jumpToEntryWhereTimer) clearTimeout(this.jumpToEntryWhereTimer);
			if (fn !== undefined) {
				this.jumpToEntryWhereTimer = setTimeout(() => {
					this.jumpToEntryWhere.set(undefined);
				}, 1000);
			}
		});
		this.curationInfoExpanded.subscribe((expanded) => {
			if (!expanded) {
				this.currentStatusEntryTab.set(CurationStatusEntryTab.Hide);
				this.progressInfoExpanded.set(false);
			}
		});
	}

	private setState(curation: CurationClass | null, loading: boolean, error: string | null) {
		this._curation.set(curation);
		this._loadingCuration.set(loading);
		this._loadingError.set(error);
	}

	private fetch() {
		const datasetId: number | null = get(this.datasetId);
		if (!datasetId) {
			this.setState(null, false, 'No dataset selected');
			return;
		}
		this.setState(null, true, null);

		getCurationDataset(datasetId)
			.then((response) => {
				this.setState(new CurationClass(response), false, null);
			})
			.catch((error) => {
				this.setState(null, false, error.message);
				console.error('🎈 ~ Error fetching curation dataset:', error);
			});
	}

	private addEntryToDebounceList(entryId: number) {
		this.debounceList.update((ids) => {
			if (!ids.includes(entryId)) {
				return [...ids, entryId];
			}
			return ids;
		});
		if (this.timer) {
			clearTimeout(this.timer);
		}
		this.timer = setTimeout(() => {
			this.saveDebouncedEntries();
		}, this.debounceTime);
	}

	private static getCurationEntry(
		entryId: number,
		curation: CurationClass | null
	): CurationEntryClass | null {
		if (!curation) return null;
		const entry = curation.curationEntries.find((entry) => entry.id === entryId);
		if (!entry) return null;
		return entry;
	}

	private applyAndSaveEntry(
		entryId: number,
		callback: (entry: CurationEntryClass) => CurationEntryClass,
		debounce: boolean = false
	) {
		this._curation.update((curation) => {
			const entry = CurationStore.getCurationEntry(entryId, curation);
			if (!entry) return curation;
			const newEntry = callback(entry);
			if (!debounce) this.saveEntry(newEntry);
			else this.addEntryToDebounceList(newEntry.id);
			return curation!.addEntry(newEntry);
		});
	}

	public saveDebouncedEntries() {
		this.debounceList.update((ids) => {
			ids.forEach((entryId) => {
				this._curation.update((curation) => {
					const entry = CurationStore.getCurationEntry(entryId, curation);
					if (!entry) return curation;
					this.saveEntry(entry);
					return curation!;
				});
			});
			return [];
		});
		this.timer = null;
	}

	public addNote(entryId: number, comment: string) {
		if (entryId <= 0) return;
		this.applyAndSaveEntry(entryId, (entry) => entry.addNote(comment, true));
	}

	public updateNote(entryId: number, noteId: number, comment: string) {
		this.applyAndSaveEntry(entryId, (entry) => entry.updateNote(noteId, comment, true));
	}

	public deleteNote(entryId: number, noteId: number) {
		this.applyAndSaveEntry(entryId, (entry) => entry.deleteNote(noteId));
	}

	public setUnread(entryId: number, unread: boolean = true) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setUnread(unread));
	}

	public toggleStatus(entryId: number) {
		this.applyAndSaveEntry(entryId, (entry) => entry.toggleStatus(), true);
	}

	public setStatusBoolean(
		entryId: number,
		newuserIsDone: boolean,
		newIsApproved: boolean,
		debounce = true
	) {
		this.applyAndSaveEntry(
			entryId,
			(entry) => entry.setStatusBoolean(newuserIsDone, newIsApproved),
			debounce
		);
	}

	public setStatus(entryId: number, status: CurationEntryStatus, debounce: boolean = true) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setStatus(status), debounce);
	}

	public setName(entryId: number, name: string, debounce: boolean = false) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setName(name), debounce);
	}

	public setTopic(entryId: number, topic: string, debounce: boolean = false) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setTopic(topic), debounce);
	}

	public setDescription(entryId: number, description: string, debounce: boolean = false) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setDescription(description), debounce);
	}

	public updateEntryPosition(entryId: number, position: number) {
		if (position <= 0) return;
		this._curation.update((curation) => {
			let oldPosition = curation?.getEntryById(entryId)?.position;
			let newCuration = curation?.updateEntry(entryId, { position });
			if (!newCuration || newCuration == curation) return curation;
			if (newCuration.getEntryById(entryId)?.position === oldPosition) return curation;
			this.saveEntry(newCuration.getEntryById(entryId)!);
			return newCuration;
		});
	}

	public addEmptyEntry(
		entryModel: Partial<CurationEntryCreationModel>,
		acceptCurationStatusEntry = false
	) {
		if (
			!acceptCurationStatusEntry &&
			entryModel.type &&
			entryModel.type === CurationEntryType.StatusEntryItem
		)
			return;
		this._curation.update((curation) => {
			if (!curation) return curation;
			return curation.addEmptyEntry({ ...DefaultCurationEntryCreationModel, ...entryModel });
		});
	}

	public updateEntry(entryId: number, updates: Partial<CurationEntryCreationModel>) {
		this._curation.update((curation) => {
			let newCuration = curation?.updateEntry(entryId, updates);
			if (!newCuration || newCuration == curation) return curation;
			if (!newCuration.getEntryById(entryId)) return curation;
			this.saveEntry(newCuration.getEntryById(entryId)!);
			return newCuration;
		});
	}

	public updateEntryFromEntry(entry: CurationEntryClass) {
		this.updateEntry(entry.id, {
			position: entry.position,
			topic: entry.topic,
			type: entry.type,
			name: entry.name,
			description: entry.description,
			solution: entry.solution,
			source: entry.source
		});
	}

	public saveEntry(entry: CurationEntryClass) {
		const prevEntryId = entry.id;
		const entryWasDraft = entry.isDraft();
		this.setEntryLoading(prevEntryId, true);
		let f = entryWasDraft ? postCurationEntry : putCurationEntry;
		f(entry)
			.then((response) => {
				this._curation.update((curation) => {
					if (!curation) return curation;
					const newEntry = new CurationEntryClass(response, curation.currentUserType);
					let newCuration = curation;
					if (entryWasDraft) {
						newCuration = curation.removeEntry(entry.id);
						this._entryCardStates.delete(prevEntryId);
						this.setEntryLoading(prevEntryId, false);
					}
					newCuration = newCuration.addEntry(newEntry);
					this.setEntryLoading(newEntry.id, false);
					return newCuration;
				});
				return response;
			})
			.catch((error) => {
				console.error('🎈 ~ Error saving entry:', error);
				this._uploadingEntries.set([]);
				this.fetch();
			});
	}

	public getEntryReadable(entryId: number): Readable<CurationEntryClass | null> {
		return derived(this._curation, ($curation) => {
			if (!$curation) return null;
			const entry = $curation.curationEntries.find((entry) => entry.id === entryId);
			if (!entry) return null;
			return entry;
		});
	}

	public setEntryLoading(entryId: number, loading: boolean) {
		this._uploadingEntries.update((ids) => {
			if (!loading) {
				return ids.filter((id) => id !== entryId);
			}
			if (!ids.includes(entryId)) {
				return [...ids, entryId];
			}
			return ids;
		});
	}

	public updateEntryFilter<TData>(
		type: CurationFilterType,
		dataUpdateFn: (data: TData | undefined) => TData | undefined,
		fn: ((entry: CurationEntryClass, data: TData) => boolean) | undefined = undefined,
		isClearedFn: ((data: TData) => boolean) | undefined = undefined
	) {
		this._entryFilters.update((filters) => {
			if (!filters.map((f) => f.type).includes(type)) {
				// create filter
				if (!fn || !isClearedFn) return filters;
				return [...filters, { type: type, data: dataUpdateFn(undefined), fn, isClearedFn }];
			}
			// update filter data
			const currentFilter = filters.find((f) => f.type === type)!;
			const newData = dataUpdateFn(currentFilter.data);
			if (newData === currentFilter.data) return filters;
			return [
				...filters.filter((f) => f.type !== type),
				{
					type: type,
					data: newData,
					fn: fn || currentFilter.fn,
					isClearedFn: isClearedFn || currentFilter.isClearedFn
				}
			];
		});
	}

	public removeEntryFilter(type: CurationFilterType) {
		this._entryFilters.update((filters) => {
			return filters.filter((f) => f.type !== type);
		});
	}

	public removeEntryFilters(types: CurationFilterType[]) {
		this._entryFilters.update((filters) => {
			const typeSet = new Set(types);
			return filters.filter((f) => !typeSet.has(f.type));
		});
	}

	public clearEntryFilters() {
		this._entryFilters.set([]);
	}

	public readonly hasFiltersApplied = derived(this._entryFilters, (filters) => {
		return filters.length > 0 && filters.some((f) => f.isClearedFn && !f.isClearedFn(f.data));
	});

	public getEntryFilterData(filterType: CurationFilterType) {
		return derived(this._entryFilters, (filters) => filters.find((f) => f.type === filterType));
	}

	public getCurrentTypeViewOrder(): Readable<CurationEntryType[]> {
		return derived([this._curation, this.editMode], ([curation, editMode]) => {
			const types = curation?.curationEntryTypes ?? [];
			return editMode ? [...types, CurationEntryType.None] : types;
		});
	}

	public getFilteredEntriesReadable(): Readable<CurationEntryClass[]> {
		return derived(
			[this._curation, this._entryFilters, this.editMode],
			([curation, filters, editMode]) => {
				if (!curation) return [];
				const typeSet = new Set(curation.curationEntryTypes);
				if (editMode) typeSet.add(CurationEntryType.None);
				return curation.curationEntries
					.filter((entry) => typeSet.has(entry.type))
					.filter((entry) => filters.every((f) => !f.fn || f.fn(entry, f.data)));
			}
		);
	}

	public getEntryCardState(entryId: number) {
		if (!this._entryCardStates.has(entryId))
			this._entryCardStates.set(
				entryId,
				writable({ isExpanded: false, editEntryMode: entryId <= 0, inputData: undefined })
			);
		return this._entryCardStates.get(entryId)!;
	}

	public resetCardState(entryId: number) {
		this._entryCardStates.delete(entryId);
	}

	public deleteEntry(entryId: number) {
		if (entryId >= 0) return;
		this.resetCardState(entryId);
		this._curation.update((curation) => {
			if (!curation) return curation;
			return curation.removeEntry(entryId);
		});
	}

	public async createAndJumpToEntry(entryModel: Partial<CurationEntryCreationModel>) {
		this.addEmptyEntry(entryModel);
		this.editMode.set(true);
		if (entryModel.type) {
			this.updateEntryFilter(
				CurationFilterType.type,
				(type: (CurationEntryType | undefined) | undefined) =>
					type === entryModel.type ? type : undefined
			);
			this.removeEntryFilters([CurationFilterType.status, CurationFilterType.search]);
		} else this.clearEntryFilters();
		await tick();
		curationStore.jumpToEntryWhere.set(
			(entry) =>
				entry.isDraft &&
				(entryModel.type === undefined || entry.type === entryModel.type) &&
				(entryModel.name === undefined || entry.name === entryModel.name) &&
				(entryModel.description === undefined || entry.description === entryModel.description)
			// needs to be updated if other parts of the entries are used
		);
	}
}

class OverviewStore {
	private readonly _curationDetails = writable<CurationDetailModel[]>([]);
	public get curationDetails(): Readable<CurationDetailModel[]> {
		return this._curationDetails;
	}
	private readonly _curationLabels = writable<CurationLabel[]>([]);
	public get curationLabels(): Readable<CurationLabel[]> {
		return this._curationLabels;
	}
	private readonly _isLoading = writable<boolean>(true);
	public get isLoading(): Readable<boolean> {
		return this._isLoading;
	}
	private readonly _errorMessage = writable<string | undefined>();
	public get errorMessage(): Readable<string | undefined> {
		return this._errorMessage;
	}

	public fetch() {
		this._curationDetails.set([]);
		this._curationLabels.set([]);
		this._isLoading.set(true);
		this._errorMessage.set(undefined);
		getCurationEntries()
			.then((response) => {
				this._curationDetails.set(response.datasets);
				this._curationLabels.set(response.curationLabels);
				this._isLoading.set(false);
			})
			.catch((error) => {
				this._errorMessage.set(error.message);
				console.error('🎈 ~ Error fetching curation dataset:', error);
				this._isLoading.set(false);
			});
	}
}

export const curationStore = new CurationStore();
export const overviewStore = new OverviewStore();
