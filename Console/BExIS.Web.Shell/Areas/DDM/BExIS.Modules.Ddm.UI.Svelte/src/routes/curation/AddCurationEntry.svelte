<script lang="ts">
	import { faSquarePlus } from '@fortawesome/free-solid-svg-icons';
	import { curationStore } from './stores';
	import Fa from 'svelte-fa';
	import { CurationEntryType } from './types';
	import { slide } from 'svelte/transition';

	export let tag: string | undefined = 'div'; // if set, use this tag instead of the default <div>

	let className: string | undefined = undefined;
	export { className as class }; // if set, use this class(es) on the element

	export let position: number = 1; // if set, use this position instead of the default 1
	export let name: string = ''; // if set, use this name instead of the default empty string
	export let type: CurationEntryType = CurationEntryType.None; // if set, use this type instead of the default CurationEntryType.None

	const { editMode } = curationStore;

	const addNewEntry = () => {
		curationStore.addEmptyEntry(position, name, type);
	};
</script>

<svelte:element
	this={tag}
	class={className}
	in:slide={{ duration: 150 }}
	out:slide={{ duration: 150 }}
>
	<button
		class="btn size-full overflow-hidden border-2 border-dashed border-surface-500 px-2 py-0.5 text-surface-600"
		on:click={addNewEntry}
		title="Add new entry at position {position}"
		disabled={!$editMode}
		class:hidden={!$editMode}
	>
		<span>
			<Fa icon={faSquarePlus} class="mr-1 inline-block" />
			Add new entry
		</span>
	</button>
</svelte:element>
