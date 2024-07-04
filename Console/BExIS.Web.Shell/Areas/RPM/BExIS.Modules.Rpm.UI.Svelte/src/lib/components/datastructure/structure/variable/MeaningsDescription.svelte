<script lang="ts">
	import type { meaningListItemType } from '../../types';

	export let list: meaningListItemType[];
	console.log('🚀 ~ file: MeaningsDescription.svelte:5 ~ list:', list);
</script>

<div class="flex-col gap-2">
	{#if list}
		{#each list as meaningItem}
			<span class="text-xs font-semibold">{meaningItem.text}</span>
			<div class="flex">
				{#if meaningItem.links}
					{#each meaningItem.links as l}
						<a
							class="chip variant-ghost-secondary"
							title="{l.releation} / {l.label}"
							href={l.link}
							target="_blank">{l.label}</a
						>
					{/each}
				{/if}
			</div>
		{/each}
	{/if}
</div>
