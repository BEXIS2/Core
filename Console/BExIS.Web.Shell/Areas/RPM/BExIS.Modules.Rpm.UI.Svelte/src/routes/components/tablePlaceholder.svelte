<script lang="ts">
	export let cols: number = 1;
	export let rows: number = 10;
</script>

<div class="placeholder animate-pulse w-full h-10 sp" />
<div class="table-container w-full">
	<table class="table table-compact w-full">
		{#each Array(rows) as _}
			<tr class="w-full">
				{#each Array(cols) as _}
					<td><div class="placeholder animate-pulse h-9 w-30 m-3" /></td>
				{/each}
			</tr>
		{/each}
	</table>
</div>
