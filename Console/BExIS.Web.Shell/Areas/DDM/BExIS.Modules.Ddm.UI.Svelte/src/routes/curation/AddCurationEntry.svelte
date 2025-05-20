<script lang="ts">
	import { faSquarePlus } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import Fa from 'svelte-fa';
	import { CurationEntryType } from './types';

	let className: string | undefined = undefined;
	export { className as class };

	export let position: number | undefined = undefined;
	export let name: string | undefined = undefined;
	export let type: CurationEntryType | undefined = undefined;

	position = position ?? 1;
	name = name ?? '';
	type = type ?? CurationEntryType.None;

	const { editMode } = curationStore;

	const addNewEntry = () => {
		// curationStore.addNewEntry(position);
		curationStore.addEmptyEntry(position, name, type);
	};
</script>

{#if $editMode}
	<li class={className} class:flex={true}>
		<button
			class="mx-2 grow rounded border-2 border-dashed border-surface-300 px-2 text-surface-400 hover:border-surface-500 hover:bg-surface-300 hover:text-surface-600 focus-visible:bg-surface-300 focus-visible:text-surface-800 active:bg-surface-400"
			on:click={addNewEntry}
			title="Add new entry at position {position}"
		>
			<Fa icon={faSquarePlus} class="mr-1 inline-block" />
			Add new entry
		</button>
	</li>
{/if}
