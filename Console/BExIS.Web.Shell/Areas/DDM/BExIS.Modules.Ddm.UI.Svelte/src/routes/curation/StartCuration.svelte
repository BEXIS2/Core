<script lang="ts">
	import Fa from 'svelte-fa';
	import { curationStore } from './stores';
	import { CurationEntryType } from './types';
	import { faPlay, faCircleInfo } from '@fortawesome/free-solid-svg-icons';

	const { curation, editMode } = curationStore;

	let introduction = '';
	let tasks = '';

	curationStore.addEmptyEntry(
		{
			name: introduction,
			description: tasks,
			type: CurationEntryType.StatusEntryItem,
			position: 0
		},
		true
	);

	const split = '\n\n---\n\n';

	$: if (tasks.length === 0 && introduction.includes(split)) {
		const splitIndex = introduction.indexOf(split);
		const intro = introduction.slice(0, splitIndex);
		const taskList = introduction.slice(splitIndex + split.length);
		introduction = intro;
		tasks = taskList;
	}

	const startCuration = () => {
		if (!$curation?.curationStatusEntry?.isDraft() || !$curation?.isCurator) return;
		curationStore.updateEntry($curation?.curationStatusEntry.id, {
			name: introduction,
			description: tasks
		});
		editMode.set(true);
	};
</script>

<p class="mx-2 mt-2 text-center text-surface-600">
	<Fa icon={faCircleInfo} class="mr-1 inline-block" />The curation process has not started yet.
</p>

{#if $curation?.isCurator}
	<form class="mx-2 mt-2 flex flex-col items-stretch gap-2 sm:mx-24">
		<button
			on:click|preventDefault={startCuration}
			class="variant-filled-success btn px-4 py-1 text-base"
		>
			<Fa icon={faPlay} class="mr-2 inline-block" />
			Start Curation
		</button>
		<label class="block">
			<span class="text-surface-700">Introduction:</span>
			<textarea
				bind:value={introduction}
				class="textarea text-sm"
				rows="6"
				placeholder="Add an introduction to greet the researcher. (can still be adjusted later)"
				required
			></textarea>
		</label>
		<label class="block">
			<span class="text-surface-700">Tasks:</span>
			<textarea
				bind:value={tasks}
				class="textarea text-sm"
				rows="12"
				placeholder="Add a list of tasks you want to complete for this dataset. (can still be adjusted later)"
				required
			></textarea>
		</label>
	</form>
{/if}
