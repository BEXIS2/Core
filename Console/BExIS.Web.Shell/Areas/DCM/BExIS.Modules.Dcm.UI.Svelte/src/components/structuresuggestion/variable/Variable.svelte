<script>

import {onMount, createEventDispatcher} from 'svelte';

import {FormGroup, Label,Input, Card,CardHeader, CardBody,CardFooter } from 'sveltestrap';
import Select from 'svelte-select';
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

const dispatch = new createEventDispatcher();

const groupBy = (item) => item.group;

onMount(()=>{

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

      console.log(variable);
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
<Card >
<CardHeader>
      <Header bind:isKey={variable.isKey} bind:isOptional={variable.isOptional} name={variable.name} {index}/>
</CardHeader>
<CardBody>

<!--Description-->
<Container pSize=8>
      <div slot="property">
            <FormGroup>
                  <Label>Description:</Label> 
                  <Input id="description" 
                  bind:value={variable.description} 
                  on:input={onChangeHandler}
                  valid={res.isValid("description")} 
                  invalid={res.hasErrors("description")}  
                  feedback={res.getErrors("description")} 
                  ></Input> 
            </FormGroup>
      </div>
</Container>

<!--Datatype-->
<Container>
      <div slot="property">
            <FormGroup>
                  <Label>Datatype:</Label> 
                  <Select 
                        id="dataType"
                        items={datatypes} 
                        optionIdentifier="id"
                        labelIdentifier="text"
                        isMulti={false}
                        isClearable={false}
                        bind:value={variable.dataType}
                        {groupBy}
                        placeholder="-- Please select --"
                        hasError = {res.hasErrors("dataType")}
                        on:select={(e)=>onSelectHandler(e,"dataType")}
                        on:clear={(e)=>onSelectHandler(e,"dataType")}
                        >
                  </Select>

                  {#if res.hasErrors("dataType")}
                    {#each res.getErrors("dataType") as error}
                         <!-- content here -->
                         <div class="invalid-feedback" style="display:block">
                          {error}
                        </div>
                    {/each}
                  {/if}
            </FormGroup>
      </div>
      <div slot="displaypattern">      
            <FormGroup>
                  <Label>Displaypattern:</Label> 
                  <Select 
                        id="displaypattern"
                        items={variable.possibleDisplayPattern} 
                        optionIdentifier="id"
                        labelIdentifier="text"
                        isMulti={false}
                        isClearable={false}
                        bind:value={variable.displayPattern}
                  
                        placeholder="-- Please select --"
                        hasError = {res.hasErrors("displayPattern")}
                        on:select={(e)=>onSelectHandler(e,"displayPattern")}
                        on:clear={(e)=>onSelectHandler(e,"displayPattern")}
                        >
                  </Select>

                  {#if res.hasErrors("displayPattern")}
                    {#each res.getErrors("displayPattern") as error}
                         <!-- content here -->
                         <div class="invalid-feedback" style="display:block">
                          {error}
                        </div>
                    {/each}
                  {/if}
            </FormGroup>
      </div>
      <div slot="description">
            <DataTypeDescription type={variable.dataType.text} />
      </div>
</Container>

<!--Unit-->
<Container>
      <div slot="property">
            <FormGroup>
                  <Label>Unit:</Label> 
                  <Select 
                        id="unit"
                        items={units} 
                        optionIdentifier="id"
                        labelIdentifier="text"
                        isMulti={false}
                        isClearable={true}
                        bind:value={variable.unit}
                        {groupBy}
                        placeholder="-- Please select --"
                        hasError = {res.hasErrors("unit")}
                        on:select={(e)=>onSelectHandler(e,"unit")}
                        on:clear={(e)=>onSelectHandler(e,"unit")}
                        ></Select>

                  {#if res.hasErrors("unit")}
                    {#each res.getErrors("unit") as error}
                         <!-- content here -->
                         <div class="invalid-feedback" style="display:block">
                          {error}
                        </div>
                    {/each}
                  {/if}
            </FormGroup>

      </div>
      <div slot="description">
            show all information about the units in a table 
            Nothing found? Make a new suggestion.
      </div>
</Container>

<!--Meaning-->
<Container>
      <div slot="property">
            <FormGroup>
                  <Label>Template:</Label> 
                  <Input id="template" bind:value={variable.template.text}></Input> 
            </FormGroup>
      </div>
      <div slot="description">
            ...
      </div>
</Container>

</CardBody>
<CardFooter>
   <Footer {...variable}></Footer>   
</CardFooter>
</Card>

{/if}
