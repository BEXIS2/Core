<script lang="ts">
	import { getModalStore } from "@skeletonlabs/skeleton";
	import { submitTailorEdits } from "./services";
    import { matchingSelection } from '../../lib/stores/selectionStore';
    import { type TailorEdit } from "./types";
	import { goto } from "$app/navigation";
    import { type SpeciesMatchingRow } from "$lib/types/types";

	export let changedRows: SpeciesMatchingRow[];
	const modalStore = getModalStore();

    function createEditsPayload(): TailorEdit[] {
        const edits: TailorEdit[] = changedRows.map(row => ({
            id: row.postgres_id,
            originalName: row.originalName,
            editedName: row.editedName,
            cleanedName: row.cleanedName
        }));

        return edits;
    }

    async function handleSubmit() {
        var payload = createEditsPayload();

        const response = await submitTailorEdits($matchingSelection.datasetId, $matchingSelection.versionId, payload);

		if (!response.success) {
            // TODO: - handling
        } else {
			goto('/progress_overview');
        }

        modalStore.close();
    }
</script>

<div class="p-5 rounded-lg bg-white grid gap-2">

	<label for="originalName">Changes detected</label>
	<div id="changesText">You are about to submit <b>{changedRows.length}</b> changed rows. Are you sure you want to continue?</div>
	<div class="flex gap-2 justify-end">
		<button class="btn variant-filled-error" on:click={() => modalStore.close()}>Cancel</button>
		<button class="btn variant-filled-success" disabled={changedRows.length <= 0} on:click={handleSubmit}>Submit</button>
	</div>
</div>