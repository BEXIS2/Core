<script lang="ts">
	import Fa from 'svelte-fa';
	import { TextInput, helpStore } from '@bexis2/bexis2-core-ui';
	import { createEventDispatcher } from 'svelte';
	import { faTrashAlt } from '@fortawesome/free-regular-svg-icons';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';

	export let displayName = '';
	export let description = '';
	export let last = true;
	export let index = 0;
	export let disabled: boolean = false;

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
	<div class="grow">
		<TextInput
			id="missing-value-name-{index}"
			bind:value={displayName}
			on:change
			placeholder="Missing Value"
			help={true}
			{disabled}
		/>
	</div>

	<!-- <Label>Description:</Label>  -->
	<div class="grow">
		<TextInput
			id="missing-value-description-{index}"
			bind:value={description}
			on:change
			placeholder="Description"
			help={true}
			{disabled}
		/>
	</div>

	{#if !disabled}
		<div class="self-center text-xl mt-5 w-11">
			<button
				id="delete-missing-value-{index}"
				title="delete"
				type="button"
				on:mouseover={() => helpStore.show('delete-missing-value')}
				on:click={remove}><Fa icon={faTrashAlt} /></button
			>
			{#if last}
				<button
					id="add-missing-value-{index}"
					title="add"
					class="add"
					type="button"
					on:mouseover={() => helpStore.show('add-missing-value')}
					on:click={add}><Fa icon={faPlus} /></button
				>
			{/if}
		</div>
	{/if}
</div>
