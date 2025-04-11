<script lang="ts">
	import { curationStore } from './stores';
	import { faBookmark, faCheckDouble, faPaperPlane } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { CurationUserType } from './types';
	import { writable } from 'svelte/store';
	import CurationNote from './CurationNote.svelte';
	import { fixedCurationUserId, type CurationEntryClass } from './CurationEntries';

	export let entry: CurationEntryClass;

	let new_note = '';
	let textarea: HTMLTextAreaElement | null = null;

	const parentText = writable('');
	parentText.subscribe((text) => {
		if (text.trim() !== '') {
			new_note = '| ' + text.split('\n').join('\n| ') + '\n' + new_note;
			textarea?.focus();
			textarea?.setSelectionRange(new_note.length, new_note.length);
			parentText.set('');
		}
	});

	function addNote() {
		new_note = new_note.trim();
		if (new_note === '') return;
		curationStore.addNote(entry.id, CurationUserType.Curator, new_note);
		new_note = '';
	}

	function textareaKeydown(event: KeyboardEvent) {
		if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
			event.preventDefault();
			addNote();
		}
	}
</script>

<div class="border-surface-500 grid grid-cols-1 border-b p-1 pl-2 text-sm">
	{#if entry.visibleNotes.length === 0}
		<p class="text-surface-700">No notes available</p>
	{:else}
		<ul>
			{#each entry.visibleNotes as note}
				<CurationNote {note} entryId={entry.id} {parentText} />
			{/each}
		</ul>
	{/if}
	{#if entry.visibleNotes.length > 0 || entry.notes.filter((note) => note.userId === fixedCurationUserId).length > 0}
		<button
			on:click={() => curationStore.setUnread(entry.id, !entry.hasUnreadNotes)}
			class="hover:bg-surface-300 text-surface-800 flex cursor-pointer items-center justify-center gap-x-1 rounded p-1 text-sm"
		>
			{#if !entry.hasUnreadNotes}
				<Fa icon={faBookmark} />
				<span>Mark as Unread</span>
			{:else}
				<Fa icon={faCheckDouble} />
				<span>Mark as Read</span>
			{/if}
		</button>
	{/if}
	<div class="border-surface-500 mt-1 flex flex-row rounded border">
		<textarea
			bind:this={textarea}
			bind:value={new_note}
			on:keydown={textareaKeydown}
			class="border-surface-500 min-h-16 grow resize-y rounded-l border-0 border-r p-2"
			placeholder="Add a note..."
			rows="1"
		></textarea>
		<button
			class="hover:bg-surface-200 text-surface-800 disabled:bg-surface-200 disabled:text-surface-500 grow-0 cursor-pointer p-2 disabled:cursor-default"
			type="button"
			name="Send Note"
			title="Send Note"
			on:click={addNote}
			disabled={new_note.trim() === ''}
		>
			<Fa icon={faPaperPlane} />
		</button>
	</div>
</div>
