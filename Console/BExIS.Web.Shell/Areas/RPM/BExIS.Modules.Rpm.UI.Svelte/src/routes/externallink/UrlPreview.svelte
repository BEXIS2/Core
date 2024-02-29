<script lang="ts">
	import type { externalLinkType } from '$lib/components/meaning/types';

	export let link: externalLinkType;
	console.log('🚀 ~ file: UrlPreview.svelte:6 ~ link:', link);
</script>

<div class="card p-5 flex-col gap-2">
	{#if link.uri != ''}
		{#if link.prefix?.id}
			<div><b class="text-sm">{link.prefix.text}:{link.uri}</b></div>
			<div>
				<a class="a" target="_blank" href="{link.prefix.url}{link.uri}"
					>{link.prefix.url}{link.uri}</a
				>
			</div>
		{:else}
			<a class="a" target="_blank">{link.uri}</a>
		{/if}
	{/if}
</div>
