<script>
 import { onMount } from 'svelte'; 
 import { hosturl } from '../../stores/store.js'
 import { Spinner,ListGroup, ListGroupItem, Button } from 'sveltestrap';

$:messages = null;

// load attributes from div
let container = document.getElementById('messages');
let id = container.getAttribute("dataset");

onMount(async () => {
   loadMessages()
})

async function loadMessages()
{
   messages = null;
   const url = hosturl+'dcm/messages/load?id='+id;
   const res = await fetch(url);
   messages = await res.json();
}

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
   <Spinner color="primary" size="sm" type ="grow" text-center />
{/if}


