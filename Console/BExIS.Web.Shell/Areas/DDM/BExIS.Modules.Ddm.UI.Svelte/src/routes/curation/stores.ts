import { derived, writable, type Readable } from 'svelte/store';
import { getCurationDataset, putCurationEntry } from './services';
import { CurationUserType } from './types';
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

	private readonly _entryFilters = writable<((entry: CurationEntryClass) => boolean)[]>([]);
	public get entryFilters(): Readable<((entry: CurationEntryClass) => boolean)[]> {
		return this._entryFilters;
	}

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
				console.log('🎈 ~ getCurationDataset ~ response:', response);
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

	public addNote(entryId: number, userType: CurationUserType, comment: string) {
		this.applyAndSaveEntry(entryId, (entry) => entry.addNote(userType, comment, true));
	}

	public updateNote(entryId: number, noteId: number, userType: CurationUserType, comment: string) {
		this.applyAndSaveEntry(entryId, (entry) => entry.updateNote(noteId, userType, comment, true));
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

	public saveEntry(entry: CurationEntryClass) {
		this.setEntryLoading(entry.id, true);
		putCurationEntry(entry)
			.then((response) => {
				this._curation.update((curation) => {
					if (!curation) return curation;
					const newEntry = new CurationEntryClass(response, curation.currentUserType);
					let newCuration = curation.addEntry(newEntry);
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

	public addEntryFilter(filter: (entry: CurationEntryClass) => boolean) {
		this._entryFilters.update((filters) => {
			if (!filters.includes(filter)) {
				return [...filters, filter];
			}
			return filters;
		});
	}

	public removeEntryFilter(filter: (entry: CurationEntryClass) => boolean) {
		this._entryFilters.update((filters) => {
			return filters.filter((f) => f !== filter);
		});
	}

	public clearEntryFilters() {
		console.log('clearing Entry Filters');
		this._entryFilters.set([]);
	}

	public getFilteredEntriesReadable(): Readable<CurationEntryClass[]> {
		return derived([this._curation, this._entryFilters], ([$curation, $filters]) => {
			if (!$curation) return [];
			if ($filters.length === 0) return $curation.curationEntries;
			return $curation.curationEntries.filter((entry) => {
				return $filters.every((filter) => filter(entry));
			});
		});
	}
}

export const curationStore = new CurationStore();
