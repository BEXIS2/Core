<script lang="ts">
	import { onMount } from 'svelte';
	import { slide, fade } from 'svelte/transition';
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		TablePlaceholder,
		notificationStore,
		notificationType,
		type helpItemType
	} from '@bexis2/bexis2-core-ui';
	import * as apiCalls from './services/apiCalls';
	import Form from './components/form.svelte';
	import TableOption from '../components/tableOptions.svelte';
	import { writable, type Writable } from 'svelte/store';
	import Fa from 'svelte-fa';
	import { faPlus, faXmark } from '@fortawesome/free-solid-svg-icons';

	import type { ModalSettings } from '@skeletonlabs/skeleton';
	import type { DimensionListItem } from './models';

	import type { linkType } from '@bexis2/bexis2-core-ui';

	// modal
	const modalStore = getModalStore();

	//help
	import help from './help/help.json';
	let helpItems: helpItemType[] = help.helpItems;

	let ds: DimensionListItem[] = [];
	const tableStore = writable<any[]>([]);
	let dimension: DimensionListItem;
	let showForm = false;
	$: dimensions = ds;
	$: tableStore.set(ds);

	onMount(async () => {
		helpStore.setHelpItemList(helpItems);
		clear();
		showForm = false;
	});

	async function reload(): Promise<void> {
		ds = await apiCalls.GetDimensions();
	}

	async function save(): Promise<void> {
		reload();
		toggleForm();
	}

	async function clear() {
		dimension = {
			id: 0,
			name: '',
			description: '',
			specification: '',
			inUse: false
		};
	}

	function editDimension(type: any) {
		if (type.action == 'edit') {
			dimension = { ...dimensions.find((d) => d.id === type.id)! };
			showForm = true;
		}
		if (type.action == 'delete') {
			let d: DimensionListItem = dimensions.find((d) => d.id === type.id)!;
			const modal: ModalSettings = {
				type: 'confirm',
				title: 'Delete Dimension',
				body:
					'Are you sure you wish to delete Dimension "' +
					d.name +
					'" (' +
					d.specification +
					')?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						let success: boolean = await deleteDimension(d);
						if (success)
						{
							reload();
							if (d.id === dimension.id) {
								toggleForm();
							}
						}
					}
				}
			};
			modalStore.trigger(modal);
		}
	}

	async function deleteDimension(d: DimensionListItem): Promise<boolean> {
		let success = await apiCalls.DeleteDimension(d.id);
		if (success != true) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete Dimension "' + d.name + '".'
			});
			return false;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Dimension "' + d.name + '" deleted.'
			});
			return true;
		}
	}

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
	}

	let links:linkType[] = [
		{
			label: 'Manual',
			url: '/home/docs/Data%20Description#dimensions',
		}
	];
</script>

<Page help={true} title="Manage Dimensions" {links}>
	<div class="w-full">
		<h1 class="h1">Dimensions</h1>

		{#await reload()}
			<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
				<div class="h-9 w-96 placeholder animate-pulse" />
				<div class="flex justify-end">
					<button class="btn placeholder animate-pulse shadow-md h-9 w-16"
						><Fa icon={faPlus} /></button
					>
				</div>
			</div>
			<div>
				<TablePlaceholder cols={4} />
			</div>
		{:then}
			<!-- svelte-ignore a11y-click-events-have-key-events -->
			<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
				<div class="h3 h-9">
					{#if dimension.id < 1}
						Create new Dimension
					{:else}
						{dimension.name}
					{/if}
				</div>
				<div class="text-right">
					{#if !showForm}
						<!-- svelte-ignore a11y-mouse-events-have-key-events -->
						<button
							in:fade
							out:fade
							class="btn variant-filled-secondary shadow-md h-9 w-16"
							title="Create new Dimension"
							id="create"
							on:mouseover={() => {
								helpStore.show('create');
							}}
							on:click={() => toggleForm()}><Fa icon={faPlus} /></button
						>
					{/if}
				</div>
			</div>

			{#if showForm}
				<div in:slide out:slide>
					<Form {dimension} {dimensions} on:cancel={toggleForm} on:save={save} />
				</div>
			{/if}

			<div class="table table-compact w-full">
				<Table
					on:action={(obj) => editDimension(obj.detail.type)}
					config={{
						id: 'dimensions',
						data: tableStore,
						optionsComponent: TableOption,
						columns: {
							id: {
								exclude: true
							},
							inUse: {
								disableFiltering: true,
								exclude: true
							}
						}
					}}
				/>
			</div>
		{:catch error}
			<ErrorMessage {error} />
		{/await}
	</div>
</Page>
<Modal />
