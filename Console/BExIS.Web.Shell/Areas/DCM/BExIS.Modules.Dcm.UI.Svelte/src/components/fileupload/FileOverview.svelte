<script>
import { Spinner, Container  } from 'sveltestrap';
import { onMount , createEventDispatcher}from 'svelte'

import FileOverviewItem from './FileOverviewItem.svelte'


export let id;
export let files;
// action to save description
export let save;
// action to remove file
export let remove

// context of file overview - data or attachments
export let context;

const dispatch = createEventDispatcher();

let el;
$:date=null;

onMount(async () => {
  date = Date.now();
  console.log("mount file overview");
})

async function handleRemoveFile(e, index) {

   files.splice(index, 1)
   files = [...files];

   dispatch("success",{text:e.detail.text})
}

async function handleSave(e) {
  dispatch("success",{text:e.detail.text})
}

</script>

{#if files}

<!-- <Container> -->
 {#each files as item, index}
  <FileOverviewItem 
    {id} type={item.Type} file={item.Name} description={item.Description} {save} {remove} 
    on:removed={e => handleRemoveFile(e, index)} 
    on:saved={handleSave} />
 {/each}
<!-- </Container> -->

{:else}
  <!-- spinner here -->
  <Spinner color="primary" size="sm" type ="grow" text-center />
{/if}