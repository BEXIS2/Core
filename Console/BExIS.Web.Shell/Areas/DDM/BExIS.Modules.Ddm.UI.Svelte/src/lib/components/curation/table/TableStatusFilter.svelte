<script lang="ts">
	import { CurationStatusLabels } from '$lib/models/CurationStatusEntry';
	import { faFilter } from '@fortawesome/free-solid-svg-icons';
	import { popup, type PopupSettings } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { writable, type Readable, type Writable } from 'svelte/store';

	export let id: string;
	export let tableId: string;

	export let filterValue: Writable<any>;

	// to prevent warnings
	export let toStringFn: (d: any) => string;
	export let toFilterableValueFn: (d: any) => any;
	export let values: Readable<any>;
	export let filters: Writable<any>;
	export let pageIndex: Writable<any>;
	toStringFn;
	toFilterableValueFn;
	values;
	filters;
	pageIndex;

	const filter = writable<Set<number> | undefined>(undefined);

	filter.subscribe((f) => {
		filterValue.set({ i: f });
	});

	const popupId = `${tableId}-${id}`;

	const popupFeatured: PopupSettings = {
		event: 'click',
		target: popupId,
		placement: 'bottom-end'
	};

	function toggleLabelFilter(status: number) {
		filter.update((f) => {
			if (!f) f = new Set();
			if (f.has(status)) f.delete(status);
			else f.add(status);
			return f;
		});
	}

	const clearFilter = () => {
		filter.set(undefined);
	};
</script>

<div id="parent-{popupId}">
	<button
		class:variant-filled-primary={$filter !== undefined && $filter?.size > 0}
		class="btn w-max p-2 text-xs"
		type="button"
		use:popup={popupFeatured}
		id="{popupId}-button"
		title="Open filter menu for {id} column"
	>
		<Fa icon={faFilter} />
	</button>
	<div data-popup={popupId} id={popupId} class="z-50 font-normal">
		<div class="bg-base-100 card grid max-w-64 gap-2 p-3 shadow-lg">
			<button
				class="variant-filled-primary btn btn-sm"
				type="button"
				title="Clear filters"
				on:click|preventDefault={clearFilter}>Clear Filters</button
			>

			<label class="label flex flex-col text-sm normal-case">
				<span>Only show datasets with labels:</span>
				<div class="label-container flex flex-wrap justify-around gap-1 rounded p-1">
					<label class="cursor-pointer rounded-full bg-surface-300 px-2 py-0.5 transition-opacity">
						<input
							class="pointer-events-none absolute opacity-0"
							type="checkbox"
							title="Hide/Show not curated entries"
							checked={$filter?.has(-1)}
							on:click={() => toggleLabelFilter(-1)}
						/>
						<span>Not Curated</span>
					</label>
					{#each CurationStatusLabels as label, index}
						<label
							class="cursor-pointer rounded-full bg-surface-300 px-2 py-0.5 transition-opacity"
							style="background-color: {CurationStatusLabels[index]
								.bgColor}; color: {CurationStatusLabels[index].fontColor};"
						>
							<input
								class="pointer-events-none absolute opacity-0"
								type="checkbox"
								title="Hide/Show '{label.name}' entries"
								checked={$filter?.has(index)}
								on:click={() => toggleLabelFilter(index)}
							/>
							<span>{label.name}</span>
						</label>
					{/each}
				</div>
			</label>
			<span class="text-center text-xs normal-case text-surface-600">
				Click on the labels to toggle them
			</span>
		</div>
	</div>
</div>

<style lang="postcss">
	.label-container label:has(:focus-visible) {
		@apply shadow-sm ring-4 ring-blue-500;
	}

	.label-container:has(input:checked) label:not(:has(input:checked)) {
		@apply opacity-25;
	}
</style>
