<script lang="ts">
	import type { missingValueType } from './types';

	import MissingValue from './MissingValue.svelte';
	export let list: missingValueType[] = [];
	export let showTitle: boolean = true;
	export let disabled: boolean = false;

	import { onMount } from 'svelte';
	import { helpStore } from '@bexis2/bexis2-core-ui';

	onMount(() => {
		console.log('🚀 ~ file: MissingValues.svelte:6 ~ list:', list);
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

		let newMissingValue: missingValueType = {
			id: 0,
			displayName: '',
			description: ''
		};

		list = [...list, newMissingValue];
		console.log('list', list);
	}
</script>

{#if showTitle}
	<label id="missingvalues" on:mouseover={() => helpStore.show('missingvalues')}
		><b>Missing Values</b></label
	>
{/if}

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
				{disabled}
				index={i}
			/>
		{/each}
	{/if}
</div>

<style>
	.missing-values-container {
		padding-bottom: 1em;
	}
</style>
