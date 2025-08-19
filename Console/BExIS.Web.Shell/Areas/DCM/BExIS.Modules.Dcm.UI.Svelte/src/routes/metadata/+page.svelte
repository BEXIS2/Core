<script lang="ts">
	import { onMount } from 'svelte';
	import ComplexComponent from './components/edit/complexComponentWrapper.svelte';

	import * as apiCalls from './services/apiCalls';
	import { helpStore, Spinner } from '@bexis2/bexis2-core-ui';

	import { Page } from '@bexis2/bexis2-core-ui';
	import { metadataStore } from './stores';
	import { schemaToJson } from './utils';

	export let schemaId: number = 2;
	export let datasetId: number = 0;

	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;

	async function load() {
		if (schemaId > 0) {
			s = await apiCalls.GetMetadataSchema(schemaId);
			if (datasetId > 0) m = await apiCalls.GetMetadata(datasetId);
			else m = schemaToJson(s);
			console.log('Metadata loaded', m);
			metadataStore.set(m);
		}
	}
</script>

<Page>
	<h1 class="h1">Metadata</h1>
	{#await load()}
		<Spinner />
	{:then}
		<div class="p-2">
			<ComplexComponent complexComponent={s} path={''} />
		</div>
	{/await}
</Page>
