
<script lang="ts">


	 import Fa from 'svelte-fa';
  import { faCheck } from '@fortawesome/free-solid-svg-icons';
	import { onMount } from 'svelte';
	import { getValidationStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import type { validationStoretype } from '$lib/components/utils/metadata/models';
 import {validationStore} from '$lib/components/utils/metadata/stores';
	import { get } from 'svelte/store';

  export let data;
  
  // Check if the current data is something we can expand
  const isObject = data !== null && typeof data === 'object';
  let expanded = false;

	 let validationStoreValues: validationStoretype;


  function hasErrors(key) {

   if(validationStoreValues){

    const invalidParts = validationStoreValues.simpleTypeValidationItems.filter(item => item.path.startsWith(key) && item.isValid === false);
    

     return (invalidParts && invalidParts.length > 0);
   };
  }

  onMount(() => {
    console.log("🚀 ~ Tree Component - Data:", data);
    
    validationStore.subscribe((s) => {
     validationStoreValues = s;
     console.log("🚀 ~ Tree validationStoreValues:", validationStoreValues)
    });
  
  });

</script>

		<!-- <div class="p-2">
			{#if validationResult && validationResult.hasErrors()}

			<ul class="list">
				{#each validationResult.errors as error}
					<li >
						<span class="text-error-500">-</span>
						<span class="text-error-500 flex-auto">{error.fieldName} - {error.message}</span>
					</li>
					
				{/each}
			</ul>

			{/if}
	</div> -->


<nav class="list-nav">
{#if validationStoreValues}
{#key validationStoreValues}
 <ul>
  {#each Object.entries(data) as [key, value]}
  {#if typeof value === 'object' && value !== null}
    <a href="#{key}" >
     <span class="flex-auto">{key}</span>
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

