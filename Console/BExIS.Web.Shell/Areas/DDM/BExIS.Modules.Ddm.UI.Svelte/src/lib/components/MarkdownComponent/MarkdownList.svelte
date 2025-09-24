<script lang="ts">
	import { derived, get, writable, type Writable } from 'svelte/store';
	import MarkdownInlineComponent from './MarkdownInlineComponent.svelte';
	import MarkdownList from './MarkdownList.svelte';
	import { createEventDispatcher } from 'svelte';

	export let markdown: string;
	export let customInlineComponents: {
		regexp: RegExp;
		component: ConstructorOfATypedSvelteComponent;
	}[] = [];
	const dispatch = createEventDispatcher();

	let lines = markdown.split('\n');

	const parsedLines = lines.map((line) => ({
		markdown: line,
		indentation: line.length - line.trimStart().length
	}));

	let parts: {
		isCheckbox: boolean;
		isChecked: boolean;
		markdown: string;
		subListMarkdown: string;
		lineIndex: number;
		subListIndentation: number;
	}[] = [];

	let currentIndentation = -1;

	parsedLines.forEach((line, idx) => {
		if (currentIndentation === -1 || line.indentation <= currentIndentation) {
			const isCheckbox = /^\s*[-*] \[[xX ]\] /.test(line.markdown);
			const isChecked = isCheckbox ? /^\s*[-*] \[[xX]\] /.test(line.markdown) : false;
			const markdown = isCheckbox
				? line.markdown.replace(/^\s*[-*] \[[xX ]\] /, '')
				: line.markdown.replace(/^\s*[-*] /, '');
			currentIndentation = line.indentation;
			parts.push({
				isCheckbox,
				isChecked,
				markdown,
				subListMarkdown: '',
				lineIndex: idx,
				subListIndentation: 0
			});
		} else {
			if (parts[parts.length - 1].subListIndentation === 0)
				parts[parts.length - 1].subListIndentation = line.indentation;
			if (line.indentation < parts[parts.length - 1].subListIndentation) {
				parts[parts.length - 1].subListMarkdown +=
					(parts[parts.length - 1].subListMarkdown ? '\n' : '') + line.markdown.trimStart();
			} else {
				parts[parts.length - 1].subListMarkdown +=
					(parts[parts.length - 1].subListMarkdown ? '\n' : '') +
					line.markdown.slice(parts[parts.length - 1].subListIndentation);
			}
		}
	});

	function handleCheckboxChange(idx: number, checked: boolean) {
		const lines = markdown.split('\n');
		const lineIdx = parts[idx].lineIndex;

		parts[idx].isChecked = checked;

		lines[lineIdx] = lines[lineIdx].replace(
			/^([ \t]*[-*] )\[[xX ]\]/,
			`$1[${checked ? 'x' : ' '}]`
		);

		markdown = lines.join('\n');
		dispatch('change', markdown);
	}

	function getChecked(e: Event): boolean {
		const target = e.target as HTMLInputElement | null;
		return !!(target && 'checked' in target && target.checked);
	}

	function handleListChange(idx: number, newMarkdown: string) {
		parts[idx].subListMarkdown = newMarkdown;
		const combined = parts
			.map((part) => {
				let line = '';
				if (part.isCheckbox) {
					line += part.isChecked ? '- [x] ' : '- [ ] ';
				} else {
					line += '- ';
				}
				line += part.markdown;
				if (part.subListMarkdown) {
					const listLines = part.subListMarkdown.split('\n');
					const indentedList = listLines
						.map((l) => ' '.repeat(part.subListIndentation) + l)
						.join('\n');
					line += '\n' + indentedList;
				}
				return line;
			})
			.join('\n');
		markdown = combined;
		dispatch('change', combined);
	}

	function handlePartChange(index: number, newMarkdown: string) {
		parts[index].markdown = newMarkdown;
		const combined = parts
			.map((part) => {
				let line = '';
				if (part.isCheckbox) {
					line += part.isChecked ? '- [x] ' : '- [ ] ';
				} else {
					line += '- ';
				}
				line += part.markdown;
				if (part.subListMarkdown) {
					const listLines = part.subListMarkdown.split('\n');
					const indentedList = listLines
						.map((l) => ' '.repeat(part.subListIndentation) + l)
						.join('\n');
					line += '\n' + indentedList;
				}
				return line;
			})
			.join('\n');
		markdown = combined;
		dispatch('change', combined);
	}
</script>

<ul class="markdown-list ml-6">
	{#each parts as part, idx}
		<li>
			{#if part.isCheckbox}
				<label
					class="label -ml-2 cursor-pointer gap-1 rounded !bg-opacity-30 px-1 hover:bg-primary-300"
				>
					<input
						class="input checkbox relative mb-1 size-4"
						type="checkbox"
						checked={part.isChecked}
						on:change={(e) => handleCheckboxChange(idx, getChecked(e))}
					/>
					<MarkdownInlineComponent
						markdown={part.markdown}
						{customInlineComponents}
						on:change={(e) => handlePartChange(idx, e.detail)}
					/>
				</label>
			{:else}
				<MarkdownInlineComponent
					markdown={part.markdown}
					{customInlineComponents}
					on:change={(e) => handlePartChange(idx, e.detail)}
				/>
			{/if}
			{#if part.subListMarkdown}
				<MarkdownList
					markdown={part.subListMarkdown}
					{customInlineComponents}
					on:change={(e) => handleListChange(idx, e.detail)}
				/>
			{/if}
		</li>
	{/each}
</ul>

<style lang="postcss">
	:global(ul.markdown-list) {
		list-style-type: disc;
	}

	:global(ul.markdown-list ul.markdown-list) {
		list-style-type: circle;
	}

	:global(ul.markdown-list ul.markdown-list ul.markdown-list) {
		list-style-type: square;
	}
</style>
