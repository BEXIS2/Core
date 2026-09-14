<script lang="ts">
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import { notificationStore, notificationType, Page, Table, TablePlaceholder, type TableConfig } from '@bexis2/bexis2-core-ui';
	import { groupsStore, getGroups, deleteGroupById } from './services';
	import groupsTableOptions from '../../lib/components/groupsTableOptions.svelte';
	import { onMount, setContext } from 'svelte';
	import { slide } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import CreateGroup from '../../lib/components/createGroup.svelte';
	import UpdateGroup from '../../lib/components/updateGroup.svelte';
	import type { ComponentType, SvelteComponent } from 'svelte';
	import { getUsers } from '../users/services';
	import { get } from 'svelte/store';
	import type { CreateGroupModel, ReadGroupModel } from './types';

	$: formTitle =
		activeComponent === UpdateGroup && selectedGroup
			? `Update Group: ${selectedGroup.name}`
			: 'Create new Group';

	let activeComponent: ComponentType<SvelteComponent> | null = null;
	let selectedGroup: ReadGroupModel | CreateGroupModel | null = null;
	let loading = true;

	const modalStore = getModalStore();

	async function reload() {
		loading = true;
		await getGroups();
		await getUsers();
		loading = false;
	}

	function closeForm() {
		activeComponent = null;
		selectedGroup = null;
	}

	function handleSuccess() {
		closeForm();
		reload();
	}

	const groupsTableActions = (action: CustomEvent<{ type: string; row: ReadGroupModel }>) => {
		const { type, row } = action.detail;
		if (!row) return;

		switch (type) {
			case 'UPDATE':
				selectedGroup = row;
				activeComponent = UpdateGroup;
				break;

			case 'DELETE':
				modalStore.trigger({
					type: 'confirm',
					title: `Delete Group (<strong>${row.name}</strong>)`,
					body: `Are you sure you want to delete <strong>${row.name}</strong>?`,
					response: async (result: boolean) => {
						if (result) {
							await deleteGroupById(row.id);
							await closeForm();
							await reload();

							notificationStore.showNotification({
    							notificationType: notificationType.success,
    							message: `Deleted group (<strong>${row.name}</strong>) successfully.`,
   							});
						}
					}
				});
				break;

			default:
				break;
		}
	};

	onMount(reload);

	const groupsConfig: TableConfig<ReadGroupModel> = {
		id: 'groupsTable',
		data: groupsStore,
		optionsComponent: groupsTableOptions as ComponentType<SvelteComponent>,
		columns: {
			creationDate: {
				header: 'Creation Date',
				instructions: {
					toStringFn: (date: Date) =>
						date.toLocaleString('en-US', {
							month: 'short',
							day: 'numeric',
							year: 'numeric'
						}),
					toSortableValueFn: (date: Date) => date.getTime(),
					toFilterableValueFn: (date: Date) => date
				}
			},
			modificationDate: {
				header: 'Modification Date',
				instructions: {
					toStringFn: (date: Date) =>
						date.toLocaleString('en-US', {
							month: 'short',
							day: 'numeric',
							year: 'numeric'
						}),
					toSortableValueFn: (date: Date) => date.getTime(),
					toFilterableValueFn: (date: Date) => date
				}
			},
			userIds: { exclude: true }
		}
	};
</script>

<Page help={true} title="Manage Groups">
	<h1 class="h1">Groups</h1>
	<div class="table-container w-full">
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500 w-full">
			<div class="h3 h-9">{formTitle}</div>
			<div class="flex justify-end">
				{#if !activeComponent}
					<button
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						disabled={loading}
						on:click={() => {
							selectedGroup = {name: '', description: '', userIds: []};
							activeComponent = CreateGroup;
						}}
					>
						<Fa icon={faPlus} />
					</button>
				{/if}
			</div>
		</div>

		<!-- Formular bleibt sichtbar, auch während loading -->
		{#if activeComponent}
			<div transition:slide class="mb-4">
				<svelte:component
					this={activeComponent}
					group={selectedGroup}
					on:close={closeForm}
					on:success={handleSuccess}
				/>
			</div>
		{/if}

		{#if loading}
			<TablePlaceholder cols={6} />
		{:else}
			<Table config={groupsConfig} on:action={groupsTableActions} />
		{/if}
	</div>

	<Modal />
</Page>
