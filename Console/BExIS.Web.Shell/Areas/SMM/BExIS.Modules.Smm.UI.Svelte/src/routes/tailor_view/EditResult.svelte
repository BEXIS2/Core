<script lang="ts">
	import { onMount } from "svelte";
    import { tailorCleanedStore, type TailorResultRow } from "./data";
	import { getModalStore } from "@skeletonlabs/skeleton";

	export let row: TailorResultRow;
	const modalStore = getModalStore();
	let editedValue: string = row.editedName;

	onMount(() => {
		if (!editedValue) {
			// Logic: Use cleanedName if it exists, otherwise fallback to originalName
			editedValue = row.cleanedName || row.originalName;
		}
	});

	function handleSave() {
		console.log("Handling save with: ", editedValue);
		tailorCleanedStore.update(currentData => {
			return currentData.map(item => 
				item.id === row.id 
				? { ...item, editedName: editedValue } 
				: item
			);
		});

		modalStore.close();
	}
</script>

<div class="p-5 rounded-lg bg-white grid gap-2">

	<label for="originalName">Original name</label>
	<div id="originalName">{row.originalName}</div>
	<label for="cleanedName">Cleaned name</label>
	<div id="cleanedName">{row.cleanedName}</div>
	<label for="editedName">Edit name</label>
	<input type="text" id="editedName" class="input input-primary" bind:value={editedValue} />
	<div class="flex gap-2 justify-end">
		<button class="btn variant-filled-error" on:click={() => modalStore.close()}>Cancel</button>
		<button class="btn variant-filled-success" on:click={handleSave}>Done</button>
	</div>
</div>