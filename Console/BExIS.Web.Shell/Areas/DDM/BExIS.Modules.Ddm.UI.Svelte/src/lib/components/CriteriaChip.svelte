<script lang="ts">
	import { createEventDispatcher } from 'svelte';

	export let removable = true;

	const dispatch = createEventDispatcher();
</script>

<button on:click class="chip variant-filled flex gap-2 items-center justify-between">
	<slot />
	{#if removable}
		<button on:click={() => dispatch('remove')}>&times;</button>
	{/if}
</button>
