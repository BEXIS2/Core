<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import CurationsTable from '$lib/components/curation/CurationsTable.svelte';
	import Curation from '$lib/components/curation/Curation.svelte';
	import { page } from '$app/stores';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import ExampleIntegration from './ExampleIntegration.svelte';

	// const container = document.getElementById('curation');
	// $: datasetIdPar = Number(container?.getAttribute('dataset-id'));

	$: searchParDatasetId = Number($page.url.searchParams.get('id'));

	let showExampleIntegration = false;
</script>

<Page title="Curation" contentLayoutType={pageContentLayoutType.full}>
	{#if !searchParDatasetId}
		<!-- Overview of datasets and ongoing curations -->
		<CurationsTable />
	{:else}
		{#if !showExampleIntegration}
			<!-- Standard implementation for Curation -->
			<div class="mx-auto max-w-3xl rounded border border-surface-500">
				<Curation datasetId={searchParDatasetId} />
			</div>
		{:else}
			<!-- Example for integration -->
			<ExampleIntegration datasetId={searchParDatasetId} />
		{/if}
		<!-- Toggle for showing example integration -->
		<div
			class="fixed bottom-0 left-0 z-50 w-full rounded-tr border-r border-t bg-surface-300 px-2 pt-1 text-center font-semibold opacity-50 hover:opacity-100 sm:w-auto"
		>
			<SlideToggle name="Data integration toggle" size="sm" bind:checked={showExampleIntegration}>
				Toggle example for data integration
			</SlideToggle>
		</div>
	{/if}
</Page>
