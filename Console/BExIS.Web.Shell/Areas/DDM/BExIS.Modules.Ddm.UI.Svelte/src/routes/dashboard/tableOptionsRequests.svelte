<script lang="ts">
	import Fa from 'svelte-fa';
	import { faEye, faCircleMinus } from '@fortawesome/free-solid-svg-icons';

	export let row: any;
	export let dispatchFn: any;

	function action(action: string) {
		dispatchFn({ type: { action, id: row.id, instanceId: row.instanceId } });
	}
</script>

<div class="flex items-center gap-1">
	<button class="btn-icon variant-ghost-surface" title="View" on:click|preventDefault={() => action('view')}>
		<Fa icon={faEye} />
	</button>
	{#if row.requestStatus === 'Open'}
		<button class="btn-icon variant-ghost-error" title="Withdraw request" on:click|preventDefault={() => action('withdraw')}>
			<Fa icon={faCircleMinus} />
		</button>
	{/if}
</div>
