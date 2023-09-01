<script lang="ts">
	import {
		Page,
		TablePlaceholder,
		Table,
		pageContentLayoutType,
		ErrorMessage,
		FileUploader,
		notificationStore,
		notificationType
	} from '@bexis2/bexis2-core-ui';


	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';

	import { getDataStructures } from '$lib/components/datastructure/services';

	import { writable } from 'svelte/store';

	import type { DataStructureModel } from '$lib/components/datastructure/types';
	import TableElements from '../components/tableElements.svelte';
	import { goTo } from '$services/BaseCaller';



	// load data
	let structures: DataStructureModel[];
	const structuresStore = writable<DataStructureModel[]>([]);

	$:structures, structuresStore.set(structures);

	async function reload() {
		structures = await getDataStructures();
	}


	let submit = "/rpm/datastructure/upload"

	function success(e) {
		console.log('success',e.detail.text, e.detail.files);
		notificationStore.showNotification({
				notificationType: notificationType.success,
				message: e.detail.files[0]+" is uploaded"
			})

			goTo("/rpm/datastructure/create?entityid=0&&file="+e.detail.files[0])
	}

	function error(e) {
		console.log('error',e);

		notificationStore.showNotification({
				notificationType: notificationType.error,
				message: e.detail.messages.join(', ')
			})
	}

</script>

<Page
	title="Data structures"
	note="overview about the data structures in bexis2."
	contentLayoutType={pageContentLayoutType.full}
>
	{#await reload()}
	 	<TablePlaceholder cols={4} rows={5}/>
	{:then}

	<FileUploader
		id=0
		version=0
		context="data structure creation"
		data= {{
			accept:[".csv",".txt",".tsv"],
			multiple:true,
			existingFiles:[],
			descriptionType:0,
			maxSize:1024
		}}
		{submit}
		on:error = {error}
		on:success ={success}
	/>

	<Table
				config={{
					id: 'xyz',
					data: structuresStore,
					columns: {
						linkedTo: {
							header: 'Linked',
							disableFiltering: true,
							instructions: {
								renderComponent: TableElements
							}
					}
				}
				}}
			/> 

	{:catch error}
		<ErrorMessage {error} />
	{/await}

</Page>
