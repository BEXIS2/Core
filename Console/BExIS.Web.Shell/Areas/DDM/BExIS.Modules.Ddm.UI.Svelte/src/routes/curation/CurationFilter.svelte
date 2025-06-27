<script lang="ts">
	import { faFilter } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import { CurationEntryStatusDetails } from './types';
	import Fa from 'svelte-fa';

	const { statusFilter } = curationStore;
</script>

<div class="filter-container flex flex-wrap items-center gap-1 p-2">
	<Fa icon={faFilter} class="inline-block" />
	<span>Filter:</span>
	{#each CurationEntryStatusDetails as statusDetails, index}
		<label class="cursor-pointer rounded border border-surface-500 bg-surface-300 px-2 py-0.5">
			<input
				class="pointer-events-none absolute opacity-0"
				type="checkbox"
				title="Hide/Show '{statusDetails.name}' entries"
				checked={$statusFilter.has(index)}
				on:click={() => curationStore.toggleStatusFilter(index)}
			/>
			<span>{statusDetails.name}</span>
		</label>
	{/each}
</div>

<style lang="postcss">
	.filter-container label:has(:focus-visible) {
		@apply shadow-sm ring-2 ring-blue-500;
	}

	.filter-container label:hover {
		@apply bg-surface-300 text-surface-900 ring-2;
	}

	.filter-container label:has(:checked) {
		@apply border-surface-800 bg-surface-500 text-surface-900 ring-1 ring-surface-800;
	}
</style>
