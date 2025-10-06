<script lang="ts">
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { tagInfoModelStore } from '../stores';
	import type { TagInfoEditModel } from '../types';

	export let value: boolean;
	export let row: any;

	let currentRow: TagInfoEditModel;

	$: currentRow =
		$tagInfoModelStore.find((x) => x.versionId == row.original.versionId) ??
		({
			versionId: row.original.versionId,
			tagId: 0,
			show: false
		} as TagInfoEditModel);

	// Update the store when the toggle is changed
	function toggleShow(versionId: number, value: boolean) {
		tagInfoModelStore.update((arr) =>
			arr.map((x) => (x.versionId === versionId ? { ...x, show: !!value } : x))
		);
	}
</script>

<div class="flex h-full items-center justify-center">
	<div title="Show the release note of this version; Click save to apply changes.">
		{#if currentRow && currentRow.tagId > 0}
			<SlideToggle
				name={'' + currentRow.versionId}
				class=""
				checked={currentRow.show}
				size="sm"
				on:change={(e) => toggleShow(currentRow.versionId, !!(e.detail ?? e.target?.checked))}
			/>
		{/if}
	</div>
</div>
