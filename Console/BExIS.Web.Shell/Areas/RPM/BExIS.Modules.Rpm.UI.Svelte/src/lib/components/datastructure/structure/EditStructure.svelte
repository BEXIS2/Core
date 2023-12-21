<script lang="ts">
	import { fade } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faArrowLeft, faSave } from '@fortawesome/free-solid-svg-icons';

	import Attributes from './Attributes.svelte';
	import Variables from './Variables.svelte';

	let areVariablesValid = false;
	let areAttributesValid = false;

	import { save } from '../services';
	import { goTo } from '$services/BaseCaller';

	import type { DataStructureEditModel } from '../types';
	export let model: DataStructureEditModel;
	export let dataExist:boolean = false;
	$: model;

	async function onSaveHandler() {
		const res = await save(model);
		console.log('save', res);
		goTo('/rpm/datastructure');
		
	}

	function back() {
		goTo(document.referrer);
	}
</script>

<div>
	<div transition:fade class="flex">
		<div class="grow">
			<button title="back" class="btn variant-filled-warning" on:click={() => back()}
				><Fa icon={faArrowLeft} /></button
			>
		</div>
		<div class="flex-none text-end">
			<button
				title="save"
				class="btn variant-filled-primary text-xl"
				on:click={onSaveHandler}
				disabled={!areVariablesValid || !areAttributesValid}><Fa icon={faSave} /></button
			>
		</div>
	</div>
	<Attributes {model} bind:valid={areAttributesValid} />
	<Variables
		bind:variables={model.variables}
		bind:valid={areVariablesValid}
		bind:missingValues={model.missingValues}
		{dataExist}
		data={[]}
	/>
</div>
