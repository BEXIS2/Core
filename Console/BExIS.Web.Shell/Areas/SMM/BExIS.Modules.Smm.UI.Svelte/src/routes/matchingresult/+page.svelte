<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner } from "@bexis2/bexis2-core-ui";
	import { Table } from '@bexis2/bexis2-core-ui';
    import { loadMatchingFileStatus, loadMatchingResult, requestResultFileDownload, submitAcceptedIds } from "./services";
    import { mappingSelection } from "$lib/stores/selectionStore";
    import type { AcceptMatchesRequest, GenericMatchingResult, MatchingFileStatus } from "$lib/types/types";
    import type { TableConfig } from "@bexis2/bexis2-core-ui";
    import AcceptedTableOptions from "./AcceptedTableOptions.svelte";
    import { resultStore, acceptedStore } from "./data";
	import ResultTableOptions from "./ResultTableOptions.svelte";
	import { get } from "svelte/store";
	import { onMount } from "svelte";

	let resultFileStatus: MatchingFileStatus;
	let statusLoaded: boolean = false;
	let resultFileExists: boolean = false;
	let pollInterval: number | undefined = undefined;

	// TODO: - use for error handling and display
	let criticalError: boolean = false;

	/**
	 * Loads content of the currently selected result file for display.
	 * Once succesfully loaded, the user can continue the result confirmation.
	 */
    async function load(): Promise<GenericMatchingResult[]> {
        var response = await loadMatchingResult($mappingSelection.datasetId, $mappingSelection.versionId, $mappingSelection.stepId);
        if (!response.success) {
            throw new Error(response.error);
        } else {
            console.log(response.data);
			var responseData: GenericMatchingResult[] = response.data.data;
			var orderedTableData = responseData.map((row: any): GenericMatchingResult => {
				return {
					original_ID: row.original_ID,
					original_scientificName: row.original_scientificName,
					scientificName: row.scientificName,
					matchType: row.matchType,
					acceptedScientificName: row.acceptedScientificName,
					original_rank: row.original_rank,
					rank: row.rank,
					original_kingdom: row.original_kingdom,
					original_authorship: row.original_authorship,
					authorship: row.authorship,
					acceptedAuthorship: row.acceptedAuthorship,
					matchIssues: row.matchIssues,
					status: row.status,
					id: row.id,
					acceptedID: row.acceptedID,
					kingdom: row.kingdom,
					phylum: row.phylum,
					class: row.class,
					order: row.order,
					family: row.family,
					genus: row.genus,
					classification: row.classification,
				}
			});

            resultStore.update(() => {
                return orderedTableData;
            });
            return response.data.data;
        }
    }

	/**
	 * Requests file status for the currently selected result file.
	 */
	async function loadFileStatus(): Promise<MatchingFileStatus> {
		var response = await loadMatchingFileStatus($mappingSelection.datasetId, $mappingSelection.versionId, $mappingSelection.stepId);
		if (!response.success) {
			throw new Error(response.error);
		} else {
			console.log(response.data);
			return response.data.data;
		}
	}

	/**
	 * Starts polling for file periodically until it's ready or some fail state is reached.
	 */
	async function startPollingFile(): Promise<void> {
		stopPolling();

		const isFileReady: boolean = await checkFileReady();

		if (isFileReady) {
			resultFileExists = true;
			return;
		}

		pollInterval = window.setInterval(async () => {
			const isFileReady: boolean = await checkFileReady();
			// TODO: - define fail state and max polling iteration

			if (isFileReady) {
				stopPolling();
				resultFileExists = true;
			}
		}, 3000);
	}

	/**
	 * Requests result file status and returns if the result file already exists.
	 */
	async function checkFileReady(): Promise<boolean> {
		const status = await loadFileStatus();
		resultFileStatus = status;

		return status.fileExists;
	}

	/**
	 * Stops file polling.
	 */
	function stopPolling(): void {
		if (pollInterval !== undefined) {
			window.clearInterval(pollInterval);
			pollInterval = undefined;
		}
	}

	/**
	 * On mount, check for file status and if necessary start result file download and polling for file.
	*/
	onMount(async () => {
		const status = await loadFileStatus();
		resultFileStatus = status;
		resultFileExists = resultFileStatus.fileExists;
		
		if (!status.fileExists) {
			// Result file does not exist
			if (status.downloadLinkPresent && status.jobKeyPresent && !status.markerStale) {
				// file can in theory be downloaded or is still downloading
				if (!status.markerExists) {
					// request file download and poll for file
					const response = await requestResultFileDownload($mappingSelection.datasetId, $mappingSelection.versionId, $mappingSelection.stepId);
					if (response.success) {
						startPollingFile();
					} else {
						// start download request failed (critical)
						criticalError = true;
						console.error("Critical error:");
						console.error(response.error);
					}
				} else {
					// file is already currently being downloaded
					// start polling for file
					startPollingFile();
				}

			} else {
				// file does not exist and can not be downloaded (or download frozen/aborted)
				// -> the user should not be here
				criticalError = true;
				console.error("Critical error 2");
			}
		}

		// toggles svelte reactive await block (html)
		statusLoaded = true;
	});
	
	/**
	 * Submits a list of user confirmed IDs which will then be marked as confirmed in the database.
	 */
    async function submitAccepted() {
		const payload = getAcceptedMatchIdsPayload();
		const response = await submitAcceptedIds(payload);

		if (!response.success) {
            console.log(response);
        } else {
			console.log(response);
        }
    }

	/**
	 * Gathers all accepted original_ID(s) and forms a payload for the submit request.
	 */
	function getAcceptedMatchIdsPayload(): AcceptMatchesRequest {
		return {
			datasetId: $mappingSelection.datasetId,
			versionId: $mappingSelection.versionId,
			stepId: $mappingSelection.stepId,
			matchIds: getMatchIds()
		}
	}

	/**
	 * Returns all accepted original_ID(s).
	 */
	function getMatchIds(): (string | undefined)[] {
		const items = get(acceptedStore);

		return items.map(item => item.original_ID);
	}

	const resultTableConfig: TableConfig<GenericMatchingResult> = {						
		id: 'resultRows',						
		data: resultStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,					
		columns: {
			original_ID: {
				exclude: true
			},
			original_scientificName: {
				header: "Original scientificname"
			},
			original_rank: {
				header: "Original rank"
			},
			original_kingdom: {
				header: "Original kingdom"
			},
			original_authorship: {
				header: "Original authorship"
			},
			matchType: {
				header: "Match type"
			},
			matchIssues: {
				header: "Match issues"
			},
			id: {
				header: "Match ID"
			},
			acceptedID: {
				header: "Accepted ID"
			},
			acceptedScientificName: {
				header: "Accepted scientificname"
			},
			acceptedAuthorship: {
				header: "Accepted authorship"
			},
            classification: {
                exclude: true,
            },
		},
		optionsComponent: ResultTableOptions
	};

	const acceptedConfig: TableConfig<GenericMatchingResult> = {
		id: 'acceptedRows',
		data: acceptedStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,
		columns: {
			original_ID: {
				exclude: true
			},
			original_scientificName: {
				header: "Original scientificname"
			},
			original_rank: {
				header: "Original rank"
			},
			original_kingdom: {
				header: "Original kingdom"
			},
			original_authorship: {
				header: "Original authorship"
			},
			matchType: {
				header: "Match type"
			},
			matchIssues: {
				header: "Match issues"
			},
			id: {
				header: "Match ID"
			},
			acceptedID: {
				header: "Accepted ID"
			},
			acceptedScientificName: {
				header: "Accepted scientificname"
			},
			acceptedAuthorship: {
				header: "Accepted authorship"
			},
            classification: {
                exclude: true,
            },
		},
		optionsComponent: AcceptedTableOptions
	};

    const resultTableActions = (action: CustomEvent<{ row: GenericMatchingResult; type: string }>) => {
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

	const acceptedTableActions = (action: CustomEvent<{ row: GenericMatchingResult; type: string }>) => {
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

    <p>Dataset with <b>ID:</b> {$mappingSelection.datasetId} <b>VerionNr:</b> {$mappingSelection.versionNr} <b>VersionID:</b> {$mappingSelection.versionId} <b>StepID:</b> {$mappingSelection.stepId}</p>

	{#if statusLoaded && resultFileExists}
		{#await load()}
			<Spinner textCss="text-surface-800" label="Loading content and preparing visualization"/>
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
	{:else}
		{#if !statusLoaded}
			<Spinner textCss="text-surface-800" label="Fetching result file status" />
		{:else}
			{#if !resultFileExists}
				<Spinner textCss="text-surface-800" label="Downloading Result File" />
			{:else}
				<Spinner textCss="text-surface-800" label="Preparing Result File visualization" />
			{/if}
		{/if}
	{/if}

	<div class="h-80"></div>
</Page>