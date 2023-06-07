<script lang="ts">
	import { onMount } from 'svelte';

	import { setApiConfig } from '@bexis2/bexis2-core-ui';
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

	const store = writable<ReadGroupModel[]>([]);
	let tableConfig: TableConfig<ReadGroupModel>;
	onMount(async () => {
		setApiConfig('https://localhost:44345', 'sdfsdfs', 'sdfsdfsdf');
		const groups = await getGroups() as ReadGroupModel[];
		console.log('type of creationdate1',typeof groups.at(0)?.creationDate);
		store.set(groups);
		console.log('type of creationdate2',typeof new Date(get(store).at(0)?.creationDate ?? "1999-12-31"));
		console.log('store', get(store));
		tableConfig = {
			id: 'groups',
			data: store,
			columns: {
				creationDate: {
					header: 'Creation Date',
					instructions: {
						toStringFn: (date: Date) => date.toDateString()//,
						// toSortableValueFn: (date: Date) => date.getTime(),
						// toFilterableValueFn: (date: Date) => date
						
					}
				},
				modificationDate: {
					header: 'Modification Date',
					instructions: {
						toStringFn: (date: Date) => date.toDateString()//,
						// toSortableValueFn: (date: Date) => date.getTime(),
						// toFilterableValueFn: (date: Date) => date
						
					}
				}
			}
		};
	});
</script>

<Page title="Groups" note="Overview of Groups">
	<div slot="description">Hier kommt die Beschreibung!</div>
	<div class="w-1/2">
		{#if $store.length > 0}
			<Table config={tableConfig} />
		{:else}
			<Spinner textCss="text-warning-500" />
		{/if}
	</div>
</Page>
