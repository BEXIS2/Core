



<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '../services/apiCalls';
	import { helpStore, notificationType, Page, pageContentLayoutType, Spinner } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import	{ faDownload } from '@fortawesome/free-solid-svg-icons';


	// import { Page } from '@bexis2/bexis2-core-ui';
	import { schemaToJson, setConfigStore, setMetadataStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { da } from 'svelty-picker/i18n';

	// import configJson from './customComponents/config.json';

	export let id: number = 3;
	export	let version: number = 0;


	let container;
	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;


	async function load() {

			container = document.getElementById('metadata');
			console.log("🚀 ~ load ~ container:", container)
			
			id = Number(container?.getAttribute('dataset'));
			version = Number(container?.getAttribute('version'));
		


		// read id from url
		//datasetId = Number(new URLSearchParams(window.location.search).get('id'));
		console.log('Loading metadata for datasetId:', id, version);
		if (id > 0) {
			const datasetInfos = await apiCalls.GetDatasetInfoById(id);
			s = await apiCalls.GetMetadataSchema(datasetInfos.metadataStructureId);
			console.log('Schema loaded', s);

			if (id > 0) m = await apiCalls.GetMetadata(id);
			else m = schemaToJson(s);
			console.log('Metadata loaded', m);
			setMetadataStore(m);
			const configJson = await apiCalls.GetComponentConfig(datasetInfos.entityTemplateId, "view");
			setConfigStore(configJson);

		}
	}

	async function DownloadMetadata(datasetId: number, versionNumber: number, format: string) {

			let type = '';
			let filename = '';	

			switch(format) {
				case "json":
				 type = 'application/json';
				 filename = 'metadata.json';
					break;
				case "xml":
				 type = 'application/xml';
				 filename = 'metadata.xml';
					break;
				case "flatten":
				 type = 'text/plain';
				 filename = 'metadata_flattened.txt';
					break;
				default:

					return;
			}
			
   let data = null;
			if(format === "json") {
				//helpStore.showNotification("Your download will start shortly. If it doesn't, please check your popup blocker settings.", notificationType.info, 5000);
				data = await apiCalls.GetMetadataAsJson(datasetId, versionNumber);
			}
			else	if(format === "xml") {
				//helpStore.showNotification("Your download will start shortly. If it doesn't, please check your popup blocker settings.", notificationType.info, 5000);
				data = await apiCalls.GetMetadataAsXml(datasetId, versionNumber);
			} else if(format === "flatten") {
				//helpStore.showNotification("Your download will start shortly. If it doesn't, please check your popup blocker settings.", notificationType.info, 5000);
				data = await apiCalls.GetMetadataAsFlattened(datasetId, versionNumber);
			}

		 if(data) {
	
					const blob = new Blob([data], { type: type });
					const url = window.URL.createObjectURL(blob);

					const a = document.createElement('a');
					a.href = url;
					a.download = filename;
					document.body.appendChild(a);
					a.click();

					a.remove();
					window.URL.revokeObjectURL(url);
			}
	}

</script>



<Page contentLayoutType={pageContentLayoutType.center}  footer={false} >
	{#await load()}
		<Spinner />
	{:then}


<div	class="container flex">

		<div class="flex-col">
				<div class="flex flex-col	gap-2">
					<button class="chip variant-filled-primary" on:click={() => DownloadMetadata(id, version,"json")}>
					 	<div class="flex gap-2"><Fa icon={faDownload} />JSON </div>
					</button>
							<button class="chip variant-filled-primary" on:click={() => DownloadMetadata(id, version,"xml")}>
									<div class="flex gap-2"><Fa icon={faDownload} />XML</div>
							</button>
							<button class="chip variant-filled-primary" on:click={() => DownloadMetadata(id, version,"flatten")}>
									<div class="flex gap-2"><Fa icon={faDownload} />Flattened</div>
							</button>
			</div>
		</div>
			<div class="content scrollable">
				<div class="px-2">
					<ComplexComponent complexComponent={schema} path={''} />
				</div>
			</div>
		</div>

		{/await}
</Page>

<style>

.container {
  display: flex;
  overflow: hidden; /* Wichtig: Der Content-Bereich selbst scrollt nicht */
		height: calc(100dvh - 180px); /* Höhe des Viewports minus Höhe des Headers */
}

.nav-left {
		width: 400px; /* Feste Breite für die Navigation */
		overflow-y: auto; /* Ermöglicht vertikales Scrollen in der Navigation */

}
	
.content {
		flex-grow: 1;
		overflow-y: auto; /* Aktiviert das unabhängige Scrollen */
}

.scrollable {
		overflow-y: auto;
		scrollbar-width: thin; /* Makes scrollbar smaller in Firefox */
		scrollbar-color: rgba(0, 0, 0, 0.3) transparent; /* Colors scrollbar */
}
</style>


