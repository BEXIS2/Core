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

// add description to file display row
export let descriptionType;
export let withDescription;

const dispatch = createEventDispatcher();

let el;
$:date=null;

onMount(async () => {
  date = Date.now();
  setDescriptionValues(descriptionType);
  console.log("mount file overview");
  console.log(descriptionType);
  console.log(withDescription);
  console.log(files);
})

async function handleRemoveFile(e, index) {

   files.splice(index, 1)
   files = [...files];

   dispatch("success",{text:e.detail.text})
}

async function handleSave(e) {
  dispatch("success",{text:e.detail.text})
}

function setDescriptionValues(type)
{
  //type can be : none = 0, active = 1, required = 2
  if(type == 0) withDescription = false;
  if(type == 1) withDescription = true;
  if(type == 2) withDescription = true;

}

</script>

{#if files}

<!-- <Container> -->
 {#each files as item, index}

  <FileOverviewItem 
    {id} file={item.name} {...item}  {save} {remove} 
    on:removed={e => handleRemoveFile(e, index)} 
    on:saved={handleSave} {withDescription}/>
 {/each}
<!-- </Container> -->

{:else}
  <!-- spinner here -->
  <Spinner color="primary" size="sm" type ="grow" text-center />
{/if}