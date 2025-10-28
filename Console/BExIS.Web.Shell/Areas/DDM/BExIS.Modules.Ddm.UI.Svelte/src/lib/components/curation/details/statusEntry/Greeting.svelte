<script lang="ts">
	import Fa from 'svelte-fa';
	import { faMessage } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from '$lib/stores/CurationStore';
	import { CurationEntryStatusDetails } from '$lib/models/CurationEntry';

	export let greeting: string;

	const { statusColorPalette } = curationStore;

	// split into markdown bold and italic syntax
	type GreetingPart = {
		text: string;
		isBold: boolean;
		isItalic: boolean;
	};

	function parseGreeting(greeting: string): GreetingPart[][] {
		const lines = greeting.split('\n');
		const r = lines.map((line) => {
			const parts: GreetingPart[] = [];
			// regex that matches bold, italic, and normal text
			const regex = /(\*\*.*?\*\*|\*.*?\*|[^*]+)/g;
			let match;
			while ((match = regex.exec(line))) {
				const text = match[0];
				const isBold = text.startsWith('**') && text.endsWith('**');
				const isItalic = text.startsWith('*') && text.endsWith('*') && !isBold;
				parts.push({
					text: text.replace(isBold ? /\*\*/g : isItalic ? /\*/g : '', ''), // remove markdown only if bold/italic
					isBold,
					isItalic
				});
			}
			return parts;
		});
		return r;
	}

	let parsedGreeting = parseGreeting(greeting);

	const curationStatusNamesSet = new Set(
		CurationEntryStatusDetails.map((status) => status.name.toLowerCase())
	);
	const notesNamesSet = new Set(['chat', 'messages']);
</script>

<p class="text-surface-800">
	{#each parsedGreeting as line, index}
		{#each line as part}
			{#if !part.isBold && !part.isItalic}
				{part.text}
			{:else if (part.isBold || part.isItalic) && curationStatusNamesSet.has(part.text.toLowerCase())}
				{#each CurationEntryStatusDetails as status, index}
					{#if status.name.toLowerCase() === part.text.toLowerCase()}
						<span class="italic" style="color: {$statusColorPalette.colors[index]}">
							<Fa icon={status.icon} class="ml-1 inline-block text-sm" />
							{status.name}
						</span>
					{/if}
				{/each}
			{:else if (part.isBold || part.isItalic) && notesNamesSet.has(part.text.toLowerCase())}
				<span class="italic">
					<Fa icon={faMessage} class="ml-1 inline-block text-sm" />
					Chat
				</span>
			{:else if part.isBold}
				<span class="font-bold">{part.text}</span>
			{:else if part.isItalic}
				<span class="italic">{part.text}</span>
			{/if}
		{/each}
		{#if index < parsedGreeting.length - 1}
			<br />
		{/if}
	{/each}
</p>
