<script lang="ts">
    import Fa from 'svelte-fa';
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner, type TableConfig } from "@bexis2/bexis2-core-ui";
    import { onMount } from "svelte";
    import { loadBasicDatasetInfo } from "./services";
    import { type BasicDatasetInfo } from "./data";
    import { datasetsStore } from "./data";
	import { Table } from '@bexis2/bexis2-core-ui';
    import ResultTableOptions from "./ResultTableOptions.svelte";
	import { Modal, filter, getModalStore } from '@skeletonlabs/skeleton';
    import { get } from "svelte/store";
	import { faEye, faPlus } from "@fortawesome/free-solid-svg-icons";
    import { mappingSelection } from '../../lib/stores/selectionStore';
	import { goto } from '$app/navigation';

	const modalStore = getModalStore()
    let datasetInfo: BasicDatasetInfo[];

    onMount(() => {
        async function init() {
            var responseData: BasicDatasetInfo[] = await loadBasicDatasetInfo();
            console.log(responseData);
            var filteredData: BasicDatasetInfo[] = responseData.filter(item => 
                item.isTabular === true && item.metadataComplete === true
            );

            datasetsStore.update(() => {
                return filteredData;
            });

            datasetInfo = filteredData;
        }

        // console.log(get(datasetsStore));
        init();
    });


	const tableActions = (action: CustomEvent<{ row: BasicDatasetInfo; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
            // action to begin new mapping process on selected dataset
			case 'BEGIN':
                mappingSelection.update(s => ({ datasetId: row.id, datastructureId: row.dataStructureId }));
                goto("/headermapping");
				break;

            // action to continue mapping process on selected dataset 
            case 'CONTINUE':
                mappingSelection.update(s => ({ datasetId: row.id, datastructureId: row.dataStructureId }));
                goto("/progress_overview");
                break;

			default:
				break;
		}
	};


    const tableConfig: TableConfig<BasicDatasetInfo> = {						
		id: 'resultRows',						
		data: datasetsStore,
		resizable: "columns",
		height: 700,
		fitToScreen: true,
		defaultPageSize: 50,
		pageSizes: [20, 50, 100],
		showColumnsMenu: true,
        columns: {
            metadataComplete: {
                disableFiltering: true
            },
            isTabular: {
                disableFiltering: true
            },
            hasMatchingProgress: {
                disableFiltering: true
            }
        },
		optionsComponent: ResultTableOptions
	};
</script>

<Page 
	title="Datasets Overview" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>
    <h2 class="h2">Datasets Overview</h2>

    <p>This page gives an overview of all your datasets and their respective mapping progress. Currently only tabular datasets with complete metadata are shown!</p>
    <p class="flex">If there is an Eye <Fa class="px-2" icon={faEye} /> icon at the end of the row, the dataset mapping has already been started. Click it to get an overview and continue the process as you wish.</p>
    <p class="flex">If there is a Plus <Fa class="px-2" icon={faPlus} /> icon, no mapping has been started. Click it to start a fresh mapping process on this dataset.</p>

    <p>Dataset ID: {$mappingSelection.datasetId}</p>
    <p>Structure ID: {$mappingSelection.datastructureId}</p>

    <div class="flex items-center justify-center">
		<Table config={tableConfig} on:action={tableActions}/>
	</div>

    <div class="h-80"></div>

</Page>