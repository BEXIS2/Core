<script lang="ts">
	import Fa from 'svelte-fa';
	import { faTags } from '@fortawesome/free-solid-svg-icons';

	export let value: any;
</script>

{#if value && value > 0}
	<span class="inline-flex items-center gap-1 text-success-600" title="Released tag: {value}">
		<Fa icon={faTags} class="text-xs" />
		<span class="text-xs font-medium">{value}</span>
	</span>
{:else}
	<span class="text-surface-400" title="No tag">—</span>
{/if}
