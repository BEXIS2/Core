<script lang="ts">
	import Fa from 'svelte-fa';
	import { CurationEntryStatus, CurationEntryStatusDetails } from './types';
	import { curationStore } from './stores';
	import { derived, writable } from 'svelte/store';
	import type { CurationEntryClass } from './CurationEntries';

	export let progress: number[];
	export let totalIssues: number = 0;
	export let label: string = 'Curation Progress';

	const myFilter = writable<((entry: CurationEntryClass) => boolean) | null>(null);
	const appliedFilter = writable<CurationEntryStatus | null>(null);
	const hoveredStatus = writable<CurationEntryStatus | null>(null);

	const { statusColorPalette } = curationStore;

	curationStore.entryFilters.subscribe((filters) => {
		if (filters.length === 0) {
			myFilter.set(null);
			appliedFilter.set(null);
		}
	});

	const addFilter = (status: CurationEntryStatus) => {
		myFilter.update((filters) => {
			let newFilter = (entry: CurationEntryClass) => entry.status === status;
			if (filters !== null) {
				curationStore.removeEntryFilter(filters);
			}
			curationStore.addEntryFilter(newFilter);
			return newFilter;
		});
	};

	const removeFilter = () => {
		myFilter.update((filters) => {
			if (filters !== null) {
				curationStore.removeEntryFilter(filters);
			}
			return null;
		});
	};

	const handleClick = (status: CurationEntryStatus) => {
		appliedFilter.update((last) => {
			if (last === status) {
				removeFilter();
				return null;
			} else {
				addFilter(status);
				return status;
			}
		});
	};

	const handleMouseEnter = (status: CurationEntryStatus) => {
		hoveredStatus.set(status);
	};

	const handleMouseLeave = () => {
		hoveredStatus.set(null);
	};

	const hasLowOpacity = derived(
		[appliedFilter, hoveredStatus],
		([appliedFilter, hoveredStatus]) => {
			if (hoveredStatus === null && appliedFilter === null) {
				return progress.map(() => false);
			}
			return progress.map((_, index) => {
				return index !== appliedFilter && index !== hoveredStatus;
			});
		}
	);
</script>

<div class="curation-status-progress-card p-2">
	<h3>
		<span class="mr-2 font-semibold text-surface-800">{label}</span>
		<span class="whitespace-nowrap text-right text-sm font-normal text-surface-600">
			Total Issues: {totalIssues}
		</span>
	</h3>
	<div
		class="curation-status-progress-bar my-1 flex h-1.5 w-full flex-row overflow-hidden rounded-full transition-all"
	>
		{#each progress as p, index}
			{#if p > 0}
				<button
					class="h-full transition-all"
					style="flex-grow: {p}; background-color: {$statusColorPalette.colors[index]}"
					on:click={() => handleClick(index)}
					class:opacity-50={$hasLowOpacity[index]}
					on:mouseenter={() => handleMouseEnter(index)}
					on:mouseleave={handleMouseLeave}
				></button>
			{/if}
		{/each}
	</div>
	<ul
		class="text-md curation-status-progress-list flex flex-row flex-wrap justify-between gap-x-4 text-sm"
	>
		{#each progress as p, index}
			{#if p > 0}
				<li>
					<button
						class="whitespace-nowrap transition-all"
						on:click={() => handleClick(index)}
						on:mouseenter={() => handleMouseEnter(index)}
						on:mouseleave={handleMouseLeave}
						class:opacity-50={$hasLowOpacity[index]}
					>
						<span class="font-semibold" style="color: {$statusColorPalette.colors[index]}">
							<Fa icon={CurationEntryStatusDetails[index].icon} class="inline-block" />
							{CurationEntryStatusDetails[index].name}: {p}
						</span>
						<span class="text-xs text-surface-800">{((p / totalIssues) * 100).toFixed(2)}%</span>
					</button>
				</li>
			{/if}
		{/each}
	</ul>
</div>

<style lang="postcss">
	.curation-status-progress-card:hover .curation-status-progress-bar {
		@apply h-4;
	}
</style>
