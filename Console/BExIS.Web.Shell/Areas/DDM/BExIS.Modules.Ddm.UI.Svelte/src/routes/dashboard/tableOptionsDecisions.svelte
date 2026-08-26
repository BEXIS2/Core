<script lang="ts">
	import Fa from 'svelte-fa';
	import { faEye, faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';

	export let row: any;
	export let dispatchFn: any;

	function action(action: string) {
		dispatchFn({ type: { action, id: row.id, requestId: row.requestId, instanceId: row.instanceId } });
	}
</script>

<div class="flex items-center gap-1">
	<button class="btn-icon variant-ghost-surface" title="View" on:click|preventDefault={() => action('view')}>
		<Fa icon={faEye} />
	</button>
	{#if row.status === 0}
		<button class="btn-icon variant-ghost-success" title="Accept" on:click|preventDefault={() => action('accept')}>
			<Fa icon={faCheck} />
		</button>
		<button class="btn-icon variant-ghost-error" title="Reject" on:click|preventDefault={() => action('reject')}>
			<Fa icon={faXmark} />
		</button>
	{/if}
</div>
