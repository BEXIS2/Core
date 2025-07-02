<script lang="ts">
	import {
		faFloppyDisk,
		faRotateLeft,
		faSquarePlus,
		faXmark
	} from '@fortawesome/free-solid-svg-icons';
	import type { CurationEntryClass } from './CurationEntries';
	import { curationStore } from './stores';
	import Fa from 'svelte-fa';
	import { CurationEntryType } from './types';
	import CurationEntryTemplateTool from './CurationEntryTemplateTool.svelte';

	export let entry: CurationEntryClass;

	const cardState = curationStore.getEntryCardState(entry.id);

	let inputData = $cardState.inputData || {
		type: entry.type,
		position: entry.position,
		name: entry.name,
		description: entry.description
	};

	$: cardState.update((cs) => ({ ...cs, inputData }));

	const closeEditMode = () =>
		cardState.update((cs) => ({ ...cs, editEntryMode: false, inputData: undefined }));

	const saveChanges = () => {
		curationStore.updateEntry(entry.id, $cardState.inputData || {});
		closeEditMode();
	};
</script>

<form
	class="my-1 flex flex-wrap gap-x-2 gap-y-1 overflow-hidden text-surface-900"
	on:submit|preventDefault={saveChanges}
>
	<label class="min-w-32 grow basis-2/5">
		<span class="label-text">Type:</span>
		<div class="flex items-stretch">
			<select bind:value={inputData.type} class="input rounded-r-none" required>
				<option value="" disabled>Select type</option>
				<option value={CurationEntryType.None}>None (Hidden)</option>
				<!-- Status should not be created created this way -->
				<!-- <option value={CurationEntryType.StatusEntryItem}>Status</option> -->
				<option value={CurationEntryType.MetadataEntryItem}>Metadata</option>
				<option value={CurationEntryType.PrimaryDataEntryItem}>Primary Data</option>
				<option value={CurationEntryType.DatastructureEntryItem}>Data Structure</option>
			</select>
			<button
				class="rounded-r border-y border-r border-surface-400 px-3 hover:bg-surface-500 active:bg-surface-600"
				on:click|preventDefault={() => (inputData.type = entry.type)}
				title="Undo changes"
			>
				<Fa icon={faRotateLeft} />
			</button>
		</div>
	</label>

	<label class="grow basis-1/6">
		<span class="label-text">Position:</span>
		<div class="flex items-stretch">
			<input
				type="number"
				bind:value={inputData.position}
				class="input rounded-r-none"
				placeholder="Enter position"
			/>
			<button
				class="rounded-r border-y border-r border-surface-400 px-3 hover:bg-surface-500 active:bg-surface-600"
				on:click|preventDefault={() => (inputData.position = entry.position)}
				title="Undo changes"
			>
				<Fa icon={faRotateLeft} />
			</button>
		</div>
	</label>

	<label class="grow basis-full">
		<span class="label-text">Name:</span>
		<div class="flex items-stretch">
			<input
				type="text"
				bind:value={inputData.name}
				class="input rounded-r-none"
				placeholder="Enter name"
				required
			/>
			<button
				class="rounded-r border-y border-r border-surface-400 px-3 hover:bg-surface-500 active:bg-surface-600"
				on:click|preventDefault={() => (inputData.name = entry.name)}
				title="Undo changes"
			>
				<Fa icon={faRotateLeft} />
			</button>
		</div>
	</label>

	<label class="grow basis-full">
		<span class="label-text">Description:</span>
		<div class="flex items-stretch">
			<textarea
				bind:value={inputData.description}
				class="input rounded-r-none"
				placeholder="Enter description"
				required
			></textarea>
			<button
				class="rounded-r border-y border-r border-surface-400 px-3 hover:bg-surface-500 active:bg-surface-600"
				on:click|preventDefault={() => (inputData.description = entry.description)}
				title="Undo changes"
			>
				<Fa icon={faRotateLeft} />
			</button>
		</div>
	</label>

	<div class="flex grow basis-full flex-wrap gap-x-2 gap-y-1">
		<button
			type="button"
			on:click|preventDefault={closeEditMode}
			title="Cancel edit"
			class="grow text-nowrap rounded bg-surface-300 px-2 py-1 hover:bg-surface-500 focus-visible:bg-surface-500 active:bg-surface-600"
		>
			<Fa icon={faXmark} class="mr-1 inline-block" />
			Cancel
		</button>

		<CurationEntryTemplateTool
			type={inputData.type}
			name={inputData.name}
			description={inputData.description}
		/>

		<button
			type="submit"
			title="Save entry"
			class="grow text-nowrap rounded bg-success-500 px-2 py-1 text-surface-100 hover:bg-success-600 focus-visible:bg-success-600 active:bg-success-700"
		>
			{#if entry.isDraft()}
				<Fa icon={faSquarePlus} class="mr-1 inline-block" />
				Create
			{:else}
				<Fa icon={faFloppyDisk} class="mr-1 inline-block" />
				Save
			{/if}
		</button>
	</div>
</form>
