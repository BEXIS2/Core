<script>
 import ListItem from '../create/ListItem.svelte'

 export let items;

 import {createEventDispatcher} from 'svelte';

 const dispatch = createEventDispatcher();

</script>

<div class="grid gap-1 max-h-screen/2 overflow-auto pr-4 space-y-4">
{#each items as item, index}
  <ListItem {...item} on:click={()=>dispatch("select",index)}/>
{/each}
</div>
 
