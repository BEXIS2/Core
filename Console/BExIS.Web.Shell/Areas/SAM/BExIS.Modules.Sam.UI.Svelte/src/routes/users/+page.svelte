<script lang="ts">
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import { Page, Table, TablePlaceholder, type TableConfig } from '@bexis2/bexis2-core-ui';
	import type { ReadUserModel } from './types';
	import { usersStore, getUsers, deleteUser } from './services';
	import usersTableOptions from '../../lib/components/usersTableOptions.svelte';
	import { onMount } from 'svelte';
	import { slide } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import CreateUser from '../../lib/components/createUser.svelte';
	import UpdateUser from '../../lib/components/updateUser.svelte';
	import type { ComponentType, SvelteComponent } from 'svelte';

	$: formTitle =
		activeComponent === UpdateUser && selectedUser
			? `Update User: ${selectedUser.userName}`
			: 'Create new User';

	let activeComponent: ComponentType<SvelteComponent> | null = null;
	let selectedUser: ReadUserModel | null = null; // für Update
	let loading = true;

	const modalStore = getModalStore();

	async function reload() {
		loading = true;
		await getUsers();
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

	const usersTableActions = (action: CustomEvent<{ row: ReadUserModel; type: string }>) => {
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
					title: `Delete User (<strong>${row.userName}</strong>)`,
					body: `Are you sure you want to delete <strong>${row.userName}</strong>?`,
					response: async (result: boolean) => {
						if (result) {
							await deleteUser(row.id);
							await reload();
						}
					}
				});
				break;
		}
	};

	onMount(reload);

	const usersConfig: TableConfig<ReadUserModel> = {
		id: 'usersTable',
		data: usersStore,
		optionsComponent: usersTableOptions,
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
			}
		}
	};
</script>

<Page help={true} title="Manage Users">
	<h1 class="h1">Users</h1>

	<div class="table-container w-full">
		<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h3 h-9">{formTitle}</div>
			<div class="flex justify-end">
				{#if !activeComponent}
					<button
						class="btn variant-filled-secondary shadow-md h-9 w-16"
						disabled={loading}
						on:click={() => {
							selectedUser = null;
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
