<script lang="ts">
    import { Page } from '@bexis2/bexis2-core-ui';
    import * as apiCalls from './services/apiCalls';
    import { onMount } from 'svelte';

    let metadata: any;
    let datasetTitle: string;
    let datasetAuthors: string[] = [];
    let datasetDescription: string;
    let datasetResources: any[] = [];
    
    onMount(async () => {
        metadata = await apiCalls.GetMetadata(4)
        console.log(metadata);

        datasetTitle = metadata?.publication?.Title?.['#text'] ?? 'Title not available';
        datasetAuthors = metadata?.publication?.Author?.map((author: any) => author['#text']) || ['Authors not available'];
        datasetDescription = metadata?.publication?.Abstract?.map((abstract:any) => abstract['#text']) ?? 'Description not available';

    });
    


  

    datasetTitle = metadata?.publication?.Title?.['#text'] ?? '';
</script>

<Page help={true} title="View Publications">
    <h1 class="h1">{datasetTitle}</h1>
    <div class="flex gap-5 w-full">
        <div>
            <card class="w-96 card flex gap-5 p-2 my-1">
                <p class="line-clamp-6">
                    {#each datasetAuthors as author, i}
                        <span class="inline-block border border-gray-300 rounded px-2 py-1 bg-white truncate mr-1 mb-1" title={author}>
                        {author}{i < datasetAuthors.length - 1 ? ', ' : ''}
                    </span>
                    {/each}
                </p>
            </card>
        </div>
        <div>
            <card class="w-full card flex gap-5 p-2 my-1">
                <p class="text line-clamp-6">{datasetDescription}</p>
            </card>
        </div>
    </div>
    
    
    <p class="text">{datasetResources}</p>
</Page>