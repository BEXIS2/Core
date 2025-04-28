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

<div class="flex flex-col gap-4 grow min-w-[500px]">
	<p class="text-muted text-sm">
		{$store.length}
		{`dataset${$store.length !== 1 ? 's' : ''}`} found
	</p>

	{#each cards as card (card.id)}
		<Card
			card={{
				...card,
				title: card.title || '',
				description: card.description || '',
				author: card.author || '',
				license: card.license || '',
				entity: card.entity || '',
				doi: card.doi || '',
				date: card.date || '',
				entitytemplate: card.entitytemplate || ''
			}}
		/>
	{/each}

	{#if $store.length > 0}
		<Paginator
			settings={paginationSettings}
			active="!variant-filled-secondary !text-on-secondary-token"
			controlVariant="text-on-primary-token"
			buttonClasses="!rounded-none !px-3 !py-1.5 fill-current bg-primary-500 hover:!bg-primary-600 text-on-primary-token disabled:grayscale disabled:!opacity-30"
			regionControl="btn-group"
			select="!px-3 !py-1.5 select min-w-[150px]"
			showFirstLastButtons
			showPreviousNextButtons
			on:page ={e => paginationSettings.page = e.detail}
		/>
	{/if}
</div>
