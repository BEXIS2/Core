<script>
	import ListItem from './ListItem.svelte';

	export let items;

	import { createEventDispatcher } from 'svelte';

	const dispatch = createEventDispatcher();
</script>

<div class="flex-col gap-1 pr-1 space-y-1">
	{#each items as item, index}
		<ListItem {...item} on:click={() => dispatch('select', index)} />
	{/each}
</div>
