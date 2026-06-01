<script lang="ts">
    import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner, type TableConfig } from "@bexis2/bexis2-core-ui";
	import { onMount } from "svelte";
    import { loadResult } from "./services";
    import { tailorResultStore, tailorCleanedStore, initializeTableData, toggleDataCleaning, cleanConfig, type TailorResultRow } from "./data";
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

	const modalStore = getModalStore();

    onMount(() => {
        async function test() {
            var responseData = await loadResult($mappingSelection.datasetId, $mappingSelection.versionId);

            // filter out redundant data and determine column order
            let filteredData = responseData.message.map((row: any): TailorResultRow => 
            { 
                return { 
                    id: row.id,
                    originalName: row.originalName,
                    editedName: row.editedName,
                    cleanedName: row.cleanedName,
                    confirmedByUser: row.confirmedByUser,
                    matchedName: row.matchedName,
                    matchType: row.matchType,
                    status: row.status,
                    matchSource: row.matchSource,
                    matchSourceVersion: row.matchSourceVersion,
                    timeStampMatch: row.timeStampMatch
                }
            });

            tailorResultStore.update(() => {
                return filteredData;
            });
            
            initializeTableData(filteredData);
            
            console.log(getDifference("Abies spectabilis (D. Don) Mirbel", "Abies spectabilis D. Don"));
        }

        test();
    });

	const tableActions = (action: CustomEvent<{ row: TailorResultRow; type: string }>) => {
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

    const tableConfig: TableConfig<TailorResultRow> = {						
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
                exclude: true
            },
            status: {
                header: "Status",
                exclude: true
            },
            timestampMatch: {
                header: "Match date",
                exclude: true
            },
            matchType: {
                header: "Match type",
                exclude: true
            },
            matchSource: {
                header: "Match source",
                exclude: true
            },
            matchVersion: {
                header: "Match version",
                exclude: true
            },
            matchSourceVersion: {
                header: "Match source version",
                exclude: true
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

	<div class="flex items-center justify-center">
		<Table config={tableConfig} on:action={tableActions}/>
		<Modal />
	</div>

    <div class="h-4">

    </div>

    <div class="flex justify-center items-center">
        <button class="btn variant-filled-secondary">SUBMIT</button>
    </div>

    <div class="h-80"></div>

</Page>