<script lang="ts">
	import { onMount } from 'svelte';
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '../../services/apiCalls';
	import { helpStore, Spinner } from '@bexis2/bexis2-core-ui';

	// import { Page } from '@bexis2/bexis2-core-ui';
	import { schemaToJson, setConfigStore, setMetadataStore } from '../../../../lib/components/utils/metadata/metadataComponentUtils';

	import Ajv from "ajv";
import addFormats from "ajv-formats";

const ajv = new Ajv({ allErrors: true, discriminator: true  });
// This regex accepts HH:mm:ss without requiring Z

addFormats(ajv); // This registers "date", "date-time", "email", etc.
const timeRegex = /^([01]\d|2[0-3]):[0-5]\d$/;
ajv.addFormat("time", timeRegex);

	// import configJson from './customComponents/config.json';

	//export let schemaId: number = 3;
	export let datasetId: number = 1;

 let validate;
 $:validate;

	let errors:any[]	= [];

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
			validate = ajv.compile(s);
			if (datasetId > 0) m = await apiCalls.GetMetadata(datasetId);
			else m = schemaToJson(s);
			console.log('Metadata loaded', m);
			setMetadataStore(m);
			const configJson = await apiCalls.GetComponentConfig(datasetInfos.entityTemplateId, "edit");
			setConfigStore(configJson);

		}
	}

 let valid: boolean = true;
	$:valid;
	 
	function validateFn() {
		
		valid = validate(m);
		if (!valid) 
		{
			console.log(validate.errors);
			errors = validate.errors;
		}
		else {
			errors = [];
			console.log('Metadata is valid');
		}
	}

</script>

<div>
 {#await load()}
		<Spinner />
	{:then}

	 <!-- {valid} 
		{validate}
		{validate.errors} -->
		{#if errors && errors.length > 0}

			<div class="p-2">
				<h2 class="text-red-500">Validation Errors:</h2>
				<ul class="list-disc list-inside text-red-500">
					{#each errors as err}
						<li>{err.instancePath || "Root"}: {err.message}</li>
					{/each}
				</ul>
			</div>

		{/if}


		<div class="p-2">
			<ComplexComponent complexComponent={schema} path={''} />
		</div>

		<button class="btn variant-filled-secondary m-2" on:click={validateFn}>
			validate
		</button>

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