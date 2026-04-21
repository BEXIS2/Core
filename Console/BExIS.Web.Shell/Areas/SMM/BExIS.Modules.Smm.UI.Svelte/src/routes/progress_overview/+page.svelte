<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner, type TableConfig } from "@bexis2/bexis2-core-ui";
    import { mappingSelection } from '../../lib/stores/selectionStore';
    import { loadDatasetProgress, tailorDataset, genNewMatchFile, matchNextFile } from "./services";
    import type { ProgressOverview } from "./types";
    import type { StepEntry } from "$lib/types/types";
	import { goto } from "$app/navigation";
    import { Alert } from "@bexis2/bexis2-core-ui";
    import { matchingJobStore } from "./data";
    import TableOptions from "./TableOptions.svelte";
    import { Table } from '@bexis2/bexis2-core-ui';
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';

    let tailorLoading: boolean = false;
    let tailorError: boolean = false;
    let tailorErrorMessage: string = "";
	const modalStore = getModalStore();

    const tableConfig: TableConfig<StepEntry> = {						
		id: 'matchingJobRows',
		data: matchingJobStore,
		resizable: "columns",
		height: 700,
		fitToScreen: true,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,
        columns: {
            done: {
                disableFiltering: true
            },
            inputFileName: {
                exclude: true
            },
            resultFileName: {
                exclude: true
            }
        },
		optionsComponent: TableOptions
	};

    async function loadProgress(): Promise<ProgressOverview> {
        var response = await loadDatasetProgress($mappingSelection.datasetId, $mappingSelection.versionId);
        if (!response.success) {
            throw new Error(response.error);
        } else {
            console.log(response.data);
            matchingJobStore.update(() => {
                return response.data?.mappingProgress?.steps;
            });
            return response.data;
        }
    }

	const tableActions = (action: CustomEvent<{ row: StepEntry; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'READ':
				break;

			default:
				break;
		}
	};

    async function handleTailor() {
        tailorLoading = true;
        const response = await tailorDataset($mappingSelection.datasetId, $mappingSelection.versionId);

        if (response.success) {
            console.log(response);
            goto("/tailor_view");
        } else {
            console.log(response)
            tailorErrorMessage = response.error;
            tailorError = true;
        }
        tailorLoading = false;
    }

    async function requestFileAndMatching() {
        // TODO: apiIdentifier handling
        const responseCreate = await genNewMatchFile($mappingSelection.datasetId, $mappingSelection.versionId, "CLB");
        if (!responseCreate.success) {
           console.error("Error generating new Matching input file.");
           console.log(responseCreate);
           return;
        }
        console.log(responseCreate);

        // const responseMatch = await matchNextFile($mappingSelection.datasetId, $mappingSelection.versionId);
        
        // if (!responseMatch.success) {
        //     console.error("Error generating new Matching input file.");
        //     console.log(responseMatch);
        //     return;
        // }
        // console.log(responseMatch);
    }

</script>

<Page 
	title="Progress Overview" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>
    <h2 class="h2">Progress Overview</h2>

    <p>This page shows your current mapping progress for the selected Dataset with ID: {$mappingSelection.datasetId}</p>

    {#if tailorError}
        <Alert cssClass="variant-filled-error">
            {tailorErrorMessage}
        </Alert>
    {/if}

    {#await loadProgress()}
        <Spinner />
    {:then data}
        {#if !data.hasHeaderMappings}
            <p>This dataset does not seem to be initialized. Please go back to the Datasets Overview and start from scratch.</p>
        {:else}
            <p>The dataset has {data.headerMappings.mappings.length} mapped columns.</p>

            {#if !data.isTailored}
                {#if tailorLoading}
                    <Spinner />
                {/if}
                <div class="flex items-center justify-center">
                    <button class="btn variant-filled-secondary" on:click|preventDefault={handleTailor}>Tailor Dataset</button>
                </div>
            {:else}
                {#if !data.hasMappingProgress}
                    <p>No mapping progress data available. Something went wrong.</p>
                {:else}
                    {#if true || data.mappingProgress.steps.length == 0}
                        <p>For this dataset, no matching request have been done to external APIs. Feel free to check/edit the current state or begin matching.</p>
                        <div class="flex items-center justify-center gap-x-2">
                            <button class="btn variant-filled-secondary" on:click|preventDefault={() => goto("/tailor_view")}>View State</button>
                            <button class="btn variant-filled-secondary" on:click|preventDefault={requestFileAndMatching}>Request matching</button>
                        </div>
                    {:else}
                        <h3 class="h3">Your matching jobs</h3>

                        <div class="flex items-center justify-center">
                            <Table config={tableConfig} on:action={tableActions}/>
                            <Modal />
                        </div>
                    {/if}
                {/if}
            {/if}
        {/if}
    {:catch error}
        <Alert cssClass="variant-filled-error">
            {error.message}
        </Alert>
    {/await}

    
    <div class="h-80"></div>

</Page>