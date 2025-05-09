<script lang="ts">
	import Fa from 'svelte-fa';
	import {
		faAngleDown,
		faArrowDown,
		faCircleCheck,
		faCircleDot,
		faCircleExclamation,
		faEyeSlash,
		faFileLines,
		faNoteSticky,
		faPen,
		faPersonCircleCheck,
		faTrash,
		faXmark
	} from '@fortawesome/free-solid-svg-icons';
	import { derived, writable } from 'svelte/store';
	import { curationStore } from './stores';
	import RelativeDate from '$lib/components/RelativeDate.svelte';
	import CurationNotes from './CurationNotes.svelte';
	import {
		CurationEntryStatus,
		CurationEntryStatusColors,
		CurationEntryStatusNames
	} from './types';
	import type { CurationEntryClass } from './CurationEntries';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';
	import AddCurationEntry from './AddCurationEntry.svelte';
	import CurationEntryInput from './CurationEntryInput.svelte';

	export let entry: CurationEntryClass;
	export let combined: boolean;
	export let isExpanded = writable(false);
	export let editEntryMode = writable(entry.id <= 0); // true if entry is a draft

	const { curation, editMode } = curationStore;

	let position = entry.position;
	let positiontimer: NodeJS.Timeout | null = null;

	const isUploading = derived(curationStore.uploadingEntries, (uploadingEntries) =>
		uploadingEntries.some((id) => id === entry.id)
	);

	const entryReadable = curationStore.getEntryReadable(entry.id);

	const nextStatus = derived(entryReadable, ($entry) => {
		return $entry?.getNextStatus() ?? CurationEntryStatus.Ok;
	});

	entryReadable.subscribe((entry) => {
		if (entry) {
			position = entry.position;
		}
	});

	const toggleStatus = () => {
		curationStore.toggleStatus(entry.id);
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
</script>

<li
	class="curation-entry-card relative mx-2 flex items-stretch justify-stretch overflow-hidden"
	class:curation-entry-card-expanded={$isExpanded && $editMode}
>
	<button
		class="grow-0 rounded-l p-2 text-surface-800 hover:bg-surface-200 disabled:bg-surface-50 disabled:text-surface-400 disabled:hover:bg-surface-50"
		on:click={() => isExpanded.update((o) => !o)}
		name="{$isExpanded ? 'Collapse' : 'Expand'} entry: {`"${entry.name}"`}"
		title="{$isExpanded ? 'Collapse' : 'Expand'} entry: {`"${entry.name}"`}"
		disabled={$editEntryMode || entry.isDraft()}
	>
		<Fa icon={faAngleDown} class="transition-transform {$isExpanded ? 'rotate-180' : ''}" />
	</button>

	<button
		class="curation-entry-change-status"
		style="background-color: {CurationEntryStatusColors[entry.status]};"
		name="Toggle Status"
		title="Toggle Status ({CurationEntryStatusNames[entry.status]} to {CurationEntryStatusNames[
			$nextStatus
		]})"
		on:click={toggleStatus}
		disabled={$editEntryMode || entry.isDraft()}
	>
		<!-- current status -->
		<div
			class="curation-entry-change-status-current"
			style="color: {CurationEntryStatusColors[entry.status]}"
		>
			{#if entry.status === CurationEntryStatus.Ok}
				<Fa icon={faCircleDot} />
			{:else if entry.status === CurationEntryStatus.Open}
				<Fa icon={faCircleExclamation} />
			{:else if entry.status === CurationEntryStatus.Fixed}
				<Fa icon={faPersonCircleCheck} />
			{:else if entry.status === CurationEntryStatus.Closed}
				<Fa icon={faCircleCheck} />
			{/if}
		</div>

		<div class="text-xs">
			<Fa icon={faArrowDown} />
		</div>
		<!-- next status -->
		<div
			class="curation-entry-change-status-next"
			style="color: {CurationEntryStatusColors[$nextStatus]}"
		>
			{#if $nextStatus === CurationEntryStatus.Ok}
				<Fa icon={faCircleDot} />
			{:else if $nextStatus === CurationEntryStatus.Open}
				<Fa icon={faCircleExclamation} />
			{:else if $nextStatus === CurationEntryStatus.Fixed}
				<Fa icon={faPersonCircleCheck} />
			{:else if $nextStatus === CurationEntryStatus.Closed}
				<Fa icon={faCircleCheck} />
			{/if}
		</div>
	</button>

	{#if $editMode && $editEntryMode}
		<CurationEntryInput {entry} {editEntryMode} />
	{:else}
		<div class="relative my-1 grow">
			<h2
				class="ml-1 border-b border-surface-400 px-1"
				class:font-semibold={$isExpanded && !combined}
			>
				{#if combined}
					{#if entry.name}
						<span class="font-semibold" class:text-primary-500={entry.isDraft()}>{entry.name}</span
						>:
					{/if}
				{/if}
				{#if entry.description}
					{entry.description}
				{/if}
			</h2>

			{#if $isExpanded}
				<CurationNotes {entry} />
			{/if}
			<div class="row flex justify-between gap-x-3">
				<ul class="row flex items-center gap-x-3 px-2 text-xs font-semibold text-surface-700">
					<li
						class:text-warning-500={entry.hasUnreadNotes}
						class:font-semibold={entry.hasUnreadNotes}
					>
						<button class="flex items-center gap-x-1" on:click={() => isExpanded.update((o) => !o)}>
							{#if entry.hasUnreadNotes}
								<Fa icon={faNoteSticky} class="absolute animate-ping" />
							{/if}
							<Fa icon={faNoteSticky} />
							{entry.visibleNotes ? entry.visibleNotes?.length : 0} Note{entry.visibleNotes
								?.length == 1
								? ''
								: 's'}
						</button>
					</li>
					<RelativeDate
						tag="li"
						class="flex items-center gap-x-1"
						date={entry.lastChangedDate}
						label="Last updated"
						showIcon={true}
					/>
				</ul>
				{#if $editMode}
					<label>
						<span class="text-xs font-semibold text-surface-700">Position</span>
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
						/>
					</label>
				{/if}
			</div>

			{#if $isUploading}
				<SpinnerOverlay />
			{/if}
		</div>
	{/if}

	{#if $editMode}1
		<div class="no-wrap absolute right-0 top-0 flex gap-x-1">
			{#if entry.isHidden() || entry.isDraft()}
				<div class="row mr-2 flex gap-x-1">
					{#if entry.isHidden()}
						<div class="rounded bg-tertiary-700 p-1 pr-0.5 text-xs text-surface-50" title="Hidden">
							<Fa icon={faEyeSlash} class="block size-4" />
						</div>
					{/if}
					{#if entry.isDraft()}
						<div class="rounded bg-primary-400 p-1 pr-0.5 text-xs text-surface-50" title="Draft">
							<Fa icon={faFileLines} class="block size-4" />
						</div>
					{/if}
				</div>
			{/if}

			{#if entry.isDraft()}
				<button
					class="rounded bg-error-400 p-1 text-xs text-error-700 hover:bg-error-700 hover:text-error-900"
					title="Delete Draft"
					on:click={deleteEntry}
					name="Delete Draft"
				>
					<Fa icon={faTrash} class="block size-4" />
				</button>
			{/if}

			<button
				class="rounded bg-secondary-400 p-1 text-xs text-secondary-700 hover:bg-secondary-700 hover:text-secondary-900"
				title="Toggle Edit Entry"
				on:click={() => editEntryMode.update((o) => !o)}
				name="Toggle Edit Entry"
			>
				{#if !$editEntryMode}
					<Fa icon={faPen} class="block size-4" />
				{:else}
					<Fa icon={faXmark} class="block size-4" />
				{/if}
			</button>
		</div>
	{/if}
</li>

<AddCurationEntry position={entry.position + (entry.isDraft() ? 0 : 1)} />

<style lang="postcss">
	.curation-entry-card .curation-entry-change-status {
		/* button */
		@apply w-2 shrink-0 grow-0 gap-1 overflow-hidden rounded text-surface-800 transition-all;
	}

	.curation-entry-card .curation-entry-change-status > div {
		/* button all icon divs */
		@apply ml-10 flex items-center justify-center transition-all;
	}

	.curation-entry-card
		.curation-entry-change-status:not(:hover):not(:focus-visible):not(:disabled)
		> div,
	.curation-entry-card .curation-entry-change-status:not(:disabled):active > div {
		/* button normal state all icon divs */
		color: white !important;
	}

	.curation-entry-card .curation-entry-change-status:not(:disabled):hover:not(:active),
	.curation-entry-card .curation-entry-change-status:not(:disabled):focus-visible:not(:active) {
		/* button hover state (not active) */
		background-color: rgb(216, 216, 216) !important; /* bg-surface-400 with !important */
	}

	.curation-entry-card
		.curation-entry-change-status:not(:disabled)
		.curation-entry-change-status-current {
		/* current status icon div */
		@apply mb-1 h-full text-lg;
	}

	.curation-entry-card
		.curation-entry-change-status:not(:disabled):hover
		.curation-entry-change-status-current,
	.curation-entry-card
		.curation-entry-change-status:not(:disabled):focus-visible
		.curation-entry-change-status-current {
		/* current status icon div hover state */
		@apply h-4 duration-200;
		animation: pulse 1s ease-in-out infinite;
	}

	.curation-entry-card
		.curation-entry-change-status:not(:disabled)
		.curation-entry-change-status-next {
		/* next status icon div */
		@apply mt-0.5 h-3 text-xs opacity-30;
	}

	.curation-entry-card.curation-entry-card-expanded .curation-entry-change-status:not(:disabled),
	.curation-entry-card:hover .curation-entry-change-status:not(:disabled),
	.curation-entry-card:has(*:focus-visible) .curation-entry-change-status:not(:disabled) {
		/* button expanded state */
		@apply w-8;
	}

	.curation-entry-card.curation-entry-card-expanded
		.curation-entry-change-status:not(:disabled)
		> div,
	.curation-entry-card:hover .curation-entry-change-status:not(:disabled) > div,
	.curation-entry-card:has(*:focus-visible) .curation-entry-change-status:not(:disabled) > div {
		/* button expanded state all icon divs */
		@apply ml-0;
	}
</style>
