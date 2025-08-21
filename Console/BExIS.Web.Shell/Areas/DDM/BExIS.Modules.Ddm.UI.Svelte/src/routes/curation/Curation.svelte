<script lang="ts">
	import { createEventDispatcher, onMount } from 'svelte';
	import CurationDatasetInfo from './CurationDatasetInfo.svelte';
	import { curationStore } from './stores';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faAngleUp, faBroom, faExpand, faPen } from '@fortawesome/free-solid-svg-icons';
	import {
		CurationEntryType,
		CurationEntryTypeNames,
		CurationEntryTypeViewOrders,
		CurationFilterType,
		type CurationEntryHelperModel,
		type CurationEntryModel
	} from './types';
	import { derived, type Readable } from 'svelte/store';
	import { CurationEntryClass } from './CurationEntries';
	import CurationStatusEntryCard from './CurationStatusEntryCard.svelte';
	import CurationProgressInfo from './CurationProgressInfo.svelte';
	import CurationFilter from './CurationFilter.svelte';
	import CurationEntryList from './CurationEntryList.svelte';
	import StartCuration from './StartCuration.svelte';
	import { slide } from 'svelte/transition';
	import ColorPalettePicker from './ColorPalettePicker.svelte';

	const {
		loadingCuration,
		loadingError,
		curation,
		editMode,
		hasFiltersApplied,
		curationInfoExpanded,
		progressInfoExpanded,
		jumpToDataEnabled
	} = curationStore;

	export let datasetId: number;

	export function applyTypeFilter(type: CurationEntryType | undefined) {
		curationStore.updateEntryFilter(
			CurationFilterType.type,
			(_) => type,
			(entry: CurationEntryClass, data: CurationEntryType | undefined) =>
				data === undefined || entry.type === data,
			(data) => data === undefined
		);
	}

	export function addEntry(entry: Partial<CurationEntryModel>) {
		curationStore.createAndJumpToEntry(entry);
	}

	// Jump To Entry
	export function jumpToEntryWhere(entryWhere: (entry: CurationEntryHelperModel) => boolean) {
		curationStore.jumpToEntryWhere.set(entryWhere);
	}

	// Jump To Data
	export let enableJumpToData = false;
	$: jumpToDataEnabled.set(enableJumpToData);
	const jumpToDataDispatcher = createEventDispatcher<{
		jumpToData: Partial<CurationEntryHelperModel>;
	}>();
	curationStore.setJumpToDataCallback((curationEntryHelper) => {
		jumpToDataDispatcher('jumpToData', curationEntryHelper);
	});

	// Curation Entries Readable
	export const curationEntriesReadable: Readable<Partial<CurationEntryHelperModel>[]> = derived(
		[curationStore.curation, curationStore.statusColorPalette],
		([curation, colorPalette]) => {
			const entries = curation?.curationEntries;
			if (!entries) return [];
			return entries.filter((e) => !e.isHidden()).map((e) => e.getHelperModel(colorPalette));
		}
	);

	// ---

	const typeFilter = curationStore.getEntryFilterData(CurationFilterType.type);

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
		<CurationStatusEntryCard />
		<!-- Color Palette Picker -->
		{#if $curationInfoExpanded}
			<div
				class="overflow-x-hidden border-b border-surface-500 p-2"
				in:slide={{ duration: 150 }}
				out:slide={{ duration: 150 }}
			>
				<ColorPalettePicker />
			</div>
		{/if}
		<!-- Progress -->
		<div class="border-b border-surface-500">
			<!-- Curation Progress -->
			<CurationProgressInfo expandWritable={progressInfoExpanded} />
			{#if $progressInfoExpanded}
				<div in:slide={{ duration: 150 }} out:slide={{ duration: 150 }}>
					{#each $curationTypeViewOrder as progressType}
						{#if progressType !== CurationEntryType.None}
							<div class="ml-8 opacity-30 hover:opacity-100">
								<CurationProgressInfo type={progressType} />
							</div>
						{/if}
					{/each}
				</div>
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
					Collapse
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
					Switch to {$editMode ? 'View' : 'Edit'} Mode
				</button>
			{/if}
		</div>
		<!-- Curation Entries -->
		<div class="overflow-hidden py-2">
			{#each $curationTypeViewOrder as type}
				{#if $typeFilter?.data !== undefined ? $typeFilter.data === type : true}
					<CurationEntryList {type} />
				{/if}
			{/each}
		</div>
	{/if}
</div>
