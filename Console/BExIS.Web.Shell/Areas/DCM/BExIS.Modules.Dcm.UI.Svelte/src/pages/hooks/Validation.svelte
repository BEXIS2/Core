<script>
import {getHookStart}  from '../../services/Caller'
import { latestFileUploadDate, latestDataDescriptionDate } from '../../stores/editStores';
import { Spinner, Alert,Button, Modal,ModalHeader, ModalBody } from 'sveltestrap';
import { onMount }from 'svelte'

import Selection from '../../components/structuresuggestion/Selection.svelte';

export let id=0;
export let version=1;
export let status=0;
export let displayName="";
export let start="";
export let description="";

let model;

// modal window for selection
let open = false;


$:$latestFileUploadDate, reload()
$:$latestDataDescriptionDate, reload()

onMount(async () => {
  load();
})

async function load()
{
  //const res = await fetch(url);
  model = await getHookStart(start,id,version);
  //console.log("validation",model);

}

async function reload()
{
  //console.log("run validation");
  load();

} 

const toggle = () => {
    open = !open;
  };

</script>
  
  {#if model}

    {#if model.isValid == true}
          <Alert color="success" dismissible>valid</Alert>  
      {:else}
          <Alert color="danger" dismissible>not valid</Alert>

          <b>errors</b>
          
          <ul>
            {#each model.errors as error}
              <!-- content here -->
              <li>{error}</li>
            {/each}
          </ul>

          <b>sorted errors</b>
          <ul>
            {#each model.sortedErrors as error}
            <!-- content here -->
            <li >{error}</li>
            {/each}
          </ul>
          
          <Button color="primary" on:click={toggle}>open reader informations</Button>
          <Modal isOpen={open} {toggle} fullscreen="{true}">
            <ModalHeader {toggle}>Setup filer reader information {model.isValid}</ModalHeader>
            <ModalBody>  
              <Selection id={id} on:saved={reload}/>
            </ModalBody>
          </Modal>
    {/if}
  


    {:else} <!-- while data is not loaded show a loading information -->

    <Spinner color="info" size="sm" type ="grow" text-center />

  {/if}
