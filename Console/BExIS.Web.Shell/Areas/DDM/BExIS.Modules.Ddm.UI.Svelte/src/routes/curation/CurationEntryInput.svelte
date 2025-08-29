<script lang="ts">
	import {
		faFloppyDisk,
		faMessage,
		faRotateLeft,
		faSquarePlus,
		faXmark
	} from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import Fa from 'svelte-fa';
	import { CurationEntryType } from './types';
	import CurationEntryTemplateTool from './CurationEntryTemplateTool.svelte';
	import { onMount } from 'svelte';
	import { slide } from 'svelte/transition';

	export let entryId: number;

	const entry = curationStore.getEntryReadable(entryId);

	const cardState = curationStore.getEntryCardState(entryId);

	let form: HTMLFormElement;

	onMount(() => {
		form?.scrollIntoView({ behavior: 'smooth', block: 'center' });
	});

	const defaultInputData = {
		type: CurationEntryType.None,
		position: 0,
		name: '',
		description: '',
		comment: ''
	};

	let inputData = $cardState.inputData || {
		type: $entry?.type ?? defaultInputData.type,
		position: $entry?.position ?? defaultInputData.position,
		name: $entry?.name ?? defaultInputData.name,
		description: $entry?.description ?? defaultInputData.description,
		comment: $entry?.notes.at(-1)?.comment ?? defaultInputData.comment
	};

	$: cardState.update((cs) => ({ ...cs, inputData }));

	const closeEditMode = () =>
		cardState.update((cs) => ({ ...cs, editEntryMode: false, inputData: undefined }));

	const saveChanges = () => {
		curationStore.updateEntry(entryId, $cardState.inputData || {});
		closeEditMode();
	};

	let showCommentField = inputData.comment?.trim().length > 0;
</script>

<form
	class="my-1 flex flex-wrap gap-x-2 gap-y-1 overflow-hidden text-surface-900"
	on:submit|preventDefault={saveChanges}
	bind:this={form}
	in:slide={{ duration: 150 }}
	out:slide={{ duration: 150 }}
>
	<h3 class="w-full text-lg font-semibold">
		{#if $entry?.isDraft()}
			Create Curation Entry
			<span class="text-sm text-primary-500">(Draft)</span>
		{:else}
			Edit Curation Entry
			<span class="text-sm text-primary-500">(#{$entry?.id})</span>
		{/if}
	</h3>
	<label class="min-w-32 grow basis-2/5">
		<span class="label-text">Category:</span>
		<div class="flex items-stretch">
			<select bind:value={inputData.type} class="input rounded-r-none" required>
				<option value="" disabled>Select category</option>
				<option value={CurationEntryType.None}>None (Hidden)</option>
				<!-- Status should not be created created this way -->
				<!-- <option value={CurationEntryType.StatusEntryItem}>Status</option> -->
				<option value={CurationEntryType.MetadataEntryItem}>Metadata</option>
				<option value={CurationEntryType.PrimaryDataEntryItem}>Primary Data</option>
				<option value={CurationEntryType.DatastructureEntryItem}>Data Structure</option>
			</select>
			<button
				class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
				disabled={$entry?.isDraft()}
				on:click|preventDefault={() => (inputData.type = $entry?.type ?? CurationEntryType.None)}
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
				min="1"
				required
			/>
			<button
				class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
				disabled={$entry?.isDraft()}
				on:click|preventDefault={() =>
					(inputData.position = $entry?.position ?? defaultInputData.position)}
				title="Undo changes"
			>
				<Fa icon={faRotateLeft} />
			</button>
		</div>
	</label>

	<label class="grow basis-full">
		<span class="label-text">Title:</span>
		<div class="flex items-stretch">
			<input
				type="text"
				bind:value={inputData.name}
				class="input rounded-r-none"
				placeholder="Enter name"
				maxLength={255}
				required
			/>
			<button
				class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
				disabled={$entry?.isDraft()}
				on:click|preventDefault={() => (inputData.name = $entry?.name ?? defaultInputData.name)}
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
				class="btn rounded-l-none border-y border-r border-surface-500 px-3 ring-0"
				disabled={$entry?.isDraft()}
				on:click|preventDefault={() =>
					(inputData.description = $entry?.description ?? defaultInputData.description)}
				title="Undo changes"
			>
				<Fa icon={faRotateLeft} />
			</button>
		</div>
	</label>

	<!-- Comment Field -->
	{#if $entry?.isDraft()}
		<div class="my-2 w-full">
			{#if !showCommentField}
				<button
					class="variant-outline-surface btn w-full px-2 py-1 text-sm text-surface-700"
					on:click={() => (showCommentField = true)}
				>
					<Fa icon={faMessage} class="mr-1 inline-block" />
					Add Comment
				</button>
			{:else}
				<label class="grow basis-full">
					<span class="label-text">Comment:</span>
					<div class="flex items-stretch">
						<textarea
							bind:value={inputData.comment}
							class="input rounded-r-none"
							placeholder="Enter comment"
						></textarea>
					</div>
				</label>
			{/if}
		</div>
	{/if}

	<div class="flex grow basis-full flex-wrap gap-x-2 gap-y-1">
		<button
			type="button"
			on:click|preventDefault={closeEditMode}
			title="Cancel edit"
			class="variant-ghost-surface btn grow text-nowrap px-2 py-1 text-surface-800"
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
			class="variant-filled-success btn grow text-nowrap px-2 py-1"
		>
			{#if $entry?.isDraft()}
				<Fa icon={faSquarePlus} class="mr-1 inline-block" />
				Create
			{:else}
				<Fa icon={faFloppyDisk} class="mr-1 inline-block" />
				Save
			{/if}
		</button>
	</div>
</form>
