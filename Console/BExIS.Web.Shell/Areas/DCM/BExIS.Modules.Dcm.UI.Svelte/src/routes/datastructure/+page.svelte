<script lang="ts">
	import Structure from '$lib/components/datastructure/structure/Structure.svelte';
	import Selection from '$lib/components/datastructure/Selection.svelte';

	import { onMount } from 'svelte';
	import { fade } from 'svelte/transition';

	import { Spinner, Page } from '@bexis2/bexis2-core-ui';
	import {
		generate,
		save,
		load,
		getDisplayPattern,
		getStructures
	} from '$lib/components/datastructure/services';


	import type { DataStructureCreationModel } from '$lib/components/datastructure/types';
	import { displayPatternStore, structureStore } from '$lib/components/datastructure/store';
	import { pageContentLayoutType } from '@bexis2/bexis2-core-ui';


	// load attributes from div
	let container;
	let entityId: number;
	let version: number = 0;
	let file: string;

	let model: DataStructureCreationModel;
	$: model;

	let selectionIsActive = true;


	let init: boolean = true;

	onMount(async () => {
		// get data from parent
		container = document.getElementById('datastructure');
		entityId = container?.getAttribute('dataset');
		version = container?.getAttribute('version');
		file = container?.getAttribute('file');

		console.log('start structure suggestion', entityId, version, file);

		// load data from server
		model = await load(entityId, file, 0);

		// load sturctures for validation against existings
		const structures = await getStructures();
		structureStore.set(structures);

		// load display pattern onces for all edit types
		const displayPattern = await getDisplayPattern();
		displayPatternStore.set(displayPattern);

		console.log('model', model);
	});

	async function update(e) {
		console.log('update', e.detail);
		model = e.detail;

		let res = await generate(e.detail);

		if (res != false) {
			model = res;
			selectionIsActive = false;
		}
	}


	function back() {
		selectionIsActive = true;
		init = false;
	}


</script>

<Page
	title="Structure Suggestion"
	note="generate a structure from a file."
	contentLayoutType={pageContentLayoutType.full}
>
	{#if !model}
		<Spinner label="the file {file} is currently being analyzed" />
	{:else if selectionIsActive}
		<div transition:fade>
			<Selection bind:model on:saved={update} {init} />
		</div>
	{:else if model.variables.length > 0}

			<Structure {model} on:back={back}></Structure>
	{/if}
</Page>
