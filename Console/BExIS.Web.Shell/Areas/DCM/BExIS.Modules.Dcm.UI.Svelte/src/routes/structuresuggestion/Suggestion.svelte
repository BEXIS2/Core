<script>

import Variable from './variable/Variable.svelte'
import {Spinner} from '@bexis2/bexis2-core-ui';
import {onMount} from 'svelte';
import {getDataTypes,getUnits} from '$services/StructureSuggestionCaller'

export let variables = [];
export let missingValues = [];

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



</script>
<div class="suggestion-container" >

{#if variables && datatypes && units && variableValidationStates && missingValues}
    <!-- else content here -->
    {#each variables as variable, i}
      <!-- content here -->
      <Variable {variable} index={i} on:var-change={checkValidationState}  {datatypes} {units} bind:isValid={variableValidationStates[i]} bind:missingValues={missingValues}/>
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