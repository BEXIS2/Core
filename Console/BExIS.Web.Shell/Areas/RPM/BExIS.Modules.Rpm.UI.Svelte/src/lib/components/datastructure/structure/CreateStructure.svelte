<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import { fade } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faArrowLeft, faSave } from '@fortawesome/free-solid-svg-icons';

	import Attributes from './Attributes.svelte';
	import Variables from './Variables.svelte';
	import { enforcePrimaryKeyStore } from '../store';

	let areVariablesValid = false;
	let areAttributesValid = false;

	const dispatch = createEventDispatcher();

	import { create } from '../services';
	import { goTo } from '$services/BaseCaller';
	import { get } from 'svelte/store';

	import type { DataStructureCreationModel } from '../types';
	import { Alert, helpStore } from '@bexis2/bexis2-core-ui';
	export let model: DataStructureCreationModel;
	let enforcePrimaryKey: boolean = get(enforcePrimaryKeyStore);

	$: isPKSet = false;

	$: model, updatePks();

	function updatePks() {
		let pktemp = false;
		model.variables?.forEach((v) => {
			if (v.isKey == true) {
				pktemp = true;
			}
		});

		isPKSet = pktemp;
	}

	async function onSaveHandler() {
		const res = await create(model);
		console.log('save', res);

		// here are 2 usecases
		// 1. from edit dataset -> return to dataset edit page
		// 2. create structure -> return to datastructure overview
		if (model.entityId > 0) {
			goTo('/dcm/edit?id=' + model.entityId);
		} else {
			goTo('/rpm/datastructure');
		}
	}

	// preview data consit also aboutd varaibles, unit, description
	// get only data from preview data, for suggestion and varaible creation
	function getDataArea() {
		let dataOnly: string[][] = [];

		if (model && model.markers) {
			const dataMarker = model.markers.find((m) => m.type == 'data');
			if (dataMarker) {
				for (let index = dataMarker.row; index < model.preview.length; index++) {
					const row = model.preview[index];
					dataOnly.push(row.split(String.fromCharCode(model.delimeter)));
				}
			}
		}
		return dataOnly;
	}

	function back() {
		goTo(document.referrer);
	}
</script>

<div>
	<div transition:fade class="flex">
		<div class="grow">
			<button
				id="back"
				title="back"
				class="btn variant-filled-warning"
				on:mouseover={() => helpStore.show('back')}
				on:click={() => back()}><Fa icon={faArrowLeft} /></button
			>
		</div>
		<div class="flex-none text-end">
			<button
				id="save"
				title="save"
				class="btn variant-filled-primary text-xl"
				on:mouseover={() => helpStore.show('save')}
				on:click={onSaveHandler}
				disabled={!areVariablesValid ||
					!areAttributesValid ||
					!((enforcePrimaryKey && isPKSet) || !enforcePrimaryKey)}><Fa icon={faSave} /></button
			>
		</div>
	</div>

	<Attributes {model} bind:valid={areAttributesValid} />
	{#if enforcePrimaryKey && model.variables.length > 0 && !isPKSet}
		<Alert message="please select a primary key" cssClass="variant-filled-warning"></Alert>
	{/if}

	<Variables
		bind:variables={model.variables}
		bind:valid={areVariablesValid}
		bind:missingValues={model.missingValues}
		data={getDataArea()}
	/>
</div>
