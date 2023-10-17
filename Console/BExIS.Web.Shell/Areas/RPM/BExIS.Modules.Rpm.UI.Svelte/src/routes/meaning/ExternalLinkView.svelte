<script lang="ts">
	import { TextInput } from "@bexis2/bexis2-core-ui";
	import Fa from "svelte-fa";
	import { faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher} from 'svelte';

	import type { externalLinkType } from "./types";

export let id:number;
export let index:number;
export let link:externalLinkType;
export let last = true;

export let isValid:boolean = false;
// validation

// block if id exist

const dispatch = createEventDispatcher();

function remove() {
		dispatch('remove');
	}


</script>

<div id="{""+index}" class="flex space-x-3 items-center">


 <TextInput 
		id="link_name" 
		bind:value={link.name} 
		on:change 
		placeholder="Name"
		disabled={true}
					/>

	<TextInput id="link_type" bind:value={link.type} on:change placeholder="Type" disabled={true}/>

	<TextInput 
	id="link_uri" 
	bind:value={link.uri} 
	on:change 
	placeholder="Uri"

	disabled={true}
	/>


	<div class="flex text-xl w-12 gap-2">
		<button title="remove" type="button" on:click={remove}><Fa class="text-error-500" icon={faXmark} /></button>
	</div>
</div>
