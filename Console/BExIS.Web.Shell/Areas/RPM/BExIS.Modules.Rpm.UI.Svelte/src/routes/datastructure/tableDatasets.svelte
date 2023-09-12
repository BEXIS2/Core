<script lang="ts">
	export let value: any[];

	console.log("value", value);
	

</script>

<div>
	{#if value != undefined}
		{#if value.length > 0}
		<ul class="list-none">
				{#each value as elem}
					{#if elem != undefined}
						<li><a class="a" href="/ddm/show?id={elem}">{elem}</a></li>
				{/if}
			{/each}
		</ul>
	{/if}
	{/if}
</div>
