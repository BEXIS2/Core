<script lang="ts">
	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';
	import { withMinorStore, tagInfoModelStore, originalTagInfoModelStore } from '../stores';
	import type { TagInfoEditModel } from '../types';

	export let row: TagInfoEditModel;
	export let dispatchFn;

	let noTag: boolean = hasNoTag();
	// use auto-subscription so changes to the store re-run reactive statements
	let withMinor: boolean;
	$: withMinor = $withMinorStore;

	function hasNoTag() {
		return row.tagId == 0;
	}

	const eventDispatchFn = (type: string) => {
		return dispatchFn({ type, row });
	};

	const buttons = [
		{
			icon: faSave,
			color: 'variant-filled-primary',
			type: 'SAVE'
		},
		{
			icon: faSave,
			color: 'variant-filled-secondary',
			type: 'MINOR'
		},
		{
			icon: faSave,
			color: 'variant-filled-secondary',
			type: 'MAJOR'
		}
	];

	// Check if there are changes compared to the original rows
	$: hasChanges = (() => {
		const originalRows = $originalTagInfoModelStore;
		const currentRow = $tagInfoModelStore.find((x) => x.versionId == row.versionId);
		const originalRow = originalRows.find((x) => x.versionId == row.versionId);

		if (!currentRow) return false; // No current row
		if (!originalRow) return true; // New row

		// Return true if there are changes, otherwise false
		return (
			currentRow.releaseNote !== originalRow.releaseNote ||
			currentRow.publish !== originalRow.publish ||
			currentRow.show !== originalRow.show
		);
	})();
</script>

<div class="flex gap-2 w-min p-2">
	{row.tagId}
	{#if hasChanges && row.tagId > 0}
		<button
			class="btn btn-sm variant-filled-primary"
			title="Save changes"
			on:click|preventDefault={() => eventDispatchFn('SAVE')}
		>
			<Fa icon={faSave} />
		</button>
	{/if}

	{#if noTag}
		{#if withMinor}
			<button
				class="btn btn-sm variant-filled-primary"
				title="Create new minor version. All untagged versions until this will belong to this minor version."
				on:click|preventDefault={() => eventDispatchFn('MINOR')}>minor</button
			>
		{/if}
		<button
			class="btn btn-sm variant-filled-primary"
			title="Create new major version. All untagged versions until this will belong to this major version."
			on:click|preventDefault={() => eventDispatchFn('MAJOR')}>major</button
		>
	{/if}
</div>
