
<script lang="ts">

	import Fa from 'svelte-fa';
  import { faCheck, faSave } from '@fortawesome/free-solid-svg-icons';
	import { onMount } from 'svelte';
  import * as apiCalls from '../services/apiCalls';
	import { activateShow, getValidationStore, toggleShow } from '$lib/components/utils/metadata/metadataComponentUtils';
	import type { validationStoretype } from '$lib/components/utils/metadata/models';
  import {metadataStore, validationStore} from '$lib/components/utils/metadata/stores';

	import { notificationStore, notificationType, TextInput } from '@bexis2/bexis2-core-ui';
  import {convertDisplayName} from '../metadataShared';
	import { goTo } from '$services/BaseCaller';

  export let datasetId: number;
  export let metadata;
  export let saveWithError: boolean = false;
  let hasChanged: boolean = true; // need to implement change detection to enable/disable save button based on whether there are unsaved changes or not, for now it's always enabled
  let showErrorOverview: boolean = false;
  let comment: string = '';

  const unsubscribedMetadata = metadata;

  $:showErrorOverview;
  $:metadata; //console.log("functions - metadata:", metadata);
  

	let disbaleSaveBtn: boolean = false;
	$:disbaleSaveBtn;

	 let validationStoreValues: validationStoretype;
  $:{
    validationStoreValues;
    disbaleSaveBtn = disableSaveFn();
    //console.log("🚀 ~ file: +page.svelte:92 ~ $: ~ disbaleSaveBtn:", disbaleSaveBtn)
    //console.log("🚀 ~ validationStoreValues ~ $: ~ validationStoreValues:", validationStoreValues)
  }

  onMount(() => {
  
    metadataStore.subscribe((s) => {
      metadata = s;
    });

    validationStore.subscribe((s) => {
     validationStoreValues = s;
    });
  
  });

  
  function hasErrors(key) {

   if(validationStoreValues){
    const invalidParts = validationStoreValues.simpleTypeValidationItems.filter(item => item.path.startsWith(key) && item.isValid === false);
     return (invalidParts && invalidParts.length > 0);
   };
  }


	function disableSaveFn():boolean {
    //console.log("🚀 ~ disableSaveFn ~ hasChanged:", hasChanged, saveWithError)
    if (hasChanged == false) return true; // when there are changes, the save button is enabled, so return false for disabled
		if (saveWithError) return false; // when save with error is allowd, the save button is always enabled
		if (!validationStoreValues) return true; // if there is no validation result, we consider the form as not valid, so the save button is disabled
	
		return !validationStoreValues.allSimpleRequiredValid; //	disable save button when the metadata is not valid
	}

  function toggleAll(path: string) {

    const complexItem=path.split(".");
    //console.log("🚀 ~ toggleAll",path,complexItem)

    for(let i=complexItem.length;i>0;i--){
      const p = complexItem.slice(0,i).join(".");
      //console.log("🚀 ~ toggleAll ~ p:", p)
      activateShow(p);
    }

    setTimeout(() => {
      const ziel = document.getElementById(path+".item");
      ziel?.scrollIntoView({ behavior: "smooth" });
    }, 500);
    
  }

</script>


<div class="flex flex-col gap-2" >
<div id="metadata-options" class="flex w-full gap-2" >
    <div class="flex-auto"> 
     <TextInput
        label=""
        placeholder="What changed? Add a comment..."
        bind:value={comment}
      />

    </div>
			<button
				class="btn variant-filled-primary"
				disabled={disbaleSaveBtn}

				on:click={async () => {
					try {

						console.log('Saving metadata:', datasetId, metadata);
						const savedMetadata = await apiCalls.SaveMetadata(datasetId, metadata,comment);
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
				<Fa icon={faSave}/>
			</button>
</div>
<!-- Error messages-->
 <div class="text-error-500">
 {#if validationStoreValues}
  {#key validationStoreValues}
  {#if !validationStoreValues.allSimpleRequiredValid}
      <button class="chip variant-soft-error" on:click={() => (showErrorOverview = !showErrorOverview)}>
        Errors: {validationStoreValues.simpleTypeValidationItems.filter(item => item.isValid === false).length}
      </button>
      {#if showErrorOverview}
      <div class="card py-3 my-2 ">
        {#each validationStoreValues.simpleTypeValidationItems.filter(item => item.isValid === false) as item}
          <div class="ml-4 flex flex-col gap-2 ">
            <a  class="text-sm text-error-500" on:click={()=>toggleAll(item.path) }>{item.path.replaceAll(".", "/")}</a>
          </div>
        {/each}
      </div>
      {/if}
    {/if}
  {/key}
  {/if}
</div>

<!-- Main blocks-->
<div>
  <hr/>
  <nav class="list-nav">
  {#if validationStoreValues}
  {#key validationStoreValues}
  <ul>
    {#each Object.entries(metadata) as [key, value]}
    {#if typeof value === 'object' && value !== null}
      <a href="#{key}" >
      <span class="flex-auto">{convertDisplayName(key)}</span>
      {#if validationStoreValues && hasErrors(key)}
        <span class="text-error-500">#</span>
      {:else if validationStoreValues && !hasErrors(key)}
        <span class="text-success-500"><Fa icon={faCheck} /></span>
      {/if}
        </a>
    {/if}
    {/each}
  </ul>
  {/key}
  {/if}
  </nav>
 </div>
 </div>

