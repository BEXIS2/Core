<script lang="ts">
	import Fa from 'svelte-fa';
	import { faTable, faFile } from '@fortawesome/free-solid-svg-icons';

	export let value: any;
</script>

{#if value === 'tabular'}
	<Fa icon={faTable} class="text-surface-700 dark:text-surface-200" title="Tabular" />
{:else}
	<Fa icon={faFile} class="text-surface-700 dark:text-surface-200" title="File" />
{/if}
