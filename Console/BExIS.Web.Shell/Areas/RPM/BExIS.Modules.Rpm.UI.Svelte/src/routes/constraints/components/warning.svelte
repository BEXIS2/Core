<script lang="ts">
	import { onMount } from 'svelte';
    import type {
		ConstraintListItem,
		DatasetInfo,
	} from '../models';
	import { GetDatasetsByConstreint } from '../services/apiCalls';

    export let constraint: ConstraintListItem;
    let ds: DatasetInfo[] = [];
    $: datasets = ds;
    $: console.log('Datasets',datasets)

    onMount(async () => {
		//ds = await GetDatasetsByConstreint(constraint.id);		
	});
</script>

<div class="w-full mb-1 variant-ghost-warning text-center card">
    Changing the Contsraint may cause inconsistencies in following Datasets.
    <!-- <ul>
        {#each datasets as dataset}
            <li>
                {dataset.name}, {dataset.id}
            </li>
        {/each}
    </ul> -->
</div>