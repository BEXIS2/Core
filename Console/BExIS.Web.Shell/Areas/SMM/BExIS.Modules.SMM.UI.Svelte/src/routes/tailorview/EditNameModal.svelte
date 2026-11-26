<script lang="ts">
	import { onMount } from "svelte";
	import { getModalStore } from "@skeletonlabs/skeleton";
	import { type SpeciesMatchingRow } from "$lib/types/types";

	export let row: SpeciesMatchingRow;
	export let parent: any = undefined;

	const modalStore = getModalStore();
	let editedValue: string = row.editedName;
	let draft = { ...row };

	onMount(() => {
		if (!editedValue) {
			// Use cleanedName if it exists, otherwise fallback to originalName
			editedValue = row.cleanedName || row.originalName;
		}
	});

	function handleSave() {
		draft.editedName = editedValue;

		// send updated data back through the response handler
		if ($modalStore[0].response) {
			$modalStore[0].response(draft);
		}

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