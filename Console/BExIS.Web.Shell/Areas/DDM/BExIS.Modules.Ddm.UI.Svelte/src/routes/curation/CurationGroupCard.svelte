<script lang="ts">
	import CurationEntryCard from './CurationEntryCard.svelte';
	import type { CurationEntryClass } from './CurationEntries';
	import AddCurationEntry from './AddCurationEntry.svelte';

	export let entries: CurationEntryClass[];
	export let groupName: string | undefined = undefined;

	groupName = groupName ?? entries[0].name;
</script>

{#if entries.length === 1}
	<CurationEntryCard entry={entries[0]} combined={true} tag="li" />
{:else}
	<li class="rounded border border-surface-400 p-2">
		<h2 class="mb-1">
			<span class="font-semibold">{groupName}</span>
			<span class="text-sm text-surface-600"
				>[{entries.length} {entries.length === 1 ? 'Entry' : 'Entries'}]</span
			>
		</h2>

		<ul class="flex flex-col gap-2">
			<AddCurationEntry
				position={entries[0].position}
				name={groupName}
				type={entries[0].type}
				class="mb-2"
				tag="li"
			/>
			{#each entries as entry (entry.id)}
				<CurationEntryCard {entry} combined={false} tag="li" />

				<AddCurationEntry
					position={entry.position + (entry.isDraft() ? 0 : 1)}
					name={entry.name}
					type={entry.type}
					tag="li"
				/>
			{/each}
		</ul>
	</li>
{/if}

<AddCurationEntry
	position={entries[0].position + (entries[0].isDraft() ? 0 : 1)}
	type={entries[0].type}
	tag="li"
/>
