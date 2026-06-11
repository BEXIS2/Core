<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner } from "@bexis2/bexis2-core-ui";
	import { Table } from '@bexis2/bexis2-core-ui';
    import { loadMatchingFileStatus, loadMatchingResult, requestResultFileDownload, submitAcceptedIds } from "./services";
    import { matchingSelection } from "$lib/stores/selectionStore";
    import type { AcceptMatchesRequest, GenericMatchingResult, MatchingFileStatus, SpeciesMatchingRow } from "$lib/types/types";
    import type { Columns, TableConfig } from "@bexis2/bexis2-core-ui";
    import AcceptedTableOptions from "./AcceptedTableOptions.svelte";
    import { resultStore, acceptedStore, mismatchStore, doneStore } from "./data";
	import ResultTableOptions from "./ResultTableOptions.svelte";
	import { get } from "svelte/store";
	import { onMount } from "svelte";
	import Fa from 'svelte-fa';
	import { faAngleDown } from '@fortawesome/free-solid-svg-icons';
	import { faAngleRight } from '@fortawesome/free-solid-svg-icons';

	let resultFileStatus: MatchingFileStatus;
	let statusLoaded: boolean = false;
	let resultFileExists: boolean = false;
	let pollInterval: number | undefined = undefined;

	let hideMismatches: boolean = true;
	let hideDone: boolean = true;

	// API specific acceptable match types (everything else is assumed to be a mismatch)
	let acceptableMatchTypes: Set<string> = new Set(["exact"])

	// TODO: - use for error handling and display
	let criticalError: boolean = false;

	let resultColumns: Columns = {
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
		}
	}

	let totalCount = 1000;
	let wipCount: number = 0;
	let mismatchCount: number = 0;
	let doneCount: number = 0;
	let wipPercent: number = 0.0;
	let mismatchPercent: number = 0.0;
	let donePercent: number = 0.0;

	/**
	 * Loads content of the currently selected result file for display.
	 * Once succesfully loaded, the user can continue the result confirmation.
	 */
    async function load(): Promise<GenericMatchingResult[]> {
        var response = await loadMatchingResult($matchingSelection.datasetId, $matchingSelection.versionId, $matchingSelection.stepId);
        if (!response.success) {
            throw new Error(response.error);
        } else {
            console.log(response.data);
			acceptableMatchTypes = new Set(response.data.acceptableMatchTypes);

			var responseData: GenericMatchingResult[] = response.data.matchingResults;

			const doneMap: Map<number, SpeciesMatchingRow> = new Map(response.data.speciesMatchingResults.map((row: SpeciesMatchingRow) => [row.id, row]));

			const workInProgressData: GenericMatchingResult[] = [];
			const mismatchData: GenericMatchingResult[] = [];
			const doneData: SpeciesMatchingRow[] = [];

			for (const row of responseData) {
				const mappedRow: GenericMatchingResult = {
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
				};

				if (doneMap.has(parseInt(row.original_ID))) {
					var doneRow = doneMap.get(parseInt(row.original_ID));
					if (doneRow && doneRow.confirmedByUser) {
						doneData.push(doneRow)
						continue;
					}
				}

				if (row.matchType && row.matchType !== "") {
					if (acceptableMatchTypes.has(row.matchType.toLowerCase())) {
						workInProgressData.push(mappedRow);
					} else {
						mismatchData.push(mappedRow);
					}
				} else {
					mismatchData.push(mappedRow);
				}
			}

			console.log(workInProgressData)
			console.log(mismatchData)
			console.log(doneData)

			// updating progress bar values
			totalCount = responseData.length;
			wipCount = workInProgressData.length;
			mismatchCount = mismatchData.length;
			doneCount = doneData.length;

			wipPercent = totalCount > 0 ? (wipCount / totalCount) * 100 : 0;
			mismatchPercent = totalCount > 0 ? (mismatchCount / totalCount) * 100 : 0;
			donePercent = totalCount > 0 ? (doneCount / totalCount) * 100 : 0;

            resultStore.update(() => {
                return workInProgressData;
            });

			mismatchStore.update(() => {
				return mismatchData;
			});

			var doneDataOrdered: SpeciesMatchingRow[] = doneData.map((row: any): SpeciesMatchingRow => 
            {
                return { 
                    id: row.id,
                    originalName: row.originalName,
                    editedName: row.editedName ?? "",
                    cleanedName: row.cleanedName ?? "",
                    confirmedByUser: row.confirmedByUser ?? false,
                    matchType: row.matchType ?? "",
                    matchedName: row.matchedName ?? "",
                    matchAuthorship: row.matchAuthorship ?? "",
                    status: row.status ?? "",
                    matchRank: row.matchRank ?? "",
                    acceptedScientificName: row.acceptedScientificName ?? "",
                    acceptedId: row.acceptedId ?? "",
                    acceptedAuthorship: row.acceptedAuthorship ?? "",
                    taxonKingdom: row.taxonKingdom ?? "",
                    taxonPhylum: row.taxonPhylum ?? "",
                    taxonClass: row.taxonClass ?? "",
                    taxonOrder: row.taxonOrder ?? "",
                    taxonFamily: row.taxonFamily ?? "",
                    taxonGenus: row.taxonGenus ?? "",
                    matchId: row.matchId ?? "",
                    matchSource: row.matchSource ?? "",
                    matchSourceVersion: row.matchSourceVersion ?? "",
                    timeStampMatch: row.timeStampMatch ?? ""
                }
            });

			doneStore.update(() => {
				return doneDataOrdered;
			});

            return response.data.data;
        }
    }

	/**
	 * Requests file status for the currently selected result file.
	 */
	async function loadFileStatus(): Promise<MatchingFileStatus> {
		var response = await loadMatchingFileStatus($matchingSelection.datasetId, $matchingSelection.versionId, $matchingSelection.stepId);
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
					const response = await requestResultFileDownload($matchingSelection.datasetId, $matchingSelection.versionId, $matchingSelection.stepId);
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
			datasetId: $matchingSelection.datasetId,
			versionId: $matchingSelection.versionId,
			stepId: $matchingSelection.stepId,
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
		columns: resultColumns,
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
		columns: resultColumns,
		optionsComponent: AcceptedTableOptions
	};

	const mismatchConfig: TableConfig<GenericMatchingResult> = {
		id: 'mismatchRows',
		data: mismatchStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,
		columns: resultColumns,
	};

	const doneConfig: TableConfig<SpeciesMatchingRow> = {
		id: 'doneRows',
		data: doneStore,
		resizable: "columns",
		height: 700,
		fitToScreen: false,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,
		columns: {
			id: {
                exclude: true
            },
            originalName: {
                header: "Original name"
            },
            editedName: {
                header: "Edited name"
            },
            matchedName: {
                header: "Matched name",
                // exclude: true
            },
            confirmedByUser: {
                disableFiltering: true,
                header: "Confirmed by user"
            },
            datasetVersionId: {
                header: "Dataset Version ID",
                exclude: true
            },
            status: {
                header: "Status",
                exclude: true
            },
            timeStampMatch: {
                header: "Match date",
                exclude: true
            },
            matchType: {
                header: "Match type",
                // exclude: true
            },
            matchRank: {
                header: "Match rank"
            },
            matchAuthorship: {
                header: "Match authorship"
            },
            acceptedScientificName: {
                header: "Accepted scientific name"
            },
            acceptedId: {
                header: "Accepted ID",
                exclude: true,
            },
            acceptedAuthorship: {
                header: "Accepted authorship",
            },
            taxonKingdom: {
                header: "Kingdom"
            },
            taxonPhylum: {
                header: "Phylum"
            },
            taxonClass: {
                header: "Class"
            },
            taxonOrder: {
                header: "Order"
            },
            taxonFamily: {
                header: "Family"
            },
            taxonGenus: {
                header: "Genus"
            },
            matchSource: {
                header: "Match source",
                // exclude: true
            },
            matchVersion: {
                header: "Match version",
                // exclude: true
            },
            matchSourceVersion: {
                header: "Match source version",
                // exclude: true
            },
		}
	}

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

    <p>Dataset with <b>ID:</b> {$matchingSelection.datasetId} <b>VerionNr:</b> {$matchingSelection.versionNr} <b>VersionID:</b> {$matchingSelection.versionId} <b>StepID:</b> {$matchingSelection.stepId}</p>

	{#if statusLoaded && resultFileExists}
		{#await load()}
			<Spinner textCss="text-surface-800" label="Loading content and preparing visualization"/>
		{:then data}
			<div class="w-full max-w-xl mx-auto my-6 space-y-3">
				<div class="grid grid-cols-2 gap-2 sm:flex sm:justify-between text-sm font-medium text-gray-700">
					<div class="flex items-center space-x-2">
						<span class="w-3 h-3 bg-emerald-500 rounded-full"></span>
						<span>Done ({doneCount})</span>
					</div>
					<div class="flex items-center space-x-2">
						<span class="w-3 h-3 bg-yellow-300 rounded-full"></span>
						<span>WIP ({wipCount})</span>
					</div>
					<div class="flex items-center space-x-2">
						<span class="w-3 h-3 bg-red-400 rounded-full"></span>
						<span>Mismatch ({mismatchCount})</span>
					</div>
					
					<div class="text-gray-400 sm:ml-auto">
						Total: {totalCount}
					</div>
					</div>

					<div class="w-full h-5 bg-gray-100 rounded-full overflow-hidden flex shadow-inner">


					<div 
						class="h-full bg-emerald-500 transition-all duration-300 ease-out"
						style="width: {donePercent}%"
						title="Mismatch"
					></div>

					<div 
						class="h-full bg-yellow-300 transition-all duration-300 ease-out"
						style="width: {wipPercent}%"
						title="Accepted"
					></div>

					<div 
						class="h-full bg-red-400 transition-all duration-300 ease-out"
						style="width: {mismatchPercent}%"
						title="Done"
					></div>
				</div>
			</div>

			<div class="text-5xl">
				Work in progress
			</div>
			<h4 class="h4 !mt-0">(Acceptable results from this matching step)</h4>
			<div class="flex items-center justify-center">
				<Table config={resultTableConfig} on:action={resultTableActions}/>
			</div>
		
			<div class="h-10"></div>
			<div class="text-5xl">
				Accepted
			</div>
			<h4 class="h4 !mt-0">(Will be stored on Submit)</h4>
			<div class="flex items-center justify-center">
				<Table config={acceptedConfig} on:action={acceptedTableActions}/>
			</div>

			<div class="h-10"></div>
			
			<div class="text-5xl">
				<button
				type="button"
				class="unstyled flex items-center gap-2 text-left"
				on:click={() => hideMismatches = !hideMismatches}
				>
					Mismatches
					<span class="inline-block items-center ml-2">
						{#if hideMismatches}
							<Fa icon={faAngleDown} />
						{:else}
							<Fa icon={faAngleRight} />
						{/if}
					</span>
				</button>
			</div>
			<h4 class="h4 !mt-0">(Can not be accepted)</h4>
			<div class="{hideMismatches ? 'hidden' : ''}">
				<div class="flex items-center justify-center">
					<Table config={mismatchConfig}/>
				</div>
	
			</div>
			<div class="h-10"></div>

			<div class="text-5xl">
				<button
				type="button"
				class="unstyled flex items-center gap-2 text-left"
				on:click={() => hideDone = !hideDone}
				>
					Done
					<span class="inline-block items-center ml-2">
						{#if hideDone}
							<Fa icon={faAngleDown} />
						{:else}
							<Fa icon={faAngleRight} />
						{/if}
					</span>
				</button>
			</div>
			<h4 class="h4 !mt-0">(Already accepted and stored previously)</h4>

			<div class="{hideDone ? 'hidden' : ''}">
				<div class="flex items-center justify-center">
					<Table config={doneConfig}/>
				</div>
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