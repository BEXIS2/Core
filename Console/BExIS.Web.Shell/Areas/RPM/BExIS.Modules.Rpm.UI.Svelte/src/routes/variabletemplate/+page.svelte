<script lang="ts">
	import { onMount } from 'svelte';
	import { slide, fade } from 'svelte/transition';
	import { Modal, modalStore, Toast } from '@skeletonlabs/skeleton';
	import { Page, Table, ErrorMessage, helpStore, TablePlaceholder, type helpItemType, type listItemType, } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faPlus, faXmark, faBan } from '@fortawesome/free-solid-svg-icons';

 // Components
	import VariableTemplate from './VariableTemplate.svelte'

	// types
	import {VariableTemplateModel, type missingValueType} from '$lib/components/datastructure/types'

 // services
	import { getVariableTemplates, getDataTypes, getUnitsWithDataTypes, getDisplayPattern } from '$lib/components/datastructure/services'

	// data
	import { variableTemplateHelp } from './help'
	import { writable } from 'svelte/store';
	import { displayPatternStore } from '$lib/components/datastructure/store';

	let vt: VariableTemplateModel[] = [];
	let variableTemplate: VariableTemplateModel = new VariableTemplateModel();

	const tableStore = writable<VariableTemplateModel[]>([]);
	
	let helpItems: helpItemType[] = variableTemplateHelp;

 let missingValues: missingValueType[] = [];

	let datatypes:listItemType[] = [];
	$: datatypes;
	let units:listItemType[] = [];
	$: units;
	let showForm = false;

	onMount(async () => {
		helpStore.setHelpItemList(helpItems);
		datatypes = await getDataTypes();
		units = await getUnitsWithDataTypes();

		
		// load display pattern onces for all edit types
		const displayPattern = await getDisplayPattern();
		displayPatternStore.set(displayPattern);

		console.log(units);
		
		
		showForm = false;
	});


	async function reload() {
		showForm = false;
		vt = await getVariableTemplates();
		console.log("variable templates",vt)

	}

	function toggleForm() {
		if (showForm) {
			clear();
		}
		showForm = !showForm;
	}

	function clear() {
		variableTemplate = new VariableTemplateModel();
		showForm = false;
	}

</script>

<Page help={true} title="Manage Variable Template">

	{#await reload()}
		<div class="grid w-full grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
			<div class="h-9 w-96 placeholder animate-pulse" />
			<div class="flex justify-end">
				<button class="btn placeholder animate-pulse shadow-md h-9 w-16"
					><Fa icon={faPlus} /></button>
			</div>
		</div>

		<div class="table-container w-full">
			<TablePlaceholder cols={7} />
		</div>
	{:then}

	<!-- svelte-ignore a11y-click-events-have-key-events -->
	<div class="grid grid-cols-2 gap-5 my-4 pb-1 border-b border-primary-500">
		<div class="h3 h-9">
			{#if variableTemplate.id < 1}
				<span in:fade={{ delay: 400 }} out:fade>Create neẇ Variable Template</span>
			{:else}
				<span in:fade={{ delay: 400 }} out:fade>{variableTemplate.name}</span>
			{/if}
		</div>
		<div class="text-right">
			{#if !showForm}
				<!-- svelte-ignore a11y-mouse-events-have-key-events -->
				<button
					transition:fade
					class="btn variant-filled-secondary shadow-md h-9 w-16"
					title="Create neẇ Unit"
					id="create"
					on:mouseover={() => {
						helpStore.show('create');
					}}
					on:click={() => toggleForm()}><Fa icon={faPlus} /></button
				>
			{/if}
		</div>
	</div>

	{#if showForm}
	<div in:slide out:slide>
			<VariableTemplate variable = {variableTemplate} {units} {datatypes} {missingValues} on:cancel={()=>clear()}/>
		</div>
	{/if}

	<div class="table table-compact w-full">
		

		<Table
			config={{
				id: 'VariableTemplates',
				data: tableStore
			}}
		/>
	</div>

	{:catch error}
				<ErrorMessage {error} />
	{/await}
</Page>
<Modal/>
