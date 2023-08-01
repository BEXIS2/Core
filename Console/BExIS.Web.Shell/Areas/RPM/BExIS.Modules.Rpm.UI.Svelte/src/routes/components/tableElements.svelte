<script lang="ts">
	export let value: any[];
</script>

<div>
	{#if value != undefined}
		{#if value.length > 1}
			{#each value as elem}
				{#if elem.name != undefined}
					<li>{elem.name}</li>
				{/if}
			{/each}
		{:else if value[0] != undefined && value[0].name != undefined}
			{value[0].name}
		{/if}
	{/if}
</div>
