<script lang="ts">

import {goTo}  from '../../../services/BaseCaller'
import {MultiSelect, Spinner} from '@bexis2/bexis2-core-ui';
import type {ListItem} from '@bexis2/bexis2-core-ui';
import { onMount, createEventDispatcher } from 'svelte';

import {availableStructues, setStructure} from '$services/DataDescriptionCaller'


export let id = 0; // entity id
export let files:any[];

const dispatch = createEventDispatcher();

function goToGenerate(file)
{
   // if its possible the file will be used to start structure analyze
    goTo('/dcm/structuresuggestion/?id='+id+'&file='+file);
}


let list:ListItem[]; 
$:list;
let loading:boolean; 
$:loading;
let structures=[];
$:structures;
// list is a comibnation of options, already existing datastructures and files
function setList(files)
{
  
  list = [];
  list.push({id:0,text:"create new", group:"options"})
  //console.log(structures)
  if(structures!== null && structures != undefined) 
  {
      list = [...list,...structures];
  }
  
  if(files!== null && files !== undefined)
  {
    files.forEach(i => 
      list.push({id:i.name,text:i.name, group:"file"})
    );
  }

  console.log("list", list)
}


onMount(async () => {
  loading = false;
  //setList(files);
  structures = await availableStructues(id);
  setList(files);
});

// after select a value from the dropdown 
// it will go to the generator or selet a exiting structure
async function change(e)
{
  let item = e.detail;
  console.log("select item",item)

  if(item.group==="options")
  {
    console.log("go to create a datastructure");
  }
  else if(item.group==="file")
  {
    loading = true;
    goToGenerate(e.detail.text);
  }
  else if(item.group==="structure")
  {
    console.log("select a structure",id,item.id);
    loading = true;
    await setStructure(id, item.id)
    dispatch("selected")

  }
}

</script>

{#if list && structures}

<div class="grid grid-cols-2 py-3">
   <MultiSelect 
   id="SelectDataStructure" 
   title="Select a Datastructure or generate from File"
   itemId="id"
   itemLabel="text"
   itemGroup="group"
   bind:source={list} 
   on:change={change}
   complexSource={true}
   complexTarget={true}
   isMulti={false}
   />
   {#if loading}
   <span class="p-5">
    <Spinner textCss="text-surface-500"/>
  </span>
   {/if}
</div>


{/if}

