<script>

import { createEventDispatcher } from 'svelte'
import ContentContainer from '../../lib/components/ContentContainer.svelte';

export let id;
export let name = "";
export let description = "";
export let entityType;
export let metadataStructure;

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
        <span class="badge variant-filled-secondary">{entityType.text}</span>
      </div>
    </div>
    
  </header>
 
	<section class="p-4">
 

  {#if description }<blockquote class="blockquote mb-5">{description}</blockquote>{/if}

  

 </section>
	<footer class="card-footer">
  <slot>
   no action setup
  </slot>
 </footer>
</ContentContainer>