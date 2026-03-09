



<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '../services/apiCalls';
	import { helpStore, notificationType, Page, pageContentLayoutType, Spinner } from '@bexis2/bexis2-core-ui';
	import Functions from './MetadataFunctions.svelte';

	// import { Page } from '@bexis2/bexis2-core-ui';
	import { schemaToJson, setConfigStore, setMetadataStore } from '$lib/components/utils/metadata/metadataComponentUtils';

	// import configJson from './customComponents/config.json';
	//export let schemaId: number = 3;
	export let datasetId: number = 3;
	export let saveWithError: boolean = true;

	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;


	async function load() {
		// read id from url
		//datasetId = Number(new URLSearchParams(window.location.search).get('id'));
		console.log('Loading metadata for datasetId:', datasetId);
		if (datasetId > 0) {
			const datasetInfos = await apiCalls.GetDatasetInfoById(datasetId);
			s = await apiCalls.GetMetadataSchema(datasetInfos.metadataStructureId);
			console.log('Schema loaded', s);

			if (datasetId > 0) m = await apiCalls.GetMetadata(datasetId);
			else m = schemaToJson(s);
			console.log('Metadata loaded', m);
			setMetadataStore(m);
			const configJson = await apiCalls.GetComponentConfig(datasetInfos.entityTemplateId, "edit");
			setConfigStore(configJson);

		}
	}


</script>



<Page contentLayoutType={pageContentLayoutType.full}  footer={false} >
	{#await load()}
		<Spinner />
	{:then}

<div	class="container">
	<div class="nav-left scrollable">
		{#if m}
			<Functions bind:metadata={m} saveWithError={saveWithError} bind:datasetId={datasetId} />
		{/if}
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


