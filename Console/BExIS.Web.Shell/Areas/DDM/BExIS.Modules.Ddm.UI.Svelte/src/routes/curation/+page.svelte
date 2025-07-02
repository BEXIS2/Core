<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import CurationsOverview from './CurationsOverview.svelte';
	import Curation from './Curation.svelte';
	import { page } from '$app/stores';

	// const container = document.getElementById('curation');
	// $: datasetIdPar = Number(container?.getAttribute('dataset-id'));

	$: searchParDatasetId = Number($page.url.searchParams.get('id'));
</script>

<Page title="Curation" contentLayoutType={pageContentLayoutType.full}>
	{#if !searchParDatasetId}
		<CurationsOverview />
	{:else}
		<Curation datasetId={searchParDatasetId} />
	{/if}
</Page>
