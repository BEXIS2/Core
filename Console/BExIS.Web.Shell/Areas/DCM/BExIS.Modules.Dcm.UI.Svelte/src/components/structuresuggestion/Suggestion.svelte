<script>

import Variable from './variable/Variable.svelte'
import {Spinner} from 'sveltestrap';
import {onMount} from 'svelte';
import {getDataTypes,getUnits} from '../../services/StructureSuggestionCaller'

$:datatypes=null;
$:units=null;


onMount(async ()=>{

datatypes = await getDataTypes();
units = await getUnits();

console.log("datatypes",datatypes);
console.log("units", units);

})

export let variables = [];

</script>
<div class="suggestion-container">
{#if variables && datatypes && units}
    <!-- else content here -->
    {#each variables as variable, i}
      <!-- content here -->
      <Variable {variable} index={i} on:change={console.log(variable)} {datatypes} {units}/>
      <br>
    {/each}
  
{:else}
  <Spinner color="primary" size="sm" type ="grow" text-center />
{/if}
</div>

<style>
  .suggestion-container{
    padding-top: 1rem;
  }
</style>