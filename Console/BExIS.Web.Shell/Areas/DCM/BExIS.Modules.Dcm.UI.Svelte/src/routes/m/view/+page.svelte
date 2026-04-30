



<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '../services/apiCalls';
	import { helpStore, notificationType, Page, pageContentLayoutType, Spinner } from '@bexis2/bexis2-core-ui';


	// import { Page } from '@bexis2/bexis2-core-ui';
	import { schemaToJson, setConfigStore, setMetadataStore } from '$lib/components/utils/metadata/metadataComponentUtils';

	// import configJson from './customComponents/config.json';

	export let id: number = 3;


	let container;
	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;


	async function load() {

			container = document.getElementById('metadata');
			console.log("🚀 ~ load ~ container:", container)
			
			id = Number(container?.getAttribute('dataset'));
		
		// read id from url
		//datasetId = Number(new URLSearchParams(window.location.search).get('id'));
		console.log('Loading metadata for datasetId:', id);
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


</script>



<Page contentLayoutType={pageContentLayoutType.center}  footer={false} >
	{#await load()}
		<Spinner />
	{:then}


<div	class="container">

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


