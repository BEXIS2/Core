<script lang="ts">
	import Fa from 'svelte-fa';
	import { faKey } from '@fortawesome/free-solid-svg-icons';

	export let value;
	export let row;

	// console.log(row);
</script>

<div class="flex gap-2">
	{value}
	{#if row.original.isKeys}
		<span title="Is part of the primary key"><Fa icon={faKey} /></span>
	{/if}
	
</div>
