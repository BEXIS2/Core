<script lang="ts">
	export let value: any[];
</script>

{#if value != undefined && value.name != undefined}
	{value.name}
{/if}
