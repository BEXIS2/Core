<script lang="ts">
	import { CurationEntryType, type CurationEntryClass } from '$lib/models/CurationEntry';
	import type { CurationNoteClass } from '$lib/models/CurationNote';
	import {
		CurationStatusLabels,
		getBooleanFromCurationStatus,
		getCurationStatusFromBoolean
	} from '$lib/models/CurationStatusEntry';
	import { curationStore } from '$lib/stores/CurationStore';
	import { getContrastColor } from '$lib/utils/ColorUtils';
	import { faAngleDown, faPlus, faTrash } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { writable } from 'svelte/store';

	export let curationStatusEntry: CurationEntryClass;
	export let labelNote: CurationNoteClass | undefined = undefined;
	export let remainingLabels: { name: string; color: string }[] | undefined = undefined;

	const { curation } = curationStore;

	if (!curationStatusEntry || curationStatusEntry.type !== CurationEntryType.StatusEntryItem) {
		throw new Error('Invalid CurationStatusEntry provided');
	}

	function setStatus(statusIndex: number) {
		const cs = getBooleanFromCurationStatus(statusIndex);
		if (!cs) return;
		curationStore.setStatusBoolean(curationStatusEntry.id, cs.userIsDone, cs.isApproved, false);
	}

	let statusIndex = getCurationStatusFromBoolean(
		curationStatusEntry.userIsDone,
		curationStatusEntry.isApproved
	);

	const statusChange = () => {
		if (
			statusIndex == 3 &&
			$curation?.curationEntries.some((entry) => entry.status < 2 && entry.isVisible())
		) {
			// If the status is set to "finished" but there are still "Open" or "Changed" entries,
			// show an error message and prevent the status change.
			alert('Cannot set status to finished while there are still unfinished entries.');
			statusIndex = getCurationStatusFromBoolean(
				curationStatusEntry.userIsDone,
				curationStatusEntry.isApproved
			);
			return;
		}
		setStatus(statusIndex);
	};

	let noteLabelText: string | undefined = undefined;
	let noteLabelColor: string | undefined = undefined;
	let noteTextColor: string | undefined;

	const isCustomLabel =
		!labelNote?.hidden && labelNote?.comment
			? /^\S*\s#[0-9a-fA-F]+$/.test(labelNote?.comment)
			: false;
	if (isCustomLabel) {
		noteLabelText = labelNote?.comment
			.match(/^\S*\s/)
			?.toString()
			.trim();
		noteLabelColor = labelNote?.comment
			.match(/\s#[0-9a-fA-F]+$/)
			?.toString()
			.slice(1, 8);
		noteTextColor = getContrastColor(noteLabelColor);
	}

	const deleteLabel = () => {
		if (!labelNote) return;
		curationStore.deleteNote(curationStatusEntry.id, labelNote?.id);
	};

	const currentLabel = writable<{ name: string; color: string } | undefined>(undefined);

	currentLabel.subscribe((label) => {
		if (!label) return;
		curationStore.addNote(curationStatusEntry.id, `${label.name} ${label.color}`);
		currentLabel.set(undefined);
	});
</script>

{#if !labelNote && !remainingLabels}
	<div>
		<div
			class="relative rounded-full bg-primary-500"
			style="background-color: {CurationStatusLabels[statusIndex]
				.bgColor}; color: {CurationStatusLabels[statusIndex].fontColor};"
		>
			<select
				class="size-full cursor-pointer rounded-full border-none bg-transparent bg-none py-0.5 text-center font-semibold focus-visible:ring-2 focus-visible:ring-black"
				title="Change Curation Status"
				bind:value={statusIndex}
				on:change={statusChange}
			>
				{#each CurationStatusLabels as sl, index}
					<option class="bg-white text-surface-900" value={index}>{sl.name}</option>
				{/each}
			</select>
			<div
				class="pointer-events-none absolute right-0 top-0 h-full border-l px-2 py-0.5 opacity-90"
			>
				<Fa icon={faAngleDown} class="inline" />
			</div>
		</div>
	</div>
{:else if isCustomLabel}
	<button
		class="label-button relative overflow-hidden text-nowrap rounded-full bg-surface-300 px-4 py-0.5"
		style="background-color: {noteLabelColor}"
		title="Remove '{noteLabelText}' label"
		on:click={deleteLabel}
	>
		<span style="color: {noteTextColor}">{noteLabelText}</span>
		<div
			class="pointer-events-none absolute left-0 top-0 size-full bg-black bg-opacity-50 text-white opacity-0 transition-opacity"
		>
			<Fa icon={faTrash} class="inline-block px-3 py-0.5" />
		</div>
	</button>
{:else if remainingLabels && remainingLabels.length > 0}
	<div>
		<div class="relative rounded-full bg-surface-300">
			<select
				class="size-full cursor-pointer rounded-full border-none bg-transparent bg-none py-0.5 text-center focus-visible:ring-2 focus-visible:ring-black"
				title="Add custom label"
				bind:value={$currentLabel}
			>
				<option value={undefined}>Add label</option>
				{#each remainingLabels as label}
					<option class="bg-white text-surface-900" value={label}>{label.name}</option>
				{/each}
			</select>
			<div class="pointer-events-none absolute right-0 top-0 h-full px-3 py-0.5">
				<Fa icon={faPlus} class="inline" />
			</div>
		</div>
	</div>
{/if}

<style lang="postcss">
	.label-button:hover div {
		@apply opacity-100;
	}
</style>
