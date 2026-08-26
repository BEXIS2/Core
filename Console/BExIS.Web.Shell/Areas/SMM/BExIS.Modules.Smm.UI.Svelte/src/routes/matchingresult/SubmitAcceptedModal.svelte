<script lang="ts">
	import { onMount } from "svelte";
	import { getModalStore } from "@skeletonlabs/skeleton";
	import { type SpeciesMatchingRow } from "$lib/types/types";

	export let count: number;
	export let parent: any = undefined;

	const modalStore = getModalStore();

	function handleSave() {

		// send updated data back through the response handler
		if ($modalStore[0].response) {
			$modalStore[0].response(true);
		}

		modalStore.close();
	}
</script>

<div class="p-5 rounded-lg bg-white grid gap-2">

	<label for="originalName">Submit Accepted</label>
    <div>You are about to submit {count} accepted entries. Are you sure?</div>
	<div class="flex gap-2 justify-end">
		<button class="btn variant-filled-error" on:click={() => modalStore.close()}>Cancel</button>
		<button class="btn variant-filled-success" disabled={count == 0} on:click={handleSave}>Done</button>
	</div>
</div>