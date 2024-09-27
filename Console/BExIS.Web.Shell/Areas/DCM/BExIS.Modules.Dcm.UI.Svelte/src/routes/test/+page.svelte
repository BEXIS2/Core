<script lang="ts">
	import { Page, Table, type TableConfig, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import { host } from '@bexis2/bexis2-core-ui';
	import { writable } from 'svelte/store';
	import { createEventDispatcher } from 'svelte';

	// load attributes from div
	let container;
	let id: number | undefined = undefined;
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
		// get data from parent
		container = document.getElementById('test');
		id = Number(container?.getAttribute('dataset'));
		const tableStore = writable<any[]>([]);
		const url = host + '/api/datatable/';

		serverTableConfig = {
			id: 'serverTable', // a unique id for the table
			data: tableStore, // store to hold and retrieve data
			search: false, // enable search
			server:	{
			entityId: id, // dataset ID
			versionId: -1, // vesion ID
			baseUrl: url
		}
		};

	}
</script>

<Page contentLayoutType="{pageContentLayoutType.full}">
	{#if serverTableConfig}
		<Table config={serverTableConfig} />
	{/if}
</Page>
