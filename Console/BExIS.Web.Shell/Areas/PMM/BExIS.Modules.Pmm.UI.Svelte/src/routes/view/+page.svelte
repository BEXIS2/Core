<script lang="ts">
    import { Page } from '@bexis2/bexis2-core-ui';
    import * as apiCalls from './services/apiCalls';
    import { onMount } from 'svelte';
    import type { ResourceArray } from './models';

    let metadata: any;
    let datasetTitle: string;
    let datasetAuthors: string[] = [];
    let datasetDescription: string;
    let datasetResources: ResourceArray[] = [];

    let showAllAuthors = false;
    let showFullDescription = false;
    
    onMount(async () => {
        metadata = await apiCalls.GetMetadata(10);
        console.log(metadata);

        datasetTitle = metadata?.publication?.Title?.['#text'] ?? 'Title not available';
        datasetAuthors = metadata?.publication?.Author?.map((author: any) => author['#text']) || ['Authors not available'];
        datasetDescription = metadata?.publication?.Abstract?.map((abstract:any) => abstract['#text']) ?? 'Description not available';

        datasetResources = (metadata?.Resource ?? []).map((resource: any) => ({
            Name: resource?.Name?.['#text'] ?? '',
            SubmissionDate: resource?.Submission_Date?.['#text'] ?? '',
            EmbargoEnd: resource?.Embargo_End?.['#text'] ?? '',
            URI: resource?.Access?.URI?.['#text'] ?? '',
            URIHealth: resource?.Access?.URI_Health?.['#text'] ?? '',
            DOI: resource?.Access?.DOI?.['#text'] ?? '',
            DOIHealth: resource?.Access?.DOI_Health?.['#text'] ?? '',
            RepositoryName: resource?.Repository_Name?.['#text'] ?? '',
            Licence: resource?.Access?.Licence?.['#text'] ?? '',
            ProgrammingLanguage: Array.isArray(resource?.Code?.Programing_Language)
                ? resource.Code.Programing_Language.map((lang: any) => lang['#text']).join(', ')
                : resource?.Code?.Programing_Language?.['#text'] ?? ''
        }));
        console.log("datasetResources",datasetResources);
    });
    
   // Testdaten
//    let testData = [
//   {
//     name: "Dataset A",
//     doi: "10.5061/dryad.ab12cd34",
//     language: "en",
//     date: "2022-05-01",
//     sizeMB: 12.5,
//     format: "csv",
//     license: "cc0 1.0",
//     submittedBy: "dryad"
//   },
//   {
//     name: "Dataset B",
//     doi: "10.5061/dryad.ef56gh78",
//     language: "de",
//     date: "2023-01-12",
//     sizeMB: 8.3,
//     format: "json",
//     license: "cc-by 4.0",
//     submittedBy: "dryad"
//   },
//   {
//     name: "Dataset C",
//     doi: "10.5061/dryad.xy98kl45",
//     language: "en",
//     date: "2021-09-30",
//     sizeMB: 15.0,
//     format: "csv",
//     license: "cc0 1.0",
//     submittedBy: "dryad"
//   },
//   {
//     name: "Dataset D",
//     doi: "10.5061/dryad.mq34zr12",
//     language: "fr",
//     date: "2024-03-22",
//     sizeMB: 5.7,
//     format: "xlsx",
//     license: "cc-by 4.0",
//     submittedBy: "dryad"
//   },
//   {
//     name: "Dataset E",
//     doi: "10.5061/dryad.vt67bn89",
//     language: "es",
//     date: "2020-11-05",
//     sizeMB: 9.8,
//     format: "tsv",
//     license: "cc0 1.0",
//     submittedBy: "dryad"
//   },
//   {
//     name: "Dataset F",
//     doi: "10.5061/dryad.hu23qw67",
//     language: "en",
//     date: "2023-07-18",
//     sizeMB: 22.1,
//     format: "csv",
//     license: "cc-by-sa 3.0",
//     submittedBy: "dryad"
//   }
// ];

    

    datasetTitle = metadata?.publication?.Title?.['#text'] ?? '';
</script>

<Page help={true} title="View Publication">
    <h1 class="h1">{datasetTitle}</h1>
    <div class="w-full grid grid-cols-1 gap-0">
        
            <div class="w-full flex flex-col gap-3 p-2 my-1">
                <h4 class="h4">Description</h4>
                <p class={showFullDescription ? "text" : "text line-clamp-6"}>{datasetDescription}</p>
                {#if datasetDescription && datasetDescription.length}
                    <!-- svelte-ignore a11y-click-events-have-key-events -->
                    <!-- svelte-ignore a11y-no-static-element-interactions -->
                    <span class="cursor-pointer font-semibold self-end w-16 text-xs" on:click={() => showFullDescription = !showFullDescription}>
                        {showFullDescription ? 'Show less' : 'Show more'}
                    </span>
                {/if}
                </div>
        
        
            <div class="flex flex-col p-2 my-1">
                <div class="flex flex-col gap-3">
                    <h4 class="h4">Authors</h4>
                    <span class={showAllAuthors ? "" : "line-clamp-4"}>
                        {#each datasetAuthors as author, i}
                            <span class="w-40 inline-block px-2 py-1 truncate mr-1" title={author}>
                                {author}
                            </span>
                        {/each}
                    </span>
                </div>
                {#if datasetAuthors.length > 6}
                    <!-- svelte-ignore a11y-click-events-have-key-events -->
                    <!-- svelte-ignore a11y-no-static-element-interactions -->
                    <span class="cursor-pointer font-semibold self-end text-xs" on:click={() => showAllAuthors = !showAllAuthors}>
                        {showAllAuthors ? 'Show less' : 'Show more'}
                    </span>
                {/if}
            </div>
        
    </div>
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-8">
        {#each datasetResources as dataset, i}
            <card class="card w-full flex flex-col gap-2 p-4 my-1">
                <h4 class="h4 mb-2">
                    {dataset.Name
                        ? dataset.Name
                        : dataset.DOI
                            ? dataset.DOI
                            : dataset.URI
                                ? dataset.URI
                                : `Resource ${i + 1} of Dataset ${dataset.DOI || 'unknown'}`
                    }
                </h4>
                {#if dataset.SubmissionDate}
                    <p><b>Submission Date:</b> {dataset.SubmissionDate}</p>
                {/if}
                {#if dataset.EmbargoEnd}
                    <p><b>Embargo End:</b> {dataset.EmbargoEnd}</p>
                {/if}
                {#if dataset.URI}
                    <p><b>URI:</b> <a href={dataset.URI} target="_blank" class="text-blue-600 underline">{dataset.URI}</a></p>
                {/if}
                {#if dataset.URIHealth}
                    <p><b>URI Health:</b> {dataset.URIHealth}</p>
                {/if}
                {#if dataset.DOI}
                    <p><b>DOI:</b> {dataset.DOI}</p>
                {/if}
                {#if dataset.DOIHealth}
                    <p><b>DOI Health:</b> {dataset.DOIHealth}</p>
                {/if}
                {#if dataset.RepositoryName}
                    <p><b>Repository Name:</b> {dataset.RepositoryName}</p>
                {/if}
                {#if dataset.Licence}
                    <p><b>Licence:</b> {dataset.Licence}</p>
                {/if}
                {#if dataset.ProgrammingLanguage}
                    <p><b>Programming Language:</b> {dataset.ProgrammingLanguage}</p>
                {/if}
            </card>
        {/each}
    </div>
</Page>