<script lang="ts">
	import { Table, type TableConfig, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import { host } from '@bexis2/bexis2-core-ui';
	import { writable } from 'svelte/store';
	import { createEventDispatcher } from 'svelte';

	export let id: number | undefined = undefined;
	type ServerTableType = {
		id: number;
		name: string;
	};
	let serverTableConfig: TableConfig<ServerTableType>;
	$: serverTableConfig;

	let t: string = '';

	const dispatch = createEventDispatcher();

	load();

	async function load() {
		const tableStore = writable<any[]>([]);
		const url = host + '/api/datatable/';

		serverTableConfig = {
			id: 'serverTable', // a unique id for the table
			data: tableStore, // store to hold and retrieve data
			search: false, // enable search
			server: {
				entityId: id, // dataset ID
				versionId: -1, // vesion ID
				baseUrl: url,
				defaultPageSize: 5,
				pageSizes: [5, 10, 50, 100, 500, 1000]
			}
		};
	}
</script>

{#if serverTableConfig}
	<Table config={serverTableConfig} />
{/if}
