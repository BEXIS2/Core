<script lang="ts">
    import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner, type TableConfig } from "@bexis2/bexis2-core-ui";
	import { onMount } from "svelte";
    import { loadResult } from "./services";
    import { tailorResultStore, tailorCleanedStore, initializeTableData, toggleDataCleaning, cleanConfig, getChangedTailorRows } from "./data";
    import { type SpeciesMatchingRow } from "$lib/types/types";
	import { Table } from '@bexis2/bexis2-core-ui';
    import ResultTableOptions from "./ResultTableOptions.svelte";
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
    import { SlideToggle } from "@skeletonlabs/skeleton";
    import EditResult from "./EditResult.svelte";
    import { faPen, faMousePointer } from "@fortawesome/free-solid-svg-icons";
    import { mappingSelection } from '../../lib/stores/selectionStore';
    import { get } from "svelte/store";
    import Fa from 'svelte-fa';
    import { getDifference } from "$lib/helper/custom_diff";
    import CleanedName from "./cleanedName.svelte";
	import EditSubmit from "./EditSubmit.svelte";

	const modalStore = getModalStore();
    let totalCount: number = 1000;
    let confirmedCount: number = 0;
    let percentage: number = totalCount > 0 ? (confirmedCount / totalCount) * 100 : 0;

    let rowChangesToSubmit: SpeciesMatchingRow[];

    onMount(() => {
        async function test() {
            var responseData = await loadResult($mappingSelection.datasetId, $mappingSelection.versionId);

            // filter out redundant data and determine column order
            let filteredData: SpeciesMatchingRow[] = responseData.message.map((row: any): SpeciesMatchingRow => 
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

            totalCount = filteredData.length;
            confirmedCount = filteredData.filter(item => item.confirmedByUser).length;
            percentage = totalCount > 0 ? (confirmedCount / totalCount) * 100 : 0;
            tailorResultStore.update(() => {
                return filteredData;
            });
            
            initializeTableData(filteredData);
        }

        test();
    });

    function prepareForSubmit() {
        rowChangesToSubmit = getChangedTailorRows();
        modalStore.trigger({
            type: 'component',
            title: `Submitting user changes`,
            component: {
                ref: EditSubmit,
                props: { changedRows: rowChangesToSubmit }
            }
        });
    }

	const tableActions = (action: CustomEvent<{ row: SpeciesMatchingRow; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
			case 'UPDATE':
				modalStore.trigger({
					type: 'component',
					title: `Edit Result name ${row.originalName}`,
					component: {
						ref: EditResult,
						props: { row: row }
					}
				});
				break;

			default:
				break;
		}
	};

    const tableConfig: TableConfig<SpeciesMatchingRow> = {						
		id: 'resultRows',						
		data: tailorCleanedStore,
		resizable: "columns",
		height: 700,
		fitToScreen: true,
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
            cleanedName: {
                header: "Cleaned name",
                instructions: {
                    renderComponent: CleanedName
                },
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
        },
		optionsComponent: ResultTableOptions
	};
</script>

<Page 
	title="Tailor Result" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>
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


    <h2 class="h2">Data cleaning config</h2>
    <div class="grid grid-cols-3 gap-x-14 gap-y-1">
        {#each Object.entries(cleanConfig) as [key, conf]}
        <div>
            <span class="flex items-center gap-x-2"><SlideToggle name={"label"} bind:checked={conf.apply} on:change={toggleDataCleaning}></SlideToggle> {key}</span>
        </div>
        {/each}
    </div>

    <h2 class="h2">Global Actions</h2>
    <button class="btn variant-filled-primary">Match all internal</button>

    <div class="w-full max-w-md mx-auto my-4 space-y-2">
        <div class="flex justify-between text-sm font-medium text-gray-700">
            <span>Confirmed Progress ({confirmedCount}/{totalCount})</span>
            <span>{Math.round(percentage)}%</span>
        </div>

        <div class="w-full h-4 bg-gray-200 rounded-full overflow-hidden">
            <div 
            class="h-full bg-blue-600 transition-all duration-300 ease-out"
            style="width: {percentage}%"
            ></div>
        </div>
    </div>

	<div class="flex items-center justify-center">
		<Table config={tableConfig} on:action={tableActions}/>
		<Modal />
	</div>

    <div class="h-4">

    </div>

    <div class="flex justify-center items-center">
        <button class="btn variant-filled-secondary" on:click|preventDefault={prepareForSubmit}>SUBMIT</button>
    </div>

    <div class="h-80"></div>

</Page>