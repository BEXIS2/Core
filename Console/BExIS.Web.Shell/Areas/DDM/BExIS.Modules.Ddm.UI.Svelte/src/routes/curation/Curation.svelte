<script lang="ts">
	import { onMount } from 'svelte';
	import CurationDatasetInfo from './CurationDatasetInfo.svelte';
	import { curationStore } from './stores';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faAngleUp, faBroom, faExpand, faPen } from '@fortawesome/free-solid-svg-icons';
	import {
		CurationEntryStatusColorPalettes,
		CurationEntryType,
		CurationEntryTypeNames,
		CurationEntryTypeViewOrders,
		CurationFilterType,
		type CurationEntryModel
	} from './types';
	import { derived } from 'svelte/store';
	import { CurationEntryClass } from './CurationEntries';
	import CurationStatusEntryCard from './CurationStatusEntryCard.svelte';
	import CurationProgressInfo from './CurationProgressInfo.svelte';
	import CurationFilter from './CurationFilter.svelte';
	import CurationEntryList from './CurationEntryList.svelte';
	import StartCuration from './StartCuration.svelte';
	import { slide } from 'svelte/transition';

	export let datasetId: number;
	export let jumpToEntryWhere: ((entry: CurationEntryClass) => boolean) | undefined = undefined;
	export let applyTypeFilter: CurationEntryType | undefined = undefined;
	export let addEntry: Partial<CurationEntryModel> | undefined = undefined;
	export let moveToDataFunction: ((entry: CurationEntryClass) => any) | undefined = undefined;
	export let moveToData: any = undefined;

	const {
		loadingCuration,
		loadingError,
		curation,
		editMode,
		statusColorPalette,
		hasFiltersApplied,
		curationInfoExpanded,
		progressInfoExpanded
	} = curationStore;

	// Apply jump to entry and reset
	$: if (jumpToEntryWhere) {
		curationStore.jumpToEntryWhere.set(jumpToEntryWhere);
		jumpToEntryWhere = undefined;
	}

	// Apply type filter and reset
	$: if (applyTypeFilter !== undefined) {
		curationStore.updateEntryFilter(
			CurationFilterType.type,
			(_) => applyTypeFilter,
			(entry: CurationEntryClass, data: CurationEntryType | undefined) =>
				data === undefined || entry.type === data,
			(data) => data === undefined
		);
		applyTypeFilter = undefined;
	}

	// add and jump to entry and reset (only if the user is a curator)
	$: if (addEntry) {
		if ($curation?.isCurator) curationStore.createAndJumpToEntry(addEntry);
		addEntry = undefined;
	}

	// Move to data
	$: curationStore.moveToDataFunction.set(moveToDataFunction);
	const moveToDataStore = curationStore.moveToData;
	moveToDataStore.subscribe((mtds) => {
		if (!mtds) return;
		moveToDataStore.set(undefined);
		moveToData = mtds;
	});

	// ---

	const filteredEntries = curationStore.getFilteredEntriesReadable();

	const typeFilter = curationStore.getEntryFilterData(CurationFilterType.type);

	const typeGroupedEntries = derived(filteredEntries, ($filteredEntries) => {
		// empty list for each entry of CurationEntryTypeNames
		let groupedEntries: CurationEntryClass[][] = CurationEntryTypeNames.map((_) => []);
		let previousPos: (number | undefined)[] = groupedEntries.map((_) => 2);
		$filteredEntries.forEach((e) => {
			groupedEntries[e.type as number].push(e);
		});
		groupedEntries.slice(0, -1).forEach((ge, i) => (previousPos[i + 1] = ge.at(-1)?.position ?? 2));
		return groupedEntries;
	});

	const curationTypeViewOrder = derived(editMode, ($editMode) =>
		!$editMode ? CurationEntryTypeViewOrders.default : CurationEntryTypeViewOrders.editMode
	);

	onMount(async () => {
		if (datasetId) {
			curationStore.datasetId.set(datasetId);
		}
	});

	const clearFilters = () => {
		curationStore.clearEntryFilters();
	};

	const toggleEditMode = () => {
		editMode.update((value) => {
			if (!value && $curation?.isCurator) return true;
			else return false;
		});
	};

	const toggleCurationInfoExpand = () => {
		curationInfoExpanded.update((i) => !i);
	};
</script>

<div class="w-full overflow-x-hidden rounded">
	<CurationDatasetInfo />

	{#if $loadingCuration}
		<div class="flex h-96 items-center justify-center">
			<Spinner label="Loading Curation Entries..." />
		</div>
	{:else if $loadingError}
		<div>
			<p class="text-red-500">Error loading curation entries</p>
		</div>
	{:else if !$curation?.curationStatusEntry?.isNoDraft()}
		<StartCuration />
	{:else}
		<!-- Status Entry -->
		<CurationStatusEntryCard curationStatusEntry={$curation.curationStatusEntry} />
		<!-- Color Palette Picker -->
		{#if $curationInfoExpanded}
			<div
				class="overflow-x-hidden border-b border-surface-500 p-2"
				in:slide={{ duration: 150 }}
				out:slide={{ duration: 150 }}
			>
				<label class="flex">
					<span class="mr-1">Color palette for the entry status:</span>
					<select
						bind:value={$statusColorPalette}
						title="Change color palette of entry status"
						class="rounded-l py-0.5 text-sm"
					>
						{#each CurationEntryStatusColorPalettes as colorPalette}
							<option value={colorPalette}>{colorPalette.name}</option>
						{/each}
					</select>
					<div
						class="flex items-center gap-x-1 overflow-hidden rounded-r border-y border-r border-surface-500 px-2"
					>
						{#each $statusColorPalette.colors as c}
							<div class="inline size-2 rounded-full" style="background-color: {c}">&nbsp;</div>
						{/each}
					</div>
				</label>
			</div>
		{/if}
		<!-- Progress -->
		<div class="border-b border-surface-500">
			<!-- Curation Progress -->
			<CurationProgressInfo
				progress={$curation?.curationProgressTotal}
				label="Curation Progress"
				expandWritable={progressInfoExpanded}
			/>
			{#if $progressInfoExpanded}
				{#each $curationTypeViewOrder as progressType}
					<div
						class="ml-8 opacity-30 hover:opacity-100"
						in:slide={{ duration: 150 }}
						out:slide={{ duration: 150 }}
					>
						<CurationProgressInfo
							progress={$curation?.curationProgressPerType[progressType]}
							label="{CurationEntryTypeNames[progressType]} Progress"
						/>
					</div>
				{/each}
			{/if}
		</div>
		<!-- Filter and search -->
		{#if $curationInfoExpanded}
			<div
				class="border-b border-surface-500 text-sm"
				in:slide={{ duration: 150 }}
				out:slide={{ duration: 150 }}
			>
				<CurationFilter />
			</div>
		{/if}
		<!-- Curation Entry Actions -->
		<div class="flex flex-wrap items-center justify-between gap-1 border-b border-surface-500 p-1">
			<button
				on:click={toggleCurationInfoExpand}
				class="variant-filled-surface btn overflow-hidden text-ellipsis text-wrap px-2 py-1 enabled:text-surface-800"
			>
				<Fa
					icon={faAngleUp}
					class="mr-2 inline-block transition-transform {$curationInfoExpanded ? '' : 'rotate-180'}"
				/>
				{#if $curationInfoExpanded}
					Contract
				{:else}
					Expand
				{/if}
			</button>
			<button
				on:click={clearFilters}
				disabled={!$hasFiltersApplied}
				class="variant-soft-tertiary btn grow px-2 py-1 enabled:text-surface-800"
			>
				{#if $hasFiltersApplied}
					<Fa icon={faBroom} class="mr-2 inline-block" />
					Clear Applied Filters
				{:else}
					No Filters Applied
				{/if}
			</button>
			{#if $curation?.isCurator}
				<button on:click={toggleEditMode} class="variant-soft-secondary btn grow px-2 py-1">
					<Fa icon={$editMode ? faExpand : faPen} class="mr-2 inline-block" />
					Go to {$editMode ? 'View' : 'Edit'} Mode
				</button>
			{/if}
		</div>
		<!-- Curation Entries -->
		<div class="overflow-hidden py-2">
			{#each $curationTypeViewOrder as type}
				{#if $typeFilter?.data !== undefined ? $typeFilter.data === type : true}
					<!-- {#key $typeGroupedEntries[type]} -->
					<CurationEntryList
						heading={CurationEntryTypeNames[type]}
						curationEntries={$typeGroupedEntries[type]}
						{type}
					/>
					<!-- {/key} -->
				{/if}
			{/each}
		</div>
	{/if}
</div>
