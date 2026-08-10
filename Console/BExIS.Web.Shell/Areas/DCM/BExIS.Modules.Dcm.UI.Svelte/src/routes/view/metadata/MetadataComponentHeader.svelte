<script lang="ts">
	import { isActive, setActive, setInactive, toggleShow } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';
	import { faChevronUp, faChevronDown } from '@fortawesome/free-solid-svg-icons';
  import Fa from 'svelte-fa';
  import { activeStore, hideStore } from '$lib/components/utils/metadata/stores';
  import { onMount } from 'svelte';
	

 export let required: boolean = false;
 //  $:required;
 export let path: string;
 export let p:string = '';
 export let description: string = '';
 
 let active: boolean = false;
 $:active;

 $: depth = Math.max(0, path.split('.').length - 1);
 $: leftIndentPx = depth * 8;


 let label: string = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
 let togglePath: string = path;
 $: togglePath = p !== '' ? p : path;
 
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
	function handleToggleShow() {
		if (!active || !$activeStore.includes(path)) {
			return;
		}
		toggleShow(togglePath);
	}

</script>



<div class=" dark:bg-primary-500 items-center" class:first-level-sticky={depth === 0}>
<!--if depth is greater than 0, add a left border to indicate hierarchy-->
	
			
		
	

 <div class="pl-2 card flex  bg-primary-300 dark:bg-primary-800 rounded-sm border-l border-gray-300">
	  <div>
  
  	{#if !$hideStore.includes(path)}
				<button
					class="btn-sm text-right"
					title="Open or close {convertDisplayName(label, true)}"
					on:click={handleToggleShow}><Fa icon={faChevronUp} /></button
				>
			{:else}
				<button
					class="btn-sm text-right"
					title="Open or close {convertDisplayName(label, true)}"
					on:click={handleToggleShow}><Fa icon={faChevronDown} /></button
				>
			{/if}
   </div>
  <button class="text-left grow" on:click={handleToggleShow} type="button">
	<h5 id ={path} class="text-md font-bold" title={description || convertDisplayName(label, true)}>
      {convertDisplayName(label, true)} 
    </h5>
    </button>
 </div>

 <!-- {#if description && showDescription}
  <div	class="text-sm text-gray-500 py-1">{@html description}</div>
 {/if}
 <div class="text-left flex justify-end w-2 px-6 ">
 {#if description}
			<button class="badge" on:click={()=>showDescription = !showDescription}><Fa icon={faQuestion} /></button>
	{/if}
 </div>-->

</div>

<style>
.first-level-sticky {
	position: sticky;
	top: 0;
	z-index: 30;
}
</style>