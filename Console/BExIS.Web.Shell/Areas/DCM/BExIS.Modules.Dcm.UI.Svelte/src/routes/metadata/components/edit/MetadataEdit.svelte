<script lang="ts">
	import { onMount } from 'svelte';
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '../../services/apiCalls';
	import { helpStore, Spinner } from '@bexis2/bexis2-core-ui';

	// import { Page } from '@bexis2/bexis2-core-ui';
	import { schemaToJson, setConfigStore, setMetadataStore } from '../../../../lib/components/utils/metadata/metadataComponentUtils';

	// import configJson from './customComponents/config.json';

	//export let schemaId: number = 3;
	export let datasetId: number = 1;

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

<div>
 {#await load()}
		<Spinner />
	{:then}
		<div class="p-2">
			<ComplexComponent complexComponent={schema} path={''} />
		</div>

		<button
			class="btn variant-filled-secondary m-2"
			on:click={async () => {
				try {
					console.log('Saving metadata:', datasetId, m);
					const savedMetadata = await apiCalls.SaveMetadata(datasetId, m);
					console.log('Metadata saved successfully:', savedMetadata);
					alert('Metadata saved successfully!');
				} catch (error) {
					console.error('Error saving metadata:', error);
					alert('Error saving metadata. Please check the console for details.');
				}
			}}>
			Save Metadata
		</button>
	{/await}
</div>