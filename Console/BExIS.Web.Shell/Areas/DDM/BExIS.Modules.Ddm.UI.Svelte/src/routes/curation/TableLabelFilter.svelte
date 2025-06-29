<script lang="ts">
	import { faFilter } from '@fortawesome/free-solid-svg-icons';
	import { popup, type PopupSettings } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import { writable, type Readable, type Writable } from 'svelte/store';
	import { overviewStore } from './stores';

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

	const { curationLabels } = overviewStore;

	const filter = writable<string | undefined>(undefined);

	filter.subscribe((f) => {
		filterValue.set({ c: f });
	});

	const popupId = `${tableId}-${id}`;

	const popupFeatured: PopupSettings = {
		event: 'click',
		target: popupId,
		placement: 'bottom-start'
	};
</script>

<div id="parent-{popupId}">
	<button
		class:variant-filled-primary={$filter !== undefined}
		class="btn w-max p-2 text-xs"
		type="button"
		use:popup={popupFeatured}
		id="{popupId}-button"
		title="Open filter menu for {id} column"
	>
		<Fa icon={faFilter} />
	</button>
	<div data-popup={popupId} id={popupId} class="z-50">
		<div class="bg-base-100 card max-w-64 p-3 shadow-lg">
			<label class="label flex flex-col text-sm normal-case">
				<span>Only show datasets with label:</span>
				<select
					bind:value={$filter}
					class="select border border-primary-500 p-1 text-sm"
					title="Pick curation status to show"
				>
					<option value={undefined}>All</option>
					{#each $curationLabels as label}
						<option value={label.name}>{label.name}</option>
					{/each}
				</select>
			</label>
		</div>
	</div>
</div>
