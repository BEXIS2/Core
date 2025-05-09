<script lang="ts">
	import { faFloppyDisk, faSquarePlus } from '@fortawesome/free-solid-svg-icons';
	import type { CurationEntryClass } from './CurationEntries';
	import { curationStore } from './stores';
	import Fa from 'svelte-fa';
	import { CurationEntryType } from './types';
	import { writable } from 'svelte/store';

	export let entry: CurationEntryClass;
	export let editEntryMode = writable(false);

	let topic = entry.topic;
	let type = entry.type;
	let name = entry.name;
	let description = entry.description;
	let solution = entry.solution;
	let position = entry.position;
	let source = entry.source;

	const saveChanges = () => {
		curationStore.updateEntry(entry.id, topic, type, name, description, solution, source);
		editEntryMode.set(false);
	};
</script>

<form
	class="ml-2 flex flex-wrap gap-x-2 gap-y-1 overflow-hidden"
	on:submit|preventDefault={saveChanges}
>
	<!-- <label class="grow basis-2/5">
		<span class="label-text">Topic:</span>
		<input type="text" bind:value={topic} class="input" placeholder="Enter topic" required />
	</label> -->

	<label class="grow basis-2/5">
		<span class="label-text">Type:</span>
		<select bind:value={type} class="input" required>
			<option value="" disabled>Select type</option>
			<option value={CurationEntryType.None}>None (Hidden)</option>
			<option value={CurationEntryType.StatusEntryItem}>Status</option>
			<option value={CurationEntryType.MetadataEntryItem}>Metadata</option>
			<option value={CurationEntryType.PrimaryDataEntryItem}>Primary Data</option>
			<option value={CurationEntryType.DatastructureEntryItem}>Data Structure</option>
		</select>
	</label>

	<label class="grow basis-1/6">
		<span class="label-text">Position:</span>
		<input
			type="number"
			bind:value={position}
			class="input"
			placeholder="Enter position"
			disabled
		/>
	</label>

	<label class="grow basis-full">
		<span class="label-text">Name:</span>
		<input type="text" bind:value={name} class="input" placeholder="Enter name" required />
	</label>

	<label class="grow basis-full">
		<span class="label-text">Description:</span>
		<textarea bind:value={description} class="input" placeholder="Enter description" required
		></textarea>
	</label>

	<!-- <label class="grow basis-2/5">
		<span class="label-text">Solution:</span>
		<textarea bind:value={solution} class="input" placeholder="Enter description" rows="1"
		></textarea>
	</label> -->

	<button
		type="submit"
		name="save-entry"
		title="Save entry"
		class="h-8 basis-full rounded bg-success-500 px-2 py-1 text-surface-100 hover:bg-success-600 focus-visible:bg-success-600 active:bg-success-700"
	>
		{#if entry.isDraft()}
			<Fa icon={faSquarePlus} class="mr-1 inline-block" />
			Create
		{:else}
			<Fa icon={faFloppyDisk} class="mr-1 inline-block" />
			Save
		{/if}
	</button>
</form>
