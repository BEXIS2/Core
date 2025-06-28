<script lang="ts">
	import { onMount } from 'svelte';
	import CurationDatasetInfo from './CurationDatasetInfo.svelte';
	import { curationStore } from './stores';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import CurationGroupCard from './CurationGroupCard.svelte';
	import Fa from 'svelte-fa';
	import { faBroom, faExpand, faPen, faPlay } from '@fortawesome/free-solid-svg-icons';
	import {
		CurationEntryStatusColorPalettes,
		CurationEntryType,
		CurationEntryTypeNames,
		CurationUserType
	} from './types';
	import AddCurationEntry from './AddCurationEntry.svelte';
	import { derived } from 'svelte/store';
	import { CurationEntryClass } from './CurationEntries';
	import CurationStatusEntryCard from './CurationStatusEntryCard.svelte';
	import CurationProgressInfo from './CurationProgressInfo.svelte';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';
	import CurationFilter from './CurationFilter.svelte';

	export let datasetId: number;

	const {
		loadingCuration,
		loadingError,
		uploadingEntries,
		curation,
		editMode,
		statusColorPalette,
		hasFiltersApplied
	} = curationStore;

	const filteredEntries = curationStore.getFilteredEntriesReadable();

	const groupedFilteredEntries = derived(filteredEntries, ($filteredEntries) => {
		// empty list for each entry of CurationEntryTypeNames
		let typeGroupedEntries: CurationEntryClass[][] = Array.from(
			{ length: CurationEntryTypeNames.length },
			() => []
		);
		$filteredEntries.forEach((e) => {
			typeGroupedEntries[e.type as number].push(e);
		});
		let groupedEntries: CurationEntryClass[][][] = [];
		typeGroupedEntries.forEach((entrygroup, index) => {
			groupedEntries.push([]);
			entrygroup.forEach((entry) => {
				if (groupedEntries[index].length === 0) {
					groupedEntries[index].push([entry]);
				} else {
					const lastEntry = groupedEntries[index].at(-1)!.at(-1)!;
					if (
						(lastEntry.position === entry.position - 1 || lastEntry.position === entry.position) &&
						lastEntry.name === entry.name
					) {
						groupedEntries[index].at(-1)!.push(entry);
					} else {
						groupedEntries[index].push([entry]);
					}
				}
			});
		});
		return groupedEntries;
	});

	onMount(async () => {
		if (datasetId) {
			curationStore.datasetId.set(datasetId);
		}
	});

	const clearFilters = () => {
		curationStore.clearEntryFilters();
	};

	const toggleEditMode = () => {
		editMode.update((value) => !value);
	};

	const startCuration = () => {
		curationStore.startCuration();
		editMode.set(true);
	};
</script>

<div class="container mx-auto grid max-w-3xl rounded border border-surface-500">
	<CurationDatasetInfo />

	{#if $loadingCuration}
		<div class="flex h-96 items-center justify-center">
			<Spinner label="Loading Curation Entries..." />
		</div>
	{:else if $loadingError}
		<div>
			<p class="text-red-500">Error loading curation entries</p>
		</div>
	{:else if $curation?.curationEntries.length === 0 || !$curation?.curationStatusEntry}
		<div class="flex h-48 items-center justify-center">
			<p class="text-surface-800">The curation process has not started yet.</p>
		</div>
		<!-- TODO: TEMPLATE -->
		{#if $curation?.currentUserType === CurationUserType.Curator}
			<button
				on:click={startCuration}
				class="m-1 rounded bg-success-500 p-1 text-surface-100 hover:bg-success-600 focus-visible:bg-success-600"
			>
				<Fa icon={faPlay} class="mr-1 inline-block" />
				Start Curation
			</button>
		{/if}
	{:else}
		<!-- Status Entry -->
		<CurationStatusEntryCard curationStatusEntry={$curation.curationStatusEntry} />
		<!-- Color Palette Picker -->
		<div class="overflow-x-hidden border-b border-surface-500 p-2">
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
		<!-- Progress -->
		{#if $curation.curationEntries.length > 0}
			<div class="border-b border-surface-500">
				<!-- Curation Progress -->
				<CurationProgressInfo
					progress={$curation?.curationProgressTotal}
					totalIssues={$curation?.curationProgressTotal.reduce((a, b) => a + b, 0)}
					label="Curation Progress Test"
				/>
				<!-- Spinner overlay -->
				{#if $uploadingEntries.length > 0}
					<SpinnerOverlay />
				{/if}
			</div>
		{/if}
		<!-- Filter and search -->
		<div class="border-b border-surface-500 text-sm">
			<CurationFilter />
		</div>
		<!-- Curation Entry Actions -->
		<div class="flex flex-wrap items-center justify-between gap-1 border-b border-surface-500 p-1">
			<button
				on:click={clearFilters}
				disabled={!$hasFiltersApplied}
				class="grow basis-3/4 rounded bg-surface-200 px-2 py-1 enabled:hover:bg-surface-400 enabled:focus-visible:bg-surface-400 disabled:text-surface-500"
			>
				{#if $hasFiltersApplied}
					<Fa icon={faBroom} class="mr-1 inline-block" />
					Clear Applied Filters
				{:else}
					No Filters Applied
				{/if}
			</button>
			{#if $curation?.currentUserType === CurationUserType.Curator}
				<button
					on:click={toggleEditMode}
					class="grow rounded bg-secondary-200 px-2 py-1 text-secondary-800 hover:bg-secondary-400 hover:text-secondary-900 focus-visible:bg-secondary-400"
				>
					<Fa icon={$editMode ? faExpand : faPen} class="mr-1 inline-block" />
					Go to {$editMode ? 'View' : 'Edit'} Mode
				</button>
			{/if}
		</div>
		<!-- Curation Entries -->
		<div class="py-2">
			{#each $groupedFilteredEntries as entryTypeGroup, index}
				{#if index > CurationEntryType.StatusEntryItem && ($editMode || entryTypeGroup.some( (entry) => entry[0].isVisible() ))}
					<h2 class="m-2 mt-3 text-xl font-semibold">{CurationEntryTypeNames[index]}</h2>
					<ul class="flex flex-col gap-2 p-2">
						<AddCurationEntry
							position={entryTypeGroup.at(0)?.at(0)?.position ?? 1}
							type={index}
							tag="li"
						/>
						{#each entryTypeGroup as entryNameGroup (entryNameGroup[0].id)}
							{#if $editMode || entryNameGroup.some((entry) => entry.isVisible())}
								<CurationGroupCard entries={entryNameGroup} />
							{/if}
						{/each}
					</ul>
				{/if}
			{/each}
			{#if $editMode}
				<h2 class="m-2 text-xl font-semibold">
					{CurationEntryTypeNames[CurationEntryType.None]} (Hidden)
				</h2>
				<ul class="flex flex-col gap-2 p-2">
					{#each $groupedFilteredEntries[CurationEntryType.None] as hiddenGroup (hiddenGroup[0].id)}
						<CurationGroupCard entries={hiddenGroup} />
					{/each}
				</ul>
			{/if}
		</div>
	{/if}
</div>
