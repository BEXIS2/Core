<script lang="ts">

	import * as apiCalls from '../../services/apiCalls';
	import { helpStore, Spinner } from '@bexis2/bexis2-core-ui';

	// import configJson from './customComponents/config.json';
	import { schemaToJson, setConfigStore, setMetadataStore } from '../../../../lib/components/utils/metadata/metadataComponentUtils';

	//export let schemaId: number = 3;
	export let datasetId: number = 1;

	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;



	async function load() {
		
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
	

		<b>..... later</b>


	{/await}
</div>