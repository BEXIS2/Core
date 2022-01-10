<script>

import {onMount} from 'svelte';
import { hosturl } from '../../stores/store.js'
import { Spinner, Button, Modal,ModalBody, ModalFooter,ModalHeader } from 'sveltestrap';

export let id=0;
export let version=1;
export let status=0;
export let displayName="";
export let start="";
export let description="";

let model;

export let open = false;
const toggle = () => (open = !open);
// onMount(async () => {
//   load();
// })

async function load()
{
  console.log("validation on mount")
  let url = hosturl+start+"?id="+id+"&version="+version;

  // load menu froms server
  const res = await fetch(url);
  model = await res.json();
}

</script>
<Modal isOpen={open} on:close on:open={load}>
  <ModalHeader {toggle}>{displayName}</ModalHeader>
  <ModalBody>
    {#if model}

      <!-- valid-->  
      {#if model.isValid==true}
      <p>imported data is valid</p>
      {/if}
      <!-- invalid-->  
      {#if model.isValid==false}
      <p>imported data is invalid</p>
      {/if}

      {:else} <!-- while data is not loaded show a loading information -->

      <Spinner color="info" size="sm" type ="grow" text-center />

    {/if}
  </ModalBody>
  <ModalFooter>
    <Button color="primary" on:click={toggle} >Cancel</Button>
  </ModalFooter>
</Modal>
