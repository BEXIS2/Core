<script lang="ts">
	import { onMount } from 'svelte';
	import CurationDatasetInfo from './CurationDatasetInfo.svelte';
	import { curationStore } from './stores';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import CurationGroupCard from './CurationGroupCard.svelte';
	import Fa from 'svelte-fa';
	import { faBroom, faExpand, faPen, faPlay } from '@fortawesome/free-solid-svg-icons';
	import { CurationUserType } from './types';
	import AddCurationEntry from './AddCurationEntry.svelte';

	export let datasetId: number;

	const { loadingCuration, loadingError, entryFilters, curation, editMode } = curationStore;

	const filteredEntries = curationStore.getFilteredEntriesReadable();

	onMount(async () => {
		if (datasetId) {
			console.log('Curation datasetId:', datasetId);
			curationStore.datasetId.set(datasetId); // automatically fetches dataset
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
	{:else if $curation?.curationEntries.length === 0}
		<div class="flex h-48 items-center justify-center">
			<p class="text-surface-800">No curation entries available</p>
		</div>
		<button
			on:click={startCuration}
			class="m-1 rounded bg-success-500 p-1 text-surface-100 hover:bg-success-600 focus-visible:bg-success-600"
		>
			<Fa icon={faPlay} class="mr-1 inline-block" />
			Start Curation
		</button>
	{:else}
		<div class="flex flex-wrap items-center justify-between gap-1 border-b border-surface-500 p-1">
			<button
				on:click={clearFilters}
				disabled={$entryFilters.length === 0}
				class="grow basis-3/4 rounded bg-surface-200 px-2 py-1 enabled:hover:bg-surface-400 enabled:focus-visible:bg-surface-400 disabled:text-surface-500"
			>
				{#if $entryFilters.length > 0}
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
		<div class="grid grid-cols-1 gap-2 py-2">
			<AddCurationEntry position={1} />
			{#each $filteredEntries as entry (entry.id)}
				{#if $editMode || entry.isVisible()}
					<CurationGroupCard entries={[entry]} />
				{/if}
			{/each}
		</div>
	{/if}
</div>
