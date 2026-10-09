<script lang="ts">
	export let value: any;
</script>

{#if value === 'yes'}
	<span class="badge variant-soft-success text-xs mt-3">valid</span>
{:else}
	<span class="badge variant-filled-error text-xs mt-3">invalid</span>
{/if}
