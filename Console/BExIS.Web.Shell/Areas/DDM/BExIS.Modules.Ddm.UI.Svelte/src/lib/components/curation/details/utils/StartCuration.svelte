<script lang="ts">
	import Fa from 'svelte-fa';
	import { curationStore } from '$lib/stores/CurationStore';
	import { CurationEntryType } from '$lib/models/CurationEntry';
	import { faPlay, faCircleInfo } from '@fortawesome/free-solid-svg-icons';
	import CurationTemplate from './CurationTemplate.svelte';
	import { tick } from 'svelte';
	import type { CurationTemplateModel } from '$lib/models/Curation';

	const { curation, editMode } = curationStore;

	let greeting = '';
	let tasks = '';

	curationStore.addEmptyEntry(
		{
			name: greeting,
			description: tasks,
			type: CurationEntryType.StatusEntryItem,
			position: 0
		},
		true
	);

	const split = '\n\n---\n\n';

	$: if (tasks.length === 0 && greeting.includes(split)) {
		const splitIndex = greeting.indexOf(split);
		const intro = greeting.slice(0, splitIndex);
		const taskList = greeting.slice(splitIndex + split.length);
		greeting = intro;
		tasks = taskList;
	}

	const startCuration = () => {
		if (!$curation?.curationStatusEntry?.isDraft() || !$curation?.isCurator) return;
		curationStore.updateEntry($curation?.curationStatusEntry.id, {
			topic: greeting,
			description: tasks
		});
		editMode.set(true);
	};

	let greetingTextarea: HTMLTextAreaElement | null = null;

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

	let tasksTextarea: HTMLTextAreaElement | null = null;

	const addTaskTemplate = (template: CurationTemplateModel) => {
		if (tasks.length > 0 && !tasks.endsWith('\n')) tasks += '\n';
		tasks += template.content;
		// Set cursor to end after DOM updates
		tick().then(() => {
			if (tasksTextarea) {
				tasksTextarea.selectionStart = tasksTextarea.selectionEnd = tasksTextarea.value.length;
				tasksTextarea.focus();
			}
		});
	};
</script>

<p class="m-2 text-center text-surface-600">
	<Fa icon={faCircleInfo} class="mr-1 inline-block" />The curation process has not started yet.
</p>

{#if $curation?.isCurator}
	<form class="m-2 flex flex-col items-stretch gap-2 sm:mx-24">
		<button
			on:click|preventDefault={startCuration}
			class="variant-filled-success btn px-4 py-1 text-base"
		>
			<Fa icon={faPlay} class="mr-2 inline-block" />
			Start Curation
		</button>
		<label class="block">
			<span class="text-surface-700">Greeting:</span>
			<textarea
				bind:this={greetingTextarea}
				bind:value={greeting}
				class="textarea text-sm"
				rows="6"
				placeholder="Add a greeting to greet the researcher. (can still be adjusted later)"
				required
			></textarea>
			<CurationTemplate
				title="Greeting"
				templates={$curation?.greetingTemplates ?? []}
				addFunction={addGreetingTemplate}
			/>
		</label>
		<label class="block">
			<span class="text-surface-700">Tasks:</span>
			<textarea
				bind:this={tasksTextarea}
				bind:value={tasks}
				class="textarea text-sm"
				rows="12"
				placeholder="Add a list of tasks you want to complete for this dataset. (can still be adjusted later)"
				required
			></textarea>
			<CurationTemplate
				title="Tasks"
				templates={$curation?.taskListTemplates ?? []}
				addFunction={addTaskTemplate}
			/>
		</label>
	</form>
{/if}
