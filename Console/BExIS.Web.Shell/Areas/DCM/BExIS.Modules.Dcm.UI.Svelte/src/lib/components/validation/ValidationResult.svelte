<script lang="ts">

import { ErrorType, type Check, type SortedError } from "$models/ValidationModels";
import { onMount } from "svelte";

import Fa from 'svelte-fa'
import { faCheck, faXmark, faBan  } from '@fortawesome/free-solid-svg-icons'
	import Message from "./Message.svelte";



export let file;
export let sortedErrors: SortedError[];
$:sortedErrors;

let workflow:ErrorType[] = [ErrorType.Dataset,ErrorType.File,ErrorType.FileReader, ErrorType.Datastructure, ErrorType.Value, ErrorType.PrimaryKey];
let checks:Check[]= [];
$:checks;
let selected:Check;
$:selected;

let errorCount = 0;

onMount(async ()=>{

let faild:boolean = false;

for (let index = 0; index < workflow.length; index++) {

  const type = workflow[index];
  const name = ErrorType[type];
  const errors = sortedErrors.filter(e=>e.type === type) // get list of sorted errors based on a type e.g. data structure or value
  const style = getStyle(errors.length, faild);
  let c:Check = {name, type, errors, style}
  
  errorCount += errors.length;
  
  checks = [...checks,c];

  if(errors.length>0){ 
   faild=true; 
   selected = c;
  }

}

console.log("checks",checks)

})

function getStyle(count, faild)
{
  if(count>0) return "error";

  if(count==0 && faild)  return "surface";

  if(count==0 && !faild) return "success"

  return "";
}


</script>
{#if sortedErrors}
<div class="variant-ghost-success variant-ghost-error variant-ghost-surface hidden"> </div>

<div class="card p-5 space-y-3 mb-5">
<div class="flex gap-1">
 <h4 class="h4">{file} 
 </h4>
  {#if errorCount==0} 
  <span class="badge-icon variant-filled-surface text-success-500"><Fa icon={faCheck}></Fa></span>
  {/if}

</div>
 <div class="flex  space-x-2">
  <ol class="breadcrumb">
   {#each checks as check, i}
    <li class="crumb">
     <button class="btn variant-ghost-{check.style} p-2 flex justify-center space-x-2" >
      <span>{check.name}</span>
      <span class="pt-1">

        {#if check.style=="error"}
         <Fa icon={faXmark}></Fa>
        {/if}
        {#if check.style=="success"}
         <Fa icon={faCheck}></Fa>
        {/if}
        {#if check.style=="surface"}
         <Fa icon={faBan}></Fa>
        {/if}

      </span>
     </button>
    </li>
    {#if i < checks.length - 1}
     <li class="crumb-separator" aria-hidden>&rsaquo;</li>
    {/if}
   {/each}
  </ol>
</div>

{#if selected}
<div class="card shadow-sm border-error-300 border-solid border">

{#each selected.errors as error}
  <Message title={error.issue} count={error.count} messages={error.errors}/>
{/each}

</div>
{/if}

</div>
{/if}