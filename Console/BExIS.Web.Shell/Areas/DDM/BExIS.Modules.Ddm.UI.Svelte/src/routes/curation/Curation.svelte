<script lang="ts">
	import { onMount } from 'svelte';
	import CurationDatasetInfo from './CurationDatasetInfo.svelte';
	import { curationStore } from './stores';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import CurationGroupCard from './CurationGroupCard.svelte';
	import Fa from 'svelte-fa';
	import { faBroom } from '@fortawesome/free-solid-svg-icons';

	export let datasetId: number;

	const { loadingCuration, loadingError, entryFilters } = curationStore;

	const entries = curationStore.getFilteredEntriesReadable();

	onMount(async () => {
		if (datasetId) {
			console.log('Curation datasetId:', datasetId);
			curationStore.datasetId.set(datasetId); // automatically fetches dataset
		}
	});

	const clearFilters = () => {
		curationStore.clearEntryFilters();
	};
</script>

<div class="border-surface-500 container mx-auto grid max-w-3xl rounded border">
	<CurationDatasetInfo />

	<button
		on:click={clearFilters}
		disabled={$entryFilters.length === 0}
		class="disabled:text-surface-500 bg-surface-200 enabled:focus-visible:bg-surface-400 enabled:hover:bg-surface-400 m-1 rounded p-1"
	>
		<Fa icon={faBroom} class="mr-1 inline-block" />
		{$entryFilters.length === 0 ? 'No Filters' : 'Clear Filters'}
	</button>

	{#if $loadingCuration}
		<div class="flex h-96 items-center justify-center">
			<Spinner label="Loading Curation Entries..." />
		</div>
	{:else if $loadingError}
		<div>
			<p class="text-red-500">Error loading curation entries</p>
		</div>
	{:else}
		<div class="grid grid-cols-1 gap-2 py-2">
			{#each $entries as entry (entry.id)}
				<CurationGroupCard entries={[entry]} />
			{/each}
		</div>
	{/if}
</div>
