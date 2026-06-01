<script lang="ts">
    import Fa from 'svelte-fa';
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner, type TableConfig } from "@bexis2/bexis2-core-ui";
    import { onMount } from "svelte";
    import { loadBasicDatasetInfo } from "./services";
    import { type BasicDatasetInfo, type DisplayDatasetVersion } from "./data";
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
    let displayDatasetInfo: DisplayDatasetVersion[] = [];

    onMount(() => {
        async function init() {
            var responseData: BasicDatasetInfo[] = await loadBasicDatasetInfo();
            console.log(responseData);
            var filteredData: BasicDatasetInfo[] = responseData.filter(item => 
                item.isTabular === true && item.metadataComplete === true
            );

            
            filteredData.forEach(item => {
                item.versions.forEach(versionItem => {
                    displayDatasetInfo.push({
                        id: item.id,
                        dataStructureId: item.dataStructureId,
                        title: item.title,
                        abstract: item.abstract,
                        isTabular: item.isTabular,
                        metadataComplete: item.metadataComplete,
                        timestamp: versionItem.timestamp,
                        versionType: versionItem.versionType,
                        versionName: versionItem.versionName,
                        hasMatchingProgress: versionItem.hasMatchingProgress,
                        versionId: versionItem.versionId,
                        versionNr: versionItem.versionNr,
                    })
                });
            });

            datasetsStore.update(() => {
                return displayDatasetInfo;
            });
        }

        // console.log(get(datasetsStore));
        init();
    });


	const tableActions = (action: CustomEvent<{ row: DisplayDatasetVersion; type: string }>) => {
		const { type, row } = action.detail;
		switch (type) {
            // action to begin new mapping process on selected dataset
			case 'BEGIN':
                mappingSelection.update(s => ({ datasetId: row.id, datastructureId: row.dataStructureId, versionId: row.versionId, versionNr: row.versionNr }));
                goto("/headermapping");
				break;

            // action to continue mapping process on selected dataset 
            case 'CONTINUE':
                mappingSelection.update(s => ({ datasetId: row.id, datastructureId: row.dataStructureId, versionId: row.versionId, versionNr: row.versionNr }));
                goto("/progress_overview");
                break;

			default:
				break;
		}
	};


    const tableConfig: TableConfig<DisplayDatasetVersion> = {						
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
            },
            versionId: {
                exclude: true
            },
            timestamp: {
                exclude: true
            },
            versionType: {
                exclude: true
            },
            versionName: {
                exclude: true
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

    <p>This page gives an overview of all your datasets and their respective matching progress. This matching process is bound to a specific dataset version (shown by the column <b>VersionNr</b>). Currently only tabular datasets with complete metadata are shown!</p>
    <p class="">If there is an Eye <span class="inline-flex"><Fa class="px-2" icon={faEye} /></span> icon at the end of the row, the dataset matching has already been started. Click it to get an overview and continue the process as you wish.</p>
    <p class="">If there is a Plus <span class="inline-flex self-center"><Fa class="px-2" icon={faPlus} /></span> icon, no matching has been started. Click it to start a fresh matching process on this dataset version. Keep in mind that a new matching process right now can only be started with the latest version of a dataset.</p>

    <div class="flex items-center justify-center">
		<Table config={tableConfig} on:action={tableActions}/>
	</div>

    <div class="h-80"></div>

</Page>