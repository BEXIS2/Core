<script lang="ts">
	import type { meaningEntryType } from '$lib/components/meaning/types';

	export let value: meaningEntryType[];
</script>

<div>
	{#each value as link}
		{#if link != undefined && link.mappingRelation != undefined}
			<li>{link.mappingRelation.text} | {link.mappedLinks.map(m=>m.text).join(",")}</li>
		{/if}
	{/each}
</div>
