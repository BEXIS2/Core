<script lang="ts">
	import { writable } from 'svelte/store';
	import { Table, type TableConfig } from '@bexis2/bexis2-core-ui';
	import TableLabelFilter from './TableLabelFilter.svelte';
	import { overviewStore } from './stores';
	import TableLabelCell from './TableLabelCell.svelte';
	import { CurationEntryStatusDetails, getCurationStatusFromBoolean } from './types';
	import TableStatusFilter from './TableStatusFilter.svelte';
	import TableOptions from './TableOptions.svelte';
	import { goto } from '$app/navigation';
	import { noteCommentToLabel } from './CurationEntries';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';

	const { curationDetails, isLoading, errorMessage } = overviewStore;

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
		progress_0: number; // Open Entries
		progress_1: number; // Changed Entries
		progress_2: number; // Paused Entries
		progress_3: number; // Approved Entries
		lastChangeDatetime_User: Date | undefined;
		lastChangeDatetime_Curator: Date | undefined;
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
				curationEntryCount:
					c.count_UserIsDone_False_IsApproved_False +
					c.count_UserIsDone_False_IsApproved_True +
					c.count_UserIsDone_True_IsApproved_False +
					c.count_UserIsDone_True_IsApproved_True,
				progress_0: c.count_UserIsDone_False_IsApproved_False, // Open Entries
				progress_1: c.count_UserIsDone_True_IsApproved_False, // Changed Entries
				progress_2: c.count_UserIsDone_False_IsApproved_True, // Paused Entries
				progress_3: c.count_UserIsDone_True_IsApproved_True, // Approved Entries
				lastChangeDatetime_User: c.lastChangeDatetime_User
					? new Date(c.lastChangeDatetime_User)
					: undefined,
				lastChangeDatetime_Curator: c.lastChangeDatetime_Curator
					? new Date(c.lastChangeDatetime_Curator)
					: undefined
			}))
		)
	);

	const dateInstructions = {
		toStringFn: (date: Date | undefined) =>
			!date || date.getFullYear() <= 1970
				? 'Never'
				: date.toLocaleString('en-US', {
						month: 'short',
						day: 'numeric',
						year: 'numeric'
					}),
		toSortableValueFn: (date: Date | undefined) => (date ? date.getTime() : 0),
		toFilterableValueFn: (date: Date | undefined) => (date ? date : new Date(0))
	};

	const curationsConfig: TableConfig<tableDetails> = {
		id: 'curationsOverview',
		data: curationDetailsPartial,
		search: false,
		toggle: false,
		showColumnsMenu: true,
		optionsComponent: TableOptions as any,
		columns: {
			curationStatus: {
				instructions: {
					toFilterableValueFn: (s: CurationStatus) =>
						// not started equals -1
						s.curationStarted ? getCurationStatusFromBoolean(s.userIsDone, s.isApproved) : -1,
					renderComponent: TableLabelCell as any
				},
				disableSorting: true,
				colFilterFn: (f: { filterValue: any | undefined; value: Set<number> }) => {
					if (!f.filterValue.i || f.filterValue.i.size === 0) return true;
					return f.filterValue.i.has(f.value);
				},
				colFilterComponent: TableStatusFilter as any
			},
			labels: {
				instructions: {
					toFilterableValueFn: (l: string[]) =>
						new Set(l.map((a) => noteCommentToLabel(a).name)) as any,
					renderComponent: TableLabelCell as any
				},
				disableSorting: true,
				colFilterFn: (f: { filterValue: any | undefined; value: Set<string> }) => {
					if (!f.filterValue.i || f.filterValue.i.size === 0) return true;
					return f.filterValue.i.intersection(f.value).size > 0;
				},
				colFilterComponent: TableLabelFilter as any
			},
			curationEntryCount: {
				header: 'All Entries'
			},
			progress_0: {
				// Open Entries
				header: `${CurationEntryStatusDetails[0].name} Entries`
			},
			progress_1: {
				// Paused Entries
				header: `${CurationEntryStatusDetails[1].name} Entries`
			},
			progress_2: {
				// Changed Entries
				header: `${CurationEntryStatusDetails[2].name} Entries`
			},
			progress_3: {
				// Approved Entries
				header: `${CurationEntryStatusDetails[3].name} Entries`
			},
			lastChangeDatetime_User: {
				header: 'Last User Change',
				instructions: dateInstructions
			},
			lastChangeDatetime_Curator: {
				header: 'Last Curator Change',
				instructions: dateInstructions
			},
			optionsColumn: {}
		}
	};

	function openCuration(id: number) {
		if (!id) return;
		const params = new URLSearchParams(window.location.search);
		params.set('id', id.toString());
		goto(`${window.location.pathname}?${params.toString()}`);
	}
</script>

<h1>Curations Overview</h1>

{#if !$errorMessage || $isLoading}
	<div class="relative">
		<Table config={curationsConfig} on:action={(obj) => openCuration(obj.detail.datasetId)} />

		{#if $isLoading}
			<SpinnerOverlay label="Loading Curation Overview" />
		{/if}
	</div>
{:else}
	<div>
		<p class="text-red-500">Error loading curation entries</p>
	</div>
{/if}
