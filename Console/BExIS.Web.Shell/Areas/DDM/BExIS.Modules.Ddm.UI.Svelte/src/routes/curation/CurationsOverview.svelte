<script lang="ts">
	import { writable } from 'svelte/store';
	import { Table, type TableConfig } from '@bexis2/bexis2-core-ui';
	import TableLabelFilter from './TableLabelFilter.svelte';
	import { overviewStore } from './stores';
	import TableLabelCell from './TableLabelCell.svelte';
	import {
		curationDataSetIdSearchParam,
		CurationEntryStatusDetails,
		getCurationStatusFromBoolean
	} from './types';
	import TableStatusFilter from './TableStatusFilter.svelte';
	import TableOptions from './TableOptions.svelte';
	import { goto } from '$app/navigation';

	const { curationDetails, curationLabels, isLoading, errorMessage } = overviewStore;

	overviewStore.fetch();

	type CurationStatus = {
		curationStarted: boolean;
		userIsDone: boolean;
		isApproved: boolean;
	};

	type tableDetails = {
		datasetId: number;
		datasetName: string;
		curationStatus: CurationStatus;
		labels: string[];
		curationEntryCount: number;
		progress_0: number;
		progress_2: number;
		progress_1: number;
		progress_3: number;
		lastChangeDatetime_User: Date;
		lastChangeDatetime_Curator: Date;
	};

	const curationDetailsPartial = writable<tableDetails[]>([]);

	curationDetails.subscribe(($curationDetails) =>
		curationDetailsPartial.set(
			$curationDetails.map((c) => ({
				datasetId: c.datasetId,
				datasetName: c.datasetName,
				curationStatus: {
					curationStarted: c.curationStarted,
					userIsDone: c.userIsDone,
					isApproved: c.isApproved
				},
				labels: c.notesComments,
				curationEntryCount: c.statusCounts?.reduce((sum, val) => sum + val, 0) || 0,
				progress_0: c.statusCounts?.at(0) || 0,
				progress_2: c.statusCounts?.at(2) || 0,
				progress_1: c.statusCounts?.at(1) || 0,
				progress_3: c.statusCounts?.at(3) || 0,
				lastChangeDatetime_User: new Date(c.lastChangeDatetime_User || 0),
				lastChangeDatetime_Curator: new Date(c.lastChangeDatetime_Curator || 0)
			}))
		)
	);

	const curationsConfig: TableConfig<tableDetails> = {
		id: 'curationsOverview',
		data: curationDetailsPartial,
		toggle: true,
		optionsComponent: TableOptions as any,
		columns: {
			curationStatus: {
				instructions: {
					toFilterableValueFn: (s: CurationStatus) =>
						s.curationStarted ? getCurationStatusFromBoolean(s.userIsDone, s.isApproved) + 2 : 1,
					renderComponent: TableLabelCell as any
				},
				disableSorting: true,
				colFilterComponent: TableStatusFilter as any
			},
			labels: {
				instructions: {
					toFilterableValueFn: (l: string[]) => l.join(' '),
					renderComponent: TableLabelCell as any
				},
				disableSorting: true,
				colFilterComponent: TableLabelFilter as any
			},
			curationEntryCount: {
				header: 'All Entries'
			},
			progress_0: {
				header: `${CurationEntryStatusDetails[0].name} Entries`
			},
			progress_2: {
				header: `${CurationEntryStatusDetails[1].name} Entries`
			},
			progress_1: {
				header: `${CurationEntryStatusDetails[2].name} Entries`
			},
			progress_3: {
				header: `${CurationEntryStatusDetails[3].name} Entries`
			},
			lastChangeDatetime_User: {
				header: 'Last User Change',
				instructions: {
					toStringFn: (date: Date) =>
						date.getFullYear() <= 1970
							? 'Never'
							: date.toLocaleString('en-US', {
									month: 'short',
									day: 'numeric',
									year: 'numeric'
								}),
					toSortableValueFn: (date: Date) => date.getTime(),
					toFilterableValueFn: (date: Date) => date
				}
			},
			lastChangeDatetime_Curator: {
				header: 'Last Curator Change',
				instructions: {
					toStringFn: (date: Date) =>
						date.getFullYear() <= 1970
							? 'Never'
							: date.toLocaleString('en-US', {
									month: 'short',
									day: 'numeric',
									year: 'numeric'
								}),
					toSortableValueFn: (date: Date) => date.getTime(),
					toFilterableValueFn: (date: Date) => date
				}
			},
			optionsColumn: {}
		}
	};

	function openCuration(datasetId: any) {
		if (!datasetId) return;
		if (typeof datasetId === 'number') {
			const a = curationDataSetIdSearchParam;
			goto(`/curation?${curationDataSetIdSearchParam}=${datasetId}`);
		}
	}
</script>

<h1>Curations Overview</h1>

<Table config={curationsConfig} on:action={(obj) => openCuration(obj.detail.datasetId)} />
