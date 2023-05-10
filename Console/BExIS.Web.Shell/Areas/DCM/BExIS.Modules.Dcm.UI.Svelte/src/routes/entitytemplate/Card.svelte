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
	<header class="card-header"><h2>{name}</h2></header>
 
	<section class="p-4">
  {#if description }{description}{/if}
  <p>{entityType.text}</p> <br>
  <span>{metadataStructure.text}</span>

 </section>
	<footer class="card-footer">
  <slot>
   no action setup
  </slot>
 </footer>
</ContentContainer>