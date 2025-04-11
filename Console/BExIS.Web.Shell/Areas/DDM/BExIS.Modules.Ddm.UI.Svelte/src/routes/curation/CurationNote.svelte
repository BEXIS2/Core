<script lang="ts">
	import { curationStore } from './stores';
	import { faReply, faTrash } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import RelativeDate from '$lib/components/RelativeDate.svelte';
	import { CurationUserType } from './types';
	import { derived, type Writable } from 'svelte/store';
	import { CurationNoteClass, CurationUserClass, fixedCurationUserId } from './CurationEntries';

	export let note: CurationNoteClass;
	export let entryId: number;
	export let parentText: Writable<string>;

	function deleteNote() {
		curationStore.deleteNote(entryId, note.id);
	}

	const userColorList = [
		'#003f5c',
		'#374c80',
		'#7a5195',
		'#bc5090',
		'#ef5675',
		'#ff764a',
		'#ffa600'
	];

	const userName = derived(curationStore.curation, (curation) => {
		return curation?.userMap.get(note.userId)?.displayName ?? 'Unknown User';
	});

	const getUserColor = (userId: number) => {
		// color list length is 7 and userId starts from 1
		// we multiply by 3 (7 % 3 = 1) to space colors more evenly
		// especially for smaller numbers of users
		return userColorList[((userId - 1) * 2) % userColorList.length];
	};

	const commentLines = note.comment.split('\n');
	const commentLinesGrouped: string[][] = [];
	let inQuote = false;
	commentLines.forEach((line) => {
		if (line.startsWith('| ')) {
			if (!inQuote) {
				commentLinesGrouped.push([]);
				inQuote = true;
			}
			commentLinesGrouped[commentLinesGrouped.length - 1].push(line);
		} else {
			if (inQuote) inQuote = false;
			commentLinesGrouped.push([line]);
		}
	});

	console.log(commentLinesGrouped);
</script>

<li class="note-card rounded px-2 py-1">
	<div class="flex items-center justify-between gap-x-2">
		<p>
			<span style="color: {getUserColor(note.userId)}">
				{$userName}
				{#if note.userId === fixedCurationUserId}
					(You)
				{/if}
			</span>
			<span class="text-surface-600 ml-1 text-xs">
				[{note.userType === CurationUserType.Curator ? 'Curator' : 'User'}]
			</span>
			<RelativeDate
				date={note.creationDateObj}
				label="Note created"
				class="badge text-surface-600 ml-1 text-xs"
			/>
		</p>
		<div class="flex items-center gap-x-1">
			<button
				class="text-surface-600 hover:bg-primary-400 hover:text-primary-800 cursor-pointer rounded p-1"
				on:click={() => parentText.set(note.comment)}
				title="Reply to Note"
				name="Reply to Note"
			>
				<Fa icon={faReply} />
			</button>
			{#if note.userId === fixedCurationUserId}
				<!-- <button
					class="text-surface-600 hover:bg-secondary-400 hover:text-secondary-800 cursor-pointer rounded p-1"
					on:click={editNote}
					title="Edit Note"
					name="Edit Note"
				>
					<Fa icon={faPen} />
				</button> -->
				<button
					class="text-surface-600 hover:bg-error-400 hover:text-error-800 cursor-pointer rounded p-1"
					on:click={deleteNote}
					title="Delete Note"
					name="Delete Note"
				>
					<Fa icon={faTrash} />
				</button>
			{/if}
		</div>
	</div>
	<p class="break words overflow-hidden text-wrap">
		{#each commentLinesGrouped as group}
			{#if group.length > 1 && group[0].startsWith('| ')}
				<p class="bg-surface-200 border-surface-600 my-0.5 w-full rounded border-l-4 px-2 py-0.5">
					{#each group as line}
						{line.slice(2)}
						<br />
					{/each}
				</p>
			{:else}
				{group[0]}
				<br />
			{/if}
		{/each}
	</p>
</li>

<style lang="postcss">
	.note-card:has(*:hover) {
		@apply bg-surface-300;
	}

	.note-card:has(*:hover) button.text-surface-600:not(:hover) {
		@apply text-surface-800;
	}
</style>
