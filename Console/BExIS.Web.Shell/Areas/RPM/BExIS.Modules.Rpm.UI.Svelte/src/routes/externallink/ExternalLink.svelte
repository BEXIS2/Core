<script lang="ts">
	import { TextInput, helpStore } from "@bexis2/bexis2-core-ui";
	import Fa from "svelte-fa";
	import { faXmark,faSave } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher, onMount, afterUpdate } from 'svelte';

	import suite from './externalLink';
	import type { externalLinkType } from "$lib/components/meaning/types";
 import {create, update} from './services'


export let link:externalLinkType;
$:link;

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

		console.log("🚀 ~ file: ExternalLink.svelte:45 ~ onMount ~ types:", types)

		setTimeout(async () => {
			if (link.id > 0) {
				res = suite(link, '');
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
			res = suite(link, e.target.id);
		}, 100);
	}

 function cancel() {
		suite.reset();
		dispatch('cancel');
	}

	async function submit() {

		var s = await (link.id==0)?create(link):update(link);

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
<div id="link-{link.id}-form" class=" space-y-5 card shadow-md p-5">

 <div class="flex gap-5 grow">
  
  <div class="grow">
 <TextInput 
		id="name" 
		bind:value={link.name} 
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
	  <TextInput id="type" bind:value={link.type} on:change placeholder="Type" {help} />
 </div>

</div>
<div>
	<TextInput 
	id="uri" 
	bind:value={link.uri} 
	on:change 
	on:input={onChangeFn}
	placeholder="Uri"
	valid={res.isValid('uri')}
	invalid={res.hasErrors('uri')}
	feedback={res.getErrors('uri')}
	{help}
	/>
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
   title="Save external link, {link.name}"
   id="save"
   disabled={!isValid}
			>
   <Fa icon={faSave} /></button>
 </div>
</div>
</form>
{/if}
