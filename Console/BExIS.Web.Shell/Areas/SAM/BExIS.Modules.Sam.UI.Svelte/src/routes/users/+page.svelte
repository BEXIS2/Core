<script lang="ts">
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import { notificationStore, notificationType, Page, Table, TablePlaceholder, type TableConfig } from '@bexis2/bexis2-core-ui';
	import { usersStore, getUsers, deleteUserById } from './services';
	import usersTableOptions from '../../lib/components/usersTableOptions.svelte';
	import { onMount, setContext } from 'svelte';
	import { slide } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import CreateUser from '../../lib/components/createUser.svelte';
	import UpdateUser from '../../lib/components/updateUser.svelte';
	import type { ComponentType, SvelteComponent } from 'svelte';
	import { get } from 'svelte/store';
	import type { CreateUserModel, ReadUserModel } from './types';
	import { getGroups } from '../groups/services';

	$: formTitle =
		activeComponent === UpdateUser && selectedUser
			? `Update User: ${selectedUser.userName}`
			: 'Create new User';

	let activeComponent: ComponentType<SvelteComponent> | null = null;
	let selectedUser: ReadUserModel | CreateUserModel | null = null;
	let loading = true;

	const modalStore = getModalStore();

	async function reload() {
		loading = true;
		await getUsers();
		await getGroups();
		loading = false;
	}

	function closeForm() {
		activeComponent = null;
		selectedUser = null;
	}

	function handleSuccess() {
		closeForm();
		reload();
	}

	const usersTableActions = (action: CustomEvent<{ type: string; row: ReadUserModel }>) => {
		const { type, row } = action.detail;
		if (!row) return;

		switch (type) {
			case 'UPDATE':
				selectedUser = row;
				activeComponent = UpdateUser;
				break;

			case 'DELETE':
				modalStore.trigger({
					type: 'confirm',
					title: `Delete Group (<strong>${row.userName}</strong>)`,
					body: `Are you sure you want to delete <strong>${row.userName}</strong>?`,
					response: async (result: boolean) => {
						if (result) {
							await deleteUserById(row.id);
							await closeForm();
							await reload();

							notificationStore.showNotification({
    							notificationType: notificationType.success,
    							message: `Deleted group (<strong>${row.userName}</strong>) successfully.`,
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

	const usersConfig: TableConfig<ReadUserModel> = {
		id: 'usersTable',
		data: usersStore,
		optionsComponent: usersTableOptions as ComponentType<SvelteComponent>,
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
			groupIds: { exclude: true }
		}
	};
</script>

<Page help={true} title="Manage Users">
	<h1 class="h1">Users</h1>
	<div class="table-container w-full">
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500 w-full">
			<div class="h3 h-9">{formTitle}</div>
			<div class="flex justify-end">
				{#if !activeComponent}
					<button
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						disabled={loading}
						on:click={() => {
							selectedUser = {userName: '', email: '', groupIds: []};
							activeComponent = CreateUser;
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
					user={selectedUser}
					on:close={closeForm}
					on:success={handleSuccess}
				/>
			</div>
		{/if}

		{#if loading}
			<TablePlaceholder cols={6} />
		{:else}
			<Table config={usersConfig} on:action={usersTableActions} />
		{/if}
	</div>

	<Modal />
</Page>
