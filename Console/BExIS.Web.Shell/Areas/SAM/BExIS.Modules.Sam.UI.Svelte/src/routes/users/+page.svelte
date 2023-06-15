<script lang="ts">
	import { onMount } from 'svelte';

	import { setApiConfig } from '@bexis2/bexis2-core-ui';
	import { getUsers } from '../../services/userService';
	import type { ReadUserModel } from '../../models/userModels';
	import { Table, Spinner, TableFilter, columnFilter, searchFilter } from '@bexis2/bexis2-core-ui';
	import type { TableConfig, Columns, Column } from '@bexis2/bexis2-core-ui';
	import { writable, get, type Writable } from 'svelte/store';

	let groups: ReadUserModel[];
	let store = writable<ReadUserModel[]>([]);

	onMount(async () => {
		setApiConfig('https://localhost:44345', 'sdfsdfs', 'sdfsdfsdf');
		store.set(await getGroups());
		console.log('store', get(store));
	});
</script>

{#if $store.length > 0}
	<Table config={{id: 'groups', data: store}} />
{:else}
	<Spinner />
{/if}