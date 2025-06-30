<script lang="ts">
	import type { CurationEntryClass } from './CurationEntries';
	import { CurationEntryType, type taskLine } from './types';
	import { curationStore } from './stores';
	import CurationEntryTemplateTool from './CurationEntryTemplateTool.svelte';

	export let curationStatusEntry: CurationEntryClass;
	export let highlightOpen: string | undefined = undefined;

	if (!curationStatusEntry || curationStatusEntry.type !== CurationEntryType.StatusEntryItem) {
		throw new Error('Invalid CurationStatusEntry provided');
	}

	const idPrefix = 'task-list-entry-';

	// Split tasks into lines
	let taskLinesString = curationStatusEntry.description.split('\n');
	let taskLines: taskLine[] = taskLinesString.map((line, index) => {
		const trimmedLine = line.trimStart();
		const isListItem = trimmedLine.startsWith('- ') || trimmedLine.startsWith('* ');
		const indentation = line.length - trimmedLine.length;
		const isCheckbox = /^\s*(- |\* )?\s*\[[xX ]?\]/.test(line);
		const mdLinkString = line.match(/\[*\]\(\?createEntryFromJSON=.*\)/)?.toString();
		return {
			id: idPrefix + index.toString(),
			fullString: line,
			text: line
				.replaceAll(/^\s*(- |\* )?\s*(\[[xX ]?\])?|\*\*|__|\[.*\]\(\?createEntryFromJSON=.*\)/g, '')
				.trim(),
			indentation: Math.floor(indentation / 2),
			isListItem: isListItem,
			isBold: /\*\*.*\*\*|__.*__/.test(line),
			isCheckbox: isCheckbox,
			isChecked: isCheckbox ? /^\s*(- |\* )?\s*(\[[xX]\])/.test(line) : undefined,
			linkString: mdLinkString?.match(/\?.[^)]*/)?.toString()
		};
	});

	function toggleChecked(id: string) {
		const newLines = taskLines.map((tl) => {
			if (!tl.isCheckbox || tl.id !== id) return tl.fullString;
			const isChecked = /^\s*(- |\* )?\s*(\[[xX]\])/.test(tl.fullString);
			return tl.fullString.replace(/\[[xX ]?\]/, isChecked ? '[ ]' : '[X]');
		});
		const newDescription = newLines.join('\n');
		console.log(newDescription);
		curationStore.setDescription(curationStatusEntry.id, newDescription, true);
	}
</script>

<p>
	{#each taskLines as tl}
		{#if tl.isListItem}
			<ul
				class:ps-8={tl.indentation === 0}
				class="list-disc"
				style:list-style-type={tl.indentation > 0 ? 'circle' : undefined}
				class:ps-16={tl.indentation === 1}
				class:ps-24={tl.indentation > 1}
			>
				<li class:font-semibold={tl.isBold}>
					{#if !tl.isCheckbox || tl.isChecked === undefined}
						{tl.text}
					{:else}
						<label
							id={tl.id}
							class="inline"
							style={!tl.isChecked && highlightOpen ? `color: ${highlightOpen}` : ''}
						>
							<input
								id={`${tl.id}-checkbox`}
								type="checkbox"
								class="size-3 rounded checked:bg-surface-800 hover:bg-primary-300 hover:checked:bg-primary-500 focus:ring-primary-500 focus-visible:ring-primary-500"
								bind:checked={tl.isChecked}
								on:change={() => toggleChecked(tl.id)}
							/>
							{tl.text}
						</label>
						{#if tl.linkString}
							<CurationEntryTemplateTool linkString={tl.linkString} />
						{/if}
					{/if}
				</li>
			</ul>
		{:else}
			<span class:font-semibold={tl.isBold}>
				{#if !tl.isCheckbox || tl.isChecked === undefined}
					{tl.text}
				{:else}
					<label
						id={tl.id}
						class="inline"
						style={!tl.isChecked && highlightOpen ? `color: ${highlightOpen}` : ''}
					>
						<input
							id={`${tl.id}-checkbox`}
							type="checkbox"
							class="size-3 rounded checked:bg-surface-800 hover:bg-primary-300 hover:checked:bg-primary-500 focus:ring-primary-500 focus-visible:ring-primary-500"
							bind:checked={tl.isChecked}
							on:change={() => toggleChecked(tl.id)}
						/>
						{tl.text}
					</label>
				{/if}
			</span>
			{#if tl.linkString}
				<CurationEntryTemplateTool linkString={tl.linkString} />
			{/if}
			<br />
		{/if}
	{/each}
</p>
