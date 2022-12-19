<script>
import {Fa} from 'svelte-fa/src/index.js'

import {FileInfo} from '@bexis2/bexis2-core-ui/src/lib/index'
import { Spinner, Button, Input, Col, Row  } from 'sveltestrap';
import { faTrash} from '@fortawesome/free-solid-svg-icons'

import {removeFile, saveFileDescription}  from '../../services/Caller'

import { createEventDispatcher, onMount } from 'svelte';

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

// set if its possible to generate a structure based on that file
export let generateAble = false;

let loading = false;

onMount(async ()=>{
 

})

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
 {#if type}
  <div class="file-overview-item row">
  <Col xs="1"><FileInfo {type} size="x-large" />test</Col>
  <Col > 
      {file}
  </Col>
  {#if withDescription}
    <Col xs="5"><Input  bind:value="{description}" placeholder="description" on:change={e => handleSaveFileDescription()}/></Col>
  {/if}
  <Col >
    <div class="file-overview-item-options">
      <div class="file-overview-item-option"><Button size="sm" on:click={e => handleRemoveFile()}><Fa icon={faTrash}/></Button></div>
      <div class="file-overview-item-option">{#if loading}<Spinner color="info" size="sm" type ="grow" /> {/if}</div>    
    </div>
  </Col>
  </div>
{/if}
<style>
 

 .file-overview-item {
  border-bottom: 1px solid var(--bg-grey);
  color: var(--text-color);
  padding: 0.5em 0;
 }

 .file-overview-item-options{
    width:100%;
  }

.file-overview-item-option{
    float: right;
    padding-left: 3px;
  }

</style>