<script lang="ts">
	import { TextInput, helpStore } from "@bexis2/bexis2-core-ui";
	import Fa from "svelte-fa";
	import { faXmark,faSave } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher, onMount, afterUpdate } from 'svelte';

	import suite from './prefixcategory';
	import type { prefixCategoryType } from "$lib/components/meaning/types";
 import {create, update} from './services'


export let prefixCategory:prefixCategoryType;
$:prefixCategory;

let help: boolean = true;
let loaded = false;

let types:string[]=[];
$:types;
// validation
let res = suite.get();
$: isValid = res.isValid();

// data
import { helpInfoList } from './help'

// block if id exist

const dispatch = createEventDispatcher();


onMount(() => {

	 helpStore.setHelpItemList(helpInfoList);

		// reset & reload validation
		suite.reset();

		// set types =

		setTimeout(async () => {
			if (prefixCategory.id > 0) {
				res = suite(prefixCategory, '');
			} // run validation only if start with an existing
		}, 10);

		//updateAutoCompleteList();

		loaded = true;
	});


	function onChangeFn(e)
	{
	// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			res = suite(prefixCategory, e.target.id);
		}, 100);
	}

 function cancel() {
		suite.reset();
		dispatch('cancel');
	}

	async function submit() {

		var s = await (prefixCategory.id==0)?create(prefixCategory):update(prefixCategory);

		if ((await s).status === 200) {
			dispatch('success');
		} else {
			dispatch('fail');
		}

  suite.reset()
	}


</script>
{#if loaded}
<form on:submit|preventDefault={submit}>
<div id="prefixCategory-{prefixCategory.id}-form" class=" space-y-5 card shadow-md p-5">

 <div class="flex gap-5 grow">
  
  <div class="grow">
 <TextInput 
		id="name" 
		bind:value={prefixCategory.name} 
		on:change 
		on:input={onChangeFn}
		placeholder="Name"
		valid={res.isValid('name')}
		invalid={res.hasErrors('name')}
		feedback={res.getErrors('name')}
		{help}
					/>
  </div>
 <div class="grow">
	  <TextInput id="description" bind:value={prefixCategory.description} on:change placeholder="Description" {help} />
 </div>

</div>

 <div class="py-5 text-right col-span-2">
  <!-- svelte-ignore a11y-mouse-events-have-key-events -->
  <button
   type="button"
   class="btn variant-filled-warning h-9 w-16 shadow-md"
   title="Cancel"
   id="cancel"
   on:click={() => cancel()}><Fa icon={faXmark} /></button
  >
  <!-- svelte-ignore a11y-mouse-events-have-key-events -->
  <button
   type="submit"
   class="btn variant-filled-primary h-9 w-16 shadow-md"
   title="Save prefix category, {prefixCategory.name}"
   id="save"
   disabled={!isValid}
			>
   <Fa icon={faSave} /></button>
 </div>
</div>
</form>
{/if}
