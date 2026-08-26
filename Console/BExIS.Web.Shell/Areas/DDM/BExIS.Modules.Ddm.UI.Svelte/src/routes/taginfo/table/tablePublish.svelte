<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { tagInfoModelStore } from '../stores';
	import type { TagInfoEditModel } from '../types';

	export let value: boolean;
	export let row: any;
	let currentRow: TagInfoEditModel | undefined;

	$: currentRow = $tagInfoModelStore.find((x) => x.versionId == row.original.versionId);
	// next row is the row with the next version number, if it exists
	// It is used to determine if the current row is the last version of a tag, in which case the publish toggle should be shown
	$: nextRow = $tagInfoModelStore.find((x) => x.versionNr == row.original.versionNr + 1);

	// Update the store when the toggle is changed
	function togglePublish(versionId: number, value: boolean) {
		tagInfoModelStore.update((arr) =>
			arr.map((x) => (x.versionId === versionId ? { ...x, publish: !!value } : x))
		);
	}
</script>

<div class="flex h-full items-center justify-center">
	<div title="Make this release tag visible; Click save to apply changes.">
		{#if currentRow && currentRow.tagId > 0 && ((nextRow && nextRow.tagId != currentRow.tagId) || !nextRow)}
			<SlideToggle
				id="publish-{currentRow.versionId}"
				name={currentRow.versionId.toString()}
				class=""
				checked={currentRow.publish}
				size="sm"
				on:change={(e) => togglePublish(currentRow.versionId, !!(e.detail ?? e.target?.checked))}
			/>
		{/if}
	</div>
</div>