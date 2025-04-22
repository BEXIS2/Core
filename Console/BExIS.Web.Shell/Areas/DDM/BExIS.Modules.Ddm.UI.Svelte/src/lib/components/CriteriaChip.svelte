<script lang="ts">
	import { createEventDispatcher } from 'svelte';

	export let title = '';
	export let removable = true;

	const dispatch = createEventDispatcher();
</script>

<button {title} on:click class="chip variant-filled flex gap-2 items-center justify-between">
	<slot />
	{#if removable}
		<button {title} on:click={() => dispatch('remove')}>&times;</button>
	{/if}
</button>
