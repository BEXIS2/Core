



<script lang="ts">
	import { onMount } from 'svelte';
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '../services/apiCalls';
	import { helpStore, notificationType, Page, pageContentLayoutType, Spinner } from '@bexis2/bexis2-core-ui';
	import Functions from './MetadataFunctions.svelte';
	import suite from './simpleComponent';

	// import { Page } from '@bexis2/bexis2-core-ui';
	import { schemaToJson, setConfigStore, setMetadataStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { type	notificationItemType, notificationStore } from '@bexis2/bexis2-core-ui';
	import { validationStore } from '$lib/components/utils/metadata/stores';
	import type { validationStoretype } from '$lib/components/utils/metadata/models';
	import { get } from 'svelte/store';


// This regex accepts HH:mm:ss without requiring Z


	// import configJson from './customComponents/config.json';
	//export let schemaId: number = 3;
	export let datasetId: number = 2;
	export let saveWithError: boolean = true;
	let disbaleSaveBtn: boolean = false;
	$:disbaleSaveBtn;

	let errors:any[]	= [];

	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;

	let validationStoreValues: validationStoretype;

$:{
 validationStoreValues;
	disbaleSaveBtn = disableSaveFn();
	console.log("🚀 ~ file: +page.svelte:92 ~ $: ~ disbaleSaveBtn:", disbaleSaveBtn)
}


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

			
			// listen to  validation store changes
			validationStoreValues	= get(validationStore);

			validationStore.subscribe((s) => {
     validationStoreValues = s;
    });

		}
	}

 let valid: boolean = true;
	$:valid;


	function validateFn() {
		
		if (!validationStoreValues.allSimpleRequiredValid) 
		{
			//  console.log(validationStoreValues.simpleTypeValidationItems.filter(item => item.isValid === false));
			// // errors = validate.errors;

			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: 'error validating metadata. Please check the console for details.',
			});

		}
		else {
			errors = [];
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Metadata is valid.',
			});
		}
	}


	function disableSaveFn():boolean {

		console.log("🚀 ~ disbaleSaveFn ~ validationResult:")

		if (saveWithError) return false; // when save with error is allowd, the save button is always enabled
		if (!validationStoreValues) return true; // if there is no validation result, we consider the form as not valid, so the save button is disabled
		console.log("🚀 ~ disableSaveFn ~ validationStoreValues:", validationStoreValues)
	
		return !validationStoreValues.allSimpleRequiredValid; //	disable save button when the metadata is not valid

	}

</script>



<Page contentLayoutType={pageContentLayoutType.full}  footer={false} >
	{#await load()}
		<Spinner />
	{:then}

<div	class="container">
	<div class="nav-left scrollable">

		<div id="metadata-options" >
			<button class="btn variant-filled-secondary m-2" on:click={validateFn}>
				validate
			</button>

			<button
				class="btn variant-filled-secondary m-2"
				disabled={disbaleSaveBtn}

				on:click={async () => {
					try {
						console.log('Saving metadata:', datasetId, m);
						const savedMetadata = await apiCalls.SaveMetadata(datasetId, m);
						console.log('Metadata saved successfully:', savedMetadata);
						notificationStore.showNotification({
							notificationType: notificationType.success,
							message: 'Metadata saved successfully.',
						});
					} catch (error) {
						console.error('Error saving metadata:', error);
						notificationStore.showNotification({
							notificationType: notificationType.error,
							message: 'Error saving metadata. Please check the console for details.',
						});
					}
				}}>
				Save Metadata
			</button>
		</div>

		{#if m}
			<Functions bind:data={m}/>
		{/if}
	</div>



	 <div class="content scrollable">
			<div class="p-2">
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
		padding: 1rem;
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


