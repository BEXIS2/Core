<script lang="ts">

	import { ErrorMessage, notificationStore, notificationType, Page, pageContentLayoutType, positionType, Spinner} from "@bexis2/bexis2-core-ui";
 import { goTo, load, register } from "./services";
	import {type GbifPublicationModel } from "./types";

	import { Table } from '@bexis2/bexis2-core-ui';
	import type { TableConfig } from '@bexis2/bexis2-core-ui';
	import { writable } from "svelte/store";
	import	TableOption from "./tableOptions.svelte";

	const publicationStore = writable<GbifPublicationModel[]>([]);

	let publications:GbifPublicationModel[] = [];

	async function init() {
		
		const res = await load();
		if(res.status===200){
			publications	= res.data;
			publicationStore.set(publications);
		}
	}

 async function evenHandler(type:any){

		if(type.action === 'request'){

			console.log('request');
			const res = await register(type.publicationId)
			if(res.status===200){
				notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Dataset (' + type.id + ') registered.'

			});

			init();
			}
			
		}else	if(type.action === 'show'){
	
			let url= "/ddm/data/show?id="+type.id+"&version="+type.version;
			goTo(url,);
		}else		if(type.action === 'delete'){
	
	// let url= "/ddm/data/show?id="+type.id+"&version="+type.version;
	// goTo(url,);
}
	}

</script>
<Page 
	title="GBIF Export Manager"
	note="Manage all datasets that are ready to exported to GBIF."
	contentLayoutType={pageContentLayoutType.center}
>
{#await init()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading darwin core archive ready publications for gbif" />
			</div>
		{:then result}
			<Table
			on:action={(obj) => evenHandler(obj.detail.type)}
			config={{
					id: 'publications',
					data: publicationStore,
					optionsComponent: TableOption,
					columns:{
						publicationId: {								
							exclude:true		
						},
						datasetId:{
							header: 'dataset',
							fixedWidth: 30
						}
						,
						datasetVersionId:{
							exclude:true
						},
						datasetVersionNr:{
							header: 'Version',
							fixedWidth: 30
						},
						brokerRef:{
							exclude:true
						},
						repositoryRef:{
							exclude:true
						}

					}
			}} 	/>
		{:catch error}
			<ErrorMessage {error} />
		{/await}

</Page>