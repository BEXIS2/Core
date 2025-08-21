<script lang="ts">
    import { Page } from '@bexis2/bexis2-core-ui';
    import * as apiCalls from './services/apiCalls';
    import { onMount } from 'svelte';
    import type { ResourceArray, LinkType } from './models';
    import ResourceLink from './components/link.svelte';

    let metadata: any;
    let datasetTitle: string;
    let datasetAuthors: string[] = [];
    let datasetDescription: string;
    let datasetComment: string;
    let datasetDataCodeAvailiabilityStatement: string[] = [];
    let datasetResources: ResourceArray[] = [];

    let showAllAuthors = false;
    let showFullDescription = false;

    let groupedResources: Record<string, ResourceArray[]> = {};

    onMount(async () => {
        metadata = await apiCalls.GetMetadata(2);
        console.log(metadata);

        datasetTitle = metadata?.publication?.Title?.['#text'] ?? 'Title not available';
        datasetAuthors = metadata?.publication?.Author?.map((author: any) => author['#text']) || ['Authors not available'];
        datasetDescription = metadata?.publication?.Abstract?.map((abstract: any) => abstract['#text'])?.join('\n\n') ?? 'Description not available';
        datasetComment = metadata?.publication?.comment?.['#text'] ?? 'Comment not available';
        datasetDataCodeAvailiabilityStatement = metadata?.publication?.Recourses?.Data_code_availiablity_statement?.map((statement: any) => statement['#text']) || ['Data code availability statement not available'];

        datasetResources = (metadata?.Resource ?? []).map((resource: any) => ({
            Name: resource?.Name?.['#text'] ?? '',
            ResourceType: resource?.Resources_Type?.['#text'] ?? '',
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

        groupedResources = datasetResources.reduce((acc: any, resource) => {
            const type = resource.ResourceType || 'Unknown';
            if (!acc[type]) acc[type] = [];
            acc[type].push(resource);
            return acc;
        }, {});
    });

function getGlobalIndex(resource: ResourceArray): number {
    return datasetResources.findIndex(r => r === resource) + 1;
}

  function isValidUrl(value: any) {
    try {
      new URL(value);
      return true;
    } catch {
      return false;
    }
  }

  function isValidDoi(doi: any) {
    return doi && doi !== "unknown" && doi.includes("/");
  }

</script>

<Page help={true} title="View Publication">
    <h1 class="h1">{datasetTitle}</h1>

    <div class="w-full grid grid-cols-1 gap-0">
        <div class="w-full flex flex-col gap-3 p-2 my-1">
            <h4 class="h4">Description</h4>
            <p class={showFullDescription ? "text" : "text line-clamp-6"}>{datasetDescription}</p>
            {#if datasetDescription && datasetDescription.length}
                <span class="cursor-pointer font-semibold self-end w-16 text-xs" on:click={() => showFullDescription = !showFullDescription}>
                    {showFullDescription ? 'Show less' : 'Show more'}
                </span>
            {/if}
        </div>

        <div class="flex flex-col p-2 my-1">
            <div class="flex flex-col gap-3">
                <h4 class="h4">Authors</h4>
                <span class={showAllAuthors ? "" : "line-clamp-4"}>
                    {#each datasetAuthors as author}
                        <span class="w-40 inline-block px-2 py-1 truncate mr-1" title={author}>
                            {author}
                        </span>
                    {/each}
                </span>
            </div>
            {#if datasetAuthors.length > 6}
                <span class="cursor-pointer font-semibold self-end text-xs" on:click={() => showAllAuthors = !showAllAuthors}>
                    {showAllAuthors ? 'Show less' : 'Show more'}
                </span>
            {/if}
        </div>
    </div>

    <div class="w-full flex flex-col gap-3 p-2 my-1">
            <h4 class="h4">Data Code Availability Statement</h4>
            <p>{datasetDataCodeAvailiabilityStatement}</p>
    </div>

    <div class="w-full flex flex-col gap-3 p-2 my-1">
            <h4 class="h4">Comment</h4>
            <p>{datasetComment}</p>
    </div>

    {#each Object.entries(groupedResources) as [type, resources]}
        <h3 class="text-xl mt-8 mb-2">{type}</h3>
        <div class="grid grid-cols-3 w-full gap-6">
            {#each resources as dataset, i}
                <div>
                    <div class="h-52 bg-gray-50 rounded-lg shadow flex flex-col gap-2">
                        <div class="h4 h-12 text-lg bg-primary-300 rounded-tl rounded-tr font-semibold text-bexis2-primary mb-2 pt-2 pb-2 pl-5 pr-5 truncate">
                            {dataset.Name
                                ? dataset.Name
                                        : `Resource ${getGlobalIndex(dataset)}`
                            }
                        </div>
                        <div class="flex flex-col gap-1 pb-5 pr-5 pl-5 text-sm text-gray-700">
                            {#if dataset.SubmissionDate}
                                <div class="font-medium text-gray-500">Submission Date: {dataset.SubmissionDate}</div>
                            {/if}
                            {#if dataset.EmbargoEnd}
                                <div class="font-medium text-gray-500">Embargo End: {dataset.EmbargoEnd}</div>
                            {/if}

                            {#if dataset.DOI && isValidDoi(dataset.DOI)}
                                <ResourceLink type="doi" dataset={dataset} />
                            {:else}
                                {#if dataset.URI && isValidUrl(dataset.URI)}
                                    <ResourceLink type="uri" dataset={dataset} /> 
                                {/if}
                            {/if}
                            {#if dataset.RepositoryName}
                                <div class="font-medium text-gray-500">Repository Name: {dataset.RepositoryName}</div>
                            {/if}
                            {#if dataset.Licence}
                                <div class="font-medium text-gray-500">Licence: {dataset.Licence}</div>
                            {/if}
                        </div>
                    </div>
                </div>
            {/each}
        </div>
    {/each}
</Page>
