<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner, type TableConfig } from "@bexis2/bexis2-core-ui";
    import { matchingSelection } from '../../lib/stores/selectionStore';
    import { loadDatasetProgress, tailorDataset, genNewMatchFile, matchNextFile } from "./services";
    import type { ProgressOverview } from "./types";
    import type { ExternalApiMetadata, IApiOptions, StepEntry } from "$lib/types/types";
	import { goto } from "$app/navigation";
    import { Alert } from "@bexis2/bexis2-core-ui";
    import { matchingJobStore } from "./data";
    import TableOptions from "./TableOptions.svelte";
    import { Table } from '@bexis2/bexis2-core-ui';
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import DownloadLinkCell from "./downloadLinkCell.svelte";
	import JobKeyCell from "./jobKeyCell.svelte";
	import ApiMatchingSelector from "./ApiMatchingSelector.svelte";

    let tailorLoading: boolean = false;
    let tailorError: boolean = false;
    let tailorErrorMessage: string = "";
    let externalApiMetadata: ExternalApiMetadata
    let selectedApiOptions: IApiOptions
	const modalStore = getModalStore();

    const tableConfig: TableConfig<StepEntry> = {						
		id: 'matchingJobRows',
		data: matchingJobStore,
		resizable: "columns",
		height: 300,
		fitToScreen: true,
		defaultPageSize: 20,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,
        columns: {
            id: {
                exclude: true
            },
            done: {
                disableFiltering: true,
                header: "Done"
            },
            inputFileName: {
                exclude: true
            },
            resultFileName: {
                exclude: true
            },
            numRows: {
                header: "#Rows"
            },
            // matchSource: {
            //     header: "Match source"
            // },
            timeStamp: {
                header: "Timestamp"
            },
            jobKey: {
                header: "Job key",
                instructions: {
                    renderComponent: JobKeyCell
                },
            },
            downloadLink: {
                header: "Download link",
                instructions: {
                    renderComponent: DownloadLinkCell
                },
            }
        },
		optionsComponent: TableOptions
	};

    async function load(): Promise<ProgressOverview> {
        var response = await loadDatasetProgress($matchingSelection.datasetId, $matchingSelection.versionId);
        if (!response.success) {
            throw new Error(response.error);
        } else {
            console.log(response.data);
            matchingJobStore.update(() => {
                return response.data?.matchingProgress?.steps;
            });
            return response.data;
        }
    }

	const tableActions = (action: CustomEvent<{ row: StepEntry; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'READ':
                matchingSelection.update(s => ({ ...s, stepId: row.id }));
                goto("/matchingresult");
				break;

			default:
				break;
		}
	};

    async function handleTailor() {
        // TODO: redirecting variable, to hide original page when finishing tailor
        tailorLoading = true;
        const response = await tailorDataset($matchingSelection.datasetId, $matchingSelection.versionId);

        if (response.success) {
            console.log(response);
            goto("/tailorview");
        } else {
            console.log(response)
            tailorErrorMessage = response.error;
            tailorError = true;
        }
        tailorLoading = false;
    }

    async function requestFileAndMatching() {
        // TODO: apiIdentifier handling
        const responseCreate = await genNewMatchFile($matchingSelection.datasetId, $matchingSelection.versionId, "CLB");
        if (!responseCreate.success) {
           console.error("Error generating new Matching input file.");
           console.log(responseCreate);
           return;
        }
        console.log(responseCreate);
        var stepId: number = responseCreate.data.data.stepId;

        if (stepId == undefined || stepId == null || stepId < 0) {
            console.error("Variable stepId could not be read from genNewMatchFile response: ", stepId);
            return;
        }


        console.log("Selected Api Options: ");
        console.log(selectedApiOptions)

        const responseMatch = await matchNextFile($matchingSelection.datasetId, $matchingSelection.versionId, stepId, selectedApiOptions);
        
        if (!responseMatch.success) {
            console.error("Error generating new Matching input file.");
            console.log(responseMatch);
            return;
        }
        console.log(responseMatch);
        matchingSelection.update(s => ({ ...s, stepId: responseMatch.data.data?.stepId }));
    }

</script>

<Page 
	title="Progress Overview" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>

    <div class="flex items-center gap-x-1">
		<button class="btn variant-filled-primary cursor-default p-2 py-1 text-sm"><b>Dataset ID {$matchingSelection.datasetId}</b></button>
		<button class="btn variant-filled-success cursor-default p-2 py-1 text-sm"><b>Verion Nr {$matchingSelection.versionNr}</b></button>
		<button class="btn variant-filled-primary cursor-default p-2 py-1 text-sm"><b>Version ID {$matchingSelection.versionId}</b></button>
	</div>

    <h2 class="h2">Progress Overview</h2>

    {#if tailorError}
        <Alert cssClass="variant-filled-error">
            {tailorErrorMessage}
        </Alert>
    {/if}

    {#await load()}
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
                {#if !data.hasMatchingProgress}
                    <p>No matching progress data available. Something went wrong.</p>
                {:else}
                    {#if data.matchingProgress.steps.length == 0}
                        <p>For this dataset, no matching request have been done to external APIs. Feel free to check/edit the current state or begin matching.</p>
                        <div class="flex items-center justify-center gap-x-2">
                            <button class="btn variant-filled-secondary" on:click|preventDefault={() => goto("/tailorview")}>View State</button>
                            <button class="btn variant-filled-secondary" on:click|preventDefault={requestFileAndMatching}>Request matching</button>
                        </div>
                    {:else}
                        <h3 class="h3">Your matching jobs</h3>

                        <div class="flex items-center justify-center">
                            <Table config={tableConfig} on:action={tableActions}/>
                            <Modal />
                        </div>
                    {/if}

                    <ApiMatchingSelector externalApiMetadata={data.externalApiMetadata} bind:selectedOptions={selectedApiOptions}></ApiMatchingSelector>
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