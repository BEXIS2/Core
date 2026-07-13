<script lang="ts">
	import { Modal } from '@skeletonlabs/skeleton';
	import { ErrorMessage, Page, Table, TablePlaceholder, type TableConfig } from '@bexis2/bexis2-core-ui';
	import type { ReadUserModel } from './types';
	import { usersStore, getUsers } from './services';
	import usersTableOptions from '../../lib/components/usersTableOptions.svelte'
	import { onMount } from 'svelte';

	
const usersTableActions = (action: CustomEvent<{ row: ReadUserModel; type: string }>) => {
	const { type, row } = action.detail;
	switch (type) {
		case 'UPDATE':
			break;
		case 'DELETE':
			break;

		default:
			break;
	}
};


	const usersConfig: TableConfig<ReadUserModel> = {
		id: 'usersTable',
		data: usersStore,
		optionsComponent: usersTableOptions,
		columns: {
			creationDate: {
				header: 'Creation Date',
				instructions: {
					toStringFn: 
						(date: Date) =>
							date.toLocaleString('en-US', { 
								month: 'short', 
								day: 'numeric', 
								year: 'numeric' 
							}
						),
					toSortableValueFn: 
						(date: Date) => date.getTime(),
					toFilterableValueFn: 
						(date: Date) => date
				}
			},
			modificationDate: {
				header: 'Modification Date',
				instructions: {
					toStringFn: 
						(date: Date) =>
							date.toLocaleString('en-US', { 
								month: 'short', 
								day: 'numeric', 
								year: 'numeric' 
							}
						),
					toSortableValueFn: 
						(date: Date) => date.getTime(),
					toFilterableValueFn: 
						(date: Date) => date
				}
			}
		}
	};

	onMount(async () => {

	});

	async function reload() {
		await getUsers();
	}

</script>

<Page help={true} title="Manage Users">
	<h1 class="h1">Users</h1>
	
	{#await reload()}
		<div class="table-container w-full">
			<TablePlaceholder cols={6} />
		</div>
	{:then}
    	<Table config={usersConfig} on:action={usersTableActions} />
	{:catch error}
		<ErrorMessage {error} />
	{/await}

    <Modal />
</Page>