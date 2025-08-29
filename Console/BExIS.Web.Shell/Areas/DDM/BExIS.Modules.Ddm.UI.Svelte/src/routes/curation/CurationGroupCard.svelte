<script lang="ts">
	import CurationEntryCard from './CurationEntryCard.svelte';
	import AddCurationEntry from './AddCurationEntry.svelte';
	import { curationStore } from './stores';
	import { get } from 'svelte/store';

	export let entryIds: number[];
	export let groupName: string;

	const entryReadables = entryIds
		.map((id) => curationStore.getEntryReadable(id))
		.filter((entry) => !!entry);

	$: entryValues = entryReadables && entryReadables.map((entry) => get(entry));

	const { editMode } = curationStore;
</script>

{#if entryIds.length === 1}
	<CurationEntryCard entryId={entryIds[0]} combined={true} tag="li" />
{:else}
	<li class="rounded border border-surface-400">
		<h2 class="mb-1 mt-2 px-2">
			<span class="font-semibold">{groupName}</span>
			<span class="text-sm text-surface-600">
				[{entryIds.length}
				{entryIds.length === 1 ? 'Entry' : 'Entries'}]
			</span>
		</h2>

		<ul class="my-2 flex flex-col gap-2">
			{#each entryValues as entry}
				{#if entry}
					<CurationEntryCard entryId={entry?.id} combined={false} tag="li" />
				{/if}
			{/each}
			{#if $editMode}
				<AddCurationEntry
					position={(entryValues.at(-1)?.position || 0) + (entryValues.at(-1)?.isDraft() ? 0 : 1)}
					name={groupName}
					type={entryValues[0]?.type}
					tag="li"
					class="mx-2"
				/>
			{/if}
		</ul>
	</li>
{/if}

{#if $editMode}
	<AddCurationEntry
		position={(entryValues.at(-1)?.position || 0) + (entryValues.at(-1)?.isDraft() ? 0 : 1)}
		type={entryValues[0]?.type}
		tag="li"
	/>
{/if}
