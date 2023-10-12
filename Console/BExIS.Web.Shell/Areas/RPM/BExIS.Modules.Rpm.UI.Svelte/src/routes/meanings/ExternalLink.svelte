<script lang="ts">
	import { TextInput } from "@bexis2/bexis2-core-ui";
	import Fa from "svelte-fa";
	import { faTrashAlt } from '@fortawesome/free-regular-svg-icons';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';

	import { createEventDispatcher } from 'svelte';

export let name:string = "";
export let type:string = "";
export let uRI:string = "";
export let last = true;

const dispatch = createEventDispatcher();

function remove() {
		dispatch('remove');
	}


</script>

<div class="flex space-x-3 items-center">

 <TextInput id="link_name" bind:value={name} on:change placeholder="Name"/>

	<TextInput id="link_type" bind:value={type} on:change placeholder="Type"/>

	<TextInput id="link_uri" bind:value={uRI} on:change placeholder="Uri"/>

	<div class="self-center text-xl w-12">
		<button title="delete" type="button" on:click={remove}><Fa icon={faTrashAlt} /></button>

	</div>
</div>
