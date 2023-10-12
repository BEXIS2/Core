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

	function add() {
		dispatch('add');
	}

</script>

<div class="flex space-x-3 content-center">

 <TextInput id="link_name" bind:value={name} on:change placeholder="Name"/>

	<TextInput id="link_type" bind:value={type} on:change placeholder="Type"/>

	<TextInput id="link_uri" bind:value={uRI} on:change placeholder="Uri"/>

	<div class="self-center text-xl mt-5 w-12">
		<button title="delete" type="button" on:click={remove}><Fa icon={faTrashAlt} /></button>
		{#if last}
			<button title="add" class="add" type="button" on:click={add}><Fa icon={faPlus} /></button>
		{/if}
	</div>
</div>
