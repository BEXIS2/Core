<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import MarkdownInlineComponent from './MarkdownInlineComponent.svelte';
	import MarkdownList from './MarkdownList.svelte';

	export let markdown: string;
	export let customInlineComponents: {
		regexp: RegExp;
		component: ConstructorOfATypedSvelteComponent;
	}[] = [];
	const dispatch = createEventDispatcher();

	let lines = markdown.split('\n');
	let parsedLines = lines.map((line) => ({
		markdown: line,
		indentation: line.length - line.trimStart().length,
		isHeading: /^(#{1,6})\s/.test(line),
		isListItem: /^\s*[-*] /.test(line),
		isEnum: /^\s*\d+\. /.test(line),
		isHr: /^\s*([-*_]){3,}\s*$/.test(line),
		isCode: /^\s*```/.test(line),
		isBlockquote: /^\s*> /.test(line)
	}));

	let parts: {
		component:
			| 'none'
			| 'isHeading'
			| 'isListItem'
			| 'isEnum'
			| 'isHr'
			| 'isCode'
			| 'isBlockquote'
			| 'isBreak';
		markdown: string;
		onChange?: ((newMarkdown: string) => void) | null;
	}[] = [];

	let isCodeBlock = false;
	let isListBlock = false;
	let isEnumBlock = false;

	parsedLines.forEach((line) => {
		if (line.isCode) {
			isCodeBlock = !isCodeBlock;
			if (isCodeBlock) {
				parts.push({ component: 'isCode', markdown: '' });
			}
			return;
		}
		if (isCodeBlock) {
			parts[parts.length - 1].markdown +=
				(parts[parts.length - 1].markdown ? '\n' : '') + line.markdown;
			return;
		}
		if (line.isListItem) {
			if (!isListBlock) {
				isListBlock = true;
				parts.push({ component: 'isListItem', markdown: line.markdown });
			} else {
				parts[parts.length - 1].markdown += '\n' + line.markdown;
			}
			return;
		} else {
			isListBlock = false;
		}
		if (line.isEnum) {
			if (!isEnumBlock) {
				isEnumBlock = true;
				parts.push({ component: 'isEnum', markdown: line.markdown });
			} else {
				parts[parts.length - 1].markdown += '\n' + line.markdown;
			}
		} else {
			isEnumBlock = false;
		}
		if (line.isHeading) {
			const heading = line.markdown.replace(/^(#{1,6})\s/, '');
			parts.push({ component: 'isHeading', markdown: heading });
		} else if (line.isHr) {
			parts.push({ component: 'isHr', markdown: '' });
		} else if (line.isBlockquote) {
			const blockquote = line.markdown.replace(/^\s*> /, '');
			parts.push({ component: 'isBlockquote', markdown: blockquote });
		} else {
			const trimmed = line.markdown.trim();
			if (parts.length === 0 || parts[parts.length - 1].component !== 'none') {
				parts.push({ component: 'none', markdown: trimmed });
			} else {
				if (trimmed === '') {
					parts.push({ component: 'isBreak', markdown: '' });
					return;
				}
				parts[parts.length - 1].markdown += trimmed.length > 0 ? ' ' + trimmed : '';
			}
		}
	});

	function handlePartChange(index: number, newMarkdown: string) {
		parts[index].markdown = newMarkdown;
		const combined = parts
			.map((part) => {
				// Re-add markdown syntax for headings, lists, etc.
				switch (part.component) {
					case 'isHeading':
						return `## ${part.markdown}`;
					case 'isListItem':
						return part.markdown;
					case 'isEnum':
						return part.markdown;
					case 'isHr':
						return '---';
					case 'isCode':
						return `\`\`\`\n${part.markdown}\n\`\`\``;
					case 'isBlockquote':
						return `> ${part.markdown}`;
					case 'isBreak':
						return '';
					default:
						return part.markdown;
				}
			})
			.join('\n');
		markdown = combined;
		dispatch('change', combined);
	}
</script>

<p>
	{#each parts as part, index (index)}
		{#if part.component === 'none'}
			<MarkdownInlineComponent markdown={part.markdown} {customInlineComponents} />
		{:else if part.component === 'isHeading'}
			<h2 class="mt-2 text-xl font-semibold">{part.markdown}</h2>
		{:else if part.component === 'isListItem' && part.markdown}
			<MarkdownList
				markdown={part.markdown}
				on:change={(e) => handlePartChange(index, e.detail)}
				{customInlineComponents}
			/>
		{:else if part.component === 'isEnum'}
			<ol>
				{#each part.markdown.split('\n') as item}
					<li>{item.replace(/^\s*\d+\. /, '')}</li>
				{/each}
			</ol>
		{:else if part.component === 'isHr'}
			<hr class="my-2 border-t !border-surface-400" />
		{:else if part.component === 'isCode'}
			<pre class="rounded bg-surface-300 px-2 text-sm"><code>{part.markdown}</code></pre>
		{:else if part.component === 'isBlockquote'}
			<blockquote>
				<MarkdownInlineComponent markdown={part.markdown} {customInlineComponents} />
			</blockquote>
		{:else if part.component === 'isBreak'}
			<br />
		{/if}
	{/each}
</p>
