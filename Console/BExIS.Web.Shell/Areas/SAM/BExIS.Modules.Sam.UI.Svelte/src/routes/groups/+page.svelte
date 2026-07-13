<script lang="ts">
	import { Modal } from '@skeletonlabs/skeleton';
	import { Page, Table, type TableConfig } from '@bexis2/bexis2-core-ui';
	import type { ReadGroupModel } from './types';
	import { groupsStore, getGroups } from './services';
	import groupsTableOptions from '../../lib/components/usersTableOptions.svelte'
	import { onMount } from 'svelte';

	
const groupsTableActions = (action: CustomEvent<{ row: ReadGroupModel; type: string }>) => {
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


	const groupsConfig: TableConfig<ReadGroupModel> = {
		id: 'usersTable',
		data: groupsStore,
		optionsComponent: groupsTableOptions,
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
		await getGroups();
	});
</script>

<Page help={true} title="Manage Groups">
	<h1 class="h1">Groups</h1>
    <Table config={groupsConfig} on:action={groupsTableActions} />
    <Modal />
</Page>