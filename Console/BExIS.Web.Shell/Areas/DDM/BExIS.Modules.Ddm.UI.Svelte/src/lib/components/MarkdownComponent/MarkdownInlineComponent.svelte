<script lang="ts">
	import MarkdownInlineComponent from '$lib/components/markdownComponent/MarkdownInlineComponent.svelte';
	import { createEventDispatcher } from 'svelte';

	export let markdown: string;
	export let customInlineComponents: {
		regexp: RegExp;
		component: ConstructorOfATypedSvelteComponent;
	}[] = [];
	const dispatch = createEventDispatcher();

	if (markdown.includes('\n')) {
		console.warn(
			'MarkdownInlineComponent received multiline markdown input. Consider using MarkdownComponent instead.'
		);
	}

	const regex =
		/(?<bolditalic>\*\*\*([\s\S]+?)\*\*\*)|(?<bold>\*\*([\s\S]+?)\*\*)|(?<italic>(?<=^|\s)\*([^\*][^\*]*?)\*(?=\s|$))|(?<code>`([^`]+)`)/g;

	let parts: {
		component: 'none' | 'italic' | 'bold' | 'bolditalic' | 'inlineCode' | 'custom';
		markdown: string;
		customComponent?: ConstructorOfATypedSvelteComponent;
	}[] = [];

	parts = [];
	let rest = markdown;
	while (rest.length > 0) {
		// Check for custom component match first
		let customMatch = null;
		let customIndex = -1;
		let customComponent = null;
		for (const { regexp, component } of customInlineComponents) {
			regexp.lastIndex = 0;
			const match = regexp.exec(rest);
			if (match && (customMatch === null || match.index < customIndex)) {
				customMatch = match;
				customIndex = match.index;
				customComponent = component;
			}
		}

		// Check for markdown match
		regex.lastIndex = 0;
		const mdMatch = regex.exec(rest);
		let mdIndex = mdMatch ? mdMatch.index : -1;

		// Decide which comes first: custom or markdown
		if (customMatch && (mdMatch === null || customIndex <= mdIndex) && customComponent) {
			if (customIndex > 0) {
				parts.push({ component: 'none', markdown: rest.slice(0, customIndex) });
			}
			parts.push({ component: 'custom', markdown: customMatch[0], customComponent });
			rest = rest.slice(customIndex + customMatch[0].length);
		} else if (mdMatch) {
			if (mdIndex > 0) {
				parts.push({ component: 'none', markdown: rest.slice(0, mdIndex) });
			}
			if (mdMatch.groups?.bolditalic) {
				parts.push({ component: 'bolditalic', markdown: mdMatch.groups.bolditalic.slice(3, -3) });
			} else if (mdMatch.groups?.bold) {
				parts.push({ component: 'bold', markdown: mdMatch.groups.bold.slice(2, -2) });
			} else if (mdMatch.groups?.italic) {
				parts.push({ component: 'italic', markdown: mdMatch.groups.italic.slice(1, -1) });
			} else if (mdMatch.groups?.code) {
				// inline code
				parts.push({ component: 'inlineCode', markdown: mdMatch.groups.code.slice(1, -1) });
			}
			rest = rest.slice(mdIndex + mdMatch[0].length);
		} else {
			// No more matches
			parts.push({ component: 'none', markdown: rest });
			break;
		}
	}

	function handlePartChange(index: number, newMarkdown: string) {
		parts[index].markdown = newMarkdown;
		const combined = parts
			.map((part) => {
				if (part.component === 'bolditalic') return `***${part.markdown}***`;
				if (part.component === 'bold') return `**${part.markdown}**`;
				if (part.component === 'italic') return `*${part.markdown}*`;
				if (part.component === 'inlineCode') return `\`${part.markdown}\``;
				if (part.component === 'custom') return part.markdown; // Assume custom components handle their own markdown
				return part.markdown;
			})
			.join('');
		markdown = combined;
		dispatch('change', combined);
	}

	function handleCustomComponentChange(index: number, event: CustomEvent) {
		handlePartChange(index, event.detail);
	}

	function handleCustomComponentChangeWrapper(index: number) {
		return (event: CustomEvent) => handleCustomComponentChange(index, event);
	}
</script>

{#each parts as part, index (index)}
	{#if part.component === 'none'}
		{part.markdown.trim() + ' '}
	{:else if part.component === 'bolditalic'}
		<strong>
			<em>
				<MarkdownInlineComponent bind:markdown={part.markdown} {customInlineComponents} />
			</em>
		</strong>
	{:else if part.component === 'italic'}
		<em>
			<MarkdownInlineComponent bind:markdown={part.markdown} {customInlineComponents} />
		</em>
	{:else if part.component === 'bold'}
		<strong>
			<MarkdownInlineComponent bind:markdown={part.markdown} {customInlineComponents} />
		</strong>
	{:else if part.component === 'inlineCode'}
		<code class="rounded bg-gray-200 px-1 font-mono text-sm">{part.markdown}</code>&nbsp;
	{:else if part.component === 'custom'}
		<svelte:component
			this={part.customComponent}
			markdown={part.markdown}
			on:change={handleCustomComponentChangeWrapper(index)}
		/>
	{/if}
{/each}
