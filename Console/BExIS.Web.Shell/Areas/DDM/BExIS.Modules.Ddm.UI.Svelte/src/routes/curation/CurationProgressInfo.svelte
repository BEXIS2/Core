<script lang="ts">
	import Fa from 'svelte-fa';
	import { CurationEntryStatus, CurationEntryStatusDetails, CurationFilterType } from './types';
	import { curationStore } from './stores';
	import { derived, writable, type Writable } from 'svelte/store';
	import { faAngleDown, faAward } from '@fortawesome/free-solid-svg-icons';

	export let progress: number[];
	export let label: string = 'Curation Progress';
	export let expandWritable: Writable<boolean> | undefined = undefined;

	const { statusColorPalette } = curationStore;

	const totalIssues = progress.reduce((a, b) => a + b, 0);

	const hoveredStatus = writable<CurationEntryStatus | null>(null);

	const handleMouseEnter = (status: CurationEntryStatus) => {
		hoveredStatus.set(status);
	};

	const handleMouseLeave = () => {
		hoveredStatus.set(null);
	};

	const statusFilter = curationStore.getEntryFilterData(CurationFilterType.status);

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

	const toggleExpand = () => {
		if (!expandWritable) return;
		expandWritable.update((ew) => !ew);
	};
</script>

<div class="curation-status-progress-card p-2">
	<div class="flex">
		{#if expandWritable}
			<button on:click={toggleExpand} class="btn flex items-baseline rounded px-1 py-0.5">
				<Fa
					icon={faAngleDown}
					class="my-auto transition-all {$expandWritable ? ' rotate-180' : ''}"
				/>
				<span class="mr-2 font-semibold text-surface-800">{label}</span>
				<span class="whitespace-nowrap text-right text-sm font-normal text-surface-600">
					Total Issues: {totalIssues}
				</span>
			</button>
		{:else}
			<h3>
				<span class="mr-2 font-semibold text-surface-800">{label}</span>
				<span class="whitespace-nowrap text-right text-sm font-normal text-surface-600">
					Total Issues: {totalIssues}
				</span>
			</h3>
		{/if}
	</div>
	<div
		class="curation-status-progress-bar my-1 flex h-1.5 w-full flex-row overflow-hidden rounded-full transition-all"
	>
		{#if progress.some((p) => p > 0)}
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
		{:else}
			<div
				class="h-full grow transition-all"
				style="background-color: {$statusColorPalette.colors[CurationEntryStatus.Closed]}"
				class:opacity-25={$hasLowOpacity[CurationEntryStatus.Closed]}
				on:mouseenter={() => handleMouseEnter(CurationEntryStatus.Closed)}
				on:mouseleave={handleMouseLeave}
				role="presentation"
			></div>
		{/if}
	</div>
	<ul
		class="text-md curation-status-progress-list flex flex-row flex-wrap justify-between gap-x-4 text-sm"
	>
		{#if progress.slice(0, CurationEntryStatus.Closed).some((p) => p > 0)}
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
		{:else}
			<li class="w-full text-center">
				<div
					class="whitespace-nowrap transition-all"
					on:mouseenter={() => handleMouseEnter(CurationEntryStatus.Closed)}
					on:mouseleave={handleMouseLeave}
					class:opacity-25={$hasLowOpacity[CurationEntryStatus.Closed]}
					role="presentation"
				>
					<span
						class="font-semibold"
						style="color: {$statusColorPalette.colors[CurationEntryStatus.Closed]}"
					>
						{#if progress[CurationEntryStatus.Closed] > 0}
							<Fa icon={faAward} class="inline-block" />
							Everything clean and finished
						{:else}
							No entries
						{/if}
					</span>
				</div>
			</li>
		{/if}
	</ul>
</div>

<style lang="postcss">
	.curation-status-progress-card:hover .curation-status-progress-bar {
		@apply h-4;
	}
</style>
