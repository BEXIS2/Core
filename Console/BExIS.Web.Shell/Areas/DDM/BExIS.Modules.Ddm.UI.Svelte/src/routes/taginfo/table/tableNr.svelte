<script lang="ts">
	export let value: number;
	export let row: any;
</script>

<div>
	<span title="Release Tag">
		{#if value > 0}
			{value.toFixed(1)}
		{/if}
	</span>
</div>
