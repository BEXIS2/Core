<script>

import Fa from 'svelte-fa/src/index'
import { faTrashAlt } from '@fortawesome/free-solid-svg-icons'
import {Row,Col,Button, Spinner, FormGroup, Label } from 'sveltestrap';
import {onMount} from 'svelte'
import { getCreate, create }  from '../../services/CreateCaller'


import suite from './form'
import InputEntry from './InputEntry.svelte'

export let id;
let titleField;
let descriptionField;
let metadataFields = [];

import {createEventDispatcher} from 'svelte'
const dispatch = createEventDispatcher();


// validation
let result = suite.get();
$:disabled = !result.isValid();

let model = null;
let onSaving = false;


onMount(async () => {

console.log("form");

const res = await getCreate(id);
console.log("res",res);
if(res != false) model = res;

filterInputs();
suite.reset();

});


async function handleSubmit() {
  // check if form is valid
  if(result.isValid())
  {
    onSaving = true;
    console.log("before submit", model);
    const res = await create(model);
    if(res.success!=false)
    {
      console.log("save", res);
      dispatch("save", res.id);
      
    }

    onSaving = false;

  }
}

//change event: if input change check also validation only on the field
// e.target.id is the id of the input component
function onChangeHandler(e)
{
  // add some delay so the entityTemplate is updated 
  // otherwise the values are old
  setTimeout(async () => {
    result = suite(model.inputFields, e.target.id)
 },10)
}

function onCancel(){

  suite.reset();
  dispatch("cancel");

}

// set title and description and remove from other fields
// because of getting a good form structure
function filterInputs()
{
  for (let index = 0; index < model.inputFields.length; index++) {
    const element = model.inputFields[index];
    if(element.name.toLowerCase() =="title")
    {
      titleField = element;
    }
    else
    if(element.name.toLowerCase() =="description")
    {
      descriptionField = element;
    }
    else
    {
      metadataFields = [...metadataFields,element]
    }

  }
}


</script>

{#if model}
<h1>Create a {model.name}</h1>
<p>{model.description}</p>



<form on:submit|preventDefault={handleSubmit}>

<Row>
  <Col>
    {#if titleField}
      <InputEntry label={titleField.name} type={titleField.type} bind:value={titleField.value}
      valid={result.isValid(titleField.name)} 
      invalid={result.hasErrors(titleField.name)}  
      feedback={result.getErrors(titleField.name)} 
      on:input={onChangeHandler}
      />
    {/if}
    {#if descriptionField}
      <InputEntry label={descriptionField.name} type="Text" bind:value={descriptionField.value}
      valid={result.isValid(descriptionField.name)} 
      invalid={result.hasErrors(descriptionField.name)}  
      feedback={result.getErrors(descriptionField.name)} 
      on:input={onChangeHandler}
      />
    {/if}
    {#each metadataFields as item}
      <InputEntry label={item.name} type={item.type} bind:value={item.value}
      valid={result.isValid(item.name)} 
      invalid={result.hasErrors(item.name)}  
      feedback={result.getErrors(item.name)} 
      on:input={onChangeHandler}
      />
    {/each}
  </Col>
</Row>


{#if model.datastructures && model.datastructures.length>0} <!--data structures exist-->
<Row>
  <Col>
    <b>Usable structures</b>
    <ul>
      {#each model.datastructures as item}
          <li>{item}</li>
      {/each}
    </ul>   
  </Col>
</Row>
{/if}

{#if model.fileTypes && model.fileTypes.length>0} <!--file types exist-->
<Row>
  <Col>
    <FormGroup>
      <Label>Supported file types</Label>
      <ul>
        {#each model.fileTypes as item}
            <li>{item}</li>
        {/each}
      </ul>
    </FormGroup>  
  </Col>
</Row>
{/if}
<Row>
 <Col>
  <p class="text-end">
    <FormGroup>
      {#if onSaving}
        <Spinner color="primary" size="sm" type ="grow" />
      {/if}
      <Button color="primary" {disabled} >Create </Button>
      <Button color="danger" type="button" on:click={onCancel}><Fa icon={faTrashAlt}/></Button>
    </FormGroup>  
  </p>
 </Col>
</Row>

</form>
{:else} <!-- while data is not loaded show a loading information -->
<Spinner color="primary" size="sm" type ="grow" />
{/if}