



<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '$services/MetadataCaller';
	import { ErrorMessage, helpStore, notificationType, Page, pageContentLayoutType, Spinner } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import	{ faDownload } from '@fortawesome/free-solid-svg-icons';


	// import { Page } from '@bexis2/bexis2-core-ui';
	import { schemaToJson, setConfigStore, setMetadataStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';
	import Forbidden from '../error/Forbidden.svelte';

	import {
		activeStore,
		showAllDescriptionsStore,
		hideStore,
		descriptionStore
	} from '$lib/components/utils/metadata/stores';
	import { faEye, faEyeSlash, faChevronUp, faChevronDown, faArrowUp } from '@fortawesome/free-solid-svg-icons';
	// import configJson from './customComponents/config.json';

	export let id: number = 3;
	export	let version: number = 0;


	let container;
	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;


	async function load() {

			container = document.getElementById('metadata');
			console.log("🚀 ~ load ~ container:", container)
			
			id = Number(container?.getAttribute('dataset'));
			version = Number(container?.getAttribute('version'));

		if (id > 0) {
			const res = await apiCalls.GetDatasetInfoById(id);

			if(res.status === 200)
		 {
					const datasetInfos = res.data;
					console.log("🚀 ~ load ~ datasetInfos:", datasetInfos)

					s = await apiCalls.GetMetadataSchema(datasetInfos.metadataStructureId);
					console.log('Schema loaded', s);

					if (id > 0) m = await apiCalls.GetMetadata(id);
					else m = schemaToJson(s);
					console.log('Metadata loaded', m);
					setMetadataStore(m);

					const configJson = await apiCalls.GetComponentConfig(datasetInfos.entityTemplateId, "view");
					setConfigStore(configJson);
			}
		}
	}

	async function DownloadMetadata(datasetId: number, versionNumber: number, format: string) {

			let type = '';
			let filename = '';	

			switch(format) {
				case "json":
				 type = 'application/json';
				 filename = 'metadata.json';
					break;
				case "xml":
				 type = 'application/xml';
				 filename = 'metadata.xml';
					break;
				case "flatten":
				 type = 'text/plain';
				 filename = 'metadata_flattened.txt';
					break;
				default:

					return;
			}
			
   let data = null;
			if(format === "json") {
				//helpStore.showNotification("Your download will start shortly. If it doesn't, please check your popup blocker settings.", notificationType.info, 5000);
				data = await apiCalls.GetMetadataAsJson(datasetId, versionNumber);
			}
			else	if(format === "xml") {
				//helpStore.showNotification("Your download will start shortly. If it doesn't, please check your popup blocker settings.", notificationType.info, 5000);
				data = await apiCalls.GetMetadataAsXml(datasetId, versionNumber);
			} else if(format === "flatten") {
				//helpStore.showNotification("Your download will start shortly. If it doesn't, please check your popup blocker settings.", notificationType.info, 5000);
				data = await apiCalls.GetMetadataAsFlattened(datasetId, versionNumber);
			}

		 if(data) {
	
					const blob = new Blob([data], { type: type });
					const url = window.URL.createObjectURL(blob);

					const a = document.createElement('a');
					a.href = url;
					a.download = filename;
					document.body.appendChild(a);
					a.click();

					a.remove();
					window.URL.revokeObjectURL(url);
			}
	}

	function downloadSectionWithCSS(elementId, filename = 'styled-section.html') {
    const element = document.getElementById(elementId);
    if (!element) return console.error('Element not found');

    // 1. Gather all style and link tags from the current page
    let cssContent = '';
    const styles = document.querySelectorAll('style, link[rel="stylesheet"]');
    styles.forEach(styleTag => {
        cssContent += styleTag.outerHTML + '\n';
    });

    // 2. Build a complete, self-contained HTML document string
    const fullHTML = `
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Downloaded Section</title>
    ${cssContent}
</head>
<body>
    ${element.outerHTML}
</body>
</html>`;

    // 3. Trigger the client-side download
    const blob = new Blob([fullHTML], { type: 'text/html' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    URL.revokeObjectURL(link.href);
}

function activateShow(key: string) {
	const element = document.getElementById(key);
	if (element) {
		element.scrollIntoView({ behavior: 'smooth', block: 'start' });
	}
}

// collapse all sections in the metadata form
	function collapseAll() {
		activeStore.subscribe((active) => {
			hideStore.update((s) => [...s, ...active]);
		})();
	}

	// Expand all sections in the metadata form
	function expandAll() {
		hideStore.set([]);
	}

</script>



<Page  contentLayoutType={pageContentLayoutType.center}  footer={false} >
	{#await load()}
		<Spinner />
	{:then}
	<div class="container">

<div class="w-full flex flex-col gap-4">
			<div>	
					<!-- Show all descriptions -->
					<div class="flex flex-col gap-2">
						<!--<button class="badge" on:click={() => showAllDescriptionsStore.update((v) => !v)}>
								{#if $showAllDescriptionsStore}
									<Fa icon={faEyeSlash} />&nbsp;Hide descriptions
								{:else}
									<Fa icon={faEye} />&nbsp;Show descriptions
								{/if}
							</button>-->

						<div class="w-full flex items-center gap-1 pr-2 text-sm">
							<!-- First block stays on the left naturally -->
							<div class="pl-2">
								<!--Collapse all sections button-->
								{#if $hideStore.length === 0}
									<button class="badge" on:click={collapseAll}>
										<Fa icon={faChevronDown} />&nbsp;Collapse all sections
									</button>
								{:else}
									<!--Expand all sections button-->
									<button class="badge" on:click={expandAll}>
										<Fa icon={faChevronUp} />&nbsp;Expand all sections
									</button>
								{/if}
							</div>

							<!-- 1. Added ml-auto to push this block all the way to the right -->
							<div class="ml-auto pr-4">
								<a href="#top" class="badge">
									Scroll to top &nbsp;<Fa icon={faArrowUp} />
								</a>
							</div>
						</div>
					</div>
					<div class="content scrollable">
						<div class="px-2" id="top">
							<ComplexComponent complexComponent={schema} path={''} />
						</div>
					</div>
				</div>	

</div>
<div class="w-full lg:w-[35%] xl:w-[25%] flex flex-col gap-3 ml-4">
    
<h3 class="h3 font-semibold text-gray-700 dark:text-gray-300  whitespace-nowrap">Metadata Overview</h3>

<p>Current version: Add version here</p>
<p>Dataset ID: {id}</p>
<p>Last modified: xx.xx.xxxx</p>
<p>Modified by: Max Mustermann</p>

    <h2 class="h3 font-semibold text-gray-700 dark:text-gray-300  whitespace-nowrap">Download Metadata</h2>
    
	<div class="flex flex-wrap gap-2">
        
		<button class="btn variant-filled-primary inline-flex items-center justify-center gap-2 whitespace-nowrap px-3 py-2 text-sm min-w-[120px]" 
                on:click={() => DownloadMetadata(id, version, "json")}>
            <Fa icon={faDownload} /><span>JSON</span>
        </button>
        
		<button class="btn variant-filled-primary inline-flex items-center justify-center gap-2 whitespace-nowrap px-3 py-2 text-sm min-w-[120px]" 
                on:click={() => DownloadMetadata(id, version, "xml")}>
            <Fa icon={faDownload} /><span>XML</span>
        </button>
        
		<button class="btn variant-filled-primary inline-flex items-center justify-center gap-2 whitespace-nowrap px-3 py-2 text-sm min-w-[120px]" 
                on:click={() => DownloadMetadata(id, version, "flatten")}>
            <Fa icon={faDownload} /><span>Text</span>
        </button>
        
		<button class="btn variant-filled-primary inline-flex items-center justify-center gap-2 whitespace-nowrap px-3 py-2 text-sm min-w-[120px]" 
                on:click={() => downloadSectionWithCSS('metadata-content', `metadata_${id}_v${version}.html`)}>
            <Fa icon={faDownload} /><span>HTML</span>
        </button>
        
    </div>
     <h2 class="h3 font-semibold text-gray-700 dark:text-gray-300  whitespace-nowrap">Content</h2>
 <nav class="list-nav">
			<ul class="list-disc space-y-2">
				{#each Object.entries(m) as [key, value]}
					{#if typeof value === 'object' && value !== null}
						<a href="#{key}" class="w-full" on:click={() => activateShow(key)}>
							<li class="flex items-center gap-1">
								<span class="h-1.5 w-1.5 rounded-full bg-gray-500 mr-2"></span>
                <span class="">{convertDisplayName(key)}</span>
							</li>
						</a>
						
					{/if}
				{/each}
			</ul>
		</nav>
	 

    
</div>
	</div>

	{:catch error}
		<ErrorMessage message={error.message} />
	{/await}	


</Page>

<style>

.container {
  display: flex;
  overflow: hidden; /* Wichtig: Der Content-Bereich selbst scrollt nicht */
		height: calc(100dvh - 180px); /* Höhe des Viewports minus Höhe des Headers */
}

.nav-left {
		width: 400px; /* Feste Breite für die Navigation */
		overflow-y: auto; /* Ermöglicht vertikales Scrollen in der Navigation */

}
	
.content {
		flex-grow: 1;
		overflow-y: auto; /* Aktiviert das unabhängige Scrollen */
}

.scrollable {
		overflow-y: auto;
		scrollbar-width: thin; /* Makes scrollbar smaller in Firefox */
		scrollbar-color: rgba(0, 0, 0, 0.3) transparent; /* Colors scrollbar */
}
</style>



