<script lang="ts">
	import Fa from 'svelte-fa';
	import AddCurationEntry from './AddCurationEntry.svelte';
	import type { CurationEntryClass } from './CurationEntries';
	import CurationGroupCard from './CurationGroupCard.svelte';
	import { curationStore } from './stores';
	import type { CurationEntryType } from './types';
	import { faBan } from '@fortawesome/free-solid-svg-icons';

	export let curationEntries: CurationEntryClass[];
	export let heading: string | undefined = undefined;
	export let combineNames = true;
	export let type: CurationEntryType | undefined = undefined;

	const { editMode } = curationStore;

	// sorted Entries is always a list of lists of CurationEntries
	$: sortedEntries = combineNames
		? curationEntries.reduce((acc, entry) => {
				if (!acc.length || acc[acc.length - 1][0].name !== entry.name) acc.push([entry]);
				else acc[acc.length - 1].push(entry);
				return acc;
			}, [] as CurationEntryClass[][])
		: curationEntries.map((entry) => [entry]);
</script>

{#if heading}
	<h2 class="m-2 mt-3 text-xl font-semibold">{heading}</h2>
{/if}
{#if curationEntries.some((entry) => entry.isVisible()) || $editMode}
	<ul class="flex flex-col gap-2 overflow-hidden p-2">
		{#if $editMode}
			<AddCurationEntry position={curationEntries.at(0)?.position} {type} tag="li" />
		{/if}
		{#each sortedEntries as entryNameGroup (entryNameGroup.map((e) => e.id).join(' '))}
			{#if $editMode || entryNameGroup.some((entry) => entry.isVisible())}
				<CurationGroupCard entries={entryNameGroup} />
			{/if}
		{/each}
	</ul>
{:else}
	<div class="flex h-16 w-full items-center justify-center">
		<span class="m-auto text-surface-700">
			<Fa icon={faBan} class="inline-block" />
			No entries
		</span>
	</div>
{/if}

<style lang="postcss">
</style>
