<script>

import Fa from 'svelte-fa/src/fa.svelte'
import { faTrashAlt } from '@fortawesome/free-solid-svg-icons'
import {Row,Col,Button, Spinner } from 'sveltestrap';
import {onMount} from 'svelte'
import { getCreate, create }  from '../../services/CreateCaller'


import suite from './form'
import InputEntry from './InputEntry.svelte'

export let id;


import {createEventDispatcher} from 'svelte'
const dispatch = createEventDispatcher();


// validation
let res = suite.get();
$:disabled = false;//!res.isValid();

let model = null


onMount(async () => {

const res = await getCreate(id);
console.log("res",res);
if(res != false) model = res;

});


async function handleSubmit() {
    console.log("before submit", model);
    const res = await create(model);
    if(res!=false)
    {
      console.log("save", res);
      //dispatch("save", res);
    }
  }

//change event: if input change check also validation only on the field
// e.target.id is the id of the input component
function onChangeHandler(e)
{
  // add some delay so the entityTemplate is updated 
  // otherwise the values are old
  setTimeout(async () => {
    res = suite(entityTemplate, e.target.id)
 },10)
}

</script>

{#if model}
<h1>Create a {model.name}</h1>
<p>{model.description}</p>



<form on:submit|preventDefault={handleSubmit}>

{#each model.inputFields as item}
<Row>
  <Col>
    <b>{item.name}</b>
  </Col>
  <Col>
    <InputEntry label={item.name} type={item.type} bind:value={item.value}/>
  </Col>
</Row>
{/each}

<Row>
  <Col>
    <b>Usable structures</b>
  </Col>
  <Col>
    <ul>
      {#each model.datastructures as item}
        <li>{item}</li>
      {/each}
    </ul>    
  </Col>
</Row>
<Row>
  <Col>
    <b>Supported file types</b>
  </Col>
  <Col>
    <ul>
      {#each model.fileTypes as item}
          <li>{item}</li>
      {/each}
      </ul>      
  </Col>
</Row>
<Row>
  
 <Col>
  <p class="text-end">
   <Button color="primary" {disabled} >Create</Button>
   <Button color="danger" on:click={()=> dispatch("cancel")}><Fa icon={faTrashAlt}/></Button>
  </p>
 </Col>
</Row>

</form>
{:else} <!-- while data is not loaded show a loading information -->
<Spinner color="primary" size="sm" type ="grow" text-center />
{/if}