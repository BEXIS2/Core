<script lang="ts">
    import { Page } from '@bexis2/bexis2-core-ui';
    import * as apiCalls from './services/apiCalls';
    import { onMount } from 'svelte';

    let metadata: any;
    let datasetTitle: string;
    let datasetAuthors: string[] = [];
    let datasetDescription: string;
    let datasetResources: any[] = [];

    let showAllAuthors = false;
    let showFullDescription = false;
    
    onMount(async () => {
        metadata = await apiCalls.GetMetadata(4)
        console.log(metadata);

        datasetTitle = metadata?.publication?.Title?.['#text'] ?? 'Title not available';
        datasetAuthors = metadata?.publication?.Author?.map((author: any) => author['#text']) || ['Authors not available'];
        datasetDescription = metadata?.publication?.Abstract?.map((abstract:any) => abstract['#text']) ?? 'Description not available';

    });
    
   // Testdaten
   let testData = [
  {
    name: "Dataset A",
    doi: "10.5061/dryad.ab12cd34",
    language: "en",
    date: "2022-05-01",
    sizeMB: 12.5,
    format: "csv",
    license: "cc0 1.0",
    submittedBy: "dryad"
  },
  {
    name: "Dataset B",
    doi: "10.5061/dryad.ef56gh78",
    language: "de",
    date: "2023-01-12",
    sizeMB: 8.3,
    format: "json",
    license: "cc-by 4.0",
    submittedBy: "dryad"
  },
  {
    name: "Dataset C",
    doi: "10.5061/dryad.xy98kl45",
    language: "en",
    date: "2021-09-30",
    sizeMB: 15.0,
    format: "csv",
    license: "cc0 1.0",
    submittedBy: "dryad"
  },
  {
    name: "Dataset D",
    doi: "10.5061/dryad.mq34zr12",
    language: "fr",
    date: "2024-03-22",
    sizeMB: 5.7,
    format: "xlsx",
    license: "cc-by 4.0",
    submittedBy: "dryad"
  },
  {
    name: "Dataset E",
    doi: "10.5061/dryad.vt67bn89",
    language: "es",
    date: "2020-11-05",
    sizeMB: 9.8,
    format: "tsv",
    license: "cc0 1.0",
    submittedBy: "dryad"
  },
  {
    name: "Dataset F",
    doi: "10.5061/dryad.hu23qw67",
    language: "en",
    date: "2023-07-18",
    sizeMB: 22.1,
    format: "csv",
    license: "cc-by-sa 3.0",
    submittedBy: "dryad"
  }
];

    

    datasetTitle = metadata?.publication?.Title?.['#text'] ?? '';
</script>

<Page help={true} title="View Publications">
    <h1 class="h1">{datasetTitle}</h1>
    <div class="flex gap-5 w-full">
        <div>
  <card class="w-96 card flex flex-col p-2 my-1">
    <div class="flex flex-col gap-5">
        <h4 class="h4">Authors</h4>
        <p class={showAllAuthors ? "" : "line-clamp-4"}>
            {#each datasetAuthors as author, i}
                <span class="w-40 inline-block border border-gray-300 rounded px-2 py-1 bg-white truncate mr-1" title={author}>
                    {author}
                </span>
            {/each}
        </p>
    </div>
    {#if datasetAuthors.length > 6}
        <button class="badge self-end w-16 text-xs mt-2" on:click={() => showAllAuthors = !showAllAuthors}>
            {showAllAuthors ? 'See less' : 'See more'}
        </button>
    {/if}
</card>
        </div>
        <div>
            <card class="w-full card flex flex-col gap-5 p-2 my-1">
                <h4 class="h4">Description</h4>
                <p class={showFullDescription ? "text" : "text line-clamp-6"}>{datasetDescription}</p>
                {#if datasetDescription && datasetDescription.length}
                    <button class="badge self-end w-16 text-xs mt-1" on:click={() => showFullDescription = !showFullDescription}>
                        {showFullDescription ? 'See less' : 'See more'}
                    </button>
                {/if}
            </card>
        </div>
    </div>
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-8">
    {#each testData as dataset}
        <card class="card w-full flex flex-col gap-2 p-4 my-1">
            <h4 class="h4 mb-2">{dataset.name}</h4>
            <p><b>DOI:</b> {dataset.doi}</p>
            <p><b>Language:</b> {dataset.language}</p>
            <p><b>Date:</b> {dataset.date}</p>
            <p><b>Size (MB):</b> {dataset.sizeMB}</p>
            <p><b>Format:</b> {dataset.format}</p>
            <p><b>License:</b> {dataset.license}</p>
            <p><b>Submitted by:</b> {dataset.submittedBy}</p>
        </card>
    {/each}
</div>
</Page>