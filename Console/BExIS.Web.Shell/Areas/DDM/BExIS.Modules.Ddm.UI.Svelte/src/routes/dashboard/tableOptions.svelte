<script lang="ts">
	import Fa from 'svelte-fa';
	import { faEye, faPenToSquare, faCopy, faTags } from '@fortawesome/free-solid-svg-icons';
	import { useTagsStore } from './stores';

	export let row: any;
	export let dispatchFn: any;

	$: useTags = $useTagsStore;

	function action(action: string) {
		dispatchFn({ type: { action, id: row.id, instanceId: row.instanceId ?? row.id } });
	}
</script>

<div class="flex items-center gap-1">
	<button class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="View" on:click|preventDefault={() => action('view')}>
		<Fa icon={faEye} />
	</button>
	{#if row.isOwn}
		<button class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="Edit" on:click|preventDefault={() => action('edit')}>
			<Fa icon={faPenToSquare} />
		</button>
		<button class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="Copy" on:click|preventDefault={() => action('copy')}>
			<Fa icon={faCopy} />
		</button>
		{#if useTags}
			<button class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="Manage Tags" on:click|preventDefault={() => action('tags')}>
				<Fa icon={faTags} />
			</button>
		{/if}
	{/if}
</div>
