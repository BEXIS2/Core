<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import CurationsOverview from './CurationsOverview.svelte';
	import Curation from './Curation.svelte';

	const urlParams = new URLSearchParams(window.location.search);

	const datasetIdPar = urlParams.get('curationDatasetId');
</script>

<Page title="Curation" contentLayoutType={pageContentLayoutType.full}>
	{#if !datasetIdPar}
		<CurationsOverview />
	{:else}
		<Curation datasetId={parseInt(datasetIdPar)} />
	{/if}
</Page>
