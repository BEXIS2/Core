<script lang="ts">
	import { curationStore } from './stores';
	import { faBookmark, faCheckDouble, faPaperPlane } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { writable } from 'svelte/store';
	import CurationNote from './CurationNote.svelte';
	import { fixedCurationUserId, type CurationEntryClass } from './CurationEntries';
	import { onMount } from 'svelte';

	export let entry: CurationEntryClass;

	let new_note = '';
	let textarea: HTMLTextAreaElement | null = null;
	let noteList: HTMLUListElement | null = null;

	const replyText = writable('');
	replyText.subscribe((text) => {
		if (text.trim() !== '') {
			new_note = '| ' + text.split('\n').join('\n| ') + '\n' + new_note;
			textarea?.focus();
			textarea?.setSelectionRange(new_note.length, new_note.length);
			replyText.set('');
		}
	});

	function scrollToBottom() {
		if (noteList) {
			noteList.scrollTop = noteList.scrollHeight;
		}
	}

	onMount(() => {
		scrollToBottom();
	});

	// Add a note to the entry
	function addNote() {
		new_note = new_note.trim();
		if (new_note === '') return;
		curationStore.addNote(entry.id, new_note);
		new_note = '';
	}

	function textareaKeydown(event: KeyboardEvent) {
		// Handle Ctrl+Enter or Cmd+Enter to add a note
		if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
			event.preventDefault();
			addNote();
		}
	}

	// Scroll to bottom when a new note is added
	$: if (entry.visibleNotes.length > 0) {
		scrollToBottom();
	}
</script>

<div class="flex h-full flex-col gap-y-1 overflow-hidden p-1 text-sm">
	<ul class="node-box flex grow flex-col gap-y-1 overflow-y-auto rounded" bind:this={noteList}>
		<!-- Notes or message area -->
		{#if entry.visibleNotes.length === 0}
			<li class="mx-2 my-auto text-center text-surface-700">
				Add a note to start the conversation about this entry.
			</li>
		{:else}
			{#each entry.visibleNotes as note}
				<CurationNote {note} entryId={entry.id} {replyText} />
			{/each}
		{/if}
	</ul>
	{#if entry.visibleNotes.length > 0 || entry.notes.filter((note) => note.userId === fixedCurationUserId).length > 0}
		<button
			on:click={() => curationStore.setUnread(entry.id, !entry.hasUnreadNotes)}
			class="flex cursor-pointer items-center justify-center gap-x-1 overflow-hidden text-ellipsis text-nowrap rounded px-2 py-1 text-surface-800 hover:bg-surface-400"
			title="Mark this conversation as {!entry.hasUnreadNotes ? 'unread' : 'read'}"
			name="Mark as read/unread"
		>
			{#if !entry.hasUnreadNotes}
				<Fa icon={faBookmark} />
				<span>Mark as unread</span>
			{:else}
				<Fa icon={faCheckDouble} />
				<span>Mark as read</span>
			{/if}
		</button>
	{/if}
	<div class="flex flex-row rounded border border-surface-500">
		<textarea
			bind:this={textarea}
			bind:value={new_note}
			on:keydown={textareaKeydown}
			class="max-h-32 min-h-16 grow resize-y rounded-l border-0 border-r border-surface-500 p-2"
			placeholder="Add a note..."
			rows="1"
		></textarea>
		<button
			class="grow-0 cursor-pointer p-2 text-surface-800 hover:bg-surface-400 disabled:cursor-default disabled:bg-surface-400 disabled:text-surface-500"
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
