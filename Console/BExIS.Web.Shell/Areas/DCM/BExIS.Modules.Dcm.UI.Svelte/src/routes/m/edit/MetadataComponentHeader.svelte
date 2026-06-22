<script lang="ts">
	import { empty, getNodeByPath, getPartyIdByPath, hasValue, isActive, setActive, setInactive, toggleShow, activateShow } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '../metadataShared';
	import { faPlus, faChevronUp, faChevronDown, faQuestion, faTrash, faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
  import {faCircleQuestion as faCircleQuestionRegular} from '@fortawesome/free-regular-svg-icons';
  import Fa from 'svelte-fa';
  import { activeStore, hideStore, metadataStore, validationStore, showAllDescriptionsStore } from '$lib/components/utils/metadata/stores';
  import { onMount } from 'svelte';

	

 export let required: boolean = false;
 //  $:required;
 export let path: string;
 export let p:string = '';
 export let description: string = '';
 



 let label: string = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;

// set showDescription  if showAllDescriptionsStore is true or false; use local if showAllDescriptionsStore is null or undefined
 $:showDescription = $showAllDescriptionsStore !== null && $showAllDescriptionsStore !== undefined ? $showAllDescriptionsStore : false;

 const togglePath = p!=='' ? p : path; 

export let active: boolean = false;
$:active;


 onMount(() => { 

		//console.log('complexComponentWrapper onMount', path, $activeStore);
    if(!$activeStore.includes(path)) {
      initActivity();
    }
    else {
      active = true;
    }

  });

function initActivity() {
  active = isActive(path,required);

  if(active) {
    setActive(path)
  }
  else {
    setInactive(path);
  } 
}

function changeFn(a: boolean) {
  

  active = !a;

  if(active) {
    setActive(path)
    activateShow(path);
  }
  else {
    setInactive(path);
    // remove from validation store
    removeFromValidationStore(path);
    // empty data in metadata store for this path and all child paths
    const data = getNodeByPath(path); 
    empty(data);
  } 

  // console.log('active',active,path, $activeStore);
}

function removeFromValidationStore(path: string) {
  validationStore.update(store => {
    return {
      ...store,
      simpleTypeValidationItems: store.simpleTypeValidationItems.filter(item => !item.path.startsWith(path)),
      complexTypeValidationItems: store.complexTypeValidationItems.filter(item => !item.path.startsWith(path))
    };
  });
}

</script>

<div class="card flex min-h-8 bg-primary-300 dark:bg-primary-800 pl-2 items-center gap-2">
<div>
    {#if !required}

      {#if !active}
         <button class="badge mt-1" on:click={()=>changeFn(active)} title="Add {convertDisplayName(label, true)} node"><Fa icon={faPlus} /></button>
      {:else}
         <button class="badge mt-1" on:click={()=>changeFn(active)} title="Remove {convertDisplayName(label, true)} node. Content will be lost." ><Fa icon={faTrash}/></button>
      {/if}

      <!-- <Fa icon={faPlus} class="text-green-500" />

      <input class="checkbox" type="checkbox" bind:checked={active} on:change={()=>changeFn(active)}/> -->     
    {/if}
</div>
 <button class="text-left grow" on:click={() => toggleShow(togglePath)} type="button">
	   <h4 id="{path}" class="h4">
    {convertDisplayName(label, true)}  
    {#if required}
      <span class="text-red-500">*</span>
    {/if}
     {#if description}
				<button class="badge h-full mt-1" on:click|stopPropagation={()=>showDescription = !showDescription} title="Show Description"><Fa icon={faCircleQuestionRegular} size="lg"/></button>
		{/if}
   </h4>
 </button>


 <div class="text-left flex justify-end w-2 px-6 ">
 
 </div>
 <div class="text-left flex justify-end w-2 px-2">

  {#if $activeStore.includes(path)}
    {#if !$hideStore.includes(path) }
      <button
        class="btn-sm text-right"
        title="Open or close {convertDisplayName(label, true)}"
        on:click={() => toggleShow(togglePath)} ><Fa icon={faChevronUp} /></button
      >
      {:else}
      <button
        class="btn-sm text-right"
        title="Open or close {convertDisplayName(label, true)}"
        on:click={() => toggleShow(togglePath)}><Fa icon={faChevronDown} /></button
      >
      {/if}
    {/if}
 </div>
</div>
 {#if description && showDescription}
  <div	class="text-sm text-gray-500 py-1 pl-2">{@html description}</div>
 {/if}