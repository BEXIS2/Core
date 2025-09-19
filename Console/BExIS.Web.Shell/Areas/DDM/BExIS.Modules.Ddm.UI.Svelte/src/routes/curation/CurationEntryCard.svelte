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
	import CurationNote from './CurationNote.svelte';
	import Confetti from '$lib/components/Confetti.svelte';
	import CurationEntryForm from './CurationEntryForm.svelte';

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

	$: isUploading = $uploadingEntries.some((id) => id === entryId);

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

	// Status Update
	let confettiRef: Confetti;
	const confettiTypeSet = new Set([CurationEntryStatus.Fixed, CurationEntryStatus.Closed]);

	const setStatus = (status: CurationEntryStatus, target: HTMLElement) => {
		if ($entry && $entry.status !== status) {
			curationStore.setStatus(entryId, status);
			if (confettiTypeSet.has(status)) confettiRef.trigger(target);
		}
	};

	const keydownHandler = (event: KeyboardEvent) => {
		const buttons = Array.from(
			(event.currentTarget as HTMLElement).parentElement?.querySelectorAll('[role="radio"]') || []
		);
		const currentIndex = buttons.indexOf(event.currentTarget as HTMLElement);
		let newIndex = currentIndex;
		if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
			newIndex = (currentIndex - 1 + buttons.length) % buttons.length;
			event.preventDefault();
		} else if (event.key === 'ArrowRight' || event.key === 'ArrowDown') {
			newIndex = (currentIndex + 1) % buttons.length;
			event.preventDefault();
		}
		if (newIndex !== currentIndex) {
			(buttons[newIndex] as HTMLElement).focus();
		}
	};

	// Position Update
	let inputPosition: number = $entry?.position || 0;
	let isEditingPosition = false;
	let positiontimer: NodeJS.Timeout | null = null;

	$: if (!isEditingPosition && $entry && $entry.position !== inputPosition) {
		inputPosition = $entry.position;
	}

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
		<CurationEntryForm {entryId} />
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
				date={$entry?.lastChangedDate}
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
					class="variant-soft-surface btn size-full !justify-start border border-surface-500 px-1.5 py-0.5 text-left text-sm text-surface-700"
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
		<div class="mb-1 flex flex-wrap items-center justify-stretch gap-1 text-sm">
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
				class="status-button-container flex max-w-full shrink grow basis-80 gap-x-1"
				class:gap-x-0={$editMode}
				class:!max-w-24={$editMode}
				class:!grow-0={$editMode}
				class:opacity-10={$entry?.isDraft()}
				class:cursor-not-allowed={$entry?.isDraft()}
				role="radiogroup"
			>
				{#each CurationEntryStatusDetails as statusDetails, index}
					{#if index === $entry?.status || (!$editMode && ($curation?.isCurator || (index !== CurationEntryStatus.Ok && index !== CurationEntryStatus.Closed)))}
						<button
							class="btn relative shrink grow basis-1/4 overflow-hidden text-ellipsis text-nowrap rounded px-2 py-0.5 text-sm"
							class:active={index === $entry?.status}
							style="--status-color: {$statusColorPalette.colors[index]};"
							title="Change Entry Status to {statusDetails.name}"
							on:click={(e) => {
								setStatus(index, e.currentTarget);
							}}
							on:keydown={keydownHandler}
							role="radio"
							aria-checked={index === $entry?.status}
							tabindex={index === $entry?.status ? 0 : -1}
						>
							<Fa icon={statusDetails.icon} class="mr-1 inline-block" />
							{statusDetails.name}
						</button>
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
				<span class="relative mr-2">
					<Fa icon={$cardState.isExpanded ? faXmark : faMessage} class="inline-block" />
					{#if $entry?.hasUnreadNotes && !$cardState.isExpanded}
						<span class="notification-badge">
							<span aria-hidden="true">&nbsp;</span>
							<span class="sr-only">New unread note</span>
						</span>
					{/if}
				</span>
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

	.status-button-container .btn {
		color: var(--status-color, inherit);
		box-shadow: 0 0 0 1px var(--status-color, transparent) inset;
		transition:
			background-color 0.15s,
			opacity 0.15s,
			filter 0.15s,
			transform 0.15s;
		z-index: 1;
	}
	.status-button-container .btn:hover {
		box-shadow: 0 0 0 2px var(--status-color, transparent) inset;
	}

	.status-button-container .btn.active {
		@apply cursor-not-allowed text-white;
		background-color: var(--status-color, inherit);
	}

	.status-button-container:has(.btn:not(.active):hover) .btn.active,
	.status-button-container:has(.btn:not(.active):focus) .btn.active {
		@apply opacity-30;
	}

	.notification-badge,
	.notification-badge * {
		@apply inline-block rounded-full bg-warning-500;
	}

	.notification-badge {
		@apply absolute border border-surface-300;
		width: 0.5rem;
		height: 0.5rem;
		top: 0;
		right: 0;
		transform: translate(50%, 0);
	}

	.notification-badge * {
		@apply size-full animate-ping;
	}
</style>
