<script lang="ts">
	import Variable from './variable/Variable.svelte';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { onMount } from 'svelte';
	import { getDataTypes, getUnits } from '../services';
	import type { missingValueType } from '../types';
	import { VariableModel } from '../types';
	import { Modal, modalStore } from '@skeletonlabs/skeleton';
	import type { ModalSettings } from '@skeletonlabs/skeleton';

	import Fa from 'svelte-fa';
	import { faShare, faShareFromSquare, faMaximize, faMinimize, faAdd, faTrash, faCopy, faAngleUp, faAngleDown } from '@fortawesome/free-solid-svg-icons';

	export let variables: VariableModel[] = [];
	export let missingValues: missingValueType[] = [];
	export let data: string[][];

	$: datatypes = null;
	$: units = null;
	$: variables;

	let expandAll=true;

	// validation array
	let variableValidationStates:any = [];

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

	function addFn()
	{
		 let newVariable:VariableModel = new VariableModel();
			variables = [...variables, newVariable];
	}

	function copyFn(i)
	{

	}

	function deleteFn(i)
	{
		const deleteVar = variables[i];

			const confirm: ModalSettings = {
				type: 'confirm',
				title: 'Delete Unit',
				body:
					'Are you sure you wish to delete the variable "' + deleteVar.name +'"?',
				// TRUE if confirm pressed, FALSE if cancel pressed
				response: (r: boolean) => {
					
		 			variables = variables.filter(v=>v != deleteVar);
				}
			};
			modalStore.trigger(confirm);
		
	}

	function upFn(i)
	{
		  const varTemp = variables[i];
				variables[i] = variables[i-1];
				variables[i-1] = varTemp;

	}

	function downFn(i)
	{
				const varTemp = variables[i];
				variables[i] = variables[i+1];
				variables[i+1] = varTemp;
		 
	}

</script>

<button class="btn variant-filled-secondary" on:click={()=> expandAll = !expandAll}>
		{#if expandAll}
			<Fa icon={faMinimize}/>
	 {:else}
		<Fa icon={faMaximize}/>
		{/if}
</button>

<div class="flex-col space-y-2 mt-5">

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
				expand = {expandAll}
			>
				<svelte:fragment slot="options">
					{#if variables.length > 0 && i < variables.length - 1}
						<button
							id="copy-next-{i}"
							type="button"
							title="copy to next"
							class="chip variant-filled-warning"
							on:click={() => copyNext(i)}><Fa icon={faShare} /></button
						>
						<button
						 id="copy-all-{i}"
							type="button"
							title="copy to all after this"
							class="chip variant-filled-warning"
							on:click={() => copyAll(i)}><Fa icon={faShareFromSquare} /></button
						>
					{/if}
				</svelte:fragment>
				<svelte:fragment slot="list-options">
			
						<button id="delete-{i}" class="chip variant-filled-error" on:click={()=>deleteFn(i)}><Fa icon="{faTrash}"></Fa></button>

						{#if i > 0}
						<button id="up-{i}" class="chip variant-filled-surface" on:click={()=>upFn(i)}><Fa icon="{faAngleUp}"></Fa></button>
						{:else}
						<button id="up-{i}" class="chip variant-filled-surface disbaled" disabled on:click={()=>upFn(i)}><Fa icon="{faAngleUp}"></Fa></button>
						{/if}

						<button id="copy-{i}" class="chip variant-filled-primary" on:click={()=>copyFn(i)}><Fa icon="{faCopy}"></Fa></button>
						{#if variables.length > 0 && i < variables.length - 1}
							<button id="down-{i}" class="chip variant-filled-surface" on:click={()=>downFn(i)}><Fa icon="{faAngleDown}"></Fa></button>
						{:else}
							<button id="down-{i}" class="chip variant-filled-surface" disabled on:click={()=>downFn(i)}><Fa icon="{faAngleDown}"></Fa></button>
						{/if}
				</svelte:fragment>
			</Variable>
		{/each}
		<div class="flex content-end px-6">
			<div class="grow"></div> 
			<button class="chip variant-filled-primary flex-none" on:click="{addFn}"><Fa icon="{faAdd}"></Fa> </button>
		</div>
	{:else}
		<Spinner label="loading suggested structure" />
	{/if}
</div>

<Modal />
