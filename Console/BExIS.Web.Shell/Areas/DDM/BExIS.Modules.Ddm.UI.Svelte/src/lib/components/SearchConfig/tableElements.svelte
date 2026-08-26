<script lang="ts">
	export let value: any[];
	console.log('TableElements value:', value);
</script>

<div>
	{#if value != undefined}
		{#each value as item }
			<li>{item?.name ?? item?.text ?? item}</li>
		{/each}
	{:else}
		<span class="text-surface-600 italic">No items</span>
	{/if}
</div>
