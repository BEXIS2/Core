<script lang="ts">
	import { type Writable } from 'svelte/store';
	import CurationEntryTemplateTool from './CurationEntryTemplateTool.svelte';

	export let line: string;
	export let taskCheckingQueue: Writable<[string, boolean][]>;
	export let highlightOpen: string | undefined = undefined;

	line = line.trim();

	// if line contains any bold part the whole line will be bold
	const isBold = /\*\*.*\*\*|__.*__/.test(line);

	const isCheckbox = /^\[[xX ]?\]/.test(line);

	let isChecked = /^\[[xX]\]/.test(line);

	let linkString = line.match(/\[*\]\(\?createEntryFromJSON=.*\)/)?.toString();
	linkString = linkString?.match(/\?.[^)]*/)?.toString();

	// remove unneeded characters
	// [], [ ], [x], [X] (only at beginning)
	// **, __, [...](...)
	// links that are for creating entries
	const removedCharsLine = line
		.replaceAll(/\*\*|__|^\[[xX ]?\]|\[.*\]\(\?createEntryFromJSON=.*\)/g, '')
		.trim();
</script>

{#if isCheckbox}
	<label class="inline" style={!isChecked && highlightOpen ? `color: ${highlightOpen}` : ''}>
		<input
			type="checkbox"
			class="size-3 rounded checked:bg-surface-800 hover:bg-primary-300 hover:checked:bg-primary-500 focus:ring-primary-500 focus-visible:ring-primary-500"
			name={removedCharsLine}
			on:change={() => taskCheckingQueue.update((queue) => [...queue, [line, !isChecked]])}
			bind:checked={isChecked}
		/>
		<span>
			{#if isBold}
				<strong class="font-semibold">{removedCharsLine}</strong>
			{:else}
				{removedCharsLine}
			{/if}
		</span>
	</label>
{:else if isBold}
	<strong class="font-semibold">{removedCharsLine}</strong>
{:else}
	{removedCharsLine}
{/if}
{#if linkString}
	<CurationEntryTemplateTool {linkString} />
{/if}
