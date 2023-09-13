<script lang="ts">
	import { onMount } from 'svelte';
	import { getGroups } from '../../services/groupService';
	import type { ReadGroupModel } from '../../models/groupModels';
	import {
		Table,
		Spinner,
		TableFilter,
		columnFilter,
		searchFilter,
		Page
	} from '@bexis2/bexis2-core-ui';
	import type { TableConfig, Columns, Column } from '@bexis2/bexis2-core-ui';
	import { writable, get, type Writable } from 'svelte/store';

	const tableStore = writable<ReadGroupModel[]>([]);
	let groups: ReadGroupModel[];
	$:tableStore.set(groups);
	let tableConfig: TableConfig<ReadGroupModel> ={
			id: 'groups',
			data: tableStore,
			columns: {
				creationDate: {
					header: 'Creation Date',
					instructions: {
						toStringFn: (date: Date) => date.toDateString() //,
						// toSortableValueFn: (date: Date) => date.getTime(),
						// toFilterableValueFn: (date: Date) => date
					}
				},
				modificationDate: {
					header: 'Modification Date',
					instructions: {
						toStringFn: (date: Date) => date.toDateString() //,
						// toSortableValueFn: (date: Date) => date.getTime(),
						// toFilterableValueFn: (date: Date) => date
					}
				}
			}
		};
	onMount(async () => {
		groups = (await getGroups()) as ReadGroupModel[];
	});
</script>

<Page title="Groups" note="Overview of Groups">
	<div slot="description">Hier kommt die Beschreibung!</div>
	<div class="w-1/2">
		{#if groups}
			<Table config={tableConfig} />
		{:else}
			<Spinner textCss="text-warning-500" />
		{/if}
	</div>
</Page>
