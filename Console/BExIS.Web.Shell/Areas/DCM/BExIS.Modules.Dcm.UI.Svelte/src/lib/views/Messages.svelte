<script>
 import { onMount } from 'svelte'; 
 import {loadMessages}  from '../../routes/edit/services'

 import { Spinner,ListGroup, ListGroupItem, Button } from 'sveltestrap';

$:messages = [];

// load attributes from div
let container = document.getElementById('messages');
let id = container?.getAttribute("dataset");

onMount(async () => {
   messages = await loadMessages(id)
})


</script>

<Button on:click={loadMessages}>reload</Button>

{#if messages} <!-- if hooks list is loaded render hooks -->

<ListGroup flush>

   {#each messages as message}

   <ListGroupItem>
      <b>{message.timestamp}</b> <br/>

      {#each message.messages as messagepart}
         {messagepart} <br>
      {/each}

   
   </ListGroupItem>

   {/each}

 </ListGroup>

{:else} <!-- while data is not loaded show a loading information -->
   <Spinner color="primary" size="sm" type ="grow" />
{/if}


