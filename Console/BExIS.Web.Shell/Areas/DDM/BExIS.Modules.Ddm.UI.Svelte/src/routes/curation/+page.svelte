<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import CurationsOverview from './CurationsOverview.svelte';
	import Curation from './Curation.svelte';
	import { page } from '$app/stores';
	import type { CurationEntryClass } from './CurationEntries';

	// const container = document.getElementById('curation');
	// $: datasetIdPar = Number(container?.getAttribute('dataset-id'));

	$: searchParDatasetId = Number($page.url.searchParams.get('id'));

	let jumpToEntryWhereExample: ((entry: CurationEntryClass) => boolean) | undefined = undefined;

	const exampleJumpToFunction = () => {
		// jumps to the first loaded entry, that has "value" written in the name
		jumpToEntryWhereExample = (e) => e.name.includes('value');
	};
</script>

<Page title="Curation" contentLayoutType={pageContentLayoutType.full}>
	{#if !searchParDatasetId}
		<CurationsOverview />
	{:else}
		<Curation datasetId={searchParDatasetId} jumpToEntryWhere={jumpToEntryWhereExample} />
	{/if}
</Page>
