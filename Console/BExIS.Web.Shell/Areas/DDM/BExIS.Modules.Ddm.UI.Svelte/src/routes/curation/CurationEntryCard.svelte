<script lang="ts">
	import Fa from 'svelte-fa';
	import {
		faAngleDown,
		faArrowDown,
		faCircleCheck,
		faCircleDot,
		faCircleExclamation,
		faNoteSticky,
		faPersonCircleCheck
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

	export let entry: CurationEntryClass;
	export let combined: boolean;
	export let isExpanded = writable(false);

	const isUploading = derived(curationStore.uploadingEntries, (uploadingEntries) =>
		uploadingEntries.some((id) => id === entry.id)
	);

	const entryReadable = curationStore.getEntryReadable(entry.id);
	const nextStatus = derived(entryReadable, ($entry) => {
		return $entry?.getNextStatus() ?? CurationEntryStatus.Ok;
	});

	let animatePingParent;

	const toggleStatus = () => {
		curationStore.toggleStatus(entry.id);
	};
</script>

<li
	class="curation-entry-card mx-2 flex items-stretch justify-stretch overflow-hidden"
	class:curation-entry-card-expanded={$isExpanded}
>
	<button
		class="text-surface-800 hover:bg-surface-200 grow-0 rounded-l p-2"
		on:click={() => isExpanded.update((o) => !o)}
		name="{$isExpanded ? 'Collapse' : 'Expand'} entry: {`"${entry.name}"`}"
		title="{$isExpanded ? 'Collapse' : 'Expand'} entry: {`"${entry.name}"`}"
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
	>
		<!-- current status -->
		<div
			class="curation-entry-change-status-current"
			style="color: {CurationEntryStatusColors[entry.status]}"
			bind:this={animatePingParent}
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

	<div class="relative my-1 grow">
		<h2
			class="border-surface-400 mx-1 border-b px-1"
			class:font-semibold={$isExpanded && !combined}
		>
			{#if combined}
				{#if entry.topic}
					<span class="font-semibold">{entry.topic}</span>:
				{/if}
			{/if}
			{#if entry.name}
				{entry.name}
			{/if}
		</h2>

		{#if $isExpanded}
			<CurationNotes {entry} />
		{/if}

		<ul class="text-surface-700 row flex gap-x-3 px-2 align-middle text-xs font-semibold">
			<li
				class="flex items-center gap-x-1"
				class:text-warning-500={entry.hasUnreadNotes}
				class:font-semibold={entry.hasUnreadNotes}
			>
				{#if entry.hasUnreadNotes}
					<Fa icon={faNoteSticky} class="absolute animate-ping" />
				{/if}
				<Fa icon={faNoteSticky} />
				{entry.visibleNotes ? entry.visibleNotes?.length : 0} Note{entry.visibleNotes?.length == 1
					? ''
					: 's'}
			</li>
			<RelativeDate
				tag="li"
				class="flex items-center gap-x-1"
				date={entry.lastChangedDate}
				label="Last updated"
				showIcon={true}
			/>
		</ul>

		{#if $isUploading}
			<SpinnerOverlay />
		{/if}
	</div>
</li>

<style lang="postcss">
	.curation-entry-card .curation-entry-change-status {
		/* button */
		@apply text-surface-800 w-2 grow-0 gap-1 overflow-hidden rounded transition-all;
	}

	.curation-entry-card .curation-entry-change-status > div {
		/* button all icon divs */
		@apply ml-10 flex items-center justify-center transition-all;
	}

	.curation-entry-card .curation-entry-change-status:not(:hover):not(:focus-visible) > div,
	.curation-entry-card .curation-entry-change-status:active > div {
		/* button normal state all icon divs */
		color: white !important;
	}

	.curation-entry-card .curation-entry-change-status:hover:not(:active),
	.curation-entry-card .curation-entry-change-status:focus-visible:not(:active) {
		/* button hover state (not active) */
		background-color: rgb(216, 216, 216) !important; /* bg-surface-400 with !important */
	}

	.curation-entry-card .curation-entry-change-status .curation-entry-change-status-current {
		/* current status icon div */
		@apply mb-1 h-full text-lg;
	}

	.curation-entry-card .curation-entry-change-status:hover .curation-entry-change-status-current,
	.curation-entry-card
		.curation-entry-change-status:focus-visible
		.curation-entry-change-status-current {
		/* current status icon div hover state */
		@apply h-4 duration-200;
		animation: pulse 1s ease-in-out infinite;
	}

	.curation-entry-card .curation-entry-change-status .curation-entry-change-status-next {
		/* next status icon div */
		@apply mt-0.5 h-3 text-xs opacity-30;
	}

	.curation-entry-card.curation-entry-card-expanded .curation-entry-change-status,
	.curation-entry-card:hover .curation-entry-change-status,
	.curation-entry-card:has(*:focus-visible) .curation-entry-change-status {
		/* button expanded state */
		@apply w-8;
	}

	.curation-entry-card.curation-entry-card-expanded .curation-entry-change-status > div,
	.curation-entry-card:hover .curation-entry-change-status > div,
	.curation-entry-card:has(*:focus-visible) .curation-entry-change-status > div {
		/* button expanded state all icon divs */
		@apply ml-0;
	}
</style>
