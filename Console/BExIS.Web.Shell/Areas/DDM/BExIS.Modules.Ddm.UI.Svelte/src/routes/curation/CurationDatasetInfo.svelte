<script lang="ts">
	import { curationStore } from './stores';
	import RelativeDate from '$lib/components/RelativeDate.svelte';
	import CurationProgressInfo from './CurationProgressInfo.svelte';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';

	const { datasetId, curation, loadingCuration, loadingError, uploadingEntries } = curationStore;

	let curationLabel = 'None';
</script>

<div class="border-surface-500 w-full border-b">
	{#if $loadingCuration}
		<div class="f-width flex animate-pulse flex-wrap justify-stretch gap-2 p-2">
			<div class="placeholder grow basis-10/12"></div>
			<div class="placeholder grow basis-1/12"></div>
			<div class="placeholder grow basis-2/5"></div>
			<div class="placeholder grow basis-2/5"></div>
			<div class="placeholder grow basis-2/5"></div>
			<div class="placeholder grow basis-2/5"></div>
			<div class="placeholder grow basis-full"></div>
		</div>
	{:else if $loadingError}
		<h1>Error: {$loadingError}</h1>
	{:else if $curation}
		<div class="row border-surface-500 flex w-full flex-nowrap justify-between border-b p-2">
			<h1 class="flex-shrink flex-grow-0 overflow-hidden text-ellipsis text-xl">
				<span class="text-surface-800 text-base">
					#{$datasetId}:
				</span>
				{$curation?.datasetTitle}
			</h1>
			{#if $uploadingEntries.length > 0}
				<span
					class="bg-surface-400 ml-2 h-7 w-20 animate-pulse rounded-full px-4 py-1 text-sm text-white"
				>
					...
				</span>
			{:else}
				<span
					class="bg-primary-400 ml-2 h-7 max-w-48 overflow-hidden text-ellipsis rounded-full px-4 py-1 text-sm text-white"
				>
					{curationLabel}
				</span>
			{/if}
		</div>
		<div class="relative">
			<!-- Dates -->
			<div
				class="row border-surface-500 flex w-full flex-wrap justify-between gap-x-4 gap-y-2 border-b p-2"
			>
				<div class="grid grid-cols-1">
					<span class="text-surface-800 text-sm font-semibold">Last Dataset Change</span>
					<RelativeDate date={$curation?.datasetVersionDateObj} class="text-surface-800 text-sm" />
				</div>
				<div class="grid grid-cols-1">
					<span class="text-surface-800 text-sm font-semibold">Begin of Curation</span>
					<RelativeDate date={$curation?.creationDate} class="text-surface-800 text-sm" />
				</div>
				<div class="grid grid-cols-1">
					<span class="text-surface-800 text-sm font-semibold">Last User Change</span>
					<RelativeDate date={$curation?.lastUserChangedDate} class="text-surface-800 text-sm" />
				</div>
				<div class="grid grid-cols-1">
					<span class="text-surface-800 text-sm font-semibold">Last Curator Change</span>
					<RelativeDate date={$curation?.lastCuratorChangedDate} class="text-surface-800 text-sm" />
				</div>
			</div>
			<!-- Curation Progress -->
			<CurationProgressInfo
				progress={$curation?.curationProgressTotal}
				label="Curation Progress Test"
			/>
			<!-- Spinner overlay -->
			{#if $uploadingEntries.length > 0}
				<SpinnerOverlay />
			{/if}
		</div>
	{/if}
</div>
