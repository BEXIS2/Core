<script lang="ts">
	import { curationStore } from '$lib/stores/CurationStore';
	import { faReply, faTrash } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { derived, type Writable } from 'svelte/store';
	import RelativeDate from '$lib/components/RelativeDate.svelte';
	import { CurationUserType, fixedCurationUserId } from '$lib/models/CurationUser';
	import { CurationNoteClass } from '$lib/models/CurationNote';

	export let note: CurationNoteClass;
	export let entryId: number;
	export let replyText: Writable<string> | undefined = undefined;
	export let shortForm: boolean = false;

	const userColorList = [
		'#003f5c',
		'#374c80',
		'#7a5195',
		'#bc5090',
		'#ef5675',
		'#ff764a',
		'#ffa600'
	];

	function deleteNote() {
		if (
			confirm(
				`Are you sure you want to delete this note:\n\n"${note.comment}"\n\nThis action cannot be undone. Maybe use the "reply to note"-action instead?`
			)
		) {
			curationStore.deleteNote(entryId, note.id);
		}
	}

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
	// Group consecutive lines that start with '| ' into a single group
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

	const reply = () => {
		if (!replyText) return;
		replyText.set(note.comment);
	};
</script>

{#if shortForm}
	<p class="line-clamp-2 h-full max-h-full overflow-hidden text-ellipsis sm:line-clamp-1">
		<span>
			{$userName}
			{#if note.userId === fixedCurationUserId}
				(You)
			{/if}
		</span>
		<RelativeDate date={note.creationDateObj} label="Note created" prefix="(" suffix=")" />:
		{note.comment}
	</p>
{:else}
	<li class="note-card rounded px-2 py-1">
		<div class="flex items-center justify-between gap-x-2">
			<!-- Header -->
			<h4>
				<!-- User and Message Info -->
				<span style="color: {getUserColor(note.userId)}">
					{$userName}
					{#if note.userId === fixedCurationUserId}
						(You)
					{/if}
				</span>
				<span class="ml-1 text-xs text-surface-600">
					[{note.userType === CurationUserType.Curator ? 'Curator' : 'User'}]
				</span>
				<RelativeDate
					date={note.creationDateObj}
					label="Note created"
					class="ml-1 text-xs text-surface-600"
				/>
			</h4>
			<div class="flex items-center gap-x-1">
				<!-- Action buttons -->
				<button
					class="rounded p-1 text-surface-600 hover:bg-primary-400 hover:text-primary-800 active:bg-primary-500 active:text-primary-900"
					on:click={reply}
					title="Reply to note"
					name="Reply to note"
				>
					<Fa icon={faReply} />
				</button>
				{#if note.userId === fixedCurationUserId}
					<button
						class="rounded p-1 text-surface-600 hover:bg-error-400 hover:text-error-800 active:bg-error-500 active:text-error-900"
						on:click={deleteNote}
						title="Delete note"
						name="Delete note"
					>
						<Fa icon={faTrash} />
					</button>
				{/if}
			</div>
		</div>
		<p class="break words overflow-hidden text-wrap">
			<!-- Note Content -->
			{#each commentLinesGrouped as group}
				{#if group.length > 0 && group[0].startsWith('| ')}
					<p class="my-0.5 w-full rounded border-l-4 border-surface-600 bg-surface-200 px-2 py-0.5">
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
{/if}

<style lang="postcss">
	.note-card:has(*:hover) {
		@apply bg-surface-400;
	}

	.note-card:has(*:hover) button.text-surface-600:not(:hover) {
		@apply text-surface-800;
	}
</style>
