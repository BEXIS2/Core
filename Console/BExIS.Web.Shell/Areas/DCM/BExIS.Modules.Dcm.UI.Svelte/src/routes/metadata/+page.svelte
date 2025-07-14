<script lang='ts'>
	import { onMount } from 'svelte';
    import ComplexComponent from './components/edit/complexComponentWrapper.svelte';

    import * as apiCalls from './services/apiCalls';
    import { helpStore, Spinner } from '@bexis2/bexis2-core-ui';

	import { Page } from '@bexis2/bexis2-core-ui';
    import { metadataStore } from './stores';
 
    
    
    let s: any;
    let m: any;
    let schema: any = s;
    $: schema = s;
    $: console.log('metadata', m);

    async function load() {
        s = await apiCalls.GetMetadataSchema(3);
        m = await apiCalls.GetMetadata(20);
        metadataStore.set(m);
    }
</script>

<Page>
    <h1 class="h1">Metadata</h1>
    {#await load()}
        <Spinner />
    {:then}
        <div class="p-2">
            <ComplexComponent complexComponent={s} path={''} />
        </div>
    {/await}
</Page>


