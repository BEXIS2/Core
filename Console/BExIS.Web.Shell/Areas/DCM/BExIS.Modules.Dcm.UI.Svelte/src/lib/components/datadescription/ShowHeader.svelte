<script>
import {Button, Row, Col, Spinner} from 'sveltestrap';

import Fa from 'svelte-fa/src/fa.svelte'
import { faTrash } from '@fortawesome/free-solid-svg-icons'

import {deleteStructure} from '../../../services/DataDescriptionCaller'
import { latestDataDescriptionDate } from '../../../routes/edit/stores';

export let id;
export let structureId;
export let title;
export let description;

let loading = false;

async function remove()
{
 loading = true;
 const res = await deleteStructure(id,structureId);

 console.log(res);
 if(res.success = true)
 {
   // update store
   latestDataDescriptionDate.set(Date.now);
 }
 else
 {
   //show message
 }

 loading = false;

}

</script>

<div class="show-datadescription-header-container">
 <Row>
  <Col> <h2>{title} ({structureId}) </h2></Col>
  <Col>
   <div class="text-end">
    <Button on:click="{remove}"><Fa icon={faTrash}/></Button>
   </div>
   {#if loading}
   <Spinner color="info" size="sm" type ="grow" text-center />
   {/if}
  </Col>
 </Row>

 <p>{description}</p>
 
</div>

<style>
 .show-datadescription-header-container{

 }
</style>