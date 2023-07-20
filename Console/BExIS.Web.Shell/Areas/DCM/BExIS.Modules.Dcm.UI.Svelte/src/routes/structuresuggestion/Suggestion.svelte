<script lang="ts">

import Variable from './variable/Variable.svelte'
import {Spinner} from '@bexis2/bexis2-core-ui';
import {onMount} from 'svelte';
import {getDataTypes,getUnits} from '$services/StructureSuggestionCaller'
import type { VariableModel, missingValueType } from '$models/StructureSuggestion';

export let variables:VariableModel[] = [];
export let missingValues:missingValueType[] = [];
export let data:string[][];


$:datatypes=null;
$:units=null;


// validation array
let variableValidationStates = [];

export let valid=true;

onMount(async ()=>{

datatypes = await getDataTypes();
units = await getUnits();

console.log("datatypes",datatypes);
console.log("units", units);
console.log("units", missingValues);

fillVariableValdationStates(variables);


})

function fillVariableValdationStates(vars)
{
  for (let index = 0; index < vars.length; index++) {
    variableValidationStates.push(false);    
  }
}


// every time when validation state of a varaible is change,
// this function triggered an check wheter save button can be active or not
function checkValidationState()
{
  valid = variableValidationStates.every(v=>v===true)
}

function getColumnData(cellIndex)
{
  let cValues:string[]= [];
  for (let index = 0; index < data.length; index++) {
    const c = data[index][cellIndex];
    cValues.push(c);
  }
  return cValues;
}

</script>
<div class="suggestion-container" >

{#if variables && datatypes && units && variableValidationStates && missingValues}
    <!-- else content here -->
    {#each variables as variable, i}
      <!-- content here -->
      <Variable 
        {variable} 
        index={i} 
        on:var-change={checkValidationState}  
        {datatypes} 
        {units} 
        bind:isValid={variableValidationStates[i]} 
        bind:missingValues={missingValues}
        data={getColumnData(i)}
        />
      <br>
    {/each}
  
{:else}
  <Spinner label="loading suggested structure" />
{/if}
</div>

<style>
  .suggestion-container{
    padding-top: 1rem;
  }
</style>