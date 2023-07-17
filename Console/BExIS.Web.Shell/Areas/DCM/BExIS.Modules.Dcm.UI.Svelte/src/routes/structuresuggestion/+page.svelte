<script lang="ts">
	import Suggestion from './Suggestion.svelte';
	import Selection from './Selection.svelte';
	import StructureAttributes from './StructureAttributes.svelte';

	import Fa from 'svelte-fa/src/fa.svelte';
	import { faSave } from '@fortawesome/free-regular-svg-icons';
	import { faArrowLeft } from '@fortawesome/free-solid-svg-icons';

	import { onMount } from 'svelte';
	import { fade } from 'svelte/transition';

	import { Spinner, Page } from '@bexis2/bexis2-core-ui';
	import { generate, save, load, getDisplayPattern,getStructures } from '$services/StructureSuggestionCaller';
	import { goTo } from '$services/BaseCaller';

	import type { StructureSuggestionModel } from '$models/StructureSuggestion';
	import { displayPatternStore, structureStore } from './store';
	

	// load attributes from div
	let container;
	let id: number;
	let version: number = 0;
	let file: string;

	let model: StructureSuggestionModel;
	$: model;

	let selectionIsActive = true;

	let areVariablesValid = false;
	let areAttributesValid = false;

	let init:boolean = true;

	onMount(async () => {
		// get data from parent
		container = document.getElementById('structuresuggestion');
		id = container?.getAttribute('dataset');
		version = container?.getAttribute('version');
		file = container?.getAttribute('file');

		console.log('start structure suggestion', id, version, file);


		// load data from server
		model = await load(id, file, 0);

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

	async function onSaveHandler() {
		const res = await save(model);
		console.log('save', res);

		goTo('/dcm/edit?id=' + model.id);
	}
 
	function back()
	{
	  selectionIsActive = true;
			init = false;
	}
</script>

<Page title="Structure Suggestion" note="generate a structure from a file.">
	{#if !model}
		<Spinner label="the file {file} is currently being analyzed"/>
	{:else if selectionIsActive}
		<div transition:fade>
			<Selection {model} on:saved={update} {init}/>
		</div>
	{:else if model.variables.length > 0}
		<div transition:fade>
			<div>
				<button on:click={()=>back()}><Fa icon={faArrowLeft} /></button>

				<div class="text-end">
					<button
						color="primary"
						on:click={onSaveHandler}
						disabled={!areVariablesValid || !areAttributesValid}><Fa icon={faSave} /></button
					>
				</div>
			</div>

			<StructureAttributes {model} bind:valid={areAttributesValid} />
			<Suggestion variables={model.variables} bind:valid={areVariablesValid} />
		</div>
	{/if}
</Page>
