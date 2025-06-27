<script lang="ts">
	import Fa from 'svelte-fa';
	import {
		faEyeSlash,
		faFileLines,
		faMessage,
		faPen,
		faTrash,
		faXmark
	} from '@fortawesome/free-solid-svg-icons';
	import { derived, writable } from 'svelte/store';
	import { curationStore } from './stores';
	import RelativeDate from '$lib/components/RelativeDate.svelte';
	import CurationNotes from './CurationNotes.svelte';
	import { CurationEntryStatus, CurationEntryStatusDetails, CurationUserType } from './types';
	import type { CurationEntryClass } from './CurationEntries';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';
	import CurationEntryInput from './CurationEntryInput.svelte';
	import CurationNote from './CurationNote.svelte';

	export let entry: CurationEntryClass;
	export let combined: boolean;
	export let isExpanded = writable(false);
	export let editEntryMode = writable(entry.id <= 0); // true if entry is a draft
	export let tag: string | null = 'div'; // if set, use this tag instead of the default <div>

	const { curation, editMode, statusColorPalette } = curationStore;

	let position = entry.position;
	let positiontimer: NodeJS.Timeout | null = null;

	const isUploading = derived(curationStore.uploadingEntries, (uploadingEntries) =>
		uploadingEntries.some((id) => id === entry.id)
	);

	const entryReadable = curationStore.getEntryReadable(entry.id);

	entryReadable.subscribe((entry) => {
		if (entry) {
			position = entry.position;
		}
	});

	const setStatus = (status: CurationEntryStatus) => {
		if (entry.status !== status) {
			curationStore.setStatus(entry.id, status);
		}
	};

	const positionUpdateDebounce = () => {
		if (positiontimer) {
			clearTimeout(positiontimer);
		}
		positiontimer = setTimeout(() => {
			curationStore.updateEntryPosition(entry.id, position);
		}, 1000);
	};

	const positionUpdateImmediate = () => {
		if (positiontimer) {
			clearTimeout(positiontimer);
		}
		curationStore.updateEntryPosition(entry.id, position);
	};

	const deleteEntry = () => {
		if (
			confirm(
				`Are you sure you want to delete the entry "${entry.name}" at position ${entry.position}?`
			)
		) {
			curationStore.deleteEntry(entry.id);
		}
	};

	const toggleExpand = () => {
		isExpanded.update((v) => !v);
	};

	$: showCollapsedNotes = !$isExpanded && entry.visibleNotes.length > 0;
</script>

<svelte:element
	this={tag}
	class="relative flex flex-col gap-y-1 overflow-hidden rounded py-0.5 transition-all"
	class:px-2={combined}
	class:border={combined}
	class:border-surface-400={combined}
	class:text-primary-500={entry.isDraft()}
>
	{#if $editMode && $editEntryMode}
		<CurationEntryInput {entry} {editEntryMode} />
	{:else}
		<div class="items-top flex flex-row gap-x-2">
			<div class="grow">
				<!-- Title -->
				{#if combined}
					<h3
						class="font-semibold"
						class:text-surface-500={entry.isHidden()}
						style="margin-bottom: -0.25em;"
					>
						{entry.name}
					</h3>
				{/if}

				<!-- Description -->
				<p class="mb-0.5 text-sm" class:text-surface-500={entry.isHidden()}>
					{entry.description}
				</p>
			</div>
			<RelativeDate
				class="hidden text-nowrap border-surface-300 pr-1 text-xs text-surface-600 sm:block"
				date={entry.lastChangedDate}
				label="Last updated"
				showIcon={true}
			/>
		</div>

		<!-- Notes -->
		<div
			class="mb-1 h-0 overflow-hidden rounded-t rounded-bl border-surface-300 transition-all"
			class:h-52={$isExpanded}
			class:h-12={showCollapsedNotes}
			class:md:h-7={showCollapsedNotes}
			class:bg-surface-300={$isExpanded}
			class:rounded-br={!$isExpanded}
		>
			{#if $isExpanded}
				<CurationNotes {entry} />
			{:else if entry.visibleNotes.length > 0}
				<button
					class="h-full w-full overflow-hidden rounded border px-1 py-0.5 text-left text-sm text-surface-700"
					title="Open Chat"
					on:click={toggleExpand}
				>
					<CurationNote
						note={entry.visibleNotes[entry.visibleNotes.length - 1]}
						entryId={entry.id}
						shortForm={true}
					/>
				</button>
			{/if}
		</div>

		<!-- Status and Actions -->
		<div class="mb-1 flex flex-wrap items-center justify-stretch gap-1 overflow-x-hidden text-sm">
			<!-- Status change -->
			<div
				class="status-button-container flex w-60 grow items-center justify-stretch gap-x-1 overflow-x-hidden"
				class:hidden={$editMode}
			>
				{#each CurationEntryStatusDetails as statusDetails, index}
					{#if $curation?.currentUserType === CurationUserType.Curator || (index !== CurationEntryStatus.Ok && index !== CurationEntryStatus.Closed) || index === entry.status}
						<button
							class="status-change-button grow overflow-x-hidden text-ellipsis text-nowrap rounded border px-1 py-0.5"
							class:active={index === entry.status}
							class:opacity-10={entry.isDraft()}
							class:cursor-not-allowed={entry.isDraft()}
							disabled={index === entry.status || $editMode || entry.isDraft()}
							style="--status-color: {$statusColorPalette.colors[index]};"
							title="Change Entry Status to {statusDetails.name}"
							on:click={() => setStatus(index)}
						>
							<Fa icon={statusDetails.icon} class="inline-block" />
							{statusDetails.name}
						</button>
					{/if}
				{/each}
			</div>

			<!-- Chat Button -->
			<button
				class="chat-button w-24 grow overflow-hidden text-ellipsis text-nowrap rounded-lg border border-surface-300 bg-surface-300 px-2 py-0.5 text-surface-900 hover:border-surface-400 hover:bg-surface-400"
				class:active={$isExpanded}
				class:hidden={$editMode}
				class:opacity-10={entry.isDraft()}
				class:cursor-not-allowed={entry.isDraft()}
				disabled={$editMode || entry.isDraft()}
				title="Toggle Chat"
				on:click={toggleExpand}
			>
				<Fa icon={$isExpanded ? faXmark : faMessage} class="inline-block" />
				{#if entry.hasUnreadNotes && !$isExpanded}
					<span class="notification-badge"><span>&nbsp;</span></span>
				{/if}
				Chat
				{#if !$isExpanded && entry.visibleNotes.length > 0}
					({entry.visibleNotes.length})
				{/if}
			</button>

			<!-- EDIT MODE -->

			<!-- Hidden or Draft Badges -->
			{#if entry.isHidden()}
				<div
					class="rounded bg-tertiary-700 px-2 py-0.5 text-surface-50 opacity-70"
					class:hidden={!$editMode}
					title="Entry is hidden"
				>
					<Fa icon={faEyeSlash} class="inline-block" />
					Hidden
				</div>
			{/if}
			{#if entry.isDraft()}
				<div
					class="rounded bg-primary-400 px-2 py-0.5 text-surface-50 opacity-70"
					class:hidden={!$editMode}
					title="Entry is a draft"
				>
					<Fa icon={faFileLines} class="inline-block" />
					Draft
				</div>
			{/if}

			{#if entry.isDraft()}
				<button
					class="rounded bg-error-300 px-2 py-0.5 text-error-700 hover:bg-error-400 hover:text-error-900"
					title="Delete Draft"
					on:click={deleteEntry}
					name="Delete Draft"
				>
					<Fa icon={faTrash} class="inline-block" />
					Delete
				</button>
			{/if}

			<!-- Edit Entry -->
			<button
				class="grow rounded bg-secondary-200 px-2 py-0.5 text-secondary-700 hover:bg-secondary-400 hover:text-secondary-800"
				class:hidden={!$editMode}
				disabled={!$editMode}
				on:click={() => editEntryMode.update((o) => !o)}
				name="Edit Entry"
				title="Edit Entry"
			>
				<Fa icon={faPen} class="inline-block" />
				Edit Entry
			</button>

			<!-- Position Input -->
			<div class="row flex h-full items-center justify-between gap-x-3" class:hidden={!$editMode}>
				<label class="ml-2">
					<span class="text-surface-700">Position</span>
					<input
						type="number"
						bind:value={position}
						class="ml-0.5 w-12 rounded border border-surface-500 px-1 py-0.5 text-xs text-surface-800 focus-visible:border-surface-700 focus-visible:outline-none"
						min="1"
						on:change={positionUpdateDebounce}
						on:blur={positionUpdateImmediate}
						on:keydown={(e) => {
							if (e.key === 'Enter') {
								positionUpdateImmediate();
							}
						}}
						disabled={!$editMode}
					/>
				</label>
			</div>
		</div>
	{/if}

	{#if $isUploading}
		<SpinnerOverlay />
	{/if}
</svelte:element>

<style lang="postcss">
	.status-change-button {
		color: var(--status-color, inherit);
		border: 1px solid var(--status-color, transparent);
	}

	.status-change-button.active {
		@apply cursor-not-allowed text-white;
		background-color: var(--status-color, transparent);
	}

	.status-change-button:hover,
	.status-change-button:focus {
		@apply text-white;
		background-color: var(--status-color, transparent);
	}

	.status-button-container:not(.active):hover .status-change-button.active,
	.status-button-container:not(.active):focus .status-change-button.active {
		@apply opacity-30;
	}

	.notification-badge,
	.notification-badge * {
		@apply inline-block rounded-full bg-warning-500;
	}

	.notification-badge {
		@apply border border-surface-300;
		width: 0.5rem;
		height: 0.5rem;
		margin-left: -0.55rem;
	}

	.notification-badge * {
		@apply size-full animate-ping;
	}

	.chat-button.active {
		@apply rounded-t-none;
	}
</style>
