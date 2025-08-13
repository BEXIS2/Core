<script lang="ts">
    import type { ResourceArray, types } from '../models';

	export let dataset: ResourceArray;

    export let type: string = "uri";

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
        const parts = doi.split('/');
        return parts.length >= 4 ? parts.slice(3).join('/') : '';
    }
</script>

<div class="flex w-full mb-1">
    {#if type == "uri"}

        <div class="shadow w-11 border border-gray-300 rounded-l px-2 py-1"
            class:bg-success-300={dataset.URIHealth === 'yes' || dataset.URIHealth === 'true'}
            class:bg-error-300={dataset.URIHealth === 'no' || dataset.URIHealth === 'false' || dataset.URIHealth === 'no data made available online'}
            class:bg-surface-300={dataset.URIHealth !== 'yes' && dataset.URIHealth !== 'true' && dataset.URIHealth !== 'no' && dataset.URIHealth !== 'false' && dataset.URIHealth !== 'no data made available online'}
        > 
            <span class="font-medium text-gray-500">URI</span>
        </div>
        <div class="shadow bg-surface-200 border-t border-b border-r border-gray-200 rounded-r px-2 py-1">
            <a
                href={dataset.URI}
                target="_blank"
                class="text-bexis2-primary underline break-all"
            >
                {shortenUri(dataset.URI)}
            </a>
        </div>
    {:else if type == "doi"}
        <div class="shadow w-11 border border-gray-300 rounded-l px-2 py-1"
            class:bg-success-300={dataset.DOIHealth === 'yes' || dataset.DOIHealth === 'true'}
            class:bg-error-300={dataset.DOIHealth === 'no' || dataset.DOIHealth === 'false' || dataset.DOIHealth === 'no data made available online'}
            class:bg-surface-300={dataset.DOIHealth !== 'yes' && dataset.DOIHealth !== 'true' && dataset.DOIHealth !== 'no' && dataset.DOIHealth !== 'false' && dataset.DOIHealth !== 'no data made available online'}
        >  
            <span class="font-medium text-gray-500">DOI</span>
        </div>
        <div class="shadow bg-surface-200 border-t border-b border-r border-gray-200 rounded-r px-2 py-1">
            <a
                href={"https://doi.org/" + shortenDoi(dataset.DOI)}
                target="_blank"
                class="text-bexis2-primary underline break-all"
            >
                {shortenDoi(dataset.DOI)}
            </a>
        </div>
    {/if}
</div>