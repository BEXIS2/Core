<script lang="ts">
	import type { CurationEntryClass } from './CurationEntries';
	import { CurationEntryType, type taskLine } from './types';
	import { curationStore } from './stores';
	import CurationEntryTemplateTool from './CurationEntryTemplateButton.svelte';
	import { entryTemplateRegex } from './CurationEntryTemplate';

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
		const entryTemplateMD = line.match(entryTemplateRegex)?.toString();
		return {
			id: idPrefix + index.toString(),
			fullString: line,
			text: line
				// remove all markdown specific features
				.replaceAll(/^\s*(- |\* )?\s*(\[[xX ]?\])?/g, '')
				.replaceAll(/\*\*|__/g, '')
				.replaceAll(entryTemplateRegex, '')
				.trim(),
			indentation: Math.floor(indentation / 2),
			isListItem: isListItem,
			isBold: /\*\*.*\*\*|__.*__/.test(line),
			isCheckbox: isCheckbox,
			isChecked: isCheckbox ? /^\s*(- |\* )?\s*(\[[xX]\])/.test(line) : undefined,
			entryTemplateMD: entryTemplateMD
		};
	});

	function toggleChecked(id: string) {
		const newLines = taskLines.map((tl) => {
			if (!tl.isCheckbox || tl.id !== id) return tl.fullString;
			const isChecked = /^\s*(- |\* )?\s*(\[[xX]\])/.test(tl.fullString);
			return tl.fullString.replace(/\[[xX ]?\]/, isChecked ? '[ ]' : '[X]');
		});
		const newDescription = newLines.join('\n');
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
								class="input variant-soft-primary size-3"
								bind:checked={tl.isChecked}
								on:change={() => toggleChecked(tl.id)}
							/>
							{tl.text}
						</label>
						{#if tl.entryTemplateMD}
							<CurationEntryTemplateTool entryTemplateMD={tl.entryTemplateMD} />
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
							class="input variant-soft-primary size-3"
							bind:checked={tl.isChecked}
							on:change={() => toggleChecked(tl.id)}
						/>
						{tl.text}
					</label>
				{/if}
			</span>
			{#if tl.entryTemplateMD}
				<CurationEntryTemplateTool entryTemplateMD={tl.entryTemplateMD} />
			{/if}
			<br />
		{/if}
	{/each}
</p>
