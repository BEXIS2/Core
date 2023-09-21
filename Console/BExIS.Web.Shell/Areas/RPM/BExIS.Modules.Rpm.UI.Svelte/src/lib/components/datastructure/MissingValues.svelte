<script lang="ts">

 import type {missingValueType} from './types'


	import MissingValue from './MissingValue.svelte';
	export let list:missingValueType[] = [];

	import { onMount } from 'svelte';

	onMount(() => {
		if (list.length === 0) {
			add();
		}
	});

	function remove(i) {
		list = [...list.filter((m, index) => i !== index)];

		if (list.length === 0) {
			add();
		}
	}

	function add() {
		console.log('list', list);

		let newMissingValue:missingValueType = {
			id:0,
			displayName: '',
			description: ''
		};

		list = [...list, newMissingValue];
		console.log('list', list);
	}
</script>

<label><b>Missing Values</b></label>

<div class="missing-values-container">
	{#if list}
		{#each list as item, i}
			<!-- content here -->
			<MissingValue
				bind:displayName={item.displayName}
				bind:description={item.description}
				on:add={add}
				on:remove={() => remove(i)}
				last={list.length - 1 === i}
			/>
		{/each}
	{/if}
</div>

<style>
	.missing-values-container {
		padding-bottom: 1em;
	}
</style>
