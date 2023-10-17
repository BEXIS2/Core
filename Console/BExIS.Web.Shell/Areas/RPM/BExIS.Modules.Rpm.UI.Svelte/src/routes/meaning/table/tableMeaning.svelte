<script lang="ts">
	import type { MeaningModel } from "../types";

	export let value: MeaningModel[];
</script>

{#each value as meaning}
{#if meaning != undefined && meaning.Name != undefined}
	{meaning.Name}
{/if}
{/each}