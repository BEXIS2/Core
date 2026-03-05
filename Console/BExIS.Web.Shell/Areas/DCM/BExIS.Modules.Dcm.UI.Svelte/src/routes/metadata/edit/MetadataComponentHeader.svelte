<script lang="ts">
	import { toggleShow, show } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '../metadataShared';

	import { faPlus, faChevronUp, faChevronDown, faQuestion } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';

 import { hideStore } from '$lib/components/utils/metadata/stores';

 export let required: boolean = false;
	export let path: string;
 export let p:string = '';
 export let description: string = '';

	let label: string = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
 let showDescription: boolean = false;

 const togglePath = p!=='' ? p : path; 

</script>



<div class="card flex h-9 bg-primary-300 dark:bg-primary-800 pl-5 items-center">

  <!-- {#if !required}
  <div>
     <input class="checkbox" type="checkbox" bind:checked={active} />
  </div>
  {/if} -->

 <div class="text-left grow">
	   <h4 id="{path}" class="h4">
    {convertDisplayName(label, true)} 
    {#if required}*{/if}
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


    {#if !$hideStore.includes(path)}
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

 </div>
</div>