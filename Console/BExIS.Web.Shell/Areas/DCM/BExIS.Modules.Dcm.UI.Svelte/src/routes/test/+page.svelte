<script lang="ts">

	import { Page, Table, type TableConfig } from '@bexis2/bexis2-core-ui';
	import { host, username, password } from '@bexis2/bexis2-core-ui';
	import { writable } from 'svelte/store';
	import { getToken } from '$services/BaseCaller';
	import { createEventDispatcher} from 'svelte'

	// load attributes from div
	let container;
	let id:number|undefined = undefined;
	type ServerTableType = {
		id: number;
		name: string;
	};
	let serverTableConfig: TableConfig<ServerTableType>;
	$:serverTableConfig;

 let t:string = "";

	const dispatch = createEventDispatcher();

	load();

		async function load() {
				// get data from parent
				container = document.getElementById('test');
				id = Number(container?.getAttribute('dataset'));
				const tableStore = writable<any[]>([]);
				const url = host+"/api/datatable/"
				t = await getToken();

				serverTableConfig = {
					id: 'serverTable', // a unique id for the table
					entityId: id, // dataset ID
					versionId: -1, // vesion ID
					data: tableStore, // store to hold and retrieve data
					serverSide: true, // serverSide needs to be set to true
					// URL for the table to be fetched from
					URL: url,
					token: t // API token to access the datasets
				};

				console.log(url,t,id, serverTableConfig)
		}



</script>
<Page>

	{#if serverTableConfig}
				<Table config={serverTableConfig} />
		{/if}

</Page>