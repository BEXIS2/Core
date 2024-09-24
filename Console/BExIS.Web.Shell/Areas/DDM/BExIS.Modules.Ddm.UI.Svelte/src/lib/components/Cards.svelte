<script lang="ts">
	import { writable, type Writable } from 'svelte/store';

	import Card from './Card.svelte';

	export let store: Writable<any[]> = writable([]);
</script>

<div class="flex flex-col gap-4">
	<p class="text-muted text-sm">
		{$store.length}
		{`dataset${$store.length !== 1 ? 's' : ''}`} found
	</p>

	{#each $store as card}
		<Card
			card={{
				...card,
				license: ''
			}}
		/>
	{/each}
</div>
