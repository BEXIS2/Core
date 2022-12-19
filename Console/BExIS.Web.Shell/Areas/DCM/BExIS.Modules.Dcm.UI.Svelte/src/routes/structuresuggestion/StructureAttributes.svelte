<script>
import { FormGroup, Input, Label, Row , Col} from 'sveltestrap';
import {onMount} from 'svelte';

export let model = null;
export let valid = false;

import suite from './structureAttributes'

// validation
let res = suite.get();

onMount(()=>{
  suite.reset();

})

//change event: if input change check also validation only on the field
// e.target.id is the id of the input component
function onChangeHandler(e)
{
  // add some delay so the entityTemplate is updated 
  // otherwise the values are old
  setTimeout(async () => {

  res = suite(model, e.target.id)
  valid = res.isValid();

 },10)
}

</script>
{#if model}
<div class="structure-attributes-container">
<Row>
  <Col>  
    <FormGroup>
      <Label>Title</Label>
      <Input 
        id="title"
        bind:value={model.title} 
        on:input={onChangeHandler}
        valid={res.isValid("title")} 
        invalid={res.hasErrors("title")}  
        feedback={res.getErrors("title")} 
        required={true}>
      </Input>
    </FormGroup>
  </Col>
  <Col>
    <FormGroup>
      <Label>Description</Label>
      <Input 
        id="description"
        type="textarea"
        bind:value={model.description} 
        on:input={onChangeHandler}
        valid={res.isValid("description")} 
        invalid={res.hasErrors("description")}  
        feedback={res.getErrors("description")} 
        required={true}>
    </Input>
    </FormGroup>
  </Col>
</Row>
</div>

{/if}


<style>
.structure-attributes-container
{
  padding-top:1em;
}
</style>