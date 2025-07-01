<script lang="ts">
	import AddCurationEntry from './AddCurationEntry.svelte';
	import type { CurationEntryClass } from './CurationEntries';
	import CurationGroupCard from './CurationGroupCard.svelte';
	import { curationStore } from './stores';
	import type { CurationEntryType } from './types';

	export let curationEntries: CurationEntryClass[];
	export let heading: string | undefined = undefined;
	export let combineNames = true;
	export let type: CurationEntryType | undefined = undefined;
	export let prevPosition: number | undefined = undefined;

	const { editMode } = curationStore;

	// sorted Entries is always a list of lists of CurationEntries
	let sortedEntries: CurationEntryClass[][] = combineNames
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
		<AddCurationEntry
			position={curationEntries.at(0)?.position ?? (prevPosition ? prevPosition + 1 : 2)}
			{type}
			tag="li"
		/>
		{#each sortedEntries as entryNameGroup}
			{#if $editMode || entryNameGroup.some((entry) => entry.isVisible())}
				<CurationGroupCard entries={entryNameGroup} />
			{/if}
		{/each}
		{#if curationEntries.some((entry) => entry.isVisible())}
			<AddCurationEntry position={curationEntries.at(-1)?.position} {type} tag="li" />
		{/if}
	</ul>
{/if}

<style lang="postcss">
</style>
