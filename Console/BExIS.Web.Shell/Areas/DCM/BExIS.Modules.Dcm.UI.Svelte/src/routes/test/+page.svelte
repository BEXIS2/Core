<script lang="ts">
	import { Page, Table, type TableConfig, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import PrimaryData from '$lib/components/data/PrimaryData.svelte';
	import { host } from '@bexis2/bexis2-core-ui';
	import { writable } from 'svelte/store';
	import { createEventDispatcher } from 'svelte';

	// load attributes from div
	let container;
	let id: number | undefined = undefined;

	load();

	async function load() {
		// get data from parent
		container = document.getElementById('test');
		id = Number(container?.getAttribute('dataset'));
	}
</script>

<Page contentLayoutType={pageContentLayoutType.full}>
	{#if id}
		<PrimaryData {id} />
	{/if}
</Page>
