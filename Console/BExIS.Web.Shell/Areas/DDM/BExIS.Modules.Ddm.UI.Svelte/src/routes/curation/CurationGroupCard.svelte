<script lang="ts">
	import CurationEntryCard from './CurationEntryCard.svelte';
	import type { CurationEntryClass } from './CurationEntries';
	import AddCurationEntry from './AddCurationEntry.svelte';
	import { curationStore } from './stores';

	export let entries: CurationEntryClass[];
	export let groupName: string | undefined = undefined;

	const { editMode } = curationStore;

	groupName = groupName ?? entries[0].name;
</script>

{#if entries.length === 1}
	<CurationEntryCard entry={entries[0]} combined={true} tag="li" />
{:else}
	<li class="rounded border border-surface-400">
		<h2 class="mb-1 mt-2 px-2">
			<span class="font-semibold">{groupName}</span>
			<span class="text-sm text-surface-600">
				[{entries.length}
				{entries.length === 1 ? 'Entry' : 'Entries'}]
			</span>
		</h2>

		<ul class="my-2 flex flex-col gap-2">
			{#if $editMode}
				<AddCurationEntry
					position={entries[0].position}
					name={groupName}
					type={entries[0].type}
					tag="li"
					class="mx-2"
				/>
			{/if}
			{#each entries as entry}
				<CurationEntryCard {entry} combined={false} tag="li" />

				{#if $editMode}
					<AddCurationEntry
						position={entry.position + (entry.isDraft() ? 0 : 1)}
						name={entry.name}
						type={entry.type}
						tag="li"
						class="mx-2"
					/>
				{/if}
			{/each}
		</ul>
	</li>
{/if}

{#if $editMode}
	<AddCurationEntry
		position={entries[0].position + (entries[0].isDraft() ? 0 : 1)}
		type={entries[0].type}
		tag="li"
	/>
{/if}
