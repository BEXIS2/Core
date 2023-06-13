<script lang="ts">

import {goTo}  from '../../../services/BaseCaller'
import Select from 'svelte-select/Select.svelte';
import { onMount } from 'svelte';

export let id = 0; // entity id
export let files:any[];

function goToGenerate(file)
{
   // if its possible the file will be used to start structure analyze
    goTo('/dcm/StructureSuggestion/?id='+id+'&file='+file);
}

type item ={
   value:string,
   label:string,
   group:string
}
let l:item[];

$:list = l;
function setList()
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
  setList();
  console.log("select list",list)
});

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

const groupBy = (item) => item.group;

</script>

{#if list}

<div class="grid grid-cols-2 py-3">
    <Select items={list} {groupBy} on:change={change}/>
</div>
{/if}

