<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner } from "@bexis2/bexis2-core-ui";
	import { Table } from '@bexis2/bexis2-core-ui';
    import { loadMatchingResult, submitAcceptedIds } from "./services";
    import { mappingSelection } from "$lib/stores/selectionStore";
    import type { AcceptMatchesRequest, CLBMatchingResult } from "$lib/types/types";
    import type { TableConfig } from "@bexis2/bexis2-core-ui";
    import AcceptedTableOptions from "./AcceptedTableOptions.svelte";
    import { resultStore, acceptedStore } from "./data";
	import ResultTableOptions from "./ResultTableOptions.svelte";
	import { get } from "svelte/store";

    async function load(): Promise<CLBMatchingResult[]> {
        // TODO: stepId
        var response = await loadMatchingResult($mappingSelection.datasetId, $mappingSelection.versionId, 0);
        if (!response.success) {
            throw new Error(response.error);
        } else {
            console.log(response.data);
            resultStore.update(() => {
                return response.data?.data;
            });
            return response.data.data;
        }
    }

    async function submitAccepted() {
		const payload = getAcceptedMatchIdsPayload();
		const response = await submitAcceptedIds(payload);

		if (!response.success) {
            console.log(response);
        } else {
			console.log(response);
        }
    }

	function getAcceptedMatchIdsPayload(): AcceptMatchesRequest {
		return {
			datasetId: $mappingSelection.datasetId,
			versionId: $mappingSelection.versionId,
			stepId: $mappingSelection.stepId,
			matchIds: getMatchIds()
		}
	}

	function getMatchIds(): string[] {
		const items = get(acceptedStore);

		return items.map(item => item.original_ID);
	}

	const resultTableConfig: TableConfig<CLBMatchingResult> = {						
		id: 'resultRows',						
		data: resultStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,					
		columns: {
            classification: {
                exclude: true,
            }
		},
		optionsComponent: ResultTableOptions
	};

	const acceptedConfig: TableConfig<CLBMatchingResult> = {
		id: 'acceptedRows',
		data: acceptedStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,
		columns: {
            classification: {
                exclude: true,
            }
		},
		optionsComponent: AcceptedTableOptions
	};

    const resultTableActions = (action: CustomEvent<{ row: CLBMatchingResult; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'ACCEPT':
				resultStore.update(items => items.filter(i => i.original_ID !== row.original_ID));
				acceptedStore.update(items => [...items, row]);
				break;

			default:
				break;
		}
	};

	const acceptedTableActions = (action: CustomEvent<{ row: CLBMatchingResult; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'REMOVE':
				acceptedStore.update(items => items.filter(i => i.original_ID !== row.original_ID));
				resultStore.update(items => [...items, row]);
				break;

			default:
				break;
		}
	};
    
</script>
<Page 
	title="Species" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>

    {#await load()}
        <Spinner />
    {:then data} 
        <h2 class="h2">Result</h2>
        <div class="flex items-center justify-center">
            <Table config={resultTableConfig} on:action={resultTableActions}/>
        </div>
	
        <div class="h-10"></div>
        
        <h2 class="h2">Accepted</h2>
        <div class="flex items-center justify-center">
            <Table config={acceptedConfig} on:action={acceptedTableActions}/>
        </div>

        <div class="h-4"></div>

        <div class="flex items-center justify-center">
            <button class="btn variant-filled-secondary" on:click|preventDefault={submitAccepted}>Submit</button>
        </div>
    {/await}

	<div class="h-80"></div>
</Page>