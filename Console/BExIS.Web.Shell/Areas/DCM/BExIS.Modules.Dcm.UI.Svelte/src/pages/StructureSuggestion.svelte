<script>

import Selection from '../components/structuresuggestion/Selection.svelte'
import Suggestion from '../components/structuresuggestion/Suggestion.svelte'

import { onMount,  } from 'svelte'; 
import { fade  } from 'svelte/transition'; 

import {Spinner, Button} from 'sveltestrap';

import { setApiConfig }  from '@bexis2/svelte-bexis2-core-ui'
import { getStructureSuggestion }  from '../services/StructureSuggestionCaller'

 // load attributes from div
 let container = document.getElementById('structuresuggestion');
 let id = container.getAttribute("dataset");
 $:version = container.getAttribute("version");
 $:file = container.getAttribute("file");

$:model = null;

let selectionIsActive = true;


onMount(async () => {

   console.log("start structure suggestion");
   setApiConfig("https://localhost:44345","davidschoene","123456");
   
    // load model froms server
    model = await getStructureSuggestion(id,file,version);
    console.log("model", model);
 })

 function update(e)
 {
    console.log("update",e.detail);
    model = e.detail;

    selectionIsActive = false;
 }

</script>

{#if !model}
  <Spinner color="primary" size="sm" type ="grow" text-center />
{:else}

   {#if selectionIsActive}
    <div transition:fade>
      <Selection {model} on:generated={update}/>
    </div>
   {:else}
    {#if model.variables.length>0}
      <div transition:fade>
        <Button on:click={()=>selectionIsActive=true}>back</Button>
        <Suggestion variables = {model.variables}/>
      </div>
    {/if}
   {/if}
{/if}