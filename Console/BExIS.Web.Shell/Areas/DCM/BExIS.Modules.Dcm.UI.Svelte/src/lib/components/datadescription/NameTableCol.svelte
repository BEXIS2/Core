<script lang="ts">
	export let value;
	export let row;

 console.log(row);

</script>

{value}
{#if row.original.isKeys}
<b class="text-error-500">*</b>
{/if}
