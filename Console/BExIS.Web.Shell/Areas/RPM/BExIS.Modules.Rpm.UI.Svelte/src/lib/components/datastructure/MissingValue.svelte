<script lang="ts">
	import Fa from 'svelte-fa';
	import { TextInput, helpStore } from '@bexis2/bexis2-core-ui';
	import { createEventDispatcher } from 'svelte';
	import { faTrashAlt } from '@fortawesome/free-regular-svg-icons';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';

	export let displayName = '';
	export let description = '';
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
	<!-- <Label>Name:</Label>  -->
	<TextInput id="missing-value-name" bind:value={displayName}
	on:change placeholder="Missing Value" help={true}/>

	<!-- <Label>Description:</Label>  -->
	<TextInput id="missing-value-description"  bind:value={description}
	on:change placeholder="Description" help={true}/>

	<div class="self-center text-xl mt-5">
		<button id="delete-missing-value" title="delete" type="button" 
		on:mouseover={() => helpStore.show('delete-missing-value')}
		on:click={remove}><Fa icon={faTrashAlt} /></button>
		{#if last}
			<button id="add-missing-value" title="add" class="add" type="button" 
			on:mouseover={() => helpStore.show('add-missing-value')}
			on:click={add}><Fa icon={faPlus} /></button>
		{/if}
	</div>
</div>
