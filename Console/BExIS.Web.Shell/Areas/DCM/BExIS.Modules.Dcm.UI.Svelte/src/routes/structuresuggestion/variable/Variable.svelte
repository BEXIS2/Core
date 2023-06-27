<script>

import {onMount, createEventDispatcher} from 'svelte';
import {TextInput,TextArea, MultiSelect, Alert} from '@bexis2/bexis2-core-ui'

import DataTypeDescription from './DataTypeDescription.svelte'
import Container from './Container.svelte'
import Header from './Header.svelte'
import Footer from './Footer.svelte'

import suite from './variable'

export let variable;
export let index;

export let datatypes;
export let units;

export let isValid=false;

// validation
let res = suite.get();

let loaded = false;

const dispatch = createEventDispatcher();

onMount(()=>{

      console.log("generate var -----------------")

      datatypes = [...datatypes.filter(d=>d.id!=variable.dataType.id)]
      datatypes = [variable.dataType,...datatypes];

      units = [...units.filter(d=> !variable.possibleUnits.some(u=>u.id==d.id))]
      units = [...variable.possibleUnits,...units];

      loaded = true;

      // reset & reload validation
      suite.reset();
      
      setTimeout(async () => {
      res = suite(variable);

      setValidationState(res);
      },10)

      console.log("variable",variable);
})

//change event: if input change check also validation only on the field
// e.target.id is the id of the input component
function onChangeHandler(e)
{
  // add some delay so the entityTemplate is updated 
  // otherwise the values are old
  setTimeout(async () => {

      res = suite(variable, e.target.id)
      setValidationState(res);

 },10)
}

//change event: if select change check also validation only on the field
// *** is the id of the input component
function onSelectHandler(e,id)
{
  //console.log(e);
  res = suite(variable, id);

  setValidationState(res);
}

function setValidationState(res)
{
 isValid = res.isValid();
 // dispatch this event to the parent to check the save button
 dispatch("var-change");
}

</script>

{#if loaded}
<div class="card">
      <header class="card-header">
            <Header bind:isKey={variable.isKey} bind:isOptional={variable.isOptional} name={variable.name} {index}/>
      </header>
      <section class="p-4">
            <!--Description-->
<Container pSize=8>

      <div slot="property">
            <TextArea id="description" 
            bind:value={variable.description} 
            on:input={onChangeHandler}
            valid={res.isValid("description")} 
            invalid={res.hasErrors("description")}  
            feedback={res.getErrors("description")} 
            ></TextArea> 

      </div>
</Container>

<!--Datatype-->
<Container>
      <div slot="property">
  
            <MultiSelect 
                  id="dataType"
                  title="Data Type"
                  source={datatypes} 
                  itemId="id"
                  itemLabel="text"
                  itemGroup="group"
                  complexSource={true}
                  complexTarget={true}
                  isMulti={false}
                  bind:target={variable.dataType}
                  placeholder="-- Please select --"
                  invalid={res.hasErrors("displayPattern")}
                  feedback={res.getErrors("displayPattern")}
                  on:change={(e)=>onSelectHandler(e,"dataType")}
                  on:clear={(e)=>onSelectHandler(e,"dataType")}
                  >
            </MultiSelect>
      </div>

      <div slot="displaypattern">      
            <!--Show only when display pattern exists-->
       {#if variable.displayPattern && variable.possibleDisplayPattern.length>0}
            <MultiSelect 
                  id="displaypattern"
                  title="Display Pattern"
                  source={variable.possibleDisplayPattern} 
                  itemId="id"
                  itemLabel="text"
                  itemGroup="group"
                  complexSource={true}
                  complexTarget={true}
                  isMulti={false}
                  bind:target={variable.displayPattern}
                  placeholder="-- Please select --"
                  invalid={res.hasErrors("displayPattern")}
                  feedback={res.getErrors("displayPattern")}
                  on:change={(e)=>onSelectHandler(e,"displaypattern")}
                  on:clear={(e)=>onSelectHandler(e,"displaypattern")}
                  >
            </MultiSelect>
          {/if}

      </div>
      <div slot="description">
            <DataTypeDescription type={variable.dataType.text} />
      </div>
</Container>

<!--Unit-->
<Container>
      <div slot="property">
            <MultiSelect 
                  id="unit"
                  title="Unit"
                  source={units} 
                  itemId="id"
                  itemLabel="text"
                  itemGroup="group"
                  complexSource={true}
                  complexTarget={true}
                  isMulti={false}
                  bind:target={variable.unit}
                  placeholder="-- Please select --"
                  invalid={res.hasErrors("unit")}
                  feedback={res.getErrors("unit")}
                  on:change={(e)=>onSelectHandler(e,"unit")}
                  on:clear={(e)=>onSelectHandler(e,"unit")}

                  >
            </MultiSelect>


      </div>
      <div slot="description">
            show all information about the units in a table 
            Nothing found? Make a new suggestion.
      </div>
</Container>

<!--Meaning-->
<Container>
      <div slot="property">
            <TextInput id="template" label="Variable Template" bind:value={variable.template.text}></TextInput> 
      </div>
      <div slot="description">
            ...
      </div>
</Container>

</section>
<footer class="card-footer">
      <Footer {...variable}></Footer>  
</footer>
</div>

{/if}
