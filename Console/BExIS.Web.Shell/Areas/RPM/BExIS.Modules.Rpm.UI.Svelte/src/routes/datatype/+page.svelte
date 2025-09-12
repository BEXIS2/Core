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
	import type { DataTypeListItem } from './models';

	import type { linkType } from '@bexis2/bexis2-core-ui';
	const modalStore = getModalStore();

	let dts: DataTypeListItem[] = [];
	const tableStore = writable<any[]>([]);
	let dataType: DataTypeListItem;
	let showForm = false;
	$: dataTypes = dts;
	$: tableStore.set(dts);

	//help
	import help from './help/help.json';
	let helpItems: helpItemType[] = help.helpItems;

	onMount(async () => {
		helpStore.setHelpItemList(helpItems);
		showForm = false;
		clear();
	});

	async function reload(): Promise<void> {
		dts = await apiCalls.GetDataTypes();
	}

	async function save(): Promise<void> {
		reload();
		toggleForm();
	}

	async function clear() {
		dataType = {
			id: 0,
			name: '',
			description: '',
			systemType: '',
			inUse: false
		};
	}

	function editDataType(type: any) {
		if (type.action == 'edit') {
			dataType = { ...dataTypes.find((dt) => dt.id === type.id)! };
			showForm = true;
		}
		if (type.action == 'delete') {
			let dt: DataTypeListItem = dataTypes.find((dt) => dt.id === type.id)!;
			const modal: ModalSettings = {
				type: 'confirm',
				title: 'Delete Data Type',
				body:
					'Are you sure you wish to delete Data Type "' +
					dt.name +
					'" (' +
					dt.systemType +
					')?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						let success :boolean = await deleteDataType(dt);
						if (success)
						{
							reload();
							if (dt.id === dataType.id) {
								toggleForm();
							}
						}
					}
				}
			};
			modalStore.trigger(modal);
		}
	}

	async function deleteDataType(dt: DataTypeListItem): Promise<boolean> {
		let success = await apiCalls.DeleteDataType(dt.id);
		if (success != true) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete Data Type "' + dt.name + '".'
			});
			return false;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Data Type "' + dt.name + '" deleted.'
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
			url: '/home/docs/Data%20Description#data-types',
		}
	];
</script>

<Page help={true} title="Manage Data Types" {links}>
	<div class="w-full">
		<h1 class="h1">Data Types</h1>

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
					{#if dataType.id < 1}
						Create new Data Type
					{:else}
						{dataType.name}
					{/if}
				</div>
				<div class="text-right">
					{#if !showForm}
						<!-- svelte-ignore a11y-mouse-events-have-key-events -->
						<button
							in:fade
							out:fade
							class="btn variant-filled-secondary shadow-md h-9 w-16"
							title="Create new Data Type"
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
					<Form {dataType} {dataTypes} on:cancel={toggleForm} on:save={save} />
				</div>
			{/if}

			<div class="table table-compact w-full">
				<Table
					on:action={(obj) => editDataType(obj.detail.type)}
					config={{
						id: 'dataTypes',
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
