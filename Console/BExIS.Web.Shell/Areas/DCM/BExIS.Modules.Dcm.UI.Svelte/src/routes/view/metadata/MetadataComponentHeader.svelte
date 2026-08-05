<script lang="ts">
	import { empty, getNodeByPath, hasValue, isActive, setActive, setInactive, toggleShow } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';
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

 $: depth = Math.max(0, path.split('.').length - 1);
 $: leftIndentPx = depth * 12;


 let label: string = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
 let showDescription: boolean = false;

 const togglePath = p!=='' ? p : path; 
 
 onMount(() => {
    if(!$activeStore.includes(path)) {
      initActivity();
    }
    else {
      active = true;
    }
    console.log('init-active',path, $activeStore);
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

</script>



<div class="flex h-10 dark:bg-primary-500 items-center">
<!--if depth is greater than 0, add a left border to indicate hierarchy-->

 {#if depth == 0}
   <div class="text-left grow pl-2 pt-2" >
   <h4 id ={path} class="text-md font-bold h4">
      {convertDisplayName(label, true)} 
    </h4>
    </div>
 {:else}
 <div class="text-left grow" style={`padding-left: ${leftIndentPx }px`}>
	   <h5 id ={path} class="text-md font-bold h5">
      {convertDisplayName(label, true)} 
    </h5>
 </div>
 {/if}

 {#if description && showDescription}
  <div	class="text-sm text-gray-500 py-1">{@html description}</div>
 {/if}
 <div class="text-left flex justify-end w-2 px-6 ">
  {#if description}
			<button class="badge" on:click={()=>showDescription = !showDescription}><Fa icon={faQuestion} /></button>
	{/if}
 </div>

</div>