<script lang="ts">
	import Variable from './variable/Variable.svelte';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { onMount } from 'svelte';
	import { getDataTypes, getUnits } from '$services/StructureSuggestionCaller';
	import type { VariableModel, missingValueType } from '$models/StructureSuggestion';
	import { Modal, modalStore } from '@skeletonlabs/skeleton';
	import type { ModalSettings } from '@skeletonlabs/skeleton';

  import Fa from 'svelte-fa/src/fa.svelte'
  import {faShare, faShareFromSquare} from '@fortawesome/free-solid-svg-icons'

	export let variables: VariableModel[] = [];
	export let missingValues: missingValueType[] = [];
	export let data: string[][];

	$: datatypes = null;
	$: units = null;
	$: variables;

	// validation array
	let variableValidationStates = [];

	export let valid = true;

	onMount(async () => {
		datatypes = await getDataTypes();
		units = await getUnits();

		// console.log("datatypes",datatypes);
		// console.log("units", units);
		// console.log("units", missingValues);

		fillVariableValdationStates(variables);
	});

	function fillVariableValdationStates(vars) {
		for (let index = 0; index < vars.length; index++) {
			variableValidationStates.push(false);
		}
	}

	// every time when validation state of a varaible is change,
	// this function triggered an check wheter save button can be active or not
	function checkValidationState() {
		valid = variableValidationStates.every((v) => v === true);
	}

	function getColumnData(cellIndex) {
		let cValues: string[] = [];
		for (let index = 0; index < data.length; index++) {
			const c = data[index][cellIndex];
			cValues.push(c);
		}
		return cValues;
	}

	// copy data from varaible on index i to the next one
	function copyNext(i) {
		const modal: ModalSettings = {
			type: 'confirm',
			title: 'Copy',
			body: 'Are you sure you wish to copy the current variable to the next?',
			// TRUE if confirm pressed, FALSE if cancel pressed
			response: (r: boolean) => {
				if (r === true) {

					if (variables.length >= i + 1) {
						const v = variables[i];
						const nextIndex = i + 1;
						let v2 = variables[nextIndex];
						v2 = updateVariableFromOther(v, v2);
						variables[nextIndex] = v2;
					}
				}
			}
		};

		modalStore.trigger(modal);
	}

	// copy data from varaible on index i to all next
	function copyAll(i) {
		const modal: ModalSettings = {
			type: 'confirm',
			title: 'Copy',
			body: 'Are you sure you wish to copy the current variable to all after?',
			// TRUE if confirm pressed, FALSE if cancel pressed
			response: (r: boolean) => {
				if (r === true) {

					if (variables.length >= i + 1) {
						const v = variables[i];
						const start = i + 1;
						for (let index = start; index < variables.length; index++) {
							let v2 = variables[index];
							v2 = updateVariableFromOther(v, v2);
							variables[index] = v2;
						}
					}
				}
			}
		};

		modalStore.trigger(modal);
	}

	/*
*
    isKey: boolean;
    isOptional: boolean;
    dataType: ListItem;
    unit: ListItem;
    template: ListItem;
    displayPattern: ListItem | undefined;
*/

	function updateVariableFromOther(from, to) {
		if (from && to) {
			to.description = from.description;
			to.dataType = from.dataType;
			to.unit = from.unit;
			to.template = from.template;
			to.displayPattern = from.displayPattern;
		}

		return to;
	}
</script>

<div class="flex-col space-y-5 mt-5">
	{#if variables && datatypes && units && variableValidationStates && missingValues}
		<!-- else content here -->
		{#each variables as variable, i (variable.name)}
			<!-- content here -->
			<Variable
				bind:variable
				index={i}
				on:var-change={checkValidationState}
				{datatypes}
				{units}
				bind:isValid={variableValidationStates[i]}
				bind:missingValues
				data={getColumnData(i)}
				on:copy-next={copyNext}
				on:copy-all={copyAll}
			>
      <svelte:fragment slot="options">
        {#if (variables.length>0) && i < variables.length-1}
        
          <button type="button" title="copy to next" class="chip variant-filled-warning" on:click={()=>copyNext(i)}><Fa icon={faShare}/></button>
          <button type="button" title="copy to all after this" class="chip variant-filled-warning" on:click={()=>copyAll(i)}><Fa icon={faShareFromSquare}/></button>
 
        {/if}
      </svelte:fragment>
      </Variable>
		{/each}
	{:else}
		<Spinner label="loading suggested structure" />
	{/if}
</div>

<Modal />