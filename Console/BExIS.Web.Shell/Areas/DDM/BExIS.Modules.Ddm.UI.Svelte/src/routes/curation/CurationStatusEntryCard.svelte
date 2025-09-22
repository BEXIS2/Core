<script lang="ts">
	import Fa from 'svelte-fa';
	import { CurationEntryType, CurationStatusEntryTab, type CurationTemplateModel } from './types';
	import {
		faDoorOpen,
		faListCheck,
		faPen,
		faFloppyDisk,
		faEyeSlash,
		faXmark
	} from '@fortawesome/free-solid-svg-icons';
	import { writable } from 'svelte/store';
	import { curationStore } from './stores';
	import SpinnerOverlay from '$lib/components/SpinnerOverlay.svelte';
	import CurationLabel from './CurationLabel.svelte';
	import { slide } from 'svelte/transition';
	import Greeting from './Greeting.svelte';
	import CurationTemplate from './CurationTemplate.svelte';
	import { tick } from 'svelte';
	import MarkdownComponent from '$lib/components/MarkdownComponent/MarkdownComponent.svelte';
	import CurationEntryTemplateButton from './CurationEntryTemplateButton.svelte';
	import { entryTemplateRegex } from './CurationEntryTemplate';

	const { curation, currentStatusEntryTab, curationInfoExpanded, uploadingEntries } = curationStore;

	$: curationStatusEntry = $curation?.curationStatusEntry;

	$: if (!curationStatusEntry || curationStatusEntry.type !== CurationEntryType.StatusEntryItem) {
		console.log('CurationStatusEntryCard: No valid status entry found.');
		curationStatusEntry = undefined;
	}

	$: isUploadingStatus = curationStatusEntry && $uploadingEntries.includes(curationStatusEntry.id);

	const editGreetingMode = writable(false);

	let greetingTextarea: HTMLTextAreaElement | null = null;

	let greeting = curationStatusEntry?.topic ?? '';

	$: if (curationStatusEntry && !$editGreetingMode) {
		greeting = curationStatusEntry.topic;
	}

	const editGreeting = () => {
		if (!curationStatusEntry) return;
		greeting = curationStatusEntry?.topic;
		editGreetingMode.set(true);
	};

	const saveGreeting = () => {
		if (!curationStatusEntry) return;
		editGreetingMode.set(false);
		curationStore.setTopic(curationStatusEntry.id, greeting);
	};

	const cancelGreetingEdit = () => {
		greeting = curationStatusEntry?.topic ?? '';
		editGreetingMode.set(false);
	};

	const addGreetingTemplate = (template: CurationTemplateModel) => {
		if (greeting.length > 0 && !greeting.endsWith('\n')) greeting += '\n';
		greeting += template.content;
		// Set cursor to end after DOM updates
		tick().then(() => {
			if (greetingTextarea) {
				greetingTextarea.selectionStart = greetingTextarea.selectionEnd =
					greetingTextarea.value.length;
				greetingTextarea.focus();
			}
		});
	};

	const editTasksMode = writable(false);

	let tasksTextarea: HTMLTextAreaElement | null = null;

	let tasksContent = curationStatusEntry?.description ?? '';

	const templateMarkdownComponent = {
		component: CurationEntryTemplateButton,
		regexp: entryTemplateRegex
	};

	$: if (curationStatusEntry && !$editTasksMode) {
		tasksContent = curationStatusEntry.description;
	}

	function handleTasksChange(newMarkdown: string) {
		if (!curationStatusEntry) return;
		curationStore.setDescription(curationStatusEntry.id, newMarkdown, true);
	}

	const editTasks = () => {
		if (!curationStatusEntry) return;
		tasksContent = curationStatusEntry?.description;
		editTasksMode.set(true);
	};

	const saveTasks = () => {
		if (!curationStatusEntry) return;
		editTasksMode.set(false);
		curationStore.setDescription(curationStatusEntry.id, tasksContent);
	};

	const cancelTasksEdit = () => {
		tasksContent = curationStatusEntry?.description ?? '';
		editTasksMode.set(false);
	};

	const addTaskTemplate = (template: CurationTemplateModel) => {
		if (tasksContent.length > 0 && !tasksContent.endsWith('\n')) tasksContent += '\n';
		tasksContent += template.content;
		// Set cursor to end after DOM updates
		tick().then(() => {
			if (tasksTextarea) {
				tasksTextarea.selectionStart = tasksTextarea.selectionEnd = tasksTextarea.value.length;
				tasksTextarea.focus();
			}
		});
	};

	let highlightOpen: string | undefined = undefined;

	$: remainingLabels =
		!curationStatusEntry || !$curation?.curationLabels
			? []
			: Array.from(
					new Set($curation.curationLabels.map((l) => l.name)).difference(
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
					.map((name) => $curation.curationLabels.find((l) => l.name === name)!);

	$: taskInfoString = (() => {
		if (!curationStatusEntry) return '';
		const taskString = curationStatusEntry.description;
		const matches = Array.from(taskString.matchAll(/(^|\n)\s*[-\*]?\s*\[( ?|x|X)\]/g));
		const { open, closed } = matches.reduce(
			(acc, match) => {
				if (match[2] === ' ' || match[2] === '') acc.open++;
				else acc.closed++;
				return acc;
			},
			{ open: 0, closed: 0 }
		);
		if (open === 0) {
			if (closed === 0) return 'None';
			else return 'Done';
		}
		return `(${closed} of ${closed + open})`;
	})();
</script>

<!-- Status and Badges -->
{#if $curation?.isCurator && curationStatusEntry}
	<div class="relative flex flex-wrap gap-2 overflow-x-hidden border-b border-surface-500 p-2">
		<!-- Status -->
		<CurationLabel {curationStatusEntry} />
		<!-- Custom Labels -->
		{#each curationStatusEntry.visibleNotes.toSorted( (a, b) => a.comment.localeCompare(b.comment) ) as labelNote ((labelNote.comment, labelNote.creationDateObj))}
			<CurationLabel {curationStatusEntry} {labelNote} />
		{/each}
		<!-- Custom Label Creation -->
		<CurationLabel {curationStatusEntry} {remainingLabels} />
		{#if curationStatusEntry.visibleNotes.length > 0}
			<div class="flex grow items-center justify-center px-1 py-0.5 text-xs text-surface-600">
				<span>Click on a label to remove it</span>
			</div>
		{/if}

		<!-- Spinner overlay -->
		{#if isUploadingStatus}
			<SpinnerOverlay />
		{/if}
	</div>
{/if}

<!-- Greeting and Tasks -->
<div class="relative overflow-x-hidden border-b border-surface-500 p-2">
	{#if $curation?.isCurator && curationStatusEntry}
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
				<span class="text-sm text-surface-700">{taskInfoString}</span>
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
					{#key greeting}
						<Greeting {greeting} />
					{/key}
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
							bind:this={greetingTextarea}
							bind:value={greeting}
							class="mt-1 w-full rounded border border-surface-500 px-2 py-1 text-sm text-surface-800 focus-visible:border-surface-700 focus-visible:outline-none"
							rows="6"
							placeholder="Enter introduction text"
						></textarea>
						<CurationTemplate
							title="Tasks"
							templates={$curation?.taskListTemplates ?? []}
							addFunction={addGreetingTemplate}
						/>
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
						class:h-96={tasksContent.split('\n').length > 10}
					>
						{#key curationStatusEntry.description}
							<MarkdownComponent
								markdown={tasksContent}
								on:change={(e) => handleTasksChange(e.detail)}
								customInlineComponents={[templateMarkdownComponent]}
							/>
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
							bind:this={tasksTextarea}
							bind:value={tasksContent}
							class="mt-1 w-full rounded border border-surface-500 px-2 py-1 text-sm text-surface-800 focus-visible:border-surface-700 focus-visible:outline-none"
							rows="12"
							placeholder="Enter tasks"
						></textarea>
						<CurationTemplate
							title="Tasks"
							templates={$curation?.taskListTemplates ?? []}
							addFunction={addTaskTemplate}
						/>
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
	{:else if curationStatusEntry}
		<!-- Non-curator view -->
		<!-- Display only the greeting -->

		<div class="rounded bg-surface-200 px-2 py-1" class:line-clamp-4={!$curationInfoExpanded}>
			<Greeting {greeting} />
		</div>
	{/if}

	<!-- Spinner overlay -->
	{#if isUploadingStatus}
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
