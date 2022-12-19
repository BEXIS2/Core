<script>
 import ListItem from '../create/ListItem.svelte'
 export let items;

 import {createEventDispatcher} from 'svelte';

 const dispatch = createEventDispatcher();

</script>

{#each items as item, index}
  <ListItem {...item} on:click={()=>dispatch("select",index)}/>
{/each}
 
