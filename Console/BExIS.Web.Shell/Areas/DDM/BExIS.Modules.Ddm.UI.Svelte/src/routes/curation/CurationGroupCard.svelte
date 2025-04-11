<script lang="ts">
	import { derived, writable, type Readable, type Writable } from 'svelte/store';
	import CurationEntryCard from './CurationEntryCard.svelte';
	import { faAnglesDown } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import type { CurationEntryClass } from './CurationEntries';

	export let entries: CurationEntryClass[];
	export let groupName: string | undefined = undefined;

	groupName = groupName ?? entries[0].topic;

	const expandedEntries = entries.map(() => writable(false));

	const allExpanded = derived(expandedEntries, ($expanded) => {
		return $expanded.every((o) => o);
	});

	const toggleAll = (newState: boolean) => {
		expandedEntries.forEach((entry) => {
			entry.set(newState);
		});
	};
</script>

{#if entries.length === 1}
	<CurationEntryCard entry={entries[0]} combined={true} />
{:else}
	<div class="my-2">
		<div class="flex items-stretch justify-between gap-2">
			<button
				class="hover:bg-surface-200 text-surface-800 border-surface-500 h-full grow-0 rounded-tl border-r p-2"
				on:click={() => toggleAll(!$allExpanded)}
				name="{$allExpanded ? 'Collapse' : 'Expand'} {groupName}"
				title="{$allExpanded ? 'Collapse' : 'Expand'} {groupName}"
			>
				<Fa icon={faAnglesDown} class="transition-transform {$allExpanded ? 'rotate-180' : ''}" />
			</button>
			<h2 class="my-1 grow font-semibold">
				{groupName}
				<span class="text-surface-600 text-sm"
					>[{entries.length} {entries.length === 1 ? 'Entry' : 'Entries'}]</span
				>
			</h2>
		</div>
		<ul class="border-surface-500 grid grid-cols-1 gap-2 border-y py-2">
			{#each entries as entry, index (entry.id)}
				<CurationEntryCard {entry} combined={false} isExpanded={expandedEntries[index]} />
			{/each}
		</ul>
	</div>
{/if}
