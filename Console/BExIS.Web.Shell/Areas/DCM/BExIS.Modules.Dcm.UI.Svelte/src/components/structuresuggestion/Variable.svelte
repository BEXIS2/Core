<script>

import {FormGroup, Label,Input, Col, Row } from 'sveltestrap';
import DropdownKvP  from '../form/DropdownKvP.svelte'
export let variable;
export let index;

</script>

{#if variable}
<div class="variable-container">
<h1>{index}</h1>

<FormGroup>
 <Label>Name:</Label> 
 <Input id="name" bind:value={variable.name} ></Input> 
</FormGroup>

<FormGroup>
 <Label>Description:</Label> 
 <Input id="description" bind:value={variable.description} on:change></Input> 
</FormGroup>

<FormGroup>
 <Label>System Type: {variable.systemType}</Label>
</FormGroup>

<FormGroup>
 <Label>Datatype:</Label> 
 <Input id="dataType" bind:value={variable.dataType.text}></Input> 
</FormGroup>

<!-- <FormGroup>
 <Label>Unit:</Label> 
 <Input id="unit" bind:value={variable.unit.text}></Input> 
</FormGroup> -->

<DropdownKvP
      id="units"
      title="Unit" 
      source={variable.possibleUnits}
      bind:target={variable.unit}/>

<FormGroup>
 <Label>Template:</Label> 
 <Input id="template" bind:value={variable.template.text}></Input> 
</FormGroup>

</div>
{/if}

<style>

.variable-container
{
 background-color: #efefef;
 border: 2px solid var(--bs-info);
 border-radius: 1em;
 min-height: 200px;
 padding: 1rem;
 margin-top: 1rem;

}


</style>