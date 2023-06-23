<script>
import {TextInput,TextArea} from '@bexis2/bexis2-core-ui'
import {onMount} from 'svelte';

export let model;
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
<div class="structure-attributes-container grid md:grid-cols-2 sm:grid-cols-1 gap-5">
  <TextInput 
    id="title"
    label="Title"
    bind:value={model.title} 
    on:input={onChangeHandler}
    valid={res.isValid("title")} 
    invalid={res.hasErrors("title")}  
    feedback={res.getErrors("title")} 
    required={true}>
  </TextInput>

  <TextArea 
    id="description",
    label="Description"
    bind:value={model.description} 
    on:input={onChangeHandler}
    valid={res.isValid("description")} 
    invalid={res.hasErrors("description")}  
    feedback={res.getErrors("description")} 
    required={true}>
  </TextArea>
</div>

{/if}
