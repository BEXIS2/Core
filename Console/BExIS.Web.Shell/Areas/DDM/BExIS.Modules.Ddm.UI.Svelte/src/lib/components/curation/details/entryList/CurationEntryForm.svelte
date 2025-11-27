<script lang="ts">
	import { faCopy, faFloppyDisk, faSquarePlus, faXmark } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from '$lib/stores/CurationStore';
	import Fa from 'svelte-fa';
	import {
		DefaultCurationEntryCreationModel,
		type CurationEntryCreationModel
	} from '$lib/models/CurationEntry';
	import { onMount } from 'svelte';
	import { slide } from 'svelte/transition';
	import { get } from 'svelte/store';
	import { entryTemplatePopupState } from '$lib/models/CurationEntryTemplate';
	import CurationEntryInputs from '$lib/components/curation/details/utils/CurationEntryInputs.svelte';

	export let entryId: number;

	const entry = curationStore.getEntryReadable(entryId);

	const cardState = curationStore.getEntryCardState(entryId);

	let form: HTMLFormElement;

	onMount(() => {
		form?.scrollIntoView({ behavior: 'smooth', block: 'center' });
	});

	let inputData: CurationEntryCreationModel = $cardState.inputData || {
		...DefaultCurationEntryCreationModel,
		type: get(entry)?.type ?? DefaultCurationEntryCreationModel.type,
		position: get(entry)?.position ?? DefaultCurationEntryCreationModel.position,
		name: get(entry)?.name ?? DefaultCurationEntryCreationModel.name,
		description: get(entry)?.description ?? DefaultCurationEntryCreationModel.description,
		comment: get(entry)?.notes.at(-1)?.comment ?? DefaultCurationEntryCreationModel.comment,
		status: get(entry)?.status ?? DefaultCurationEntryCreationModel.status
	};

	$: cardState.update((cs) => ({ ...cs, inputData }));

	const closeEditMode = () =>
		cardState.update((cs) => ({ ...cs, editEntryMode: false, inputData: undefined }));

	const saveChanges = () => {
		curationStore.updateEntry(entryId, $cardState.inputData || {});
		closeEditMode();
	};

	$: resetValues = $entry?.isDraft()
		? undefined
		: {
				type: () => (inputData.type = $entry?.type ?? DefaultCurationEntryCreationModel.type),
				position: () =>
					(inputData.position = $entry?.position ?? DefaultCurationEntryCreationModel.position),
				name: () => (inputData.name = $entry?.name ?? DefaultCurationEntryCreationModel.name),
				description: () =>
					(inputData.description =
						$entry?.description ?? DefaultCurationEntryCreationModel.description)
			};
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

	<CurationEntryInputs bind:inputData isDraft={$entry?.isDraft()} {resetValues} />

	<div class="flex grow basis-full flex-wrap gap-2">
		<button
			type="button"
			on:click|preventDefault={closeEditMode}
			title="Cancel edit"
			class="variant-ghost-surface btn grow text-nowrap px-2 py-1 text-surface-800"
		>
			<Fa icon={faXmark} class="mr-1 inline-block" />
			Cancel
		</button>

		<!-- Create Template Button -->
		<button
			type="button"
			title="Create template"
			class="variant-ghost-surface btn text-nowrap px-2 py-1 text-surface-800"
			on:click={() => entryTemplatePopupState.set({ show: true, template: { ...inputData } })}
		>
			<Fa icon={faCopy} class="mr-1 inline-block" />
			Create Template
		</button>

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
