<script lang="ts">
	import { onMount } from 'svelte';
	import { fade } from 'svelte/transition';
	import { Modal, modalStore } from '@skeletonlabs/skeleton';
	import { Spinner, Table } from '@bexis2/bexis2-core-ui';
	import * as apiCalls from './services/apiCalls';
	import Form from './components/form.svelte';
	import TableOption from './components/tableOptions.svelte';
	import { writable, type Writable } from 'svelte/store';

	import type { ModalSettings } from '@skeletonlabs/skeleton';
	import type { DimensionListItem } from './models';

	let ds: DimensionListItem[] = [];
	const tableStore = writable<any[]>([]);
	let dimension: DimensionListItem;
	let showForm = false;
	$: dimensions = ds;
	$: tableStore.set(ds);

	onMount(async () => {
		ds = await apiCalls.GetDimensions();
		clear();
	});

	async function reload(): Promise<void> {
		showForm = false;
		ds = await apiCalls.GetDimensions();
		clear();
	}

	async function clear() {
		dimension = {
			id: 0,
			name: '',
			description: '',
			specification: ''
		};
	}

	function editDimension(type: any) {
		dimension = dimensions.find((d) => d.id === type.id)!;
		if (type.action == 'edit') {
			showForm = true;
		}
		if (type.action == 'delete') {
			const modal: ModalSettings = {
				type: 'confirm',
				title: 'Delete Unit',
				body:
					'Are you sure you wish to delete Dimension "' +
					dimension.name +
					'" (' +
					dimension.specification +
					')?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: (r: boolean) => {
					if (r === true) {
						deleteDimension(type.id);
					}
				}
			};
			modalStore.trigger(modal);
		}
	}

	async function deleteDimension(id: number) {
		let test = await apiCalls.DeleteDimension(id);
		console.log('deleted', test);
		reload();
	}
</script>

<div class="p-5">
	{#if ds.length > 0 && ds}
		<h1>dimensions</h1>

		<div class="py-5">
			{#if showForm}
				<div in:fade out:fade>
					<Form {dimension} {dimensions} on:cancel={reload} on:save={reload} />
				</div>
			{:else}
				<button type="button" class="btn variant-filled" on:click={() => (showForm = !showForm)}
					>+</button
				>
			{/if}
		</div>

		<div class="w-max">
			<Table
				on:action={(obj) => editDimension(obj.detail.type)}
				config={{
					id: 'Units',
					data: tableStore,
					optionsComponent: TableOption,
					columns: {
						id: {
							exclude: true
						}
					}
				}}
			/>
		</div>
	{:else}
		<Spinner />
	{/if}
</div>

<Modal />
