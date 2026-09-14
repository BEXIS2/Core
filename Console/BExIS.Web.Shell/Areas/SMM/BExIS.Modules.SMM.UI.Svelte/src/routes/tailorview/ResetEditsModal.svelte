<script lang="ts">
	import { getModalStore } from "@skeletonlabs/skeleton";

	const modalStore = getModalStore();

    async function handleReset() {
		if ($modalStore[0].response) {
			$modalStore[0].response(true);
		}

        modalStore.close();
    }
</script>

<div class="p-5 rounded-lg bg-white grid gap-2">
	<div id="changesText">You are about to RESET ALL edited rows. Are you SURE you want to continue?</div>
	<div class="flex gap-2 justify-end">
		<button class="btn variant-filled-error" on:click={() => modalStore.close()}>Cancel</button>
		<button class="btn variant-filled-success" on:click={handleReset}>Reset All</button>
	</div>
</div>