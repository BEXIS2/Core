<script>

import {onMount} from 'svelte';

import {FormGroup, Label,Input, Card,CardHeader, CardBody,CardFooter } from 'sveltestrap';
import Select from 'svelte-select';
import DataTypeDescription from './DataTypeDescription.svelte'
import Container from './Container.svelte'
import Header from './Header.svelte'
import Footer from './Footer.svelte'

export let variable;
export let index;

export let datatypes;
export let units;

let loaded = false;

const groupBy = (item) => item.group;

onMount(()=>{

      datatypes = [...datatypes.filter(d=>d.id!=variable.dataType.id)]
      datatypes = [variable.dataType,...datatypes];

      units = [...units.filter(d=> !variable.possibleUnits.some(u=>u.id==d.id))]
      units = [...variable.possibleUnits,...units];

      loaded = true;
})



</script>

{#if loaded}
<Card>
<CardHeader>
      <Header {...variable} {index}/>
</CardHeader>
<CardBody>

<!--Description-->
<Container pSize=8>
      <div slot="property">
            <FormGroup>
                  <Label>Description:</Label> 
                  <Input id="description" bind:value={variable.description} on:change></Input> 
            </FormGroup>
      </div>
</Container>

<!--Datatype-->
<Container>
      <div slot="property">
            <FormGroup>
                  <Label>Datatype:</Label> 
                  <Select 
                        items={datatypes} 
                        optionIdentifier="id"
                        labelIdentifier="text"
                        isMulti={false}
                        bind:value={variable.dataType}
                        {groupBy}
                        placeholder="-- Please select --"
                        >
                  </Select>
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
                        items={units} 
                        optionIdentifier="id"
                        labelIdentifier="text"
                        isMulti={false}
                        bind:value={variable.unit}
                        {groupBy}
                        placeholder="-- Please select --"
                        ></Select>
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
