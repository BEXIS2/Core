<script lang="ts">
	import Curation from './Curation.svelte';
	import {
		CurationEntryType,
		type CurationEntryHelperModel,
		type CurationEntryModel
	} from './types';
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { faAngleLeft, faCircleDot, faPlus } from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import type { Readable } from 'svelte/store';
	import { tick } from 'svelte';

	export let datasetId: number;

	let jumpToEntryWhere:
		| ((entryWhere: (entry: CurationEntryHelperModel) => boolean) => void)
		| undefined;

	let applyTypeFilter: ((type: CurationEntryType | undefined) => void) | undefined;

	let addEntry: (entry: Partial<CurationEntryModel>) => void;

	let curationEntriesReadable: Readable<Partial<CurationEntryHelperModel>[]>;

	// ---

	const typeViews = [
		{ value: CurationEntryType.MetadataEntryItem, name: 'Metadata' },
		{ value: CurationEntryType.DatastructureEntryItem, name: 'Datastructure' },
		{ value: CurationEntryType.PrimaryDataEntryItem, name: 'Primary Data' }
	];

	let currentTypeView = typeViews[0].value;

	$: {
		applyTypeFilter?.(currentTypeView); // filter curation entries according to current view
	}

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

	const type = CurationEntryType.MetadataEntryItem;
	const position = 999;

	const exampleMetadataInputs = [
		{ name: 'Abstract', type },
		{ name: 'Content type', type },
		{ name: 'Equipment', type },
		{ name: 'Number of plots', type },
		{ name: 'Keywords', type },
		{ name: 'Sampling design', type },
		{ name: 'Title', type },
		{ name: 'Data analysis', type },
		{ name: 'Study design', type },
		{ name: 'Process and service', type },
		{ name: 'Data upload status', type, position }
	];

	function addEntryClick(entry: Partial<CurationEntryModel>) {
		if (overlayView && !overlayActive) toggleOverlay();
		addEntry(entry);
	}

	// ---

	async function jumpToData(curationEntryHelper: Partial<CurationEntryHelperModel>) {
		if (!curationEntryHelper) return;
		currentTypeView = curationEntryHelper.type || CurationEntryType.None;
		await tick();
		const el = document.querySelector(`[data-name="${curationEntryHelper.name}"]`) as HTMLElement;
		if (el) {
			el.scrollIntoView({ behavior: 'smooth', block: 'center' });
			await tick();
			const prevBoxShadow = el.style.boxShadow;
			// huge, soft, orange box shadow
			el.style.boxShadow = '0 0 5px 5px orange';
			el.style.transition = 'box-shadow 1s ease';
			setTimeout(() => (el.style.boxShadow = prevBoxShadow), 1500);
		}
	}

	// run highlighting update whenever currentViewType or curationEntries change
	$: if (currentTypeView || $curationEntriesReadable) {
		refreshHighlighting();
	}

	async function refreshHighlighting() {
		if (!$curationEntriesReadable) return;
		await tick();
		if (!$curationEntriesReadable) return;
		$curationEntriesReadable.forEach((e) => {
			if (e.type !== CurationEntryType.MetadataEntryItem) return;
			const el = document.querySelector(`[data-name="${e.name}"]`) as HTMLElement;
			if (el) {
				el.style.boxShadow = `0 0 0 1px ${e.statusColor}`;
			}
		});
	}
</script>

<!-- How the component might be used with data next to it -->
<div class="flex gap-x-12">
	<div
		class="max-h-fit-screen size-full overflow-y-auto rounded border border-surface-500 bg-surface-300"
	>
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
			<div class="m-2">
				{#each exampleMetadataInputs as metadataInput}
					<label class="relative mt-24" data-name={metadataInput.name}>
						<span>{metadataInput.name}</span>
						<input type="text" class="input" />
						<button
							class="variant-ghost-primary btn btn-icon absolute right-3 top-3 !size-6"
							title="Create entry for {metadataInput.name}"
							on:click={() => addEntryClick(metadataInput)}
						>
							<Fa icon={faPlus} />
						</button>
					</label>
				{/each}
			</div>
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
										<td class="group relative border border-surface-500 px-2 py-1">
											{Math.floor(Math.random() * 1000)}
											<button
												class="variant-ghost-primary btn btn-icon absolute right-1 top-1 !size-6 text-xs opacity-0 transition-opacity group-hover:opacity-100"
												title="Create entry for col {colIdx + 1} row {rowIdx + 1}"
												on:click={() =>
													addEntryClick({
														name: `Col ${colIdx + 1} Row ${rowIdx + 1}`,
														type: CurationEntryType.PrimaryDataEntryItem
													})}
											>
												<Fa icon={faPlus} />
											</button>
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
			class="fixed bottom-12 max-w-2xl overflow-hidden border border-surface-500 bg-surface-100 shadow-xl transition-[border-radius,max-width]"
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
					class="flex resize items-stretch overflow-y-auto"
					style:height="calc(100vh - 12rem)"
					in:slide={{ duration: 150 }}
					out:slide={{ duration: 150 }}
				>
					{#key datasetId}
						<Curation
							{datasetId}
							enableJumpToData={true}
							on:jumpToData={(event) => jumpToData(event.detail)}
							bind:applyTypeFilter
							bind:addEntry
							bind:jumpToEntryWhere
							bind:curationEntriesReadable
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
							class="inline-block transition-[transform] {overlayPosition === 'left'
								? 'rotate-180'
								: ''}"
						/>
						Move {overlayPosition === 'left' ? 'right' : 'left'}
					</button>
					<button class="variant-filled-primary btn grow !rounded-none" on:click={toggleOverlay}>
						<Fa icon={faCircleDot} class="mr-2 inline-block" />
						Close curation overlay
					</button>
				</div>
			{/if}
		</div>
	{:else}
		<!-- Curation entries on the right side -->
		<div
			class="max-h-fit-screen w-5/12 shrink-0 grow-0 overflow-y-auto rounded border border-surface-500"
		>
			{#key datasetId}
				<Curation
					{datasetId}
					enableJumpToData={true}
					on:jumpToData={(event) => jumpToData(event.detail)}
					bind:applyTypeFilter
					bind:addEntry
					bind:jumpToEntryWhere
					bind:curationEntriesReadable
				/>
			{/key}
		</div>
	{/if}
</div>

<style lang="postcss">
	.max-h-fit-screen {
		max-height: calc(100vh - 12rem);
	}
</style>
