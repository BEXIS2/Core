<script lang="ts">
	import { writable, type Writable } from 'svelte/store';
	import { Paginator } from '@skeletonlabs/skeleton';
	import type { PaginationSettings } from '@skeletonlabs/skeleton';

	import Card from './Card.svelte';

	export let store: Writable<any[]> = writable([]);

	let paginationSettings = {
		page: 0,
		limit: 10,
		size: $store.length,
		amounts: [5, 10, 20, 50, 100]
	} satisfies PaginationSettings;

	$: cards = $store.slice(
		paginationSettings.page * paginationSettings.limit,
		paginationSettings.page * paginationSettings.limit + paginationSettings.limit
	);

	$: paginationSettings.size = $store.length;
</script>

<div class="flex flex-col gap-4 grow min-w-[500px] max-w-[800px]">
	<p class="text-muted text-sm">
		{$store.length}
		{`dataset${$store.length !== 1 ? 's' : ''}`} found
	</p>

	{#each cards as card (card.id)}
		<Card
			card={{
				...card
			}}
		/>
	{/each}

	{#if $store.length > 0}
		<Paginator
			bind:settings={paginationSettings}
			showFirstLastButtons={false}
			showPreviousNextButtons={true}
			controlVariant="variant-filled-primary"
		/>
	{/if}
</div>
