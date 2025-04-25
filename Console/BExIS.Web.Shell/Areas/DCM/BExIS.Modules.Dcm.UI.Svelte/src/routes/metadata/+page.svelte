<script lang='ts'>
	import { onMount } from 'svelte';
    import ComplexComponent from './components/complexComponentWraper.svelte';

    import * as apiCalls from './services/apiCalls';

    import type { schemaNode } from './models';

    import test from './test.json';
	import { Page } from '@bexis2/bexis2-core-ui';
 
    let s: any;
    let schema: any = test;

    //$: schema = s;
    //$: createObjectFromSchema(schema);

    onMount(async () => {
        //s = await apiCalls.GetMetadataScheema(1);
	});


    
</script>

<Page>
    <h1 class="h1">Metadata</h1>
    <ComplexComponent complexComponent={schema} path= {''} />
</Page>


