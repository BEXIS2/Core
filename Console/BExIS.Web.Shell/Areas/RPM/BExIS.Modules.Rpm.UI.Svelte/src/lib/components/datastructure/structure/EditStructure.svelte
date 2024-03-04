<script lang="ts">
	import { fade } from 'svelte/transition';
	import Fa from 'svelte-fa';
	import { faArrowLeft, faSave } from '@fortawesome/free-solid-svg-icons';

	import Attributes from './Attributes.svelte';
	import Variables from './Variables.svelte';




	import { save } from '../services';
	import { goTo } from '$services/BaseCaller';
	import { get } from 'svelte/store';

	import type { DataStructureEditModel } from '../types';
	import { enforcePrimaryKeyStore } from '../store';
	import { Alert } from '@bexis2/bexis2-core-ui';


	export let model: DataStructureEditModel;
	export let dataExist: boolean = false;


	let areVariablesValid = false;
	let areAttributesValid = false;
	let enforcePrimaryKey: boolean = get(enforcePrimaryKeyStore);
	let isPKSet: boolean = false;

	$: model, checkPK();

	function checkPK()
	{
			model.variables?.forEach(v=> {

				if(v.isKey == true) 
				{
					isPKSet = true;
						return;
				}
			})

			isPKSet = false;
	}
	
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
				disabled={!areVariablesValid || !areAttributesValid || !((enforcePrimaryKey && isPKSet) ||  !enforcePrimaryKey)  }><Fa icon={faSave} /></button
			>
		</div>
	</div>
	{#if enforcePrimaryKey}
		<Alert message="please select a primary key" cssClass="variant-filled-warning"></Alert>
	{/if}
	<Attributes {model} bind:valid={areAttributesValid} />
	{#if enforcePrimaryKey && model.variables.length>0}
			<Alert message="please select a primary key" cssClass="variant-filled-warning"></Alert>
	{/if}
	<Variables
		bind:variables={model.variables}
		bind:valid={areVariablesValid}
		bind:missingValues={model.missingValues}
		{dataExist}
		data={[]}
	/>
</div>
