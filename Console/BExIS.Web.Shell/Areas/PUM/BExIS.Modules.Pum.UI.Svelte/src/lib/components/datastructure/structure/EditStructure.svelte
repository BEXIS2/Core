<script lang="ts">
	import { fade } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faArrowLeft, faSave, faBinoculars } from '@fortawesome/free-solid-svg-icons';

	import Attributes from './Attributes.svelte';
	import Variables from './Variables.svelte';

	import { notificationType } from '@bexis2/bexis2-core-ui';
	import { notificationStore } from '@bexis2/bexis2-core-ui';

	import { save, checkPrimaryKeySet } from '../services';
	import { goTo } from '$services/BaseCaller';
	import { get } from 'svelte/store';

	import type { DataStructureEditModel } from '../types';
	import { enforcePrimaryKeyStore } from '../store';
	import { Alert, helpStore } from '@bexis2/bexis2-core-ui';

	export let model: DataStructureEditModel;
	export let dataExist: boolean = false;

	let areVariablesValid = false;
	let areAttributesValid = false;
	let enforcePrimaryKey: boolean = get(enforcePrimaryKeyStore);
	let isPKSet: boolean = false;

	$: model, updatePks();

	// defalut pk set
	const initPks: number[] = setInitPks();
	let currentPks: number[] = initPks;
	$: pksHasChanged = false;
	$: pksValid = false;

	function setInitPks(): number[] {
		let ids: number[] = [];
		model.variables?.forEach((v) => {
			if (v.isKey == true) {
				ids = [...ids, v.id];
			}
		});

		return ids;
	}

	function updatePks() {
		currentPks = [];
		let pktemp = false;
		model.variables?.forEach((v) => {
			if (v.isKey == true) {
				pktemp = true;
				currentPks = [...currentPks, v.id];
			}
		});

		isPKSet = pktemp;
		console.log('🚀 ~ isPKSet:', isPKSet);

		pksHasChanged = arraysAreEqual(initPks, currentPks) ? false : true;
		pksValid = !pksHasChanged; // reset if pks has changed

		console.log('🚀 ~ pksHasChanged:', pksHasChanged, currentPks, initPks);
	}

	async function onSaveHandler() {
		const res = await save(model);
		console.log('save', res);
		console.log('🚀 ~ back ~ goTo(document.referrer);:', document.referrer);

		goTo(document.referrer);
	}

	function back() {
		goTo(document.referrer);
	}

	async function onCheckPKHandler() {
		console.log('🚀 ~ currentPks:', currentPks);
		const res = await checkPrimaryKeySet(model.id, currentPks);
		console.log('🚀 ~ res:', res);
		pksValid = res;

		if (pksValid) {
			notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Primary key set is unique'
			});
		} else {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message:
					'The selected primary key was checked against the existing data of the datasets and is not unique, therefore the structure cannot be saved.'
			});
		}
	}

	function arraysAreEqual(arr1, arr2) {
		if (arr1.length !== arr2.length) {
			return false;
		}

		for (let i = 0; i < arr1.length; i++) {
			if (arr1[i] !== arr2[i]) {
				return false;
			}
		}

		return true;
	}
</script>

<div>
	<div transition:fade class="flex px-2">
		<div class="grow">
			<button title="back" class="btn variant-filled-warning" on:click={() => back()}
				><Fa icon={faArrowLeft} /></button
			>
		</div>
		<div class="flex-none text-end">
			{#if pksHasChanged && !pksValid}
				<button
					id="check"
					title="Check changed primary key against datasets that belong to the data structure."
					class="btn variant-filled-error text-xl"
					on:mouseover={() => helpStore.show('check')}
					on:focus={() => helpStore.show('check')}
					on:click={onCheckPKHandler}
					on:keypress={onCheckPKHandler}><Fa icon={faBinoculars} /></button
				>
			{/if}
			<button
				title="save"
				class="btn variant-filled-primary text-xl"
				on:click={onSaveHandler}
				on:keypress={onSaveHandler}
				disabled={!areVariablesValid ||
					!areAttributesValid ||
					!((enforcePrimaryKey && isPKSet) || !enforcePrimaryKey) ||
					(pksHasChanged && !pksValid)}><Fa icon={faSave} /></button
			>
		</div>
	</div>

	<Attributes {model} bind:valid={areAttributesValid} />
	<div class="px-2">
		{#if enforcePrimaryKey && model.variables.length > 0 && currentPks.length == 0}
			<Alert message="Please select a (combined) primary key." cssClass="variant-filled-warning"
			></Alert>
		{/if}
		{#if model.variables.length == currentPks.length}
			<Alert cssClass="variant-filled-warning">
				By selecting all variables as part of the primary key, it is impossible to update the data.
				At least one column must remain as a value.
			</Alert>
		{/if}
	</div>
	<Variables
		bind:variables={model.variables}
		bind:valid={areVariablesValid}
		bind:missingValues={model.missingValues}
		{dataExist}
		data={[]}
	/>
</div>
