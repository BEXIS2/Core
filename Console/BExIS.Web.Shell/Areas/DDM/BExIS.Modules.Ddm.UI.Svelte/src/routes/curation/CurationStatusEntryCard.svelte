<script lang="ts">
	import Fa from 'svelte-fa';
	import type { CurationEntryClass } from './CurationEntries';
	import { CurationEntryType, CurationStatusEntryTab } from './types';
	import {
		faDoorOpen,
		faListCheck,
		faPen,
		faFloppyDisk,
		faEyeSlash,
		faXmark
	} from '@fortawesome/free-solid-svg-icons';
	import { derived, writable } from 'svelte/store';
	import { curationStore } from './stores';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';
	import TaskList from './TaskList.svelte';
	import CurationLabel from './CurationLabel.svelte';
	import { slide } from 'svelte/transition';
	import Greeting from './Greeting.svelte';

	export let curationStatusEntry: CurationEntryClass;

	if (!curationStatusEntry || curationStatusEntry.type !== CurationEntryType.StatusEntryItem) {
		throw new Error('Invalid CurationStatusEntry provided');
	}

	const { curation, currentStatusEntryTab, curationInfoExpanded } = curationStore;

	var isUploadingStatus = derived(curationStore.uploadingEntries, ($uploadingEntries) => {
		return $uploadingEntries.includes(curationStatusEntry.id);
	});

	const editGreetingMode = writable(false);

	let greeting = curationStatusEntry.topic;

	const editGreeting = () => {
		greeting = curationStatusEntry.topic;
		editGreetingMode.set(true);
	};

	const saveGreeting = () => {
		editGreetingMode.set(false);
		curationStore.setTopic(curationStatusEntry.id, greeting);
	};

	const cancelGreetingEdit = () => {
		greeting = curationStatusEntry.topic;
		editGreetingMode.set(false);
	};

	const editTasksMode = writable(false);

	let tasks = curationStatusEntry.description;

	const editTasks = () => {
		tasks = curationStatusEntry.description;
		editTasksMode.set(true);
	};

	const saveTasks = () => {
		editTasksMode.set(false);
		curationStore.setDescription(curationStatusEntry.id, tasks);
	};

	const cancelTasksEdit = () => {
		tasks = curationStatusEntry.description;
		editTasksMode.set(false);
	};

	let highlightOpen: string | undefined = undefined;

	const labelSelectContent = derived(curation, (c) => c?.curationLabels || []);

	const remainingLabels = derived([labelSelectContent, curation], ([labels, c]) => {
		if (!labels || !c) return [];
		return Array.from(
			new Set(labels.map((l) => l.name)).difference(
				new Set(
					curationStatusEntry.visibleNotes.map((n) =>
						n.comment
							.match(/^\S*\s/)
							?.toString()
							.trim()
					)
				)
			)
		)
			.toSorted((a, b) => a.localeCompare(b))
			.map((name) => labels.find((l) => l.name === name)!);
	});

	const taskInfoString = derived(curation, (c) => {
		const taskString = c?.curationStatusEntry?.description || '';
		let open = 0;
		let closed = 0;
		for (const match of taskString.matchAll(/(^|\n)\s*[-\*]?\s*\[( ?|x|X)\]/g)) {
			if (match[2] === ' ' || match[2] === '') open++;
			else closed++;
		}
		if (open === 0) {
			if (closed === 0) return 'None';
			else return 'Done';
		}
		return `(${closed} von ${closed + open})`;
	});
</script>

<!-- Status and Badges -->
{#if $curation?.isCurator}
	<div class="relative flex flex-wrap gap-2 overflow-x-hidden border-b border-surface-500 p-2">
		<!-- Status -->
		<CurationLabel {curationStatusEntry} />
		<!-- Custom Labels -->
		{#each curationStatusEntry.visibleNotes.toSorted( (a, b) => a.comment.localeCompare(b.comment) ) as labelNote ((labelNote.comment, labelNote.creationDateObj))}
			<CurationLabel {curationStatusEntry} {labelNote} />
		{/each}
		<!-- Custom Label Creation -->
		<CurationLabel {curationStatusEntry} remainingLabels={$remainingLabels} />
		{#if curationStatusEntry.visibleNotes.length > 0}
			<div class="flex grow items-center justify-center px-1 py-0.5 text-xs text-surface-600">
				<span>Click on a label to remove it</span>
			</div>
		{/if}

		<!-- Spinner overlay -->
		{#if $isUploadingStatus}
			<SpinnerOverlay />
		{/if}
	</div>
{/if}

<!-- Greeting and Tasks -->
<div class="relative overflow-x-hidden border-b border-surface-500 p-2">
	{#if $curation?.isCurator}
		<!-- Tabs -->
		<div
			class="tab-switch relative flex items-stretch gap-1 rounded border border-surface-300 bg-surface-200 p-0.5"
		>
			<label
				title="Greeting tab"
				class="flex grow cursor-pointer flex-wrap content-center items-center justify-center gap-x-1 rounded px-2 py-0.5 text-center text-surface-800 transition-colors"
			>
				<input
					type="radio"
					class="pointer-events-none absolute opacity-0"
					title="Go to Greeting Tab"
					checked
					bind:group={$currentStatusEntryTab}
					value={CurationStatusEntryTab.Greeting}
				/>
				<Fa icon={faDoorOpen} class="inline-block" />
				<span class="font-semibold">Greeting</span>
			</label>
			<label
				title="Tasks tab"
				class="flex grow cursor-pointer flex-wrap content-center items-center justify-center gap-x-1 rounded px-2 py-0.5 text-center text-surface-800 transition-colors"
			>
				<input
					type="radio"
					class="pointer-events-none absolute opacity-0"
					title="Go to Task Tab"
					bind:group={$currentStatusEntryTab}
					value={CurationStatusEntryTab.Tasks}
				/>
				<Fa icon={faListCheck} class="inline-block" />
				<span class="font-semibold">Curation Tasks</span>
				<span class="text-sm text-surface-700">{$taskInfoString}</span>
			</label>
			<label
				title="Hide Tabs"
				class="flex cursor-pointer flex-wrap content-center items-center justify-center gap-x-1 rounded px-3 py-0.5 text-center text-surface-800 transition-colors"
			>
				<input
					type="radio"
					class="pointer-events-none absolute opacity-0"
					title="Hide Tabs"
					bind:group={$currentStatusEntryTab}
					value={CurationStatusEntryTab.Hide}
				/>
				<Fa icon={faEyeSlash} class="inline-block" />
				<span class="font-semibold">Hide</span>
			</label>
		</div>

		<!-- Greeting content -->
		{#if $currentStatusEntryTab === CurationStatusEntryTab.Greeting}
			<div
				class="mt-2 overflow-x-hidden rounded bg-surface-200 px-2 py-1"
				in:slide={{ duration: 150 }}
				out:slide={{ duration: 150 }}
			>
				{#if !$editGreetingMode}
					<p class="text-surface-800">
						<Greeting {greeting} />
					</p>
					<div class="flex flex-row-reverse justify-between">
						<button
							class="variant-soft-secondary btn mb-1 mt-2 px-2 py-0.5"
							on:click={editGreeting}
							title="Edit Greeting"
						>
							<Fa icon={faPen} class="mr-1 inline-block" />
							<span class="ml-1">Edit Greeting</span>
						</button>
					</div>
				{:else}
					<!-- Text area - Greeting -->
					<label class="block">
						<span class="text-surface-700">Greeting:</span>
						<textarea
							bind:value={greeting}
							class="mt-1 w-full rounded border border-surface-500 px-2 py-1 text-sm text-surface-800 focus-visible:border-surface-700 focus-visible:outline-none"
							rows="6"
							placeholder="Enter introduction text"
						></textarea>
					</label>
					<div class="mb-1 mt-2 flex flex-row justify-between">
						<!-- Cancel button -->
						<button
							class="variant-ghost-surface btn px-2 py-1"
							on:click={cancelGreetingEdit}
							title="Cancel introduction editing"
						>
							<Fa icon={faXmark} class="mr-2 inline-block" />
							<span class="ml-1">Cancel Editing</span>
						</button>
						<!-- Save button -->
						<button
							class="variant-filled-success btn px-2 py-1"
							on:click={saveGreeting}
							title="Save Greeting"
						>
							<Fa icon={faFloppyDisk} class="mr-2 inline-block" />
							<span class="ml-1">Save Greeting</span>
						</button>
					</div>
				{/if}
			</div>
		{/if}

		<!-- Tasks content -->
		{#if $currentStatusEntryTab === CurationStatusEntryTab.Tasks}
			<div
				class="mt-2 overflow-x-hidden rounded bg-surface-200 px-2 py-1"
				in:slide={{ duration: 150 }}
				out:slide={{ duration: 150 }}
			>
				{#if !$editTasksMode}
					<div
						class="resize-y overflow-y-auto overflow-x-hidden"
						class:h-96={tasks.split('\n').length > 10}
					>
						{#key curationStatusEntry.description}
							<TaskList {curationStatusEntry} {highlightOpen} />
						{/key}
					</div>
					<div class="flex items-center justify-between border-t">
						<label>
							<span>Highlight open tasks:</span>
							<select bind:value={highlightOpen} class="rounded py-0.5 text-sm">
								<option value={undefined}>None</option>
								<option value={'red'}>Red</option>
								<option value={'#bbbbbb'}>Gray</option>
							</select>
						</label>
						<button
							class="variant-soft-secondary btn mb-1 mt-2 px-2 py-0.5"
							on:click={editTasks}
							title="Edit Tasks"
						>
							<Fa icon={faPen} class="mr-1 inline-block" />
							<span class="ml-1">Edit Tasks</span>
						</button>
					</div>
				{:else}
					<!-- Text area - Tasks -->
					<label class="block">
						<span class="text-surface-700">Tasks:</span>
						<textarea
							bind:value={tasks}
							class="mt-1 w-full rounded border border-surface-500 px-2 py-1 text-sm text-surface-800 focus-visible:border-surface-700 focus-visible:outline-none"
							rows="12"
							placeholder="Enter tasks"
						></textarea>
					</label>

					<div class="mb-1 mt-2 flex flex-row justify-between">
						<!-- Cancel button -->
						<button
							class="variant-ghost-surface btn px-2 py-1"
							on:click={cancelTasksEdit}
							title="Cancel tasks editing"
						>
							<Fa icon={faXmark} class="mr-2 inline-block" />
							<span class="ml-1">Cancel Editing</span>
						</button>
						<!-- Save button -->
						<button
							class="variant-filled-success btn px-2 py-1"
							on:click={saveTasks}
							title="Save Tasks"
						>
							<Fa icon={faFloppyDisk} class="mr-2 inline-block" />
							<span class="ml-1">Save Tasks</span>
						</button>
					</div>
				{/if}
			</div>
		{/if}
	{:else}
		<!-- Non-curator view -->
		<!-- Display only the introduction text -->

		<div class="rounded bg-surface-200 px-2 py-1" class:line-clamp-4={!$curationInfoExpanded}>
			<p class="text-surface-800">
				{#each curationStatusEntry.name.split('\n') as line, index}
					{line}
					{#if index < curationStatusEntry.name.split('\n').length - 1}
						<br />
					{/if}
				{/each}
			</p>
		</div>
	{/if}

	<!-- Spinner overlay -->
	{#if $isUploadingStatus}
		<SpinnerOverlay />
	{/if}
</div>

<style lang="postcss">
	.tab-switch label:has(:focus-visible) {
		@apply shadow-sm ring-2 ring-surface-800;
	}

	.tab-switch label:hover {
		@apply bg-surface-300 text-surface-900;
	}

	.tab-switch label:has(:checked) {
		@apply bg-surface-400 text-surface-900;
	}
</style>
