<script>

import Fa from 'svelte-fa/src/fa.svelte'
import {Row,Col, FormGroup, Input, Label, Button} from 'sveltestrap'
import {createEventDispatcher} from 'svelte'
import {faTrashAlt } from '@fortawesome/free-regular-svg-icons'
import {faPlus } from '@fortawesome/free-solid-svg-icons'

export let displayName = "";
export let description = "";
export let last = true;

const dispatch = new createEventDispatcher();

function remove()
{
  dispatch("remove");
}

function add()
{
 dispatch("add");
}

</script>

 <Row>
  <Col xs=2>
   <FormGroup>
    <!-- <Label>Name:</Label>  -->
    <Input id="name" bind:value={displayName} on:change placeholder="Placeholder"></Input> 
   </FormGroup>
  </Col>
  <Col>
    <FormGroup>
      <!-- <Label>Description:</Label>  -->
      <Input id="description" bind:value={description} on:change placeholder="Description"></Input> 
   </FormGroup>
  </Col>
  <Col>
   <div class="missing-value-remove-container">
    <Button type="button" on:click={remove}><Fa icon={faTrashAlt}/></Button>
    {#if last} 
     <Button class="add" type="button" on:click={add}><Fa icon={faPlus}/></Button>
    {/if}
   </div>
  </Col>
 </Row>

<style>
 .missing-value-remove-container{
  /* padding-top:30px; */
 }
</style>

