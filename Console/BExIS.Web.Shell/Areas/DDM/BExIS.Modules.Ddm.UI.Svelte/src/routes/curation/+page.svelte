<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import CurationsOverview from './CurationsOverview.svelte';
	import Curation from './Curation.svelte';
	import { page } from '$app/stores';
	import type { CurationEntryClass } from './CurationEntries';
	import { CurationEntryType } from './types';
	import { RadioGroup, RadioItem, SlideToggle } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faAngleLeft, faAngleRight, faCircleDot } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';

	// const container = document.getElementById('curation');
	// $: datasetIdPar = Number(container?.getAttribute('dataset-id'));

	$: searchParDatasetId = Number($page.url.searchParams.get('id'));

	// Mock and Example Additions

	let showMock = false;

	let jumpToEntryWhereExample: ((entry: CurationEntryClass) => boolean) | undefined = undefined;

	const exampleJumpToFunction = () => {
		// jumps to the first loaded entry, that has "value" written in the name
		jumpToEntryWhereExample = (e) => e.name.includes('value');
	};

	let typeFilterExample: CurationEntryType | undefined = undefined;

	const applyfilterStatusExample = () => {
		// applies entry filter to curation so that only Datastructure Items are shown
		typeFilterExample = CurationEntryType.DatastructureEntryItem;
	};

	const typeViews = [
		{ value: CurationEntryType.MetadataEntryItem, name: 'Metadata' },
		{ value: CurationEntryType.DatastructureEntryItem, name: 'Datastructure' },
		{ value: CurationEntryType.PrimaryDataEntryItem, name: 'Primary Data' }
	];

	let currentTypeView = typeViews[0].value;

	$: typeFilterExample = currentTypeView; // filter curation entries according to current view

	enum PrimaryDataView {
		eval,
		table
	}

	const primaryDataViews = [
		{ value: PrimaryDataView.eval, name: 'Evaluation' },
		{ value: PrimaryDataView.table, name: 'Table View' }
	];

	let currentPrimaryDataView = primaryDataViews[0].value;

	$: overlayView =
		currentTypeView === CurationEntryType.PrimaryDataEntryItem &&
		currentPrimaryDataView === PrimaryDataView.table;

	let overlayActive = false;

	const toggleOverlay = () => {
		if (!overlayView) return;
		overlayActive = !overlayActive;
	};

	let overlayPosition: 'right' | 'left' = 'right';

	const toggleOverlayPosition = () => {
		overlayPosition = overlayPosition === 'right' ? 'left' : 'right';
	};
</script>

<Page title="Curation" contentLayoutType={pageContentLayoutType.full}>
	{#if !searchParDatasetId}
		<CurationsOverview />
	{:else}
		{#if !showMock}
			<!-- Standard implementation for Curation -->
			<div class="mx-auto max-w-3xl rounded border border-surface-500">
				{#key searchParDatasetId}
					<Curation
						datasetId={searchParDatasetId}
						bind:jumpToEntryWhere={jumpToEntryWhereExample}
						bind:applyTypeFilter={typeFilterExample}
					/>
				{/key}
			</div>
		{:else}
			<!-- How the component might be used with data next to it -->
			<div class="flex gap-x-12">
				<div class="size-full rounded border border-surface-500 bg-surface-300">
					<div class="m-2 rounded border border-surface-600 bg-surface-100 p-2">
						<h3 class="text-center text-lg">Pick Data View</h3>
						<RadioGroup class="-mt-2 w-full">
							{#each typeViews as typeView}
								<RadioItem bind:group={currentTypeView} {...typeView}>
									{typeView.name}
								</RadioItem>
							{/each}
						</RadioGroup>
						{#if currentTypeView === CurationEntryType.PrimaryDataEntryItem}
							<RadioGroup class="-mt-2 w-full">
								{#each primaryDataViews as primaryDataView}
									<RadioItem bind:group={currentPrimaryDataView} {...primaryDataView}>
										{primaryDataView.name}
									</RadioItem>
								{/each}
							</RadioGroup>
						{/if}
					</div>

					{#if currentTypeView === CurationEntryType.MetadataEntryItem}
						<div></div>
					{:else if currentTypeView === CurationEntryType.DatastructureEntryItem}
						<div></div>
					{:else if currentTypeView === CurationEntryType.PrimaryDataEntryItem}
						{#if currentPrimaryDataView === PrimaryDataView.eval}
							<div></div>
						{:else if currentPrimaryDataView === PrimaryDataView.table}
							<!-- Large Table -->
							<div>
								<table class="min-w-full border border-surface-500">
									<thead>
										<tr>
											{#each Array(20) as _, colIdx}
												<th class="border border-surface-500 bg-surface-200 px-2 py-1">
													Col {colIdx + 1}
												</th>
											{/each}
										</tr>
									</thead>
									<tbody>
										{#each Array(50) as _, rowIdx}
											<tr>
												{#each Array(20) as _, colIdx}
													<td class="border border-surface-500 px-2 py-1">
														{Math.floor(Math.random() * 1000)}
													</td>
												{/each}
											</tr>
										{/each}
									</tbody>
								</table>
							</div>
						{/if}
					{/if}
				</div>

				{#if overlayView}
					<!-- Curation entries overlay with toggle button -->
					<div
						class="fixed bottom-12 max-w-2xl overflow-hidden border border-surface-500 bg-surface-100 shadow-xl transition-all"
						class:rounded-full={!overlayActive}
						class:rounded-xl={overlayActive}
						class:!max-w-12={!overlayActive}
						style:left={overlayPosition === 'left' ? '3rem' : 'unset'}
						style:right={overlayPosition === 'right' ? '3rem' : 'unset'}
					>
						{#if !overlayActive}
							<button
								class="variant-filled-primary btn btn-icon"
								class:pointer-events-none={overlayActive}
								on:click={toggleOverlay}
								title="Show curation entries"
								in:fade={{ duration: 150 }}
								out:fade={{ duration: 150 }}
							>
								<Fa icon={faCircleDot} />
							</button>
						{/if}
						{#if overlayActive}
							<div
								class="flex items-stretch overflow-y-auto"
								style:height="calc(100vh - 12rem)"
								in:slide={{ duration: 150 }}
								out:slide={{ duration: 150 }}
							>
								{#key searchParDatasetId}
									<Curation
										datasetId={searchParDatasetId}
										bind:jumpToEntryWhere={jumpToEntryWhereExample}
										bind:applyTypeFilter={typeFilterExample}
									/>
								{/key}
							</div>
							<div
								class="flex w-full justify-stretch"
								class:flex-row-reverse={overlayPosition === 'left'}
								in:fade={{ duration: 150 }}
								out:fade={{ duration: 150 }}
							>
								<button
									class="variant-glass-surface btn flex gap-2 !rounded-none"
									class:flex-row-reverse={overlayPosition === 'left'}
									on:click={toggleOverlayPosition}
								>
									<Fa
										icon={faAngleLeft}
										class="inline-block transition-all {overlayPosition === 'left'
											? 'rotate-180'
											: ''}"
									/>
									Move overlay {overlayPosition === 'left' ? 'right' : 'left'}
								</button>
								<button
									class="variant-filled-primary btn grow !rounded-none"
									on:click={toggleOverlay}
								>
									<Fa icon={faCircleDot} class="mr-2 inline-block" />
									Close curation overlay
								</button>
							</div>
						{/if}
					</div>
				{:else}
					<!-- Curation entries on the right side -->
					<div class="w-5/12 shrink-0 grow-0 rounded border border-surface-500">
						{#key searchParDatasetId}
							<Curation
								datasetId={searchParDatasetId}
								bind:jumpToEntryWhere={jumpToEntryWhereExample}
								bind:applyTypeFilter={typeFilterExample}
							/>
						{/key}
					</div>
				{/if}
			</div>
		{/if}
		<div
			class="fixed bottom-0 left-0 z-50 w-full rounded-tr border-r border-t bg-surface-300 px-2 pt-1 text-center font-semibold opacity-50 hover:opacity-100 sm:w-auto"
		>
			<SlideToggle name="data integration mock toggle" size="sm" bind:checked={showMock}>
				Toggle mock for data integration
			</SlideToggle>
		</div>
	{/if}
</Page>
