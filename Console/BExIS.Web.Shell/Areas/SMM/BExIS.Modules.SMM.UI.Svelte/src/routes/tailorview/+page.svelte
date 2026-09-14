<script lang="ts">
    import { Page, pageContentLayoutType, Spinner, Table, ClientDB, type TableConfig } from "@bexis2/bexis2-core-ui";
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
    import { SlideToggle } from "@skeletonlabs/skeleton";
    import { loadResult } from "./services";
    import { tailorEditStore, tailorOnlyEditsStore, cleanConfig, cleanName } from "./data";
    import { matchingSelection } from '../../lib/stores/selectionStore';
    import { writable } from "svelte/store";
    import { type SpeciesMatchingRow } from "$lib/types/types";
    import ResultTableOptions from "./ResultTableOptions.svelte";
    import CleanedName from "./CleanedName.svelte";
    import EditNameModal from "./EditNameModal.svelte";
	import ResetEditsModal from "./ResetEditsModal.svelte";
	import EditSubmitModal from "./EditSubmitModal.svelte";
    import { faPen, faMousePointer } from "@fortawesome/free-solid-svg-icons";
    import Fa from 'svelte-fa';

	const modalStore = getModalStore();

    // progress bar variables (species mapping progress)
    let totalCount: number = 1000;
    let confirmedCount: number = 0;
    let percentage: number = totalCount > 0 ? (confirmedCount / totalCount) * 100 : 0;

    let tableInDOM: boolean = true;

    // if only showing edits at the momen
    let showEditsOnly: boolean = false;

    // table
    let PAGE_SIZE_DEFAULT: number = 50;

    // original data (re-maped for easier handling)
    let originalData: SpeciesMatchingRow[];
    // helper to row edits
    let rowChangesToSubmit: SpeciesMatchingRow[];
    // helper for table row object (how it is wrapped inside indexedDB store)
    type wrappedSpeciesMatchingRow = Array<{ __r: SpeciesMatchingRow }>;
    
    // Shared ClientDB instance for direct row manipulation
	let dbInstance: ClientDB | null = null;
	function getDB(): ClientDB {
		if (!dbInstance) {
			dbInstance = new ClientDB('resultRows');
		}

		return dbInstance;
	}

    let onlyEditsDbInstance: ClientDB | null = null;
    function getOnlyEditsDB(): ClientDB {
        if (!onlyEditsDbInstance) {
            onlyEditsDbInstance = new ClientDB('onlyEditsRows');
        }

        return onlyEditsDbInstance;
    }

	const refreshTrigger = writable(0);
    type BigTableConfig = TableConfig<SpeciesMatchingRow> & {
		clientDb?: boolean;
		clientDbSeedData?: SpeciesMatchingRow[];
		__initialServerCount?: number;
		clientDbRefresh?: typeof refreshTrigger;
    };

    function initializeTableData(data: SpeciesMatchingRow[]) {
        const processed = data.map((item, index) => ({
            ...item,
            cleanedName: cleanName(item.originalName),
            // cleanedName: item.originalName,
            editedName: item.editedName || ''
        }));

        originalData = processed;
    }

    async function load() {
        let responseData = await loadResult($matchingSelection.datasetId, $matchingSelection.versionId);
        const filteredData = formatResponseData(responseData);

        initializeTableData(filteredData);
        refreshProgressBarVariables(filteredData);
        refreshTable();
    }

    function formatResponseData(responseData: any): SpeciesMatchingRow[] {
        // filter out redundant data and determine column order
        return responseData.message.map((row: any, index: number): SpeciesMatchingRow => 
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
    }

    /**
     * Call once at the start to set and refresh first slice of table data
     */
    function refreshTable() {
        tailorEditStore.set(originalData.slice(0, PAGE_SIZE_DEFAULT));

		tableConfig = {
			...tableConfig,
			clientDbSeedData: originalData,
			__initialServerCount: originalData.length
		};
    }

    function refreshOnlyEditsTable(data: SpeciesMatchingRow[]) {
        tailorOnlyEditsStore.set(data.slice(0, PAGE_SIZE_DEFAULT));

        onlyEditsTableConfig = {
            ...onlyEditsTableConfig,
            clientDbSeedData: data,
            __initialServerCount: data.length
        };
    }

    function refreshProgressBarVariables(tableData: SpeciesMatchingRow[]) {
        totalCount = tableData.length;
        confirmedCount = tableData.filter(item => item.confirmedByUser).length;
        percentage = totalCount > 0 ? (confirmedCount / totalCount) * 100 : 0;
    }

    async function openResetEditsModal() {
        modalStore.trigger({
            type: 'component',
            title: `Reset all edits`,
            component: {
                ref: ResetEditsModal,
            },
            // The response callback catches data passed back when saving
            response: async (checkTruth: boolean) => {
                if (checkTruth) {
                    await resetEverything();
                } else {
                    modalStore.close();
                }
            }
        });
    }

    async function resetEverything() {
        tableInDOM = false;
        const db = getDB();
        await db.clear(db.tableId);
        initializeTableData(originalData);
        refreshProgressBarVariables(originalData);
        refreshTable();
        tableInDOM = true;
    }

    async function filterForEdited() {
        const db = getDB();
        let records = await db.getAll();
        const onlyEditsRecords = getChangedTailorRows(records);
        const onlyEditsDB = getOnlyEditsDB();
        onlyEditsDB.replace(onlyEditsRecords);

        refreshOnlyEditsTable(onlyEditsRecords);
        showEditsOnly = true;
        refreshTrigger.update((n) => n + 1);
    }

    async function toggleFilterForEdited() {
        if (showEditsOnly) {
            // if only showing edits right now, just reset it to show all

            showEditsOnly = false;
        } else {
            // else filter for only edits and replace the table content
            filterForEdited();
        }
    }

    // Bulk toggle function
    async function setAllToggles(to: boolean) {
        // small optimization to prevent unnecessary cleaning toggles
        if (to) {
            if (Object.values(cleanConfig).every((conf) => conf.apply)) {
                return;
            }
        } else {
            if (Object.values(cleanConfig).every((conf) => !conf.apply)) {
                return;
            }
        }

        // 1. Mutate all properties
        Object.values(cleanConfig).forEach((conf) => {
            conf.apply = to;
        });

        cleanConfig = cleanConfig;

        // 3. Run processing function exactly ONCE
        await toggleDataCleaning();
    }

    async function toggleDataCleaning() {
        const db = getDB();
        let records = await db.getAll();

        records = records.map(({ __r }) => {
            const { __id, ...rest } = __r;
            return {
                ...rest,
                cleanedName: cleanName(rest.originalName)
            };
        });

        await db.replace(records);

        if (showEditsOnly) {
            filterForEdited();
        } else {
            refreshTrigger.update((n) => n + 1);
        }
    }

    /**
     * This function filters out all effectively changed rows from the TailorTable. Effectively changed means, the row is unconfirmed
     * and the EditedName should change either due to data cleaning or manual user changes.
     * @returns Array of effectively changed ResultRows
     */
    function getChangedTailorRows(records: wrappedSpeciesMatchingRow): SpeciesMatchingRow[] {
        const originalDataMap = new Map<string | number, SpeciesMatchingRow>(
            originalData.map(row => [row.postgres_id, row])
        );

        return records.map(item => item.__r).filter(cleanedRow => {
            const resultRow = originalDataMap.get(cleanedRow.postgres_id);

            if (!resultRow) return false;

            // Entry needs to be unconfirmed
            if (resultRow.confirmedByUser) return false;

            // data cleaning detected and has not been applied yet
            const condition1 = !cleanedRow.editedName && cleanedRow.cleanedName !== cleanedRow.originalName;

            // editedName has changed
            const condition2 = cleanedRow.editedName !== '' && cleanedRow.editedName !== resultRow.editedName;

            return condition1 || condition2;
        })
    }

    /**
     * collects all effectively changed rows and opens a modal for the user to submit them
     */
    async function prepareForSubmit() {
        const db = getDB();
        const records = await db.getAll();
        rowChangesToSubmit = getChangedTailorRows(records);
        modalStore.trigger({
            type: 'component',
            title: `Submitting user changes`,
            component: {
                ref: EditSubmitModal,
                props: { changedRows: rowChangesToSubmit }
            }
        });
    }

	// get the row with a certain ID from the indexedDB store and update this row
	async function updateRowInIndexedDB(row: SpeciesMatchingRow) {
		const db = getDB();
        const record = await db.get(row.__id);
        record.__r.editedName = row.editedName;

        await db.put(record);

		// Trigger table refresh so the updated row is visible immediately
		refreshTrigger.update((n) => n + 1);
	}

	const tableActions = (action: CustomEvent<{ row: SpeciesMatchingRow; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'UPDATE':
				modalStore.trigger({
					type: 'component',
					title: `Edit Result name ${row.originalName}`,
					component: {
						ref: EditNameModal,
						props: { row: row },
					},
                    // The response callback catches data passed back when saving
                    response: async (updatedRow: SpeciesMatchingRow) => {
                        await updateRowInIndexedDB(updatedRow);
                    }
				});
				break;

			default:
				break;
		}
	};

    let tableConfig: BigTableConfig = {						
		id: 'resultRows',						
		data: tailorEditStore,
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
            __id: {
                exclude: true
            },
            originalName: {
                header: "Original name"
            },
            cleanedName: {
                header: "Cleaned name",
                instructions: {
                    renderComponent: CleanedName
                },
            },
            editedName: {
                header: "Edited name",
                instructions: {
                    renderComponent: CleanedName
                }
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
        },
		optionsComponent: ResultTableOptions
	};

    let onlyEditsTableConfig: BigTableConfig = {
        id: 'onlyEditsRows',						
		data: tailorOnlyEditsStore,
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
            __id: {
                exclude: true
            },
            originalName: {
                header: "Original name"
            },
            cleanedName: {
                header: "Cleaned name",
                instructions: {
                    renderComponent: CleanedName
                },
            },
            editedName: {
                header: "Edited name",
                instructions: {
                    renderComponent: CleanedName
                }
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
        },
    }
</script>

<Page 
	title="Tailor Result" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>
    <div class="flex items-center gap-x-1">
		<button class="btn variant-filled-primary cursor-default p-2 py-1 text-sm"><b>Dataset ID {$matchingSelection.datasetId}</b></button>
		<button class="btn variant-filled-success cursor-default p-2 py-1 text-sm"><b>Verion Nr {$matchingSelection.versionNr}</b></button>
		<button class="btn variant-filled-primary cursor-default p-2 py-1 text-sm"><b>Version ID {$matchingSelection.versionId}</b></button>
	</div>

    <div>
        Select steps for data cleaning (changes applied automatically). Use Global actions to run specific procedures across the whole dataset.
    </div>
    <div class="flex">
        Hover the Data cleaning options and Global actions <Fa class="px-2" icon={faMousePointer} />, to get an explanation for what they are doing.
    </div>
    <div class="flex">
        Click the pencil icon <Fa class="px-2" icon={faPen} /> to edit individual names (if empty, the cleaned name property is used for matching or if empty as well, the original name).
    </div>
    <div>
        When you're done here, be sure to SUBMIT the changes for them to take effect!
    </div>

    {#await load()}
        <Spinner textCss="text-surface-800" label="Loading content and preparing visualization"/>
    {:then data} 
        <h2 class="h2">Data cleaning config</h2>
        <button class="btn variant-filled-error"
            on:click|preventDefault={() => setAllToggles(false)}
        >
            Toggle all off
        </button>
        
        <button class="btn variant-filled-success"
            on:click|preventDefault={() => setAllToggles(true)}
        >
            Toggle all on
        </button>
        <div class="grid grid-cols-3 gap-x-14 gap-y-1">
            {#each Object.entries(cleanConfig) as [key, conf]}
            <div>
                <span class="flex items-center gap-x-2"><SlideToggle name={"label"} bind:checked={conf.apply} on:change={toggleDataCleaning}></SlideToggle> {key}</span>
            </div>
            {/each}
        </div>

        <h2 class="h2">Global Actions</h2>
        <button class="btn variant-filled-primary">Match all internal</button>
        <button class="btn {showEditsOnly ? 'variant-filled-secondary' : 'variant-filled-warning'}"
			on:click|preventDefault={() => toggleFilterForEdited()}
		>
			{showEditsOnly ? 'Show all' : 'Show edits only'}
		</button>
        <button class="btn variant-filled-error"
            on:click|preventDefault={() => openResetEditsModal()}
        >
            Reset all edits
        </button>

        <div class="w-full max-w-md mx-auto my-4 space-y-2">
            <div class="flex justify-between text-sm font-medium text-gray-700">
                <span>Matched ({confirmedCount}/{totalCount})</span>
                <span>{Math.round(percentage)}%</span>
            </div>

            <div class="w-full h-4 bg-gray-200 rounded-full overflow-hidden">
                <div 
                class="h-full bg-success-500 transition-all duration-300 ease-out"
                style="width: {percentage}%"
                ></div>
            </div>
        </div>

        <h2 class="h2">{showEditsOnly ? 'Edits only (row edits disabled)' : 'Table data'}</h2>
        <div class="flex items-center justify-center">
            {#if tableInDOM}
                <div class="w-full {showEditsOnly ? 'hidden' : ''}">
                    <Table config={tableConfig} on:action={tableActions}/>
                    <Modal />
                </div>
                {#if showEditsOnly}
                    <Table config={onlyEditsTableConfig}/>
                {/if}
            {/if}
        </div>
    {/await}

    <div class="h-4">

    </div>

    <div class="flex justify-center items-center">
        <button class="btn variant-filled-secondary" on:click|preventDefault={prepareForSubmit}>SUBMIT</button>
    </div>

    <div class="h-80"></div>

</Page>