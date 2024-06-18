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
	});

	async function reload(): Promise<void> {
		showForm = false;
		dts = await apiCalls.GetDataTypes();
		clear();
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
		dataType = { ...dataTypes.find((dt) => dt.id === type.id)! };
		if (type.action == 'edit') {
			showForm = true;
		}
		if (type.action == 'delete') {
			const modal: ModalSettings = {
				type: 'confirm',
				title: 'Delete Data Type',
				body:
					'Are you sure you wish to delete Data Type "' +
					dataType.name +
					'" (' +
					dataType.systemType +
					')?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: (r: boolean) => {
					if (r === true) {
						deleteDataType(type.id);
					}
				}
			};
			modalStore.trigger(modal);
		}
	}

	async function deleteDataType(id: number) {
		let success = await apiCalls.DeleteDataType(id);
		if (success != true) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete Data Type "' + dataType.name + '".'
			});
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Data Type "' + dataType.name + '" deleted.'
			});
		}
		reload();
	}

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
	}
</script>

<Page help={true} title="Manage Data Types">
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
						Create neẇ Data Type
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
					<Form {dataType} {dataTypes} on:cancel={toggleForm} on:save={reload} />
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
