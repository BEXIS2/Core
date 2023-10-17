<script lang="ts">
	import { TextInput } from "@bexis2/bexis2-core-ui";
	import Fa from "svelte-fa";
	import { faTrashAlt } from '@fortawesome/free-regular-svg-icons';
	import { faCheck } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher, onMount } from 'svelte';

	import suite from './externalLink';
	import type { externalLinkType } from "./types";


export let index:number;
export let link:externalLinkType;
export let last = true;

export let isValid:boolean = false;
// validation
let res = suite.get();
$: isValid = res.isValid();


const dispatch = createEventDispatcher();


onMount(() => {

		// reset & reload validation
		suite.reset();

		// if link is allready stored, no need for validation
		if(link.id > 0) 
		{
			isValid = true;
		}

	});

function remove() {
		dispatch('remove');
	}

	function onChangeFn(e)
	{
		if(link.id==0) // validate only new links with id == 0 
		{
			res = suite(link, e.target.id);
			setValidationState(res);
		}
	}

function setValidationState(res) {
	
		isValid = res.isValid();
		console.log("🚀 ~ file: ExternalLink.svelte:55 ~ setValidationState ~ isValid:", isValid, index)
		// dispatch this event to the parent to check the save button
		dispatch('link-change');
	}

</script>

<div id="{""+index}" class="flex space-x-3 items-center">

 <TextInput 
		id="link_name" 
		bind:value={link.name} 
		on:change 
		on:input={onChangeFn}
		placeholder="Name"
		valid={res.isValid('link_name')}
		invalid={res.hasErrors('link_name')}
		feedback={res.getErrors('link_name')}
					/>

	<TextInput id="link_type" bind:value={link.type} on:change placeholder="Type"/>

	<TextInput 
	id="link_uri" 
	bind:value={link.uri} 
	on:change 
	on:input={onChangeFn}
	placeholder="Uri"
	valid={res.isValid('link_uri')}
	invalid={res.hasErrors('link_uri')}
	feedback={res.getErrors('link_uri')}
	/>

	<div class="flex text-xl w-12 gap-2">
		<button title="delete" type="button" on:click={remove}><Fa icon={faTrashAlt} /></button>
		{#if isValid}
			<Fa class="text-success-500" icon="{faCheck}"/>
		{/if}
	</div>
</div>
