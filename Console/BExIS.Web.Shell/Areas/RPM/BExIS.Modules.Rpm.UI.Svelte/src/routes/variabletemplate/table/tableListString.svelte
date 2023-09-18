<script lang="ts">
	export let value: any[] | null | undefined;
</script>

<div>
	<!-- {#if value != undefined && value != null}
		{#if value.length > 1}
			{#each value as elem}
				{#if elem != undefined}
					<li>{elem}</li>
				{/if}
			{/each}
		{:else if value[0] != undefined && value[0] != undefined}
			{value[0]}
		{/if}
	{/if} -->
</div>
