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

    let groupedResources: Record<string, ResourceArray[]> = {};

    onMount(async () => {
        metadata = await apiCalls.GetMetadata(1);
        console.log(metadata);

        datasetTitle = metadata?.publication?.Title?.['#text'] ?? 'Title not available';
        datasetAuthors = metadata?.publication?.Author?.map((author: any) => author['#text']) || ['Authors not available'];
        datasetDescription = metadata?.publication?.Abstract?.map((abstract: any) => abstract['#text'])?.join('\n\n') ?? 'Description not available';

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

    function shortenUri(uri: string): string {
    try {
        const url = new URL(uri);
        const host = url.hostname.replace(/^www\./, ''); // z. B. "www.example.com" → "example.com"
        const path = url.pathname.replace(/^\/+/, '');    // führende Slashes entfernen
        return path ? `${host}/${path}` : host;
    } catch {
        return uri; // Wenn keine gültige URL → gib original zurück
    }
}

function shortenDoi(doi: string): string {
    return doi.replace(/^(https?:\/\/)?(dx\.)?doi\.org\//, '').replace(/^doi:/, '');
}


function getGlobalIndex(resource: ResourceArray): number {
    return datasetResources.findIndex(r => r === resource) + 1;
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

    {#each Object.entries(groupedResources) as [type, resources]}
        <h3 class="text-xl mt-8 mb-2">{type}</h3>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            {#each resources as dataset, i}
                <div class="bg-gray-50 rounded-lg shadow flex flex-col gap-2 p-5 border border-gray-100">
                    <h4 class="text-lg font-semibold text-bexis2-primary mb-2 truncate">
                        {dataset.Name
                            ? dataset.Name
                                    : `Resource ${getGlobalIndex(dataset)}`
                        }
                    </h4>
                    <div class="flex flex-col gap-1 text-sm text-gray-700">
                        {#if dataset.SubmissionDate}
                            <div><span class="font-medium text-gray-500">Submission Date:</span> {dataset.SubmissionDate}</div>
                        {/if}
                        {#if dataset.EmbargoEnd}
                            <div><span class="font-medium text-gray-500">Embargo End:</span> {dataset.EmbargoEnd}</div>
                        {/if}
                        {#if dataset.URI}
                            <div>
                                <span class="font-medium text-gray-500">URI:</span>
                                <a href={dataset.URI} target="_blank" class="text-bexis2-primary underline break-all">{shortenUri(dataset.URI)}</a>
                            </div>
                        {/if}
                        {#if dataset.URIHealth}
                            <div><span class="font-medium text-gray-500">URI Health:</span> {dataset.URIHealth}</div>
                        {/if}
                        {#if dataset.DOI}
                <div>
                    <span class="font-medium text-gray-500">DOI:</span>
                    <a
                        href={"https://doi.org/" + shortenDoi(dataset.DOI)}
                        target="_blank"
                        class="text-bexis2-primary underline break-all"
                    >
                        {shortenDoi(dataset.DOI)}
                    </a>
                </div>
{/if}
                        {#if dataset.DOIHealth}
                            <div><span class="font-medium text-gray-500">DOI Health:</span> {dataset.DOIHealth}</div>
                        {/if}
                        {#if dataset.RepositoryName}
                            <div><span class="font-medium text-gray-500">Repository Name:</span> {dataset.RepositoryName}</div>
                        {/if}
                        {#if dataset.Licence}
                            <div><span class="font-medium text-gray-500">Licence:</span> {dataset.Licence}</div>
                        {/if}
                        {#if dataset.ProgrammingLanguage}
                            <div><span class="font-medium text-gray-500">Programming Language:</span> {dataset.ProgrammingLanguage}</div>
                        {/if}
                    </div>
                </div>
            {/each}
        </div>
    {/each}
</Page>
