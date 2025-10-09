<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { tagInfoModelStore } from '../stores';
	import type { TagInfoEditModel } from '../types';

	export let value: boolean;
	export let row: any;
	let currentRow: TagInfoEditModel;

	$: currentRow = $tagInfoModelStore.find((x) => x.versionId == row.original.versionId);

	// Update the store when the toggle is changed
	function togglePublish(versionId: number, value: boolean) {
		tagInfoModelStore.update((arr) =>
			arr.map((x) => (x.versionId === versionId ? { ...x, publish: !!value } : x))
		);
	}
</script>

<div class="flex h-full items-center justify-center">
	<div title="Make this release tag visible; Click save to apply changes.">
		{#if currentRow.tagId > 0}
			<SlideToggle
				name={currentRow.versionId}
				class=""
				checked={currentRow.publish}
				size="sm"
				on:change={(e) => togglePublish(currentRow.versionId, !!(e.detail ?? e.target?.checked))}
			/>
		{/if}
	</div>
</div>
