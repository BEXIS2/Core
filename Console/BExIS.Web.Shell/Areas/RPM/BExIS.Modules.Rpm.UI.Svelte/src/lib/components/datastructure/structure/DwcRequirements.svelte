<script lang="ts">
	import { MultiSelect } from "@bexis2/bexis2-core-ui";
	import { getDWCList } from "../services";
	import { onMount } from "svelte";
 import { get } from "svelte/store";
	import type { dwcExtention, VariableInstanceModel } from "../types";
	import { showDarwinCoreValidationStore } from "../store";
	
export let variables:VariableInstanceModel[] = [];
$: variables, validateDWCfn();

let dwcExtensions: dwcExtention[] = [];
let dwcSelection: dwcExtention | null = null;
let notSet: string[] = [];

const isActive = get(showDarwinCoreValidationStore);

onMount( async () => {

  // dwc	extensions
  dwcExtensions	= await getDWCList();
		console.log("🚀 ~ start ~ dwcExtensions:", dwcExtensions)

}
);


function	validateDWCfn() {

 if(!dwcSelection) return;

 // get all set meanings
 const allMeanings = variables.map(v => v.meanings.map(m => m.text)).flat();
 // get all not set required fields
 const notSetFields = dwcSelection.requiredFields.filter((field) => !allMeanings.includes(field));

  notSet = [...notSetFields];

  if(notSet.length === 0){
   notSet = [];
  }
}


</script>

<div class="flex items-end gap-2">
{#if dwcExtensions && isActive}
<MultiSelect
 id="check_dwc"
 title="validate against darwin core datatset"
 source={dwcExtensions}
 bind:target = {dwcSelection}
 itemId="name"
 itemLabel="name"
 complexSource={true}
 complexTarget={true}
 isMulti={false}
 on:change={() => validateDWCfn()}
 on:clear={() => {
  notSet = [];
  dwcSelection = null;
 }}
/>
 {#if notSet} 
  {#if	notSet.length > 0}
   {#each notSet	as field, i}
    <div><span class="chip variant-filled-warning">{field}</span></div>
   {/each}
  {:else}
   {#if dwcSelection}
    <div><span class="chip variant-filled-success">All required fields are set.</span></div>
   {/if}
  {/if}
 {/if}
{/if}
</div>