<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import CurationsOverview from './CurationsOverview.svelte';
	import Curation from './Curation.svelte';
	import { page } from '$app/stores';
	import { curationDataSetIdSearchParam } from './types';

	let container = document.getElementById('curation');
	let datasetIdPar = Number(container?.getAttribute('dataset-id'));

	console.log('datasetId:', datasetIdPar);
	// $page.url.searchParams.get(curationDataSetIdSearchParam);
</script>

<Page title="Curation" contentLayoutType={pageContentLayoutType.full}>
	{#if datasetIdPar === 0}
		<CurationsOverview bind:datasetId={datasetIdPar} />
	{:else}
		<Curation bind:datasetId={datasetIdPar} />
	{/if}
</Page>
