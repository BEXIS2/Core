<script lang="ts">

import {goTo}  from '../../../services/BaseCaller'
import {MultiSelect} from '@bexis2/bexis2-core-ui';
import { onMount } from 'svelte';

export let id = 0; // entity id
export let files:any[];

function goToGenerate(file)
{
   // if its possible the file will be used to start structure analyze
    goTo('/dcm/structuresuggestion/?id='+id+'&file='+file);
}

type item ={
   value:string,
   label:string,
   group:string
}

let list:item[];
$:list, setList(files);

// list is a comibnation of options, already existing datastructures and files
function setList(files)
{
  
  list = [];
  list.push({value:"create new",label:"create new", group:"options"})

  if(files!== null)
  {
    files.forEach(i => 
      list.push({value:i.name,label:i.name, group:"files"})
    );
  }
}


onMount(async () => {
  //setList(files);
  console.log("select list",list)
});

// after select a value from the dropdown 
// it will go to the generator or selet a exiting structure
function change(e)
{
  let item = e.detail;

  if(item.type==="options")
  {
    console.log("go to create a datastructure");
  }
  else
  {
    goToGenerate(e.detail.value);
  }
}

</script>

{#if list}

<div class="grid grid-cols-2 py-3">
   <MultiSelect 
   id="SelectDataStructure" 
   title="Select a Datastructure or generate from File"
   itemId="value"
   itemLabel="label"
   bind:source={list} 
   itemGroup="group" 
   on:change={change}
   complexSource={true}
   target
   />
</div>
{/if}

