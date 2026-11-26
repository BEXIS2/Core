<script lang="ts">
	import Fa from 'svelte-fa';
	import { faDatabase } from '@fortawesome/free-solid-svg-icons';

	export let value: any;
</script>

{#if value}
	<Fa icon={faDatabase} class="text-primary-600" title="Has data" />
{:else}
	<span class="text-surface-400" title="No data">—</span>
{/if}
