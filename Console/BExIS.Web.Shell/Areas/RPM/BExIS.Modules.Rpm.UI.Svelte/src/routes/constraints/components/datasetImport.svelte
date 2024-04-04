<script lang="ts">
	import { MultiSelect, helpStore } from "@bexis2/bexis2-core-ui";
    import type { ColumnInfo, DatasetImportInfo, DatasetInfo, DatastructureInfo } from "../models";
    import Fa from 'svelte-fa';
	import { faXmark, faSave, faArrowUpFromBracket } from '@fortawesome/free-solid-svg-icons';
	import { RadioItem, RadioGroup, type DrawerStore } from "@skeletonlabs/skeleton";
	import { createEventDispatcher, onMount } from "svelte";
	import * as apiCalls from '../services/apiCalls';

    export let drawerStore: DrawerStore;

    type Undefinedable<T> = T | undefined;

    let meta: {
        datasets: DatasetInfo[];
        dataset?: DatasetImportInfo;
        import: boolean;
    }

    let dataset: Undefinedable<DatasetImportInfo>;
    let datasets: DatasetInfo[] = [];
    let dataStructure: DatastructureInfo = { id:0, name:'', description:'', columnInfos:[] };
    let data: object[]= [];

    $: $drawerStore.meta, updateData();
    $: dataset, updateStore();
    
    onMount(async () => {
	});

    function updateData() {
        meta = { ...$drawerStore.meta };
        if(meta.dataset)  
            dataset = meta.dataset;
        if(meta.datasets != datasets)
            datasets = meta.datasets;
	}

    function updateStore() {
        if(dataset)
        {
            if(dataset.columnId == undefined || (meta.dataset && dataset.id != meta.dataset.id))
                dataset.columnId = 0;

            load(dataset);   
        }
        meta.dataset = dataset;           
        $drawerStore.meta = { ...meta };
	}

    async function load(ds: DatasetImportInfo)
    {
        if(ds && ds.datastructureId && ds.datastructureId != dataStructure.id)
        {
            dataStructure = await apiCalls.GetDatastructure(ds.datastructureId);
            data = await apiCalls.GetData(ds.id);
        }
    }

    async function getCol()
    {
        meta.import = true;
        $drawerStore.meta = { ...meta };
        drawerStore.close()
    }

    function cancel(){
        dataset = undefined;
        meta.dataset = undefined;
        dataStructure = { id:0, name:'', description:'', columnInfos:[] };
        data = [];
        $drawerStore.meta = { ...meta };
        drawerStore.close()
    }

</script>
{#if datasets && datasets.length > 0}
<div class="w-9/10 m-5 left-1/20">
    <div class="mb-2">
        <!-- svelte-ignore a11y-click-events-have-key-events -->
        <span class="text-surface-600" on:click={() => drawerStore.close()}>
            <Fa icon={faXmark} />
        </span>
    </div>
    <div class="border-b border-primary-500 pb-1">
        <h3 class ="h3 h-9">
            Import Domain List from Dataset
        </h3>
    </div>
    <form on:submit|preventDefault={getCol}>
        <div class="grid grid-cols-1 gap-x-5">
            <div class="pb-3">
                <MultiSelect
                    title="Datasets"
                    id="datasets"
                    bind:target={dataset}
                    source={datasets}
                    itemId="id"
                    itemLabel="name"
                    complexSource={true}
                    complexTarget={true}
                    isMulti={false}
                />
            </div>
            {#if dataset}
            <div class="pb-3">
                <lable>Name</lable>
                <p class="ml-2">{dataset.name}</p>
            </div>
            <div class="pb-3">
                <lable>Description</lable>
                <p class="ml-2">{dataset.description}</p>
            </div>
                {#if dataStructure}
                <div class="overflow-x-scroll h-96 mt-2">
                    <table class="table table-compact bg-tertiary-500/30">
                        <tr class="bg-primary-300">
                            {#each dataStructure.columnInfos as columnInfo}
                                {#if dataset.columnId == columnInfo.id}
                                <th class="text-left bg-secondary-300 px-1">
                                
                                    <RadioItem class="overflow-x-hidden"
                                        on:change={() => {
                                        }}
                                        bind:group={dataset.columnId}
                                        name={columnInfo.name}
                                        id={columnInfo.id}
                                        value={columnInfo.id}>{columnInfo.name}</RadioItem
                                    >
                                </th>
                                {:else}
                                <th class="text-left px-1">
                                
                                    <RadioItem class="overflow-x-hidden"
                                        on:change={() => {
                                        }}
                                        bind:group={dataset.columnId}
                                        name={columnInfo.name}
                                        id={columnInfo.id}
                                        value={columnInfo.id}>{columnInfo.name}</RadioItem
                                    >
                                </th>
                                {/if}
                            {/each}
                        </tr>
                        {#each Object.entries(data) as [rowNr, row]}
                        <tr>
                            {#each Object.entries(row) as [varNr, valule]}
                                {#if "var"+dataset.columnId == varNr}
                                <td class="text-center bg-secondary-300/30 px-1">
                                    {valule}
                                </td>
                                {:else}
                                <td class="text-center px-1">
                                    {valule}
                                </td>
                                {/if}
                            {/each}
                        </tr>
                        {/each}
                    </table>                    
                </div>
                {/if}
            {:else}
            <div class="pb-3">
                <lable>Name</lable>
                <p class="ml-2 text-surface-600">no Dataset selected</p>
            </div>
            <div class="pb-3">
                <lable>Description</lable>
                <p class="ml-2 text-surface-600">no Dataset selected</p>
            </div>
            {/if}
        </div>

        <div class="py-5 text-right">
            <!-- svelte-ignore a11y-mouse-events-have-key-events -->
            <button
                type="button"
                class="btn variant-filled-warning h-9 w-16 shadow-md"
                title="Cancel"
                id="cancel"
                on:mouseover={() => {
                    helpStore.show('cancel');
                }}
                on:click={() => cancel()}><Fa icon={faXmark} /></button
            >
            <!-- svelte-ignore a11y-mouse-events-have-key-events -->
            <button
                type="submit"
                class="btn variant-filled-primary h-9 w-16 shadow-md"
                title=""
                id="save"
                on:mouseover={() => {
                    helpStore.show('save');
                }}><Fa icon={faArrowUpFromBracket} /></button
            >
        </div>
        
    </form>
</div>
{/if}