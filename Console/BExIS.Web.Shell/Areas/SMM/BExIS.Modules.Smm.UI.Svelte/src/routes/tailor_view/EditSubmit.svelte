<script lang="ts">
    import { type TailorResultRow } from "./data";
	import { getModalStore } from "@skeletonlabs/skeleton";
	import { submitTailorEdits } from "./services";
    import { mappingSelection } from '../../lib/stores/selectionStore';
    import { type TailorEdit, type TailorEditsRequest } from "./types";

	export let changedRows: TailorResultRow[];
	const modalStore = getModalStore();

    function createEditsPayload(): TailorEdit[] {
        const edits: TailorEdit[] = changedRows.map(row => ({
            id: row.id,
            originalName: row.originalName,
            editedName: row.editedName,
            cleanedName: row.cleanedName
        }));

        return edits;
    }

    async function handleSubmit() {
        var payload = createEditsPayload();

        const response = await submitTailorEdits($mappingSelection.datasetId, $mappingSelection.versionId, payload);

		if (!response.success) {
            console.log("ERROR");
        } else {
            console.log("SUCCESS");
			console.log(response);
        }
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