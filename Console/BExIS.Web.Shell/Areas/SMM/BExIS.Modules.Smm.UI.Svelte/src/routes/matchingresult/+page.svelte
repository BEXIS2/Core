<script lang="ts">
	import { Page, pageContentLayoutType, Spinner, ClientDB, Table } from "@bexis2/bexis2-core-ui";
    import type { Columns, TableConfig } from "@bexis2/bexis2-core-ui";
    import { loadMatchingFileStatus, loadMatchingResult, requestResultFileDownload, submitAcceptedIds } from "./services";
    import { matchingSelection } from "$lib/stores/selectionStore";
    import type { AcceptMatchesRequest, GenericMatchingResult, MatchingFileStatus, SpeciesMatchingRow } from "$lib/types/types";
    import AcceptedTableOptions from "./AcceptedTableOptions.svelte";
    import { resultStore, acceptedStore, mismatchStore, doneStore } from "./data";
	import ResultTableOptions from "./ResultTableOptions.svelte";
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import { onMount } from "svelte";
	import Fa from 'svelte-fa';
    import { writable } from "svelte/store";
	import { faAngleDown } from '@fortawesome/free-solid-svg-icons';
	import { faAngleRight } from '@fortawesome/free-solid-svg-icons';
	import SubmitAcceptedModal from "./SubmitAcceptedModal.svelte";
	import { goto } from "$app/navigation";

	const modalStore = getModalStore();

	let resultFileStatus: MatchingFileStatus;
	let statusLoaded: boolean = false;
	let resultFileExists: boolean = false;
	let pollInterval: number | undefined = undefined;

	let hideMismatches: boolean = true;
	let hideDone: boolean = true;

	// API specific acceptable match types (everything else is assumed to be a mismatch)
	let acceptableMatchTypes: Set<string> = new Set(["exact"])

	// unique match types actually occuring in the response data
	let uniqueMatchTypes: (string|undefined)[];

	let submittingEntries: boolean = false;

	// TODO: - use for error handling and display
	let criticalError: boolean = false;

	let resultColumns: Columns = {
		original_ID: {
			exclude: true
		},
		__id: {
			exclude: true
		},
		original_scientificName: {
			header: "Original scientificname"
		},
		scientificName: {
			header: "Scientific name"
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
			header: "Match issues",
			exclude: true
		},
		acceptedScientificName: {
			header: "Accepted scientificname"
		},
		rank: {
			header: "Rank"
		},
		authorship: {
			header: "Authorship"
		},
		acceptedAuthorship: {
			header: "Accepted authorship"
		},
		status: {
			header: "Status"
		},
		id: {
			header: "Match ID"
		},
		acceptedID: {
			header: "Accepted ID"
		},
		kingdom: {
			header: "Kingdom"
		},
		phylum: {
			header: "Phylum"
		},
		class: {
			header: "Class"
		},
		order: {
			header: "Order"
		},
		family: {
			header: "Family"
		},
		genus: {
			header: "Genus"
		},
		classification: {
			exclude: true,
		}
	}

	let PAGE_SIZE_DEFAULT: number = 50;
	let totalCount = 1000;
	let wipCount: number = 0;
	let mismatchCount: number = 0;
	let doneCount: number = 0;
	let wipPercent: number = 0.0;
	let mismatchPercent: number = 0.0;
	let donePercent: number = 0.0;

	const refreshTrigger = writable(0);
    type BigTableConfig = TableConfig<GenericMatchingResult> & {
		clientDb?: boolean;
		clientDbSeedData?: GenericMatchingResult[];
		__initialServerCount?: number;
		clientDbRefresh?: typeof refreshTrigger;
    };

    type BigDoneTableConfig = TableConfig<SpeciesMatchingRow> & {
		clientDb?: boolean;
		clientDbSeedData?: SpeciesMatchingRow[];
		__initialServerCount?: number;
		clientDbRefresh?: typeof refreshTrigger;
    };

	const RESULT_TABLE_ID = 'resultRows';
	let resultDbInstance: ClientDB | null = null;
	function getResultDB(): ClientDB {
		if (!resultDbInstance) {
			resultDbInstance = new ClientDB(RESULT_TABLE_ID);
		}

		return resultDbInstance;
	}

	const ACCEPT_TABLE_ID = 'acceptedRows';
	let acceptDbInstance: ClientDB | null = null;
	function getAcceptDB(): ClientDB {
		if (!acceptDbInstance) {
			acceptDbInstance = new ClientDB(ACCEPT_TABLE_ID);
		}

		return acceptDbInstance;
	}

	const MISMATCH_TABLE_ID = 'mismatchRows';
	let mismatchDbInstance: ClientDB | null = null;
	function getMismatchDB(): ClientDB {
		if (!mismatchDbInstance) {
			mismatchDbInstance = new ClientDB(MISMATCH_TABLE_ID);
		}

		return mismatchDbInstance;
	}

	const DONE_TABLE_ID = 'doneRows';
	let doneDbInstance: ClientDB | null = null;
	function getDoneDB(): ClientDB {
		if (!doneDbInstance) {
			doneDbInstance = new ClientDB(DONE_TABLE_ID);
		}

		return doneDbInstance;
	}

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

			const doneMap: Map<number, SpeciesMatchingRow> = new Map(response.data.speciesMatchingResults.map((row: any) => [row.id, row]));

			const workInProgressData: GenericMatchingResult[] = [];
			const mismatchData: GenericMatchingResult[] = [];
			const doneData: SpeciesMatchingRow[] = [];

			console.log("DONE AND RESP");
			console.log(doneMap);
			console.log(responseData);

			for (const row of responseData) {
				// @ts-expect-error (DO NOT touch __id field here, indexedDB will take care of it!)
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

			refreshProgressBarVariables(responseData.length, workInProgressData.length, mismatchData.length, doneData.length)

			console.log("DONE DATA:");
			console.log(doneData);

			var doneDataOrdered: SpeciesMatchingRow[] = doneData.map((row: any): SpeciesMatchingRow => 
            {
				// @ts-expect-error (DO NOT touch __id field here, indexedDB will take care of it!)
                return { 
                    postgres_id: row.id,
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

			uniqueMatchTypes = [...new Set(workInProgressData.map(item => item.matchType))];
			console.log(uniqueMatchTypes);

			refreshResultTable(workInProgressData);
			refreshMismatchTable(mismatchData);
			refreshAcceptTable([]);
			const acceptDb = getAcceptDB();
			acceptDb.clear(ACCEPT_TABLE_ID);

			console.log("DONE DATA ORDERED");
			console.log(doneDataOrdered);
			refreshDoneTable(doneDataOrdered);

            return [];
        }
    }

	function refreshResultTable(data: GenericMatchingResult[]) {
		// console.log(data.slice(0, PAGE_SIZE_DEFAULT));
        resultStore.set(data.slice(0, PAGE_SIZE_DEFAULT));

		resultTableConfig = {
			...resultTableConfig,
			clientDbSeedData: data,
			__initialServerCount: data.length
		};
    }

	function refreshAcceptTable(data: GenericMatchingResult[]) {
        acceptedStore.set(data.slice(0, PAGE_SIZE_DEFAULT));

		acceptTableConfig = {
			...acceptTableConfig,
			clientDbSeedData: data,
			__initialServerCount: data.length
		};
    }

	function refreshMismatchTable(data: GenericMatchingResult[]) {
        mismatchStore.set(data.slice(0, PAGE_SIZE_DEFAULT));

		mismatchTableConfig = {
			...mismatchTableConfig,
			clientDbSeedData: data,
			__initialServerCount: data.length
		};
    }

	function refreshDoneTable(data: SpeciesMatchingRow[]) {
        doneStore.set(data.slice(0, PAGE_SIZE_DEFAULT));

		doneTableConfig = {
			...doneTableConfig,
			clientDbSeedData: data,
			__initialServerCount: data.length
		};
    }

	function refreshProgressBarVariables(p_totalCount: number, p_wipCount: number, p_mismatchCount: number, p_doneCount: number) {
		// updating progress bar values
		totalCount = p_totalCount;
		wipCount = p_wipCount;
		mismatchCount = p_mismatchCount;
		doneCount = p_doneCount;

		wipPercent = totalCount > 0 ? (wipCount / totalCount) * 100 : 0;
		mismatchPercent = totalCount > 0 ? (mismatchCount / totalCount) * 100 : 0;
		donePercent = totalCount > 0 ? (doneCount / totalCount) * 100 : 0;
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
	
	async function acceptAllOfMatchType(t: string|undefined) {
		console.log("Accepting all of Match type: ", t);
		const resultDb = getResultDB();
		let records: Array<{ __r: GenericMatchingResult }> = await resultDb.getAll();
		records = records.filter(item => item.__r.matchType == t);
		const keys: number[] = records.map(item => item.__r.__id);

		resultDb.bulkDelete(keys);
		
		records = records.map(({ __r: { __id, ...restR }, ...restRecord }) => ({
			...restRecord,
			__r: restR
		}));

		const acceptDb = getAcceptDB();
		await acceptDb.bulkPut(records)
		console.log(records);

		refreshTrigger.update((n) => n + 1);
	}

	async function openSubmitAcceptedModal() {
		const acceptDb = getAcceptDB();
		const count: number = await acceptDb.count();

        modalStore.trigger({
            type: 'component',
            title: `Reset all edits`,
            component: {
                ref: SubmitAcceptedModal,
				props: { count: count }
            },
            // The response callback catches data passed back when saving
            response: async (checkTruth: boolean) => {
                if (checkTruth) {
                    await submitAccepted();
                } else {
                    modalStore.close();
                }
            }
        });
	}

	/**
	 * Submits a list of user confirmed IDs which will then be marked as confirmed in the database.
	 */
    async function submitAccepted() {
		submittingEntries = true;
		const payload = await getAcceptedMatchIdsPayload();
		console.log("Got IDs, sending request now...");

		const response = await submitAcceptedIds(payload);

		console.log("PAYLOAD");
		console.log(payload);

		if (!response.success) {
            console.log(response);
			submittingEntries = false;
        } else {
			console.log(response);
			goto("/progressoverview");
        }
    }

	/**
	 * Gathers all accepted original_ID(s) and forms a payload for the submit request.
	 */
	async function getAcceptedMatchIdsPayload(): Promise<AcceptMatchesRequest> {
		return {
			datasetId: $matchingSelection.datasetId,
			versionId: $matchingSelection.versionId,
			stepId: $matchingSelection.stepId,
			matchIds: await getMatchIds()
		}
	}

	/**
	 * Returns all accepted original_ID(s).
	 */
	async function getMatchIds(): Promise<(string | undefined)[]> {
		const acceptDb = getAcceptDB();
		const records: Array<{ __r: GenericMatchingResult }> = await acceptDb.getAll();
		
		return records.map(item => item.__r.original_ID);
	}

	let resultTableConfig: BigTableConfig = {
		id: RESULT_TABLE_ID,						
		data: resultStore,
        clientDb: true,
        clientDbSeedData: [],
		clientDbRefresh: refreshTrigger,
		__initialServerCount: 0,
		resizable: "columns",
		height: 700,
		fitToScreen: true,
		defaultPageSize: PAGE_SIZE_DEFAULT,
		pageSizes: [20, PAGE_SIZE_DEFAULT, 100],
		showColumnsMenu: true,					
		columns: resultColumns,
		optionsComponent: ResultTableOptions
	};

	let acceptTableConfig: BigTableConfig = {
		id: ACCEPT_TABLE_ID,						
		data: acceptedStore,
        clientDb: true,
        clientDbSeedData: [],
		clientDbRefresh: refreshTrigger,
		__initialServerCount: 0,
		resizable: "columns",
		height: 700,
		fitToScreen: true,
		defaultPageSize: PAGE_SIZE_DEFAULT,
		pageSizes: [20, PAGE_SIZE_DEFAULT, 100],
		showColumnsMenu: true,					
		columns: resultColumns,
		optionsComponent: AcceptedTableOptions
	};

	let mismatchTableConfig: BigTableConfig = {
		id: MISMATCH_TABLE_ID,						
		data: mismatchStore,
        clientDb: true,
        clientDbSeedData: [],
		clientDbRefresh: refreshTrigger,
		__initialServerCount: 0,
		resizable: "columns",
		height: 700,
		fitToScreen: true,
		defaultPageSize: PAGE_SIZE_DEFAULT,
		pageSizes: [20, PAGE_SIZE_DEFAULT, 100],
		showColumnsMenu: true,					
		columns: resultColumns,
	};

	let doneTableConfig: BigDoneTableConfig = {
		id: DONE_TABLE_ID,						
		data: doneStore,
        clientDb: true,
        clientDbSeedData: [],
		clientDbRefresh: refreshTrigger,
		__initialServerCount: 0,
		resizable: "columns",
		height: 700,
		fitToScreen: true,
		defaultPageSize: PAGE_SIZE_DEFAULT,
		pageSizes: [20, PAGE_SIZE_DEFAULT, 100],
		showColumnsMenu: true,
		columns: {
			postgres_id: {
				exclude: true
			},
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
            matchSourceVersion: {
                header: "Match source version",
                // exclude: true
            },
		}
	}

    const resultTableActions = async (action: CustomEvent<{ row: GenericMatchingResult; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'ACCEPT':
				const resultDb = getResultDB();
				const acceptDb = getAcceptDB();
				const record: { __r: SpeciesMatchingRow } = await resultDb.get(row.__id);
				
				const { __id, ...toBeMovedRecord } = record.__r;

				await acceptDb.put({ __r: toBeMovedRecord });
				await resultDb.delete(row.__id);

				refreshTrigger.update((n) => n + 1);
				break;

			default:
				break;
		}
	};

	const acceptedTableActions = async (action: CustomEvent<{ row: GenericMatchingResult; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'REMOVE':
				const acceptDb = getAcceptDB();
				const resultDb = getResultDB();
				const record: { __r: SpeciesMatchingRow } = await acceptDb.get(row.__id);
				
				const { __id, ...toBeMovedRecord } = record.__r;

				await resultDb.put({ __r: toBeMovedRecord });
				await acceptDb.delete(row.__id);

				refreshTrigger.update((n) => n + 1);
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

	<div class="flex items-center gap-x-1">
		<button class="btn variant-filled-primary cursor-default p-2 py-1 text-sm"><b>Dataset ID {$matchingSelection.datasetId}</b></button>
		<button class="btn variant-filled-success cursor-default p-2 py-1 text-sm"><b>Verion Nr {$matchingSelection.versionNr}</b></button>
		<button class="btn variant-filled-primary cursor-default p-2 py-1 text-sm"><b>Version ID {$matchingSelection.versionId}</b></button>
		<button class="btn variant-filled-success cursor-default p-2 py-1 text-sm"><b>StepID {$matchingSelection.stepId}</b></button>
	</div>

	{#if statusLoaded && resultFileExists}
		{#await load()}
			<Spinner textCss="text-surface-800" label="Loading content and preparing visualization"/>
		{:then data}
			<h2 class="h2">Global Actions</h2>
			{#each Object.entries(uniqueMatchTypes) as [id, value] }
				<button class="btn variant-filled-primary mr-2"
            		on:click|preventDefault={() => acceptAllOfMatchType(value)}
				>
					Accept all {value}
				</button>
			{/each}
			<div class="w-full max-w-xl mx-auto my-6 space-y-3">
				<div class="grid grid-cols-2 gap-2 sm:flex sm:justify-between text-sm font-medium text-gray-700">
					<div class="flex items-center space-x-2">
						<span class="w-3 h-3 bg-success-500 rounded-full"></span>
						<span>Done ({doneCount})</span>
					</div>
					<div class="flex items-center space-x-2">
						<span class="w-3 h-3 bg-warning-500 rounded-full"></span>
						<span>WIP ({wipCount})</span>
					</div>
					<div class="flex items-center space-x-2">
						<span class="w-3 h-3 bg-error-500 rounded-full"></span>
						<span>Mismatch ({mismatchCount})</span>
					</div>
					
					<div class="text-gray-400 sm:ml-auto">
						Total: {totalCount}
					</div>
					</div>

					<div class="w-full h-5 bg-gray-200 rounded-full overflow-hidden flex shadow-inner">


					<div 
						class="h-full bg-success-500 transition-all duration-300 ease-out"
						style="width: {donePercent}%"
						title="Mismatch"
					></div>

					<div 
						class="h-full bg-warning-500 transition-all duration-300 ease-out"
						style="width: {wipPercent}%"
						title="Accepted"
					></div>

					<div 
						class="h-full bg-error-500 transition-all duration-300 ease-out"
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
				<Table config={acceptTableConfig} on:action={acceptedTableActions}/>
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
					<Table config={mismatchTableConfig}/>
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
					<Table config={doneTableConfig}/>
				</div>
			</div>

			<div class="h-4"></div>

			<div class="flex items-center justify-center">
				{#if submittingEntries}
					<Spinner textCss="text-surface-800" label="Submitting your selected entries"/>
				{/if}
			</div>
			<div class="flex items-center justify-center">
				<button class="btn variant-filled-secondary" disabled={submittingEntries} on:click|preventDefault={openSubmitAcceptedModal}>Submit</button>
				<Modal />
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