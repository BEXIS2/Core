<script lang="ts">
	import { DropdownKVP, Page } from '@bexis2/bexis2-core-ui';
	import Papa from 'papaparse';
	import Fa from 'svelte-fa';
	import {
		faFileArrowDown,
		faArrowUpFromBracket,
		faChevronDown,
		faChevronUp
	} from '@fortawesome/free-solid-svg-icons';
	import { FileButton } from '@skeletonlabs/skeleton';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import { Table } from '@bexis2/bexis2-core-ui';
	import lineClamp from './components/lineClamp.svelte';
	import error from './components/error.svelte';
	import { invalidTableStore, validTableStore, DataCounterStore } from './stores';
	import type {errorItem, tableErrorItem, ValidationReturn, dataSetType, createDatasetReturn } from './models';
	import { onMount } from 'svelte';
	import * as apiCalls from './services/apiCalls';

	import * as validation from './validation';
	import * as createDatasets from './createDatasets';

	let filename: string = '';

	let entities: any[] = [];
	let transformedArray: any[] = [];
	let target: number;
	let dataset: dataSetType = {
		Title: '',
		Description: '',
		DataStructureId: 0,
		MetadataStructureId: 0,
		EntityTemplateId: 0
	};

	let showValid: boolean = false;

	let showInvalid: boolean = false;

	let downloadLinkValidData = '';
	let downloadLinkValidDataAsJson = '';
	let downloadLinkInvalidData = '';

	let isLoading: boolean = false;

	let showErrorMsg: boolean = false;

	let validationReturnObj: ValidationReturn = {
		validData: [],
		invalidData: [],
		invalidDataCounter: 0,
		errors: [],
	};

	let createDatasetReturn: createDatasetReturn = {
		uploadedCount: 0,
		idMapping: [], // Initial leer
		tempTitle: null
	}
	
	let columnCfg: any = {
		Author: {
			minWidth: 300,
			instructions: {
				renderComponent: lineClamp
			}
		},
		Title: {
			minWidth: 300,
			instructions: {
				renderComponent: lineClamp
			}
		},
		'Abstract Note': {
			minWidth: 600,
			instructions: {
				renderComponent: lineClamp
			}
		}
	};

	onMount(async () => {
		clear();

		entities = await apiCalls.getEntityTemplateList();
		transformedArray = entities.map((entity) => ({
			id: entity.id,
			text: entity.name
		}));
	});

	function clear() {
		validationReturnObj.invalidDataCounter = 0;
		validationReturnObj.invalidData = [];
		validationReturnObj.validData = [];
		validationReturnObj.errors = [];
		isLoading = false;
		showValid = false;
		showInvalid = false;
	}

	function handleFileUpload(event: any) {
		clear();

		let csvData: any;

		isLoading = true;
		if (event.target.files[0]) {
			let file: File = event.target.files[0];
			filename = file.name;
			const reader = new FileReader();
			reader.onload = () => {
				csvData = reader.result;
				parseCSVToJson(csvData);
			};
			reader.readAsText(file);
		}
	}

	function parseCSVToJson(data: any) { 
		Papa.parse(data, {
			header: true,
			complete: function (results) {
				let jsonData: any = results.data;

				let codeColumns = Object.keys(jsonData[0]).filter(
					(key) =>
						key.trim().toLocaleLowerCase().startsWith('code') &&
						!['code', 'code present', 'code number'].includes(key.trim().toLowerCase())
				);

				let dataColumns = Object.keys(jsonData[0]).filter(
					(key) =>
						key.trim().toLocaleLowerCase().startsWith('data') &&
						!['data present'].includes(key.trim().toLowerCase())
				);

				validation.validateRows(jsonData, codeColumns, dataColumns, validationReturnObj);
				createColumnError(codeColumns, dataColumns);

				fillInvalidTableStore(validationReturnObj.invalidData);
				showInvalid = validationReturnObj.invalidData.length > 0 ? true : false;
				validTableStore.set(validationReturnObj.validData);

				createDownloadLinks();
			}
		});
	}

	function createColumnError(codeColumns: string[], dataColumns: string[]) {
        [...codeColumns, ...dataColumns].forEach((columnHeader: string) => {
			if (!columnCfg[columnHeader]) {
				columnCfg[columnHeader] = {
					...(columnCfg[columnHeader] || {}),
					disableFiltering: true,
					instructions: {
						renderComponent: error
					}
				};
			}
		});
    }

	function fillInvalidTableStore(data: any) {
		invalidTableStore.set(data);
		console.log('invalidTableStore', data);
		let tableErrorItem: tableErrorItem;
		let errorItem: errorItem;

		for (let i: number = 0; i < validationReturnObj.errors.length; i++) {
			for (let j: number = 0; j < validationReturnObj.errors[i].cellErrors.length; j++) {
				errorItem = validationReturnObj.errors[i].cellErrors[j];
				tableErrorItem = {
					errorType: 'missmatch',
					errorMsg: errorItem.errorMsg,
					value: $invalidTableStore[i][errorItem.column]
				};
				$invalidTableStore[i][errorItem.column] = tableErrorItem;
			}
		}
	}

	function createDownloadLinks() {
		const validCSVString = Papa.unparse(validationReturnObj.validData, { delimiter: ',' }); // Ensure comma as separator
		const blobValid = new Blob([validCSVString], { type: 'text/csv' });
		downloadLinkValidData = URL.createObjectURL(blobValid);

		// fs.writeFile('valid_data', validData)
		const validDataJson = JSON.stringify(validationReturnObj.validData);
		const blobJson = new Blob([validDataJson], { type: 'application/json' });
		downloadLinkValidDataAsJson = URL.createObjectURL(blobJson);

		const invalidCSVString = Papa.unparse(validationReturnObj.invalidData, { delimiter: ',' }); // Ensure comma as separator
		const blobInvalid = new Blob([invalidCSVString], { type: 'text/csv' });
		downloadLinkInvalidData = URL.createObjectURL(blobInvalid);

		isLoading = false;
	}

	function create() {
		// console.log('click');
		createDatasets.createAllDatasets(dataset.MetadataStructureId, dataset.EntityTemplateId, validationReturnObj, createDatasetReturn, DataCounterStore);
	}

	$: totalUploads = $validTableStore.length;

	// function downloadValidData() {
	// 	const link = document.createElement('a');
	// 	link.href = downloadLinkValidData;
	// 	link.download = 'valid__data.csv';
	// 	link.click();
	// }

	function downloadValidDataAsJson() {
		const link = document.createElement('a');
		link.href = downloadLinkValidDataAsJson;
		link.download = 'valid__data.json';
		link.click();
	}

	function downloadInvalidData() {
		const link = document.createElement('a');
		link.href = downloadLinkInvalidData;
		link.download = 'invalid__data.csv';
		link.click();
	}

	function onChangeHandler(event) {
		let selectedEntity: number;
		selectedEntity = event.target.value;

		let searchEntity = entities.find((entity) => entity.id == selectedEntity);

		dataset.EntityTemplateId = selectedEntity;
		dataset.MetadataStructureId = searchEntity.metadataStructure.id;
	}

	function toggleValidForm() {
		showValid = !showValid;
	}

	function toggleInvalidForm() {
		showInvalid = !showInvalid;
	}

	function toggleErrorMsg() {
		showErrorMsg = !showErrorMsg;
	}
</script>

<Page help={true} title="Manage Publications">
	<h1 class="h1">Import Publications</h1>
	<div class="flex gap-5 w-full">
		<div id="fileLabel" class="w-36">Entity Template :</div>
		<div class="overflow-clip w-full">
			<DropdownKVP
				id="metadataStructure"
				title=""
				bind:target
				source={transformedArray}
				on:change={onChangeHandler}
			/>
		</div>
	</div>
	<div class="flex gap-5 w-full">
		{#if isLoading}
			<div class="w-full">
				<Spinner label="...loading"></Spinner>
			</div>
			<div class="w-16 placeholder h-9" />
		{:else}
			<div id="fileLabel" class="w-16">File :</div>
			<div id="fileName" class="overflow-clip w-full">
				{filename}
			</div>
			<div id="fileButton" class="w-16">
				<FileButton
					id="uploadCsv"
					title="Upload CSV"
					button="btn variant-filled-primary h-9 w-16 shadow-md"
					name="uploadCsv"
					on:change={handleFileUpload}
					><Fa icon={faArrowUpFromBracket} />
				</FileButton>
			</div>
		{/if}
	</div>

	{#if validationReturnObj.validData.length > 0 || validationReturnObj.invalidData.length > 0}
		<div class="card p-2">
			{#if validationReturnObj.validData.length > 0}
				<div class="grid gap-5 w-full">
					<div class="text-left card variant-ghost-primary w-full flex gap-5 p-2 my-1">
						{#if !showValid}
							<div class="w-full align-middle">Show Valid Data</div>
						{:else}
							<div class="w-full align-middle">Hide Valid Data</div>
						{/if}
						<div class="w-8">
							<button class="chip variant-filled-primary" on:click={downloadValidDataAsJson}
								><Fa icon={faFileArrowDown}></Fa></button
							>
						</div>
						<div class="w-9">
							{#if !showValid}
								<button class="chip" on:click={toggleValidForm}
									><Fa icon={faChevronDown}></Fa></button
								>
							{:else}
								<button class="chip" on:click={toggleValidForm}><Fa icon={faChevronUp}></Fa></button
								>
							{/if}
						</div>
					</div>

					{#if showValid}
						<Table
							config={{
								height: 600,
								id: 'Valid',
								data: validTableStore,
								columns: {
									Author: {
										minWidth: 300,
										instructions: {
											renderComponent: lineClamp
										}
									},
									Title: {
										minWidth: 300,
										instructions: {
											renderComponent: lineClamp
										}
									},
									'Abstract Note': {
										minWidth: 600,
										instructions: {
											renderComponent: lineClamp
										}
									}
								}
							}}
						/>
					{/if}
				</div>
			{/if}

			{#if validationReturnObj.invalidData.length > 0}
				<div class="w-full">
					<div class="text-left card variant-ghost-warning w-full flex gap-5 p-2 my-1">
						<div class="w-full align-middle">{validationReturnObj.invalidDataCounter} rows failed to import</div>
						<div class="w-9">
							{#if !showErrorMsg}
								<button class="chip" on:click={toggleErrorMsg}
									><Fa icon={faChevronDown}></Fa></button
								>
							{:else}
								<button class="chip" on:click={toggleErrorMsg}><Fa icon={faChevronUp}></Fa></button>
							{/if}
						</div>
					</div>
					{#if showErrorMsg}
						<div
							class="grid grid-cols-5 gap-1 w-full card variant-ringed-warning p-5 text-xs"
							in:slide={{ delay: 400 }}
							out:slide
						>
							<div class="font-bold">Row</div>
							<div class="font-bold">Column</div>
							<div class="font-bold col-span-3">Error Messages</div>
							{#each validationReturnObj.errors as error}
								<div class="col-span-5"><hr /></div>
								<div>{error.rowIndex}</div>
								{#each error.cellErrors as cellError, i}
									{#if i > 0}
										<div></div>
									{/if}
									<div>{cellError.column}</div>
									<div class="text-red-500 col-span-3">{cellError.errorMsg}</div>
								{/each}
							{/each}
						</div>
					{/if}

					<div class="text-left card variant-ghost-primary w-full flex gap-5 p-2 my-1">
						{#if !showInvalid}
							<div class="w-full align-middle">Show invalid Data</div>
						{:else}
							<div class="w-full align-middle">Hide invalid Data</div>
						{/if}
						<div class="w-8">
							<button class="chip variant-filled-primary" on:click={downloadInvalidData}
								><Fa icon={faFileArrowDown}></Fa></button
							>
						</div>
						<div class="w-9">
							{#if !showInvalid}
								<button class="chip" on:click={toggleInvalidForm}
									><Fa icon={faChevronDown}></Fa></button
								>
							{:else}
								<button class="chip" on:click={toggleInvalidForm}
									><Fa icon={faChevronUp}></Fa></button
								>
							{/if}
						</div>
					</div>

					{#if showInvalid}
						<Table
							config={{
								height: 600,
								id: 'Invalid',
								data: invalidTableStore,
								columns: columnCfg
							}}
						/>
					{/if}
				</div>
			{/if}
		</div>
	{/if}
	<div class="flex gap-5 w-full">
		<div id="datasetCounter">
			{#if validationReturnObj.validData.length > 0}
				<p class="card variant-ghost-primary gap-5 p-2 my-1 align-middle">
					{$DataCounterStore} of {totalUploads} datasets created
				</p>
			{/if}
		</div>
		<div id="progressBar" class="overflow-clip w-full flex items-center">
			{#if validationReturnObj.validData.length > 0}
				<progress max={totalUploads} value={$DataCounterStore}></progress>
			{/if}
		</div>
		<div id="importButton" class="w-24 flex items-center">
			{#if validationReturnObj.validData.length > 0 && dataset.EntityTemplateId}
				<button on:click={create} class="btn variant-filled-primary h-9 w-24 shadow-md"
					>Import</button
				>
			{:else}
				<button class="btn variant-filled-primary h-9 w-24 shadow-md disabled" disabled
					>Import</button
				>
			{/if}
		</div>
	</div>
</Page>

<!-- C:\Users\xi68neg\Desktop\Bexis\repo\Console\BExIS.Web.Shell\Areas\RPM\BExIS.Modules.Rpm.UI.Svelte\src\routes\unit\+page.svelte -->
