<script lang="ts">

	import Citation from "./Citation.svelte";

	export let id;
	export let version;
	export let title = '';
	export let labels:{ [key: string]: string; }	= {};

	const labelKeys = Object.keys(labels);

</script>

<div class="flex pb-5">	
	<div class="flex-grow">
		<Citation	{id} {version} />
	</div>
	<div>

		{#if labelKeys	&& labelKeys.length > 0}
				{#each labelKeys as key}
					<span class="badge variant-filled-primary mr-2">{key}</span>
				{/each}
		{/if}
		
	</div>
</div>
