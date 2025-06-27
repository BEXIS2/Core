<script lang="ts">
	import { curationStore } from './stores';
	import RelativeDate from '$lib/components/RelativeDate.svelte';

	const { datasetId, curation, loadingCuration, loadingError, uploadingEntries } = curationStore;
</script>

<div class="w-full">
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
		<div class="row flex w-full flex-nowrap justify-between border-b border-surface-500 p-2">
			<h1 class="flex-shrink flex-grow-0 overflow-hidden text-ellipsis text-xl">
				<span class="text-base text-surface-800">
					#{$datasetId}:
				</span>
				{$curation?.datasetTitle}
				<!-- There would be space for a label or badge here as well -->
			</h1>
		</div>
		<div class="relative">
			<!-- Dates -->
			<div
				class="row flex w-full flex-wrap justify-between gap-x-4 gap-y-2 border-b border-surface-500 p-2"
			>
				<div class="grid grid-cols-1">
					<span class="text-sm font-semibold text-surface-800">Last Dataset Change</span>
					<RelativeDate date={$curation?.datasetVersionDateObj} class="text-sm text-surface-800" />
				</div>
				{#if $curation.curationEntries.length > 0}
					<div class="grid grid-cols-1">
						<span class="text-sm font-semibold text-surface-800">Begin of Curation</span>
						<RelativeDate date={$curation?.creationDate} class="text-sm text-surface-800" />
					</div>
					<div class="grid grid-cols-1">
						<span class="text-sm font-semibold text-surface-800">Last User Change</span>
						<RelativeDate date={$curation?.lastUserChangedDate} class="text-sm text-surface-800" />
					</div>
					<div class="grid grid-cols-1">
						<span class="text-sm font-semibold text-surface-800">Last Curator Change</span>
						<RelativeDate
							date={$curation?.lastCuratorChangedDate}
							class="text-sm text-surface-800"
						/>
					</div>
				{/if}
			</div>
		</div>
	{/if}
</div>
