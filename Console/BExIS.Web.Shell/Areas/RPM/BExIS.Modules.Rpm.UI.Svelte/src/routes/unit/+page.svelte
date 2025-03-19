<script lang="ts">
	import { onMount } from 'svelte';
	import { slide, fade } from 'svelte/transition';
	import { Modal, getModalStore, Toast } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();
	import {
		Page,
		Table,
		ErrorMessage,
		helpStore,
		TablePlaceholder,
		notificationStore,
		notificationType,
		pageContentLayoutType
	} from '@bexis2/bexis2-core-ui';
	import * as apiCalls from './services/apiCalls';
	import Form from './components/form.svelte';
	import TableElement from '../components/tableElement.svelte';
	import TableElements from '../components/tableElements.svelte';
	import TableOption from '../components/tableOptions.svelte';
	import { writable, type Writable } from 'svelte/store';
	import Fa from 'svelte-fa';
	import { faPlus, faXmark, faBan } from '@fortawesome/free-solid-svg-icons';

	import type { ModalSettings } from '@skeletonlabs/skeleton';
	import type { UnitListItem } from './models';
	import type { helpItemType } from '@bexis2/bexis2-core-ui';

	//help
	import help from './help/help.json';
	let helpItems: helpItemType[] = help.helpItems;

	let u: UnitListItem[] = [];
	let unit: UnitListItem;
	const tableStore = writable<any[]>([]);
	let showForm = false;
	$: units = u;
	$: tableStore.set(u);

	onMount(async () => {
		helpStore.setHelpItemList(helpItems);
		clear();
		showForm = false;
	});

	async function reload() {
		u = await apiCalls.GetUnits();
	}

	async function save(): Promise<void> {
		reload();
		toggleForm();
	}

	function clear() {
		unit = {
			id: 0,
			name: '',
			description: '',
			abbreviation: '',
			dimension: undefined,
			datatypes: [],
			measurementSystem: '',
			inUse: false,
			link: undefined
		};
	}

	function editUnit(type: any) {
		if (type.action == 'edit') {
			unit = { ...units.find((u) => u.id === type.id)! };
			showForm = true;
		}
		if (type.action == 'delete') {
			let u: UnitListItem = units.find((u) => u.id === type.id)!;
			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete Unit',
				body:
					'Are you sure you wish to delete Unit "' + unit.name + '" (' + unit.abbreviation + ')?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: async (r: boolean) => {
					if (r === true) {
						let success :boolean = await deleteUnit(u);
						if (success)
						{
							reload();
							if (u.id === unit.id)
							{ 
								toggleForm();
							}
						}
					}
				}
			};
			modalStore.trigger(confirm);
		}
	}

	async function deleteUnit(u: UnitListItem): Promise<boolean> {
		let success: boolean = await apiCalls.DeleteUnit(id);
		if (success != true) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'Can\'t delete Unit "' + u.name + '" (' + u.abbreviation + ').'
			});
			return false;
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Unit "' + u.name + '" (' + u.abbreviation + ') deleted.'
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
</script>

<Page help={true} title="Manage Units">
	<h1 class="h1">Units</h1>
	{#await reload()}
		<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h-9 w-96 placeholder animate-pulse" />
			<div class="flex justify-end">
				<button class="btn placeholder animate-pulse shadow-md h-9 w-16"
					><Fa icon={faPlus} /></button
				>
			</div>
		</div>

		<div class="table-container w-full">
			<TablePlaceholder cols={7} />
		</div>
	{:then}
		<!-- svelte-ignore a11y-click-events-have-key-events -->
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h3 h-9">
				{#if unit.id < 1}
					<span in:fade={{ delay: 400 }} out:fade>Create neẇ Unit</span>
				{:else}
					<span in:fade={{ delay: 400 }} out:fade>{unit.name}</span>
				{/if}
			</div>
			<div class="text-right">
				{#if !showForm}
					<!-- svelte-ignore a11y-mouse-events-have-key-events -->
					<button
						transition:fade
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						title="Create neẇ Unit"
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
				<Form {unit} {units} on:cancel={toggleForm} on:save={save} />
			</div>
		{/if}

		<div class="table table-compact w-full">
			<Table
				on:action={(obj) => editUnit(obj.detail.type)}
				config={{
					id: 'Units',
					data: tableStore,
					optionsComponent: TableOption,
					columns: {
						id: {
							disableFiltering: true,
							exclude: true
						},
						datatypes: {
							header: 'Data Types',
							disableFiltering: true,
							instructions: {
								renderComponent: TableElements
							}
						},
						dimension: {
							disableFiltering: true,
							instructions: {
								renderComponent: TableElement
							}
						},
						measurementSystem: {
							header: 'Measurement System'
						},
						link: {
							disableFiltering: true,
							header: 'External Link',
							instructions: {
								renderComponent: TableElement
							}
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
</Page>
<Modal />
