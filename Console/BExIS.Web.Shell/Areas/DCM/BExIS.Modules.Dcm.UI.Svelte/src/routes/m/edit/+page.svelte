<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';

	import * as apiCalls from '$services/MetadataCaller';
	import {
		helpStore,
		notificationType,
		Page,
		pageContentLayoutType,
		Spinner
	} from '@bexis2/bexis2-core-ui';
	import Functions from './MetadataFunctions.svelte';
	import Header from './MetadataHeader.svelte';

	// import { Page } from '@bexis2/bexis2-core-ui';
	import {
		clearValidationStore,
		getValidationStore,
		schemaToJson,
		setConfigStore,
		setMetadataStore,
		setSystemMappingsStore
	} from '$lib/components/utils/metadata/metadataComponentUtils';
	import type { SystemMappingEditModel } from '$lib/components/utils/metadata/models';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import MetadataHeader from './MetadataHeader.svelte';

	// import active and hide store for metadata component
	import {
		activeStore,
		showAllDescriptionsStore,
		hideStore,
		descriptionStore
	} from '$lib/components/utils/metadata/stores';
	import {
		faEye,
		faEyeSlash,
		faChevronUp,
		faChevronDown,
		faArrowUp
	} from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';
	// import configJson from './customComponents/config.json';

	export let id: number = 3;
	export let saveWithError: boolean = true;

	let container;
	let s: any;
	let m: any = null;
	let schema: any = s;
	let reload = false;
	$: schema = s;

	let description: string = '';

	async function load() {
		container = document.getElementById('metadata');
		console.log('🚀 ~ load ~ container:', container);

		id = Number(container?.getAttribute('dataset'));
		saveWithError = container?.getAttribute('saveWithError') === 'true';

		// read id from url
		//datasetId = Number(new URLSearchParams(window.location.search).get('id'));
		console.log('Loading metadata for datasetId:', id);
		if (id > 0) {
			let result = await apiCalls.GetDatasetInfoById(id);
			const datasetInfos = result.data;
			 console.log('Dataset infos loaded', datasetInfos);
			s = await apiCalls.GetMetadataSchema(datasetInfos.metadataStructureId);
			console.log('Schema loaded', s);

			if (id > 0) m = await apiCalls.GetMetadata(id);
			else m = schemaToJson(s);
			console.log('Metadata loaded', m);
			setMetadataStore(m);
			const configJson = await apiCalls.GetComponentConfig(datasetInfos.entityTemplateId, 'edit');
			setConfigStore(configJson);
			console.log('🚀 ~ load ~ configJson:', configJson);

			const templateJson = await apiCalls.GetTemplateConfig(datasetInfos.entityTemplateId);
			console.log('🚀 ~ load ~ templateJson:', templateJson);
			saveWithError = templateJson?.metadataInvalidSaveMode ?? true;
			console.log('🚀 ~ load ~ saveWithError:', saveWithError);

			const systemMappings: SystemMappingEditModel = await apiCalls.GetSystemMappings(
				datasetInfos.metadataStructureId
			);
			console.log('🚀 ~ load ~ systemMappings:', systemMappings);
			setSystemMappingsStore(systemMappings);

			let v = getValidationStore();
			//clearValidationStore();

			const res = suite('');
			console.log('🚀 ~ load ~ res:', res.isValid());
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

<Page contentLayoutType={pageContentLayoutType.center} footer={false}>
	{#await load()}
		<Spinner />
	{:then}
		{#key reload}
			<div class="container">
				<div class="nav-left scrollable">
					{#if m}
						<Functions
							bind:metadata={m}
							{saveWithError}
							bind:datasetId={id}
							on:metadataUpdated={() => (reload = !reload)}
						/>
					{/if}
				</div>

				<div class="w-full flex flex-col gap-4">
					<MetadataHeader bind:metadata={m} {saveWithError} bind:datasetId={id} />
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
				<div
					class="justify-end gap-3 pr-5 text-sm w-[40%] ml-2 min-h-[100px] card dark:bg-secondary-800 p-3 min-w-0 break-words"
				>
					<p class="text-sm text-gray-900 dark:text-gray-400 pb-2">
						Move your cursor over a field or section header to see its description (if available).
					</p>
					<hr />

					{#if $descriptionStore && $descriptionStore.path?.length > 0 && typeof $descriptionStore.content === 'string'}
						<!-- 1. Split the path and determine if the last element is a number -->
						{@const parts = $descriptionStore.path.split('.')}
						{@const lastItem = parts[parts.length - 1] ?? ''}
						{@const isNumeric = lastItem !== '' && !isNaN(lastItem)}

						<!-- 2. Pick the target item based on the numeric check -->
						{@const targetItem = isNumeric ? (parts[parts.length - 2] ?? '') : lastItem}

						<div class="pt-2">
							{#if $descriptionStore.type === 'simple'}
								<h4 class="h4 mb-2">
									Field Description for <em><b>{convertDisplayName(targetItem, false)}</b></em>
								</h4>
								<p class="">{@html $descriptionStore.content}</p>
							{:else if $descriptionStore.type === 'complex'}
								<h4 class="h4 mb-2">
									Section Description for <em><b>{convertDisplayName(targetItem, true)}</b></em>
								</h4>
								<p class="">{@html $descriptionStore.content}</p>
							{/if}
						</div>
					{/if}
				</div>
			</div>
		{/key}
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
