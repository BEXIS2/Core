<script>
 import Fa from 'svelte-fa/src/fa.svelte'
 import Card from './Card.svelte'
 import {createEventDispatcher} from 'svelte'


 import { faTrash, faPen } from '@fortawesome/free-solid-svg-icons'
 import { onMount } from 'svelte'; 
 import {Spinner, Row,Col, Button, Container} from 'sveltestrap';
 
 import { setApiConfig }  from '@bexis2/bexis2-core-ui/src/lib/index';


 const dispatch = createEventDispatcher();

 import { 
  deleteEntityTemplate
}  from '../../services/Caller'
 
 
 export let entitytemplates = null;

function edit(id){
  console.log("edit",id)
  dispatch("edit",id);
}
async function remove(index, id){

  console.log(index,id);
//remove in backend
  const res = await deleteEntityTemplate(id)
  if(res === true)
  {
    //remove list
    entitytemplates = entitytemplates.filter((id, idx) => {
      return idx !== index;
    });
  }
}

 
 </script>
 
<div style="padding-top: 20px;">
<Row>
 {#if entitytemplates}
    {#each entitytemplates as item, i (item.id)}
      <Col xs="6" ms="4" md="5" >
        <Card {...item} >
           <Button on:click={edit(item.id)}><Fa icon="{faPen}"/></Button>
           <Button on:click={remove(i, item.id)}><Fa icon="{faTrash}"/></Button>
        </Card>
      </Col>
    {/each}
 {:else}
  <Spinner color="info" size="sm" type ="grow" text-center />
 {/if}
</Row>
</div>
