<script lang="ts">
	import Variable from './variable/Variable.svelte';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { onMount } from 'svelte';
	import { getDataTypes, getUnits, getVariableTemplates, getMeanings } from '../services';
	import type { missingValueType } from '../types';
	import { VariableInstanceModel } from '../types';
	import { Modal, modalStore } from '@skeletonlabs/skeleton';
	import type { ModalSettings } from '@skeletonlabs/skeleton';

	import Fa from 'svelte-fa';
	import { faShare, faShareFromSquare, faMaximize, faMinimize, faAdd, faTrash, faCopy, faAngleUp, faAngleDown, faAnglesDown, faAnglesUp } from '@fortawesome/free-solid-svg-icons';

	// stores
	
	import { unitStore, dataTypeStore, templateStore, meaningsStore} from '../store'

	export let variables: VariableInstanceModel[] = [];
	export let missingValues: missingValueType[] = [];
	export let data: string[][];
 export let dataExist:boolean;

	$: variables;

	let expandAll=true;

	// validation array
	let variableValidationStates:any = [];

	export let valid = true;

	let ready:boolean = false;

	onMount(async () => {

		const datatypes = await getDataTypes();
		dataTypeStore.set(datatypes);

		const units = await getUnits();
		unitStore.set(units)
		
		const variableTemplates = await getVariableTemplates();
		templateStore.set(variableTemplates)

		const meanings = await getMeanings();
		meaningsStore.set(meanings);

		fillVariableValdationStates(variables);

		ready = true;

	});

	function fillVariableValdationStates(vars) {

		variableValidationStates = [];
		
		for (let index = 0; index < vars.length; index++) {
			variableValidationStates.push(false);
		}
	}

	// every time when validation state of a varaible is change,
	// this function triggered an check wheter save button can be active or not
	function checkValidationState() {
		valid = variableValidationStates.every((v) => v === true);
		//console.log("TCL ~ file: Variables.svelte:63 ~ checkValidationState ~ variableValidationStates:", variableValidationStates)
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

			variables = [...variables, new VariableInstanceModel()];

	}

	function copyFn(i)
	{

		let copiedVariable = new VariableInstanceModel();
		copiedVariable.name = variables[i].name+" (copied)";
		copiedVariable.description = variables[i].description;
		copiedVariable.dataType = variables[i].dataType;
		copiedVariable.unit = variables[i].unit;
		copiedVariable.template = variables[i].template;
		copiedVariable.systemType = variables[i].systemType;
		copiedVariable.template = variables[i].template;
		copiedVariable.displayPattern = variables[i].displayPattern;


		variables.splice(i+1, 0, copiedVariable);
		variables = [...variables];

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
		 			console.log("🚀 ~ file: Variables.svelte:177 ~ deleteVar:", deleteVar)
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

<div class="p-2">
<div class="flex gap-2 items-baseline"> 

	<button class="btn variant-filled-secondary" on:click={()=> expandAll = !expandAll}>
			{#if expandAll}
				<Fa icon={faAnglesUp}/>
			{:else}
			<Fa icon={faAnglesDown}/>
			{/if}
	</button>

<div class="pr-32 w-auto">
	{#if !valid}
		<span class="text-sm">Variables with errors:</span>


			{#each variableValidationStates as v, i}
					{#if v==false && variables[i] != undefined}
			
									<a class="chip variant-filled-error m-1" href="#{i}">
										{#if variables[i].name !=""}
											{variables[i].name}
										{:else}
											{i+1}
										{/if}
									</a>

					{/if}
		
			{/each}

	{/if}
</div>

</div>
<div class="flex-col space-y-2 mt-1">

	{#if variables && missingValues && ready}
		<!-- else content here -->
		{#each variables as variable, i (i)}
			<Variable
				bind:variable
				index={i}
				on:var-change={checkValidationState}
				bind:isValid={variableValidationStates[i]}
				bind:missingValues
				data={getColumnData(i)}
				on:copy-next={copyNext}
				on:copy-all={copyAll}
				expand = {expandAll}
				blockDataRelevant = {dataExist}
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
			 {#if !dataExist}
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
				{/if}
					</svelte:fragment>

			</Variable>
		{/each}
		<div class="flex content-end px-6">
			<div class="grow"></div> 
			{#if !dataExist}
					<button class="chip variant-filled-primary flex-none" on:click="{addFn}"><Fa icon="{faAdd}"></Fa> </button>
			{/if}
		</div>
	{:else}
		<Spinner label="loading suggested structure" />
	{/if}
</div>
</div>

<Modal />
