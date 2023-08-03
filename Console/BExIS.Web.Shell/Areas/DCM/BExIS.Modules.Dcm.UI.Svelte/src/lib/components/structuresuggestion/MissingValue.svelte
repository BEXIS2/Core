<script lang="ts">
	import Fa from 'svelte-fa';
	import { TextInput } from '@bexis2/bexis2-core-ui';
	import { createEventDispatcher } from 'svelte';
	import { faTrashAlt } from '@fortawesome/free-regular-svg-icons';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';

	export let displayName = '';
	export let description = '';
	export let last = true;

	const dispatch = new createEventDispatcher();

	function remove() {
		dispatch('remove');
	}

	function add() {
		dispatch('add');
	}
</script>

<div class="flex space-x-3 content-center">
	<!-- <Label>Name:</Label>  -->
	<TextInput id="name" bind:value={displayName} on:change />

	<!-- <Label>Description:</Label>  -->
	<TextInput id="description" bind:value={description} on:change />

	<div class="self-center text-xl mt-5">
		<button title="delete" type="button" on:click={remove}><Fa icon={faTrashAlt} /></button>
		{#if last}
			<button title="add" class="add" type="button" on:click={add}><Fa icon={faPlus} /></button>
		{/if}
	</div>
</div>
