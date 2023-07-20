<script lang="ts">
	export let value: any[];
</script>

<div>
	{#if value.length > 1}
		{#each value as elem}
			<li>{elem.name}</li>
		{/each}
	{:else}
		{value[0].name}
	{/if}
</div>
