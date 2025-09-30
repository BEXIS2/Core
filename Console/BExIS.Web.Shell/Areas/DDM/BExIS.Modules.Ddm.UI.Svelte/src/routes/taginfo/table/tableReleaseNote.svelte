<script lang="ts">
	import { TextArea } from '@bexis2/bexis2-core-ui';
	import { tagInfoModelStore } from '../stores';

	export let value: string;
	export let row: any;
	export let dispatchFn;

	let currentRow = row.original;

	// tagInfoModelStore.update((arr) =>
	// 	arr.map((x) => (x.versionId === currentRow.versionId ? { ...x, releaseNote: value } : x))
	// );

	// update the tagModelStore when the value changes
	$: if (currentRow.releaseNote !== value) {
		tagInfoModelStore.update((arr) =>
			arr.map((x) => (x.versionId === currentRow.versionId ? { ...x, releaseNote: value } : x))
		);
	}
</script>

<span title="Release Note; Click save to apply">
	<TextArea
		bind:value={currentRow.releaseNote}
		on:input={() =>
			tagInfoModelStore.update((arr) =>
				arr.map((x) =>
					x.versionId === currentRow.versionId ? { ...x, releaseNote: currentRow.releaseNote } : x
				)
			)}
	></TextArea>
</span>
