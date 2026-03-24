<script lang="ts">
	import Variable from './variable/Variable.svelte';
	import { Spinner, helpStore } from '@bexis2/bexis2-core-ui';
	import { onMount, createEventDispatcher } from 'svelte';

	import type { missingValueType } from '../types';
	import { VariableInstanceModel } from '../types';
	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	import type { ModalSettings } from '@skeletonlabs/skeleton';

	import Fa from 'svelte-fa';
	import {
		faShare,
		faShareFromSquare,
		faMaximize,
		faMinimize,
		faAdd,
		faTrash,
		faCopy,
		faAngleUp,
		faAngleDown,
		faAnglesDown,
		faAnglesUp,
		faChevronUp,
		faChevronDown
	} from '@fortawesome/free-solid-svg-icons';

	// services
	import {
		getDataTypes,
		getUnits,
		getVariableTemplates,
		getMeanings,
		getConstraints
	} from '../services';
	// stores
	import {
		unitStore,
		dataTypeStore,
		templateStore,
		meaningsStore,
		constraintsStore
	} from '../store';
	import DwcRequirements from './DwcRequirements.svelte';

	export let variables: VariableInstanceModel[] = [];
	export let missingValues: missingValueType[] = [];
	export let data: string[][];
	export let dataExist: boolean = false;

	const dispatch = createEventDispatcher();

	$: variables;

	let expandAll = true;

	// validation array
	let variableValidationStates: any = [];

	export let valid: boolean | null = null;

	let ready: boolean = false;

	const modalStore = getModalStore();

	onMount(async () => {
		const datatypes = await getDataTypes();
		dataTypeStore.set(datatypes);

		const units = await getUnits();
		unitStore.set(units);

		const variableTemplates = await getVariableTemplates();
		templateStore.set(variableTemplates);

		const meanings = await getMeanings();
		meaningsStore.set(meanings);
		console.log('🚀 ~ file: Variables.svelte:50 ~ onMount ~ meanings:', meanings);

		const constraints = await getConstraints();
		constraintsStore.set(constraints);

		fillVariableValdationStates(variables);

		ready = true;

		if (variables.length > 100) {
			expandAll = false;
		}
	});

	function fillVariableValdationStates(vars: VariableInstanceModel[]) {
		variableValidationStates = [];

		for (let index = 0; index < vars.length; index++) {
			variableValidationStates.push(false);
		}
	}

	let isUnique: boolean;
	let uniqueNames: string[] = [];
	// triggered when a variable changed its validation state
	function varChangeFn() {
		uniqueNames = checkNamesUnique();
		if (uniqueNames.length == 0) {
			isUnique = true;
		} else {
			isUnique = false;
		}
		checkValidationState();
		dispatch('changed');
	}
	// every time when validation state of a variable is change,
	// this function triggered an check whether save button can be active or not
	function checkValidationState() {
		valid = variableValidationStates.every((v: boolean) => v === true);
		valid = valid && isUnique;
		// console.log('🚀 ~ file: Variables.svelte:87 ~ checkValidationState ~ valid:', valid);
		//console.log("TCL ~ file: Variables.svelte:63 ~ checkValidationState ~ variableValidationStates:", variableValidationStates)
	}

	function getColumnData(cellIndex: number) {
		let cValues: string[] = [];
		for (let index = 0; index < data.length; index++) {
			const c = data[index][cellIndex];
			cValues.push(c);
		}
		return cValues;
	}
	// return non-unique names
	function checkNamesUnique() {
		let names: string[] = [];
		let nonUniqueNames: string[] = [];

		variables.forEach((v) => {
			if (names.includes(v.name)) {
				if (!nonUniqueNames.includes(v.name)) {
					nonUniqueNames.push(v.name);
				}
			} else {
				names.push(v.name);
			}
		});

		return nonUniqueNames;
	}

	// copy data from variable on index i to the next one
	function copyNext(i: number) {
		const modal: ModalSettings = {
			type: 'confirm',
			title: 'Copy',
			body: 'Are you sure you want to copy the current variable to the next?',
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

	// copy data from variable on index i to all next
	function copyAll(i: number) {
		const modal: ModalSettings = {
			type: 'confirm',
			title: 'Copy',
			body: 'Are you sure you want to copy the current variable to all after?',
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

	function updateVariableFromOther(from: VariableInstanceModel, to: VariableInstanceModel) {
		if (from && to) {
			to.description = from.description;
			to.dataType = from.dataType;
			to.unit = from.unit;
			to.template = from.template;
			to.displayPattern = from.displayPattern;
		}

		return to;
	}

	function addFn() {
		var v = new VariableInstanceModel();
		variables = [...variables, v];
	}

	function copyFn(i: number) {
		let copiedVariable = new VariableInstanceModel();
		copiedVariable.name = variables[i].name + ' (copied)';
		copiedVariable.description = variables[i].description;
		copiedVariable.dataType = variables[i].dataType;
		copiedVariable.unit = variables[i].unit;
		copiedVariable.template = variables[i].template;
		copiedVariable.systemType = variables[i].systemType;
		copiedVariable.template = variables[i].template;
		copiedVariable.displayPattern = variables[i].displayPattern;
		copiedVariable.constraints = variables[i].constraints;
		copiedVariable.missingValues = variables[i].missingValues;

		variables.splice(i + 1, 0, copiedVariable);
		variables = [...variables];
	}

	function deleteFn(i: number) {
		const deleteVar = variables[i];

		const confirm: ModalSettings = {
			type: 'confirm',
			title: 'Delete Variable',
			body: 'Are you sure you want to delete the variable "' + deleteVar.name + '"?',
			// TRUE if confirm pressed, FALSE if cancel pressed
			response: (r: boolean) => {
				if (r) {
					variables = variables.filter((v) => v != deleteVar);
				}
			}
		};
		modalStore.trigger(confirm);
	}

	function upFn(i: number) {
		const varTemp = variables[i];
		variables[i] = variables[i - 1];
		variables[i - 1] = varTemp;
	}

	function downFn(i: number) {
		const varTemp = variables[i];
		variables[i] = variables[i + 1];
		variables[i + 1] = varTemp;
	}
</script>

{#if ready}
	<div><DwcRequirements bind:variables /></div>

	{#if !valid}
		<div class="pr-32 w-auto mt-4 mb-2 p-2 rounded-md border border-gray-200 bg-gray-50">
			<span class="">Variables with errors:</span>
			{#each variableValidationStates as v, i}
				{#if v == false && variables[i] != undefined}
					<a class="chip variant-soft-error m-1" href="#{i}">
						{#if variables[i].name != ''}
							{variables[i].name}
						{:else}
							{i + 1}
						{/if}
					</a>
				{/if}
			{/each}
		</div>
	{/if}
	{#if !isUnique}
		<div class="pr-32 w-auto mt-4 mb-2 p-2 rounded-md border border-gray-200 bg-gray-50">
			<span class="">Variable names must be unique. Non-unique names:</span>
			{#each uniqueNames as name}
				<div class="chip variant-soft-error m-1">
					{name}
				</div>
			{/each}
		</div>
	{/if}
{/if}
<div class="p-2">
	{#if ready}
		<div class="flex gap-2 items-end">
			<button
				id="variables-expander"
				class="btn variant-filled-secondary flex gap-1 items-center"
				title={expandAll ? 'collapse all' : 'expand all'}
				on:mouseover={() => helpStore.show('variables-expander')}
				on:focus={() => helpStore.show('variables-expander')}
				on:click={() => (expandAll = !expandAll)}
			>
				{#if expandAll}
					<Fa icon={faChevronUp} />
				{:else}
					<Fa icon={faChevronDown} />
				{/if}
				<span>{expandAll ? 'Collapse All' : 'Expand All'}</span>
			</button>
		</div>
	{/if}
	<div class="flex-col space-y-2 mt-1">
		{#if variables && missingValues && ready}
			<!-- else content here -->
			{#each variables as variable, i (i)}
				<Variable
					bind:variable
					index={i}
					on:var-change={varChangeFn}
					bind:isValid={variableValidationStates[i]}
					bind:missingValues
					data={getColumnData(i)}
					on:copy-next={copyNext}
					on:copy-all={copyAll}
					expand={expandAll}
					blockDataRelevant={dataExist}
				>
					<svelte:fragment slot="options">
						{#if variables.length > 0 && i < variables.length - 1}
							<button
								id="copy-next-{i}"
								type="button"
								title="copy content (not name) to the next variable "
								class="chip variant-filled-warning"
								on:mouseover={() => helpStore.show('copy-next')}
								on:focus={() => helpStore.show('copy-next')}
								on:click={() => copyNext(i)}><Fa icon={faShare} /></button
							>
							<button
								id="copy-all-{i}"
								type="button"
								title="copy content (not name) to all after this"
								class="chip variant-filled-warning"
								on:mouseover={() => helpStore.show('copy-all')}
								on:click={() => copyAll(i)}><Fa icon={faShareFromSquare} /></button
							>
						{/if}
					</svelte:fragment>
					<svelte:fragment slot="list-options">
						{#if !dataExist}
							<button
								id="delete-{i}"
								title="delete variable"
								class="chip variant-filled-error"
								on:mouseover={() => helpStore.show('delete-var')}
								on:focus={() => helpStore.show('delete-var')}
								on:click={() => deleteFn(i)}><Fa icon={faTrash}></Fa></button
							>

							{#if i > 0}
								<button
									id="up-{i}"
									title="move up"
									class="chip variant-filled-surface"
									on:mouseover={() => helpStore.show('up-var')}
									on:focus={() => helpStore.show('up-var')}
									on:click={() => upFn(i)}><Fa icon={faAngleUp}></Fa></button
								>
							{:else}
								<button
									id="up-{i}"
									title="move up"
									class="chip variant-filled-surface disabled"
									disabled
									on:mouseover={() => helpStore.show('up-var')}
									on:focus={() => helpStore.show('up-var')}
									on:click={() => upFn(i)}><Fa icon={faAngleUp}></Fa></button
								>
							{/if}

							<button
								id="copy-{i}"
								title="copy"
								class="chip variant-filled-primary"
								on:mouseover={() => helpStore.show('copy-var')}
								on:focus={() => helpStore.show('copy-var')}
								on:click={() => copyFn(i)}><Fa icon={faCopy}></Fa></button
							>

							{#if variables.length > 0 && i < variables.length - 1}
								<button
									id="down-{i}"
									title="move down"
									class="chip variant-filled-surface"
									on:mouseover={() => helpStore.show('down-var')}
									on:focus={() => helpStore.show('down-var')}
									on:click={() => downFn(i)}><Fa icon={faAngleDown}></Fa></button
								>
							{:else}
								<button
									id="down-{i}"
									title="move down"
									class="chip variant-filled-surface"
									disabled
									on:mouseover={() => helpStore.show('down-var')}
									on:focus={() => helpStore.show('down-var')}
									on:click={() => downFn(i)}><Fa icon={faAngleDown}></Fa></button
								>
							{/if}
						{/if}
					</svelte:fragment>
				</Variable>
			{/each}
			<div class="flex content-end px-6">
				<div class="grow"></div>
				{#if !dataExist}
					<button title="add" class="chip variant-filled-primary flex-none" on:click={addFn}
						><Fa icon={faAdd}></Fa>
					</button>
				{/if}
			</div>
		{:else if dataExist}
			<Spinner label="loading suggested structure based on your file." />
		{:else}
			<Spinner label="loading structure." />
		{/if}
	</div>
</div>

<Modal />
