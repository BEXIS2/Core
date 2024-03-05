<script lang="ts">
	import { MultiSelect, Page, helpStore } from "@bexis2/bexis2-core-ui";
    import type { DatasetInfo } from "../models";
    import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';
	import type { DrawerStore } from "@skeletonlabs/skeleton";
	import { createEventDispatcher, onMount } from "svelte";

    export let drawerStore: DrawerStore;

    let data: {
        datasets: DatasetInfo[];
        dataset?: DatasetInfo;
    }

    let dataset: DatasetInfo;
    let datasets: DatasetInfo[];
    $: datasets = data && data.datasets ? data.datasets : [] ;

    onMount(async () => {
		updateData();
	});


    function updateData() {
        if(!data || !data.dataset || $drawerStore.meta.dataset && data.dataset.id != $drawerStore.meta.dataset.id)
        {
            data = { ...$drawerStore.meta };
            if(data.dataset)
            {
                dataset = data.dataset;
            }
        }
	}

    function updateStore() {
        if(!$drawerStore.meta.dataset || data.dataset && data.dataset.id != $drawerStore.meta.dataset.id)
        {
            data.dataset = dataset;
            $drawerStore.meta = { ...data };
        }
	}

    function getCol()
    {
        console.log("getCol()")
    }

    function cancel(){
        data.dataset = undefined;
        $drawerStore.meta = { ...data };
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
                    on:change={() => updateStore()}
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
                }}><Fa icon={faSave} /></button
            >
        </div>
        
    </form>
</div>
{/if}