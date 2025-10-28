import type { CurationDetailModel } from '$lib/models/CurationsTable';
import type { CurationLabelModel } from '$lib/models/CurationStatusEntry';
import { getCurationEntries } from '$lib/services/curationService';
import { writable, type Readable } from 'svelte/store';

class OverviewStore {
	private readonly _curationDetails = writable<CurationDetailModel[]>([]);
	public get curationDetails(): Readable<CurationDetailModel[]> {
		return this._curationDetails;
	}
	private readonly _curationLabels = writable<CurationLabelModel[]>([]);
	public get curationLabels(): Readable<CurationLabelModel[]> {
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

export const overviewStore = new OverviewStore();
