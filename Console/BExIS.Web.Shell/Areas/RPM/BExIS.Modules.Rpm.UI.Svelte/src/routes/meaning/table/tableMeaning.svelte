<script lang="ts">
	import type { MeaningModel } from '$lib/components/meaning/types';

	export let value: MeaningModel[];
</script>

{#each value as meaning}
	{#if meaning != undefined && meaning.name != undefined}
		{meaning.name}
	{/if}
{/each}
