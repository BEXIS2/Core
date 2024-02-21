<script lang="ts">
	import Structure from '$lib/components/datastructure/structure/EditStructure.svelte';

	import { Spinner, Page, ErrorMessage } from '@bexis2/bexis2-core-ui';
	import {
		generate,
		save,
		get,
		getDisplayPattern,
		getStructures
	} from '$lib/components/datastructure/services';

	import type { DataStructureEditModel } from '$lib/components/datastructure/types';
	import {
		displayPatternStore,
		structureStore,
		isTemplateRequiredStore
	} from '$lib/components/datastructure/store';
	import { pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	// load attributes from div
	let container;
	let datastructureId: number = 0;

	let model: DataStructureEditModel;
	$: model;

	let init: boolean = true;
	let dataExist: boolean = false;

	async function start() {
		// get data from parent
		container = document.getElementById('datastructure');
		datastructureId = Number(container?.getAttribute('structure'));
		dataExist = container?.getAttribute('dataExist')?.toLocaleLowerCase() === 'true';
		console.log(
			"🚀 ~ file: +page.svelte:32 ~ start ~ container?.getAttribute('dataExist'):",
			container?.getAttribute('dataExist')
		);

		console.log('🚀 ~ file: +page.svelte:32 ~ start ~ dataExist:', dataExist);

		// get isTemplateRequired from settings and add it to store
		// is used by validation
		const isTemplateRequired = container?.getAttribute('isTemplateRequired') == 'true';
		isTemplateRequiredStore.set(isTemplateRequired);

		console.log('edit structure', datastructureId);

		// copy structure
		model = await get(datastructureId);

		// load sturctures for validation against existings
		const structures = await getStructures();
		structureStore.set(structures);

		// load display pattern onces for all edit types
		const displayPattern = await getDisplayPattern();
		displayPatternStore.set(displayPattern);

		// console.log('model', model);
	}

	function back() {
		init = false;
	}
</script>

<Page
	title="Data structure"
	note="generate a structure from a file."
	contentLayoutType={pageContentLayoutType.full}
	help={true}
>
	{#await start()}
		<Spinner label="the structure is loading" />
	{:then}
		{#if model}
			<Structure {model} {dataExist} on:back={back} />
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>
