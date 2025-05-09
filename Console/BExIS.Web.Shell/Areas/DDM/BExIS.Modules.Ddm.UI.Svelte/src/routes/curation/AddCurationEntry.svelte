<script lang="ts">
	import { faSquarePlus } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import Fa from 'svelte-fa';

	export let position: number;

	const { editMode } = curationStore;

	const addNewEntry = () => {
		// curationStore.addNewEntry(position);
		curationStore.addEmptyEntry(position);
	};
</script>

{#if $editMode}
	<li class="flex">
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
