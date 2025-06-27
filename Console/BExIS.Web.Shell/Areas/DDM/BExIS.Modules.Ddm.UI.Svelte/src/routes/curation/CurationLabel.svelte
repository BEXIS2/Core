<script lang="ts">
	import { faAngleDown, faPlus, faTrash, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import type { CurationEntryClass, CurationNoteClass } from './CurationEntries';
	import { CurationEntryType, CurationUserType } from './types';
	import { curationStore } from './stores';
	import { writable } from 'svelte/store';

	export let curationStatusEntry: CurationEntryClass;
	export let labelNote: CurationNoteClass | undefined = undefined;
	export let labelSelectContent: { name: string; color: string }[] | undefined = undefined;

	if (!curationStatusEntry || curationStatusEntry.type !== CurationEntryType.StatusEntryItem) {
		throw new Error('Invalid CurationStatusEntry provided');
	}

	const statusLabels = [
		{ name: 'check', bgColor: '#D55E00', fontColor: 'white' },
		{ name: 'back-to-author', bgColor: '#56B4E9', fontColor: 'white' },
		{ name: 'changes', bgColor: '#CC79A7', fontColor: 'white' },
		{ name: 'finished', bgColor: '#004D40', fontColor: 'white' }
	];

	function getStatus(userlsDone: boolean, isApproved: boolean) {
		if (userlsDone && isApproved) return 3;
		if (userlsDone && !isApproved) return 2;
		if (!userlsDone && isApproved) return 1;
		return 0;
	}

	function setStatus(statusIndex: number) {
		if (statusIndex < 0 || statusIndex > 3) return;

		if (statusIndex === 0) {
			curationStore.setStatusBoolean(curationStatusEntry.id, false, false, false);
		} else if (statusIndex === 1) {
			curationStore.setStatusBoolean(curationStatusEntry.id, false, true, false);
		} else if (statusIndex === 2) {
			curationStore.setStatusBoolean(curationStatusEntry.id, true, false, false);
		} else {
			curationStore.setStatusBoolean(curationStatusEntry.id, true, true, false);
		}
	}

	let statusIndex = getStatus(curationStatusEntry.userlsDone, curationStatusEntry.isApproved);

	const statusChange = () => {
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
		noteTextColor = noteLabelColor ? getContrastColor(noteLabelColor) : '#ffffff';
	}

	const deleteLabel = () => {
		if (!labelNote) return;
		curationStore.deleteNote(curationStatusEntry.id, labelNote?.id);
	};

	const currentLabel = writable<{ name: string; color: string } | undefined>(undefined);

	currentLabel.subscribe((label) => {
		if (!label) return;
		curationStore.addNote(
			curationStatusEntry.id,
			CurationUserType.User,
			`${label.name} ${label.color}`
		);
		currentLabel.set(undefined);
	});

	// -------------------- Copilot with GPT-4.1 --------------------
	function getContrastColor(hex: string) {
		// Remove hash if present
		hex = hex.replace('#', '');
		// Expand shorthand form (e.g. "03F") to full form ("0033FF")
		if (hex.length === 3) {
			hex = hex
				.split('')
				.map((x) => x + x)
				.join('');
		}
		const r = parseInt(hex.substring(0, 2), 16);
		const g = parseInt(hex.substring(2, 4), 16);
		const b = parseInt(hex.substring(4, 6), 16);
		// Calculate luminance
		const luminance = 0.299 * r + 0.587 * g + 0.114 * b;
		return luminance > 186 ? '#000000' : '#ffffff';
	}
	// --------------------------------------------------------------
</script>

{#if !labelNote && !labelSelectContent}
	<div>
		<div
			class="relative rounded-full bg-primary-500"
			style="background-color: {statusLabels[statusIndex].bgColor}; color: {statusLabels[
				statusIndex
			].fontColor};"
		>
			<select
				class="size-full cursor-pointer rounded-full border-none bg-transparent bg-none py-0.5 text-center font-semibold focus-visible:ring-2 focus-visible:ring-black"
				title="Change Curation Status"
				bind:value={statusIndex}
				on:change={statusChange}
			>
				{#each statusLabels as sl, index}
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
			class="pointer-events-none absolute left-0 top-0 size-full bg-black bg-opacity-50 text-white opacity-0 transition-all"
		>
			<Fa icon={faTrash} class="inline-block px-3 py-0.5" />
		</div>
	</button>
{:else if labelSelectContent}
	<div>
		<div class="relative rounded-full bg-surface-300">
			<select
				class="size-full cursor-pointer rounded-full border-none bg-transparent bg-none py-0.5 text-center focus-visible:ring-2 focus-visible:ring-black"
				title="Add custom label"
				bind:value={$currentLabel}
			>
				<option value={undefined}>Add label</option>
				{#each labelSelectContent as label}
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
