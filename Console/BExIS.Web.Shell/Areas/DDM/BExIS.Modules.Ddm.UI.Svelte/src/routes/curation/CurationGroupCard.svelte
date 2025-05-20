<script lang="ts">
	import { derived, writable, type Readable, type Writable } from 'svelte/store';
	import CurationEntryCard from './CurationEntryCard.svelte';
	import { faAnglesDown } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import type { CurationEntryClass } from './CurationEntries';
	import AddCurationEntry from './AddCurationEntry.svelte';

	export let entries: CurationEntryClass[];
	export let groupName: string | undefined = undefined;

	groupName = groupName ?? entries[0].name;

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
	<AddCurationEntry
		position={entries[0].position + (entries[0].isDraft() ? 0 : 1)}
		type={entries[0].type}
	/>
{:else}
	<div class="m-2">
		<div class="flex items-stretch justify-between gap-2">
			<button
				class="h-full grow-0 rounded p-2 text-surface-800 hover:bg-surface-200"
				on:click={() => toggleAll(!$allExpanded)}
				name="{$allExpanded ? 'Collapse' : 'Expand'} {groupName}"
				title="{$allExpanded ? 'Collapse' : 'Expand'} {groupName}"
			>
				<Fa icon={faAnglesDown} class="transition-transform {$allExpanded ? 'rotate-180' : ''}" />
			</button>
			<h2 class="my-1 grow">
				<span class="font-semibold">{groupName}</span>
				<span class="text-sm text-surface-600"
					>[{entries.length} {entries.length === 1 ? 'Entry' : 'Entries'}]</span
				>
			</h2>
		</div>

		<AddCurationEntry
			position={entries[0].position}
			name={groupName}
			type={entries[0].type}
			class="mb-2 ml-6"
		/>
		<ul class="ml-6 grid grid-cols-1 gap-2">
			{#each entries as entry, index (entry.id)}
				<CurationEntryCard {entry} combined={false} isExpanded={expandedEntries[index]} />
				<AddCurationEntry
					position={entry.position + (entry.isDraft() ? 0 : 1)}
					name={entry.name}
					type={entry.type}
				/>
			{/each}
		</ul>
	</div>
{/if}
