import { derived, writable, type Readable } from 'svelte/store';
import { get, getCurationDataset, postCurationEntry, putCurationEntry } from './services';
import {
	CurationEntryStatus,
	CurationEntryStatusColorPalettes,
	CurationEntryType,
	type CurationDetailModel,
	type CurationLabel,
	type FilterModel
} from './types';
import { CurationClass, CurationEntryClass } from './CurationEntries';

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

	private readonly _entryFilters = writable<FilterModel<any>[]>([]);

	public readonly editMode = writable<boolean>(false);
	public readonly statusColorPalette = writable(CurationEntryStatusColorPalettes[0]);

	constructor() {
		this.datasetId.subscribe((datasetId) => {
			if (!datasetId) {
				this.setState(null, true, null);
				return;
			}
			this.fetch(datasetId);
		});
	}

	private setState(curation: CurationClass | null, loading: boolean, error: string | null) {
		this._curation.set(curation);
		this._loadingCuration.set(loading);
		this._loadingError.set(error);
	}

	private fetch(datasetId: number | null) {
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

	public reload() {
		this.datasetId.update((datasetId) => {
			if (!datasetId) return datasetId;
			this.fetch(datasetId);
			return datasetId;
		});
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

	public setStatus(entryId: number, status: CurationEntryStatus) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setStatus(status), true);
	}

	public setName(entryId: number, name: string, debounce: boolean = false) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setName(name), debounce);
	}

	public setDescription(entryId: number, description: string, debounce: boolean = false) {
		this.applyAndSaveEntry(entryId, (entry) => entry.setDescription(description), debounce);
	}

	public updateEntryPosition(entryId: number, position: number) {
		this._curation.update((curation) => {
			let oldPosition = curation?.getEntryById(entryId)?.position;
			let newCuration = curation?.updateEntryPosition(entryId, position);
			if (!newCuration || newCuration == curation) return curation;
			if (newCuration.getEntryById(entryId)?.position === oldPosition) return curation;
			this.saveEntry(newCuration.getEntryById(entryId)!);
			return newCuration;
		});
	}

	public addEmptyEntry(
		position: number,
		name = '',
		type = CurationEntryType.None,
		description = '',
		acceptCurationStatusEntry = false
	) {
		if (!acceptCurationStatusEntry && type === CurationEntryType.StatusEntryItem) return;
		this._curation.update((curation) => {
			if (!curation) return curation;
			return curation.addEmptyEntry(position, name, type, description);
		});
	}

	public addEmptyEntryFromJson(json: {
		position?: number;
		type?: CurationEntryType;
		name?: string;
		description?: string;
	}) {
		const { position = 1, type = CurationEntryType.None, name = '', description = '' } = json;
		this.addEmptyEntry(position, name, type, description);
	}

	public updateEntry(
		entryId: number,
		topic: string,
		type: CurationEntryType,
		name: string,
		description: string,
		solution: string,
		source: string
	) {
		this._curation.update((curation) => {
			let newCuration = curation?.updateEntry(
				entryId,
				topic,
				type,
				name,
				description,
				solution,
				source
			);
			if (!newCuration || newCuration == curation) return curation;
			if (!newCuration.getEntryById(entryId)) return curation;
			this.saveEntry(newCuration.getEntryById(entryId)!);
			return newCuration;
		});
	}

	public updateEntryFromEntry(entry: CurationEntryClass) {
		this.updateEntry(
			entry.id,
			entry.topic,
			entry.type,
			entry.name,
			entry.description,
			entry.solution,
			entry.source
		);
	}

	public saveEntry(entry: CurationEntryClass) {
		const draftEntryId = entry.id;
		this.setEntryLoading(draftEntryId, true);
		let f = draftEntryId <= 0 ? postCurationEntry : putCurationEntry;
		f(entry)
			.then((response) => {
				this._curation.update((curation) => {
					if (!curation) return curation;
					const newEntry = new CurationEntryClass(response, curation.currentUserType);
					let newCuration = curation;
					if (draftEntryId <= 0) {
						newCuration = curation.removeEntry(entry.id);
						this.setEntryLoading(draftEntryId, false);
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
				this.fetch(entry.datasetId);
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
		filterId: string,
		dataUpdateFn: (data: TData | undefined) => TData,
		fn: (entry: CurationEntryClass, data: TData) => boolean,
		isClearedFn: (data: TData) => boolean
	) {
		this._entryFilters.update((filters) => {
			if (!filters.map((f) => f.id).includes(filterId)) {
				// create filter
				return [...filters, { id: filterId, data: dataUpdateFn(undefined), fn, isClearedFn }];
			}
			// update filter data
			const currentFilter = filters.find((f) => f.id === filterId)!;
			const newData = dataUpdateFn(currentFilter.data);
			if (newData === currentFilter.data) return filters;
			return [
				...filters.filter((f) => f.id !== filterId),
				{
					id: filterId,
					data: newData,
					fn,
					isClearedFn
				}
			];
		});
	}

	public removeEntryFilter(filterId: string) {
		this._entryFilters.update((filters) => {
			return filters.filter((f) => f.id !== filterId);
		});
	}

	public clearEntryFilters() {
		this._entryFilters.set([]);
	}

	public readonly hasFiltersApplied = derived(this._entryFilters, (filters) => {
		return filters.length > 0 && filters.some((f) => !f.isClearedFn(f.data));
	});

	public getEntryFilterData(filterId: string) {
		return derived(this._entryFilters, (filters) => filters.find((f) => f.id === filterId));
	}

	public getFilteredEntriesReadable(): Readable<CurationEntryClass[]> {
		return derived([this._curation, this._entryFilters], ([curation, filters]) => {
			if (!curation) return [];
			return curation.curationEntries.filter((entry) => {
				return filters.every((f) => f.fn(entry, f.data));
			});
		});
	}

	public deleteEntry(entryId: number) {
		if (entryId >= 0) return;
		this._curation.update((curation) => {
			if (!curation) return curation;
			return curation.removeEntry(entryId);
		});
	}

	public startCuration() {
		this.addEmptyEntry(1, 'Test', CurationEntryType.StatusEntryItem, undefined, true);

		this._curation.subscribe((curation) => {
			if (!curation) return;
			if (!curation.curationStatusEntry) return;
			const unsubscribe = this._curation.subscribe((curation) => {
				if (!curation) return;
				if (!curation.curationStatusEntry) return;
				if (curation.curationStatusEntry.id < 0) {
					this.saveEntry(curation.curationStatusEntry);
					unsubscribe();
				}
			});
		});
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
		get()
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
