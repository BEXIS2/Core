<script lang="ts">
	import Fa from 'svelte-fa';
	import {
		faEyeSlash,
		faFileLines,
		faLink,
		faMessage,
		faPen,
		faTrash,
		faXmark
	} from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import RelativeDate from '$lib/components/RelativeDate.svelte';
	import CurationNotes from './CurationNotes.svelte';
	import { CurationEntryStatus, CurationEntryStatusDetails } from './types';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';
	import CurationEntryInput from './CurationEntryInput.svelte';
	import CurationNote from './CurationNote.svelte';
	import Confetti from '$lib/components/Confetti.svelte';

	export let entryId: number;
	export let combined: boolean;
	export let tag: string | null = 'div'; // if set, use this tag instead of the default <div>

	const {
		curation,
		editMode,
		statusColorPalette,
		jumpToEntryWhere,
		jumpToDataEnabled,
		uploadingEntries
	} = curationStore;

	const entry = curationStore.getEntryReadable(entryId);
	$: visibleNotes = $entry?.visibleNotes ?? [];
	$: lastVisibleNote = visibleNotes.at(-1);

	const cardState = curationStore.getEntryCardState(entryId);

	// Jump to this card
	let cardElement: HTMLElement | null = null;

	const scrollCardIntoView = () =>
		cardElement?.scrollIntoView({ behavior: 'smooth', block: 'center' });

	$: {
		const jumpHere = $jumpToEntryWhere
			? $entry && $jumpToEntryWhere($entry.getHelperModel())
			: false;
		if (jumpHere && cardElement) {
			scrollCardIntoView();
			cardElement.classList.add('blink');
			setTimeout(() => cardElement?.classList.remove('blink'), 3000);
			jumpToEntryWhere.set(undefined);
		}
	}

	let inputPosition: number = $entry?.position || 0;
	let isEditingPosition = false;
	let positiontimer: NodeJS.Timeout | null = null;

	$: if (!isEditingPosition && $entry && $entry.position !== inputPosition) {
		inputPosition = $entry.position;
	}

	$: isUploading = $uploadingEntries.some((id) => id === entryId);

	const setStatus = (status: CurationEntryStatus) => {
		if ($entry && $entry.status !== status) {
			curationStore.setStatus(entryId, status);
		}
	};

	const positionUpdateDebounce = () => {
		if (positiontimer) {
			clearTimeout(positiontimer);
		}
		positiontimer = setTimeout(() => {
			if ($entry && inputPosition !== $entry.position) {
				curationStore.updateEntryPosition(entryId, inputPosition);
			}
		}, 1000);
	};

	const positionUpdateImmediate = () => {
		isEditingPosition = false;
		if ($entry && inputPosition !== $entry.position) {
			curationStore.updateEntryPosition(entryId, inputPosition);
		}
	};

	const deleteEntry = () => {
		if (
			confirm(
				`Are you sure you want to delete the entry "${$entry?.name}" at position ${$entry?.position}?`
			)
		) {
			curationStore.deleteEntry(entryId);
		}
	};

	const toggleExpand = () => {
		cardState.update((v) => ({ ...v, isExpanded: !v.isExpanded }));
	};

	$: showCollapsedNotes = !$cardState.isExpanded && ($entry?.visibleNotes.length ?? 0 > 0);

	let confettiRef: Confetti;

	const confettiTypeSet = new Set([CurationEntryStatus.Fixed, CurationEntryStatus.Closed]);

	const jumpToDataClick = () => {
		if (!$jumpToDataEnabled || !$entry) return;
		curationStore.dispatchJumpToData($entry.getHelperModel($statusColorPalette));
	};
</script>

<!-- class blink will be tongled automatically in the script -->
<svelte:element
	this={tag}
	bind:this={cardElement}
	class="curation-entry-card relative overflow-hidden rounded px-2 py-0.5"
	class:border={combined}
	class:border-surface-400={combined}
	class:text-primary-500={$entry?.isDraft()}
	class:blink={false}
	style:--current-status-color={$statusColorPalette.colors[$entry?.status || 0]}
>
	{#if $editMode && $cardState.editEntryMode}
		<CurationEntryInput {entryId} />
	{:else}
		<div class="items-top no-wrap mb-2 flex gap-x-2 overflow-hidden">
			<div class="grow overflow-hidden">
				<!-- Title -->
				{#if combined}
					<h3 class="font-semibold" class:text-surface-500={$entry?.isHidden()}>
						{$entry?.name}
					</h3>
				{/if}

				<!-- Description -->
				<p class="overflow-hidden break-words" class:text-surface-500={$entry?.isHidden()}>
					{$entry?.description}
				</p>
			</div>
			<RelativeDate
				class="hidden text-nowrap border-surface-300 pr-1 text-xs text-surface-600 sm:block"
				date={$entry?.lastChangedDate || new Date(0)}
				label="Last updated"
				showIcon={true}
			/>
		</div>

		<!-- Notes -->
		<div
			class="h-0 overflow-hidden rounded-t rounded-bl border-surface-300"
			class:h-52={$cardState.isExpanded && !$editMode}
			class:h-12={showCollapsedNotes && !$editMode}
			class:md:h-7={showCollapsedNotes && !$editMode}
			class:bg-surface-300={$cardState.isExpanded}
			class:rounded-br={!$cardState.isExpanded}
			class:mb-2={!$editMode && (visibleNotes.length > 0 || $cardState.isExpanded)}
			style:transition="height 0.15s"
		>
			{#if $cardState.isExpanded && !$editMode}
				<CurationNotes {entryId} />
			{:else if lastVisibleNote}
				<button
					class="variant-soft-surface btn size-full !justify-start overflow-hidden border border-surface-500 px-1.5 py-0.5 text-left text-sm text-surface-700"
					title="Open Chat"
					on:click={() => {
						toggleExpand();
						scrollCardIntoView();
					}}
				>
					<CurationNote note={lastVisibleNote} {entryId} shortForm={true} />
				</button>
			{/if}
		</div>

		<!-- Status and Actions -->
		<div class="mb-1 flex flex-wrap items-center justify-stretch gap-1 overflow-x-hidden text-sm">
			<!-- link button -->
			{#if $jumpToDataEnabled}
				<button
					class="variant-filled-surface btn hidden px-1 py-0.5 text-sm text-surface-800 sm:block"
					title="Show linked data"
					on:click={jumpToDataClick}
				>
					<Fa icon={faLink} class="inline-block" />
					Show
				</button>
			{/if}
			<!-- Status change -->
			<div
				class="status-button-container flex w-80 shrink grow gap-x-1 overflow-hidden"
				class:gap-x-0={$editMode}
				class:!w-24={$editMode}
				class:!grow-0={$editMode}
				class:opacity-10={$entry?.isDraft()}
				class:cursor-not-allowed={$entry?.isDraft()}
			>
				{#each CurationEntryStatusDetails as statusDetails, index}
					{#if $curation?.isCurator || (index !== CurationEntryStatus.Ok && index !== CurationEntryStatus.Closed)}
						{#if !$editMode || index === $entry?.status}
							<button
								class="status-change-button relative shrink grow basis-1/4 overflow-hidden text-ellipsis text-nowrap rounded p-0"
								class:active={index === $entry?.status}
								disabled={index === $entry?.status || $editMode || $entry?.isDraft()}
								style="--status-color: {$statusColorPalette.colors[index]};"
								title="Change Entry Status to {statusDetails.name}"
								on:click={(e) => {
									if (confettiTypeSet.has(index)) confettiRef.trigger(e);
									setStatus(index);
								}}
							>
								<div class="inactive-content size-full px-1 py-0.5">
									<Fa icon={statusDetails.icon} class="mr-1 inline-block" />
									{statusDetails.name}
								</div>
								<div
									class="active-content pointer-events-none absolute left-0 top-0 size-full overflow-hidden px-1 py-0.5"
								>
									<Fa icon={statusDetails.icon} class="mr-1 inline-block" />
									{statusDetails.name}
								</div>
							</button>
						{/if}
					{/if}
				{/each}
			</div>

			<!-- Chat Button -->
			<button
				class="variant-ghost-surface btn w-24 grow overflow-hidden text-ellipsis text-nowrap px-2 py-0.5 text-sm"
				class:hidden={$editMode}
				class:opacity-10={$entry?.isDraft()}
				class:cursor-not-allowed={$entry?.isDraft()}
				disabled={$editMode || $entry?.isDraft()}
				title="Toggle Chat"
				on:click={() => {
					toggleExpand();
					if ($cardState.isExpanded) scrollCardIntoView();
				}}
			>
				<Fa icon={$cardState.isExpanded ? faXmark : faMessage} class="mr-2 inline-block" />
				{#if $entry?.hasUnreadNotes && !$cardState.isExpanded}
					<span class="notification-badge"><span>&nbsp;</span></span>
				{/if}
				Chat
				{#if !$cardState.isExpanded && visibleNotes.length > 0}
					({visibleNotes.length})
				{/if}
			</button>

			<!-- EDIT MODE -->

			<!-- Hidden or Draft Badges -->
			{#if $entry?.isHidden()}
				<div
					class="rounded bg-tertiary-700 px-2 py-0.5 text-surface-50 opacity-70"
					class:hidden={!$editMode}
					title="Entry is hidden"
				>
					<Fa icon={faEyeSlash} class="inline-block" />
					Hidden
				</div>
			{/if}
			{#if $entry?.isDraft()}
				<div
					class="rounded bg-primary-400 px-2 py-0.5 text-surface-50 opacity-70"
					class:hidden={!$editMode}
					title="Entry is a draft"
				>
					<Fa icon={faFileLines} class="inline-block" />
					Draft
				</div>
			{/if}

			{#if $entry?.isDraft()}
				<button
					class="variant-soft-error btn px-2 py-0.5 text-sm"
					title="Delete Draft"
					on:click={deleteEntry}
					name="Delete Draft"
				>
					<Fa icon={faTrash} class="mr-2 inline-block" />
					Delete
				</button>
			{/if}

			<!-- Edit Entry -->
			<button
				class="variant-soft-secondary btn grow px-2 py-0.5 text-sm"
				class:hidden={!$editMode}
				disabled={!$editMode}
				on:click={() => cardState.update((cs) => ({ ...cs, editEntryMode: true }))}
				name="Edit Entry"
				title="Edit Entry"
			>
				<Fa icon={faPen} class="mr-2 inline-block" />
				Edit Entry
			</button>

			<!-- Position Input -->
			<div class="row flex h-full items-center justify-between gap-x-3" class:hidden={!$editMode}>
				<label class="ml-2">
					<span class="text-surface-700">Position</span>
					<input
						type="number"
						bind:value={inputPosition}
						class="input ml-0.5 w-12 px-1 py-0.5 text-xs text-surface-800"
						min="1"
						on:focus={() => {
							isEditingPosition = true;
						}}
						on:blur={positionUpdateImmediate}
						on:input={positionUpdateDebounce}
						disabled={!$editMode}
					/>
				</label>
			</div>
		</div>
	{/if}

	{#if isUploading}
		<SpinnerOverlay />
	{/if}

	<Confetti bind:this={confettiRef} />
</svelte:element>

<style lang="postcss">
	.curation-entry-card:hover,
	.curation-entry-card:has(*:hover),
	.curation-entry-card:has(*:focus-visible) {
		@apply bg-surface-200 ring-1;
		--tw-ring-color: var(--current-status-color);
	}

	.curation-entry-card.blink {
		animation: blink-bg 3s;
	}

	@keyframes blink-bg {
		0%,
		30%,
		100% {
			background-color: inherit;
		}
		15%,
		45% {
			background-color: rgba(255, 166, 0, 0.3);
		}
	}

	.status-change-button {
		color: var(--status-color, inherit);
		box-shadow: 0 0 0 1px var(--status-color, transparent) inset;
		transition:
			opacity 0.15s,
			filter 0.15s,
			transform 0.15s;
		z-index: 1;
	}

	.status-change-button:active {
		transform: scale(95%, 95%);
		filter: brightness(0.9);
	}

	.status-change-button:hover {
		/* box-shadow: 0 0 0 2px var(--status-color, transparent) inset; */
		opacity: 0.7;
	}

	.status-change-button.active {
		@apply cursor-not-allowed text-white;
	}

	.status-change-button .active-content {
		opacity: 0;
		background-color: var(--status-color, transparent);
		transform: scale(0.2);
		clip-path: circle(0% at 50% 50%);
		transition:
			opacity 0.3s,
			clip-path 0.3s;
	}

	.status-change-button.active .active-content {
		opacity: 1;
		transform: scale(1);
		clip-path: circle(150% at 50% 50%);
	}

	.status-change-button.active
		.status-button-container:not(.active):hover
		.status-change-button.active,
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
		margin-left: -0.8rem;
		margin-top: -0.65rem;
	}

	.notification-badge * {
		@apply size-full animate-ping;
	}
</style>
