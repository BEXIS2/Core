<script lang="ts">
	import { writable } from 'svelte/store';
	import type { CurationEntryClass } from './CurationEntries';
	import TaskLine from './TaskLine.svelte';
	import { CurationEntryType } from './types';
	import { curationStore } from './stores';

	export let curationStatusEntry: CurationEntryClass;
	export let highlightOpen: string | undefined = undefined;

	if (!curationStatusEntry || curationStatusEntry.type !== CurationEntryType.StatusEntryItem) {
		throw new Error('Invalid CurationStatusEntry provided');
	}

	// Split tasks into lines
	let taskLines = curationStatusEntry.description.split('\n');

	// Calculate the needed information for each line once
	const taskLinesHelper = taskLines.map((line) => {
		const trimmedLine = line.trimStart();
		const isListItem = trimmedLine.startsWith('- ') || trimmedLine.startsWith('* ');
		const indentation = line.length - trimmedLine.length;
		return { trimmedLine, isListItem, indentation };
	});

	// find default indentation for first level (maximum is set to 4)
	let minIndentation = 4;
	for (const helper of taskLinesHelper) {
		if (helper.indentation < minIndentation) minIndentation = helper.indentation;
		if (minIndentation === 0) break;
	}

	let taskContent: (string | (string | string[])[])[] = [];

	for (const helper of taskLinesHelper) {
		if (!helper.isListItem) {
			// normal text
			taskContent.push(helper.trimmedLine.trim());
		} else {
			// is list item
			if (taskContent.length === 0 || typeof taskContent.at(-1) === 'string') {
				taskContent.push([]);
			}
			if (helper.indentation < minIndentation + 2) {
				// first level of indentation
				const last = taskContent.at(-1);
				if (Array.isArray(last)) {
					// this check is only necessary to make the compiler happy
					last.push(helper.trimmedLine.slice(1).trim());
				}
			} else {
				// second level of indentation
				if (taskContent.at(-1)!.length === 0 || typeof taskContent.at(-1)!.at(-1) === 'string') {
					const last = taskContent.at(-1);
					if (Array.isArray(last)) {
						// this check is only necessary to make the compiler happy
						last.push([]);
					}
				}
				const last = taskContent.at(-1)!.at(-1);
				if (Array.isArray(last)) {
					// this check is only necessary to make the compiler happy
					last.push(helper.trimmedLine.slice(1).trim());
				}
			}
		}
	}

	const taskCheckingQueue = writable<[string, boolean][]>([]);

	taskCheckingQueue.subscribe((queue) => {
		if (queue.length === 0) return;
		taskCheckingQueue.update((q) => {
			if (q.length === 0) return [];

			// find line and replace checkbox
			const line = q[0][0];
			const newLine = line.replace(/^\[[xX ]?\]/, q[0][1] ? '[X]' : '[ ]');
			const newDescription = curationStatusEntry.description.replaceAll(line, newLine);

			curationStore.setDescription(curationStatusEntry.id, newDescription, true);

			return q.slice(1);
		});
	});
</script>

<p>
	{#each taskContent as tc}
		{#if typeof tc === 'string'}
			<TaskLine line={tc} {taskCheckingQueue} {highlightOpen} />
		{:else if Array.isArray(tc)}
			<ul>
				{#each tc as tc2}
					<li>
						{#if typeof tc2 === 'string'}
							<TaskLine line={tc2} {taskCheckingQueue} {highlightOpen} />
						{:else if Array.isArray(tc2)}
							<ul>
								{#each tc2 as tc3}
									<li>
										<TaskLine line={tc3} {taskCheckingQueue} {highlightOpen} />
									</li>
								{/each}
							</ul>
						{/if}
					</li>
				{/each}
			</ul>
		{/if}
	{/each}
</p>

<style lang="postcss">
	ul {
		list-style-type: disc;
		padding-inline-start: 2rem;
	}

	ul ul {
		list-style-type: circle;
	}

	ul li:has(ul) {
		list-style-type: none;
	}
</style>
