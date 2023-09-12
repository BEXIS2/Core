<script lang="ts">
	import Structure from '$lib/components/datastructure/structure/Structure.svelte';
	import Selection from '$lib/components/datastructure/Selection.svelte';

	import { onMount } from 'svelte';
	import { fade } from 'svelte/transition';

	import { Spinner, Page, ErrorMessage } from '@bexis2/bexis2-core-ui';
	import {
		generate,
		save,
		load,
		empty,
		copy,
		getDisplayPattern,
		getStructures
	} from '$lib/components/datastructure/services';

	import type { DataStructureCreationModel } from '$lib/components/datastructure/types';
	import { displayPatternStore, structureStore } from '$lib/components/datastructure/store';
	import { pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	// load attributes from div
	let container;
	let entityId: number;
	let datastructureId: number = 0;
	let version: number = 0;
	let file: string;

	let model: DataStructureCreationModel;
	$: model;

	let selectionIsActive = true;
	let init: boolean = true;

	async function start() {
		// get data from parent
		container = document.getElementById('datastructure');
		entityId = container?.getAttribute('dataset');
		version = container?.getAttribute('version');
		file = container?.getAttribute('file');
		datastructureId = container?.getAttribute('structure');

		console.log('start structure suggestion', entityId, version, file, datastructureId);

		// 2 Usecases,
		// 1. generate from file, selection needed -> load file
		// 2. create empty datastructure -> jump direct to generate
		// check if file is empty or not

		// load data from server
		if (file!="") {
			console.log("file exist",file, entityId, 0);
			
			model = await load(file, entityId, 0);
		} else if (datastructureId > 0) {
			console.log("copy structure");
			// copy structure
			model = await copy(datastructureId);
			selectionIsActive = false;
		} else {
			console.log("empty structure");
			model = await empty(); // empty structure
			selectionIsActive = false;
		}

		// load sturctures for validation against existings
		const structures = await getStructures();
		structureStore.set(structures);

		// load display pattern onces for all edit types
		const displayPattern = await getDisplayPattern();
		displayPatternStore.set(displayPattern);

		console.log('model', model);
	}

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
	title="Data structure"
	note="generate a structure from a file."
	contentLayoutType={pageContentLayoutType.full}
>
	{#await start()}
		<Spinner label="the file {file} is currently being analyzed" />
	{:then}
		{#if model}
			{#if selectionIsActive}
				<div transition:fade>
					<Selection bind:model on:saved={update} {init} />
				</div>
			{:else if model.variables}
				<Structure {model} on:back={back} />
			{/if}
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>