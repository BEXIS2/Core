<script>

import Fa from 'svelte-fa/src/fa.svelte'
import {faEye, faEyeSlash} from '@fortawesome/free-solid-svg-icons/index'

import { createEventDispatcher } from 'svelte'
import ContentContainer from '../../lib/components/ContentContainer.svelte';

export let id;
export let name = "";
export let description = "";
export let entityType;
export let metadataStructure;
export let linkedSubjects=[];

let hidden = true;

const dispatch = createEventDispatcher();

</script>

<ContentContainer>
	<header class="card-header">
    <div class="grid grid-cols-2">
      <div class="text-left">
        <h2 class="h2">{name}</h2>
        <span class="text-sm">{metadataStructure.text}</span> 
      </div>
      <div class="text-right">
        {#if linkedSubjects.length>0}
        <span class="badge variant-filled-error">in use</span>
        {/if}
        <span class="badge variant-filled-secondary">{entityType.text}</span>
      </div>
    </div>
    
  </header>
 
	<section class="p-4">
 

  {#if description }<blockquote class="blockquote mb-5">{description}</blockquote>{/if}

  {#if linkedSubjects.length>0}
    <i>Show linked {entityType.text}: 
      <button class="btn-icon" on:click={()=>hidden = !hidden}>
      {#if hidden}
      <Fa icon={faEye}/>
      {:else}
      <Fa icon={faEyeSlash}/>
      {/if}
    </button>
   </i>
    <div class:hidden="{hidden}">
      <ul class="ul list-disc pl-5">
        {#each linkedSubjects as item}
          <li>{item.text} ({item.id})</li>
        {/each}  
      </ul>
   </div>
  {/if}

 </section>
	<footer class="card-footer">
  <slot>
   no action setup
  </slot>
 </footer>
</ContentContainer>