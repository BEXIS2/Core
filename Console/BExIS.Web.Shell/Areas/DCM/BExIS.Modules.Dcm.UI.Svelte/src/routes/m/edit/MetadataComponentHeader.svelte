<script lang="ts">
	import { empty, getNodeByPath, hasValue, isActive, setActive, setInactive, toggleShow } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '../metadataShared';
	import { faPlus, faChevronUp, faChevronDown, faQuestion } from '@fortawesome/free-solid-svg-icons';
  import Fa from 'svelte-fa';
  import { activeStore, hideStore, validationStore } from '$lib/components/utils/metadata/stores';
  import { onMount } from 'svelte';
	

 export let required: boolean = false;
 //  $:required;
 export let path: string;
 export let p:string = '';
 export let description: string = '';
 
 let active: boolean = false;
 $:active;

 let label: string = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
 let showDescription: boolean = false;

 const togglePath = p!=='' ? p : path; 


 onMount(() => {
  // console.log('MetadataComponentHeader mounted with path: ', path);   
    if(!$activeStore.includes(path)) {
      initActivity();
    }
    else {
      active = true;
    }
    //console.log('init-active', $activeStore);
 });

function initActivity() {
  active = isActive(p,required);
  if(active) {
    setActive(path)
  }
  else {
    setInactive(path);
  } 
}

function changeFn(active: boolean) {
  
  if(active) {
    setActive(path)
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



<div class="card flex min-h-10 bg-primary-300 dark:bg-primary-800 pl-2 items-center gap-2">

 
<div>
    {#if !required}
      <input class="checkbox" type="checkbox" bind:checked={active} on:change={()=>changeFn(active)}/>
    {:else}
      <h4 class="h4 text-red-500">
        *
      </h4>
    {/if}
</div>
 <div class="text-left grow pl-2">
	   <h4 id="{path}" class="h4">
    {convertDisplayName(label, true)} 
   </h4>
 </div>

 
 {#if description && showDescription}
  <div	class="text-sm text-gray-500 py-1">{@html description}</div>
 {/if}
 <div class="text-left flex justify-end w-2 px-6 ">
  {#if description}
				<button class="badge" on:click={()=>showDescription = !showDescription}><Fa icon={faQuestion} /></button>
		{/if}
 </div>
 <div class="text-left flex justify-end w-2 px-2">

  {#if $activeStore.includes(path)}
    {#if !$hideStore.includes(path) }
      <button
        class="btn h-9 w-10 text-right"
        title="Open or close {convertDisplayName(label, true)}"
        on:click={() => toggleShow(togglePath)}><Fa icon={faChevronUp} /></button
      >
      {:else}
      <button
        class="btn h-9 w-10 text-right"
        title="Open or close {convertDisplayName(label, true)}"
        on:click={() => toggleShow(togglePath)}><Fa icon={faChevronDown} /></button
      >
      {/if}
    {/if}
 </div>
</div>