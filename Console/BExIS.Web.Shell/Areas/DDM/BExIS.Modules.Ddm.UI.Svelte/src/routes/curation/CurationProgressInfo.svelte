<script lang="ts">
	import Fa from 'svelte-fa';
	import { CurationEntryStatus, CurationEntryStatusDetails } from './types';
	import { curationStore } from './stores';
	import { derived, writable } from 'svelte/store';

	export let progress: number[];
	export let totalIssues: number = 0;
	export let label: string = 'Curation Progress';

	const hoveredStatus = writable<CurationEntryStatus | null>(null);

	const { statusColorPalette } = curationStore;

	const statusFilterId = 'status';

	const handleMouseEnter = (status: CurationEntryStatus) => {
		hoveredStatus.set(status);
	};

	const handleMouseLeave = () => {
		hoveredStatus.set(null);
	};

	const statusFilter = curationStore.getEntryFilterData(statusFilterId);

	const hasLowOpacity = derived(
		[statusFilter, hoveredStatus],
		([$statusFilter, $hoveredStatus]) => {
			if ($hoveredStatus === null && (!$statusFilter || $statusFilter?.data.size === 0)) {
				return progress.map(() => false);
			}
			return progress.map((_, index) => {
				return index !== $hoveredStatus && !$statusFilter?.data.has(index);
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
				<div
					class="h-full transition-all"
					style="flex-grow: {p}; background-color: {$statusColorPalette.colors[index]}"
					class:opacity-25={$hasLowOpacity[index]}
					on:mouseenter={() => handleMouseEnter(index)}
					on:mouseleave={handleMouseLeave}
					role="presentation"
				></div>
			{/if}
		{/each}
	</div>
	<ul
		class="text-md curation-status-progress-list flex flex-row flex-wrap justify-between gap-x-4 text-sm"
	>
		{#each progress as p, index}
			{#if p > 0}
				<li>
					<div
						class="whitespace-nowrap transition-all"
						on:mouseenter={() => handleMouseEnter(index)}
						on:mouseleave={handleMouseLeave}
						class:opacity-25={$hasLowOpacity[index]}
						role="presentation"
					>
						<span class="font-semibold" style="color: {$statusColorPalette.colors[index]}">
							<Fa icon={CurationEntryStatusDetails[index].icon} class="inline-block" />
							{CurationEntryStatusDetails[index].name}: {p}
						</span>
						<span class="text-xs text-surface-800">{((p / totalIssues) * 100).toFixed(2)}%</span>
					</div>
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
