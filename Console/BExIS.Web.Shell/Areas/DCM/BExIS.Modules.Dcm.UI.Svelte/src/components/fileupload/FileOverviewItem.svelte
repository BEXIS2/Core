<script>
import Fa from 'svelte-fa/src/fa.svelte'

import {FileInfo} from '@bexis2/svelte-bexis2-core-ui'
import { Spinner, Button, Input, Col, Row  } from 'sveltestrap';
import { faTrash } from '@fortawesome/free-solid-svg-icons'

import {removeFile, saveFileDescription}  from '../../services/Caller'
import { createEventDispatcher } from 'svelte';

export let id;
export let file;
export let type;
export let description;
export let withDescription;
const dispatch = createEventDispatcher();

// action to save description
export let save;
// action to remove file
export let remove

let loading = false;

async function handleRemoveFile() {
loading = true;
//remove from server
const res = await removeFile(remove,id,file);

 if(res.status==200 )
 {
   let message = file+" removed."
   dispatch("removed",{text:message})
 }
 loading = false;
}

async function handleSaveFileDescription() {

const res = await saveFileDescription(save,id, file, description );
  if(true)
  {
    let message = "Description of "+file+" is updated."
    dispatch("saved",{text:message})
  }
}

</script>
 
<div class="file-overview-item row">
 <Col xs="1"><FileInfo {type} size="x-large" /></Col>
 <Col > 
    {file}
 </Col>
 {#if withDescription}
  <Col xs="5"><Input  bind:value="{description}" placeholder="description" on:change={e => handleSaveFileDescription()}/></Col>
{/if}
 <Col xs="1">
  <Button size="sm" on:click={e => handleRemoveFile()}><Fa icon={faTrash}/></Button>
  {#if loading}<Spinner color="info" size="sm" type ="grow" text-center /> {/if}
 </Col>
</div>

<style>
 

 .file-overview-item {
  border-bottom: 1px solid var(--bg-grey);
  color: var(--text-color);
  padding: 0.5em 0;
  
 }
</style>