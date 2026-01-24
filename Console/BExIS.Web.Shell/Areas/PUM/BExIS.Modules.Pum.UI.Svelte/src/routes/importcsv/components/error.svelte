<script lang="ts">
	export let value: any;
	//console.log(value);
</script>

{#if value.errorType}
	<div class="btn variant-ghost-error" title={value.errorMsg}>{value.value}</div>
{:else}
	{value}
{/if}
