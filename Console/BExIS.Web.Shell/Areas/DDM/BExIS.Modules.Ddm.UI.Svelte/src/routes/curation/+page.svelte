<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import CurationsOverview from './CurationsOverview.svelte';
	import Curation from './Curation.svelte';
	import { page } from '$app/stores';
	import { curationDataSetIdSearchParam } from './types';

	$: datasetIdPar = $page.url.searchParams.get(curationDataSetIdSearchParam);
</script>

<Page title="Curation" contentLayoutType={pageContentLayoutType.full}>
	{#if !datasetIdPar}
		<CurationsOverview />
	{:else}
		<Curation datasetId={parseInt(datasetIdPar)} />
	{/if}
</Page>
