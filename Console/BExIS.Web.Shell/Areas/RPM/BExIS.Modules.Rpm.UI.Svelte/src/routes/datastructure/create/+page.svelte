<script lang="ts">
	import Structure from '$lib/components/datastructure/structure/CreateStructure.svelte';
	import Selection from '$lib/components/datastructure/Selection.svelte';
	import { fade } from 'svelte/transition';

	import { Spinner, Page, ErrorMessage, type helpItemType, helpStore } from '@bexis2/bexis2-core-ui';
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
	import {
		displayPatternStore,
		structureStore,
		isTemplateRequiredStore,
		isMeaningRequiredStore,
		setByTemplateStore,
		enforcePrimaryKeyStore,
		changeablePrimaryKeyStore
	} from '$lib/components/datastructure/store';
	import { pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	//help
		import { dataStructureHelp } from '../help';
		let helpItems: helpItemType[] = dataStructureHelp;


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

	let loadingMessage="the structure is loading";
	

	async function start() {

	
		helpStore.setHelpItemList(helpItems);

		// get data from parent
		container = document.getElementById('datastructure');
		entityId = Number(container?.getAttribute('dataset'));
		version = Number(container?.getAttribute('version'));
		file = '' + container?.getAttribute('file');
		datastructureId = Number(container?.getAttribute('structure'));

		if(file){
			loadingMessage ="the file "+file+" is currently being analyzed";
		}


		// get isTemplateRequired from settings and add it to store
		// is used by validation
		const isTemplateRequired = container?.getAttribute('isTemplateRequired')?.toLocaleLowerCase()=="true"?true:false;
		isTemplateRequiredStore.set(isTemplateRequired);

		// get isMeaningRequired from settings and add it to store
		// is used by validation
		const isMeaningRequired = container?.getAttribute('isMeaningRequired')?.toLocaleLowerCase()=="true"?true:false;
		isMeaningRequiredStore.set(isMeaningRequired);
		
		// get setByTemplate from settings and add it to store
		// is used by createion of variables
		const setByTemplate = container?.getAttribute('setByTemplate')?.toLocaleLowerCase()=="true"?true:false;
		setByTemplateStore.set(setByTemplate);

		// get enforcePrimaryKey from settings and add it to store
		// save structure only if pk is set
		const enforcePrimaryKey = container?.getAttribute('enforcePrimaryKey')?.toLocaleLowerCase()=="true"?true:false;
		enforcePrimaryKeyStore.set(enforcePrimaryKey);

	// get changeablePrimaryKey from settings and add it to store
	// save structure only if pk is set
	const changeablePrimaryKey = container?.getAttribute('changeablePrimaryKey')?.toLocaleLowerCase()=="true"?true:false;
	changeablePrimaryKeyStore.set(changeablePrimaryKey);

		// 2 Usecases,
		// 1. generate from file, selection needed -> load file
		// 2. create empty datastructure -> jump direct to generate
		// check if file is empty or not

		// load data from server
		console.log("🚀 ~ file: +page.svelte:69 ~ start ~ file:", file)
		if (file != '') {
			console.log('file exist', file, entityId, 0);

			model = await load(file, entityId, 0);
		} else if (datastructureId > 0) {
			console.log('copy structure');
			// copy structure
			model = await copy(datastructureId);
			selectionIsActive = false;
		} else {
			console.log('empty structure');
			model = await empty(); // empty structure
			selectionIsActive = false;
		}

		// load sturctures for validation against existings
		const structures = await getStructures();
		structureStore.set(structures);

		// load display pattern onces for all edit types
		const displayPattern = await getDisplayPattern();
		displayPatternStore.set(displayPattern);
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
	note="This page allows you to create and edit a selected data structure."
	contentLayoutType={pageContentLayoutType.full}
	help={true}
>

	{#await start()}
		<Spinner label="{loadingMessage}" />
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
