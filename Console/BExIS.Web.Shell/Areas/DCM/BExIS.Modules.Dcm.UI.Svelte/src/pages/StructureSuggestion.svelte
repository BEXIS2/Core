<script>

import Selection from '../components/structuresuggestion/Selection.svelte'
import Suggestion from '../components/structuresuggestion/Suggestion.svelte'
import StructureAttributes from '../components/structuresuggestion/StructureAttributes.svelte'

import Fa from 'svelte-fa/src/fa.svelte'
import { faSave } from '@fortawesome/free-regular-svg-icons'
import { faArrowLeft } from '@fortawesome/free-solid-svg-icons'

import { onMount,  } from 'svelte'; 
import { fade  } from 'svelte/transition'; 

import {Spinner, Button, FormGroup, Input, Label, Row, Col} from 'sveltestrap';

import { setApiConfig }  from '@bexis2/bexis2-core-ui/src/lib/index'
import { generate, save, load }  from '../services/StructureSuggestionCaller'
import { goTo }  from '../services/BaseCaller'

 // load attributes from div
 let container = document.getElementById('structuresuggestion');
 let id = container.getAttribute("dataset");
 $:version = container.getAttribute("version");
 $:file = container.getAttribute("file");

$:model = null;

let selectionIsActive = true;

let areVariablesValid = false;
let areAttributesValid = false;

onMount(async () => {

   console.log("start structure suggestion");
   setApiConfig("https://localhost:44345","davidschoene","123456");
   model = await load(id,file,0);
   
 })

 async function  update(e)
 {
    console.log("update",e.detail);
    model = e.detail;

    let res = await generate(e.detail);

    if(res != false)
    {
      model = res;
      selectionIsActive = false;
    }
 }

async function onSaveHandler()
{
  const res = await save(model);
  console.log("save",res);

  goTo("/dcm/edit?id="+model.id);
}

</script>

{#if !model}
  <Spinner color="primary" size="sm" type ="grow" text-center />
  
{:else}

   {#if selectionIsActive}
    <div transition:fade>
      <Selection {...model} on:saved={update}/>
    </div>
   {:else}
    {#if model.variables.length>0}
      <div transition:fade>
        <Row>
          <Col>
            <Button on:click={()=>selectionIsActive=true}><Fa icon={faArrowLeft}/></Button>
          </Col>
          <Col>
            <div class="text-end">
              <Button color="primary" on:click={onSaveHandler} disabled={!areVariablesValid ||!areAttributesValid }><Fa icon={faSave}/></Button>
            </div>
          </Col>
        </Row>
        
        <StructureAttributes {model} bind:valid={areAttributesValid}/>
        <Suggestion variables = {model.variables} bind:valid={areVariablesValid}/>
      </div>
    {/if}
   {/if}
{/if}

<style>

</style>