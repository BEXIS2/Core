<script>

import Variable from './variable/Variable.svelte'
import {Spinner, Button} from 'sveltestrap';
import {onMount} from 'svelte';
import {getDataTypes,getUnits} from './caller.js'


export let variables = [];

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
<div class="suggestion-container">

{#if variables && datatypes && units && variableValidationStates}
    <!-- else content here -->
    {#each variables as variable, i}
      <!-- content here -->
      <Variable {variable} index={i} on:var-change={checkValidationState}  {datatypes} {units} bind:isValid={variableValidationStates[i]}/>
      <br>
    {/each}
  
{:else}
  <Spinner color="primary" size="sm" type ="grow" />
{/if}
</div>

<style>
  .suggestion-container{
    padding-top: 1rem;
  }
</style>