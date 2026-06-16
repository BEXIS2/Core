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

	function inputChangeFn(v:string) {

		console.log('🚀 ~ inputChangeFn ~ value:', value);

			setTimeout(() => {
				console.log('🚀 ~ inputChangeFn ~ value after timeout:', value);
				tagInfoModelStore.update((arr) =>
				arr.map((x) => (x.versionId === currentRow.versionId ? { ...x, releaseNote: v } : x))
			);
		}, 0);
	}
		

</script>

<span title="Release Note; Click save to apply changes.">
	<TextArea
		id="releaseNote"
		bind:value={currentRow.releaseNote}
		on:input={() => inputChangeFn(currentRow.releaseNote)}
	></TextArea>
</span>
