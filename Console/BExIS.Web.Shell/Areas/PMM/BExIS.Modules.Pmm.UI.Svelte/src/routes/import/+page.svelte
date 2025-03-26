<script lang="ts">
	import { DropdownKVP, Page } from '@bexis2/bexis2-core-ui';
	import Papa from 'papaparse';
	import Fa from 'svelte-fa';
	import {
		faFileArrowDown,
		faArrowUpFromBracket,
		faPlus,
		faChevronDown,
		faChevronUp
	} from '@fortawesome/free-solid-svg-icons';
	import { FileButton } from '@skeletonlabs/skeleton';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import { Table } from '@bexis2/bexis2-core-ui';
	import lineClamp from './components/lineClamp.svelte';
	import error from './components/error.svelte';
	import { invalidTableStore, validTableStore } from './stores';
	import type {
		errorArray,
		errorItem,
		tableErrorItem,
	} from './models';
	import { onMount } from 'svelte';

	import * as apiCalls from './services/apiCalls';

	import mappingJson from "./mapping1.json";

	let filename: string = '';

	let entities :any[] = [];
	let transformedArray: any [] = [];
	let tempTitle: string | null = null;
	let csvInfo: any[] = [];
	let dataset = {
    Title: "",
    Description: "",
    DataStructureId: 0,
    MetadataStructureId: 0,
    EntityTemplateId: 0
  };


	let validData: any[] = [];
	let showValid: boolean = false;

	let invalidData: any[] = [];
	let invalidDataCounter: number = 0;
	let showInvalid: boolean = false;

	let downloadLinkValidData = '';
	let downloadLinkValidDataAsJson = '';
	let downloadLinkInvalidData = '';

	let isLoading: boolean = false;

	let errors: errorArray[] = [];
	let showErrorMsg: boolean = false;

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
		transformedArray = entities.map(entity => ({
			id: entity.id,
			text: entity.name
		}));
		// console.log("transofrmden", transformedArray);
		console.log("entity:",entities);
	});

	let target :number;

	

	function fillInvalidTableStore(data: any) {
		// console.time('fillInvalidTableStore');
		invalidTableStore.set(data);
		let tableErrorItem: tableErrorItem;
		let errorItem: errorItem;
		for (let i: number = 0; i < errors.length; i++) {
			for (let j: number = 0; j < errors[i].cellErrors.length; j++) {
				errorItem = errors[i].cellErrors[j];
				tableErrorItem = {
					errorType: 'missmatch',
					errorMsg: errorItem.errorMsg,
					value: $invalidTableStore[i][errorItem.column]
				};
				$invalidTableStore[i][errorItem.column] = tableErrorItem;
			}
		}
		// console.timeEnd('fillInvalidTableStore');
	}

	function clear() {
		invalidDataCounter = 0;
		invalidData = [];
		validData = [];
		errors = [];
		isLoading = false;
		showValid = false;
		showInvalid = false;
	}

	// upload file and parse to json
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

	// function that parses csv to json
	function parseCSVToJson(data: any) {
		let Sources = new Set<string>();
		Papa.parse(data, {
			header: true,
			complete: function (results) {
				let jsonData: any = results.data;

				let codeColumns = Object.keys(jsonData[0]).filter(
					(key) =>
						key.trim().toLocaleLowerCase().startsWith('code') &&
						!['code', 'code present', 'code number'].includes(key.trim().toLowerCase())
				);
				sortData(codeColumns, jsonData, 'Code URL');

				let dataColumns = Object.keys(jsonData[0]).filter(
					(key) =>
						key.trim().toLocaleLowerCase().startsWith('data') &&
						!['data present'].includes(key.trim().toLowerCase())
				);
				sortData(dataColumns, jsonData, 'Data URL');
				fillInvalidTableStore(invalidData);
				showInvalid = invalidData.length > 0 ? true : false;
				validTableStore.set(validData);
				
				createDownloadLinks();
			}
		});
	}

	const mapToApiFormat = (csvRow: any, mapping: any) => {
    	const apiData: any = {};

		mapping.Mappings.forEach((map: any, index: number) => {
			let sourceField = "";
			sourceField = map.Source;
			// console.log("Map Target1:", map.Target);
			const targetField = map.Target.match(/\$\[['"](.+)['"]\]/)?.[1];  // Extract full path after $

			if (sourceField) {
				if (index % 2 === 0) {
					// Jeder 1. Durchgang (Titel)
					tempTitle = csvRow[sourceField];
				} else {
					// Jeder 2. Durchgang (Beschreibung)
					if (tempTitle !== null) {
						csvInfo.push({
							title: tempTitle,
							description: csvRow[sourceField],
						});
						tempTitle = null; // Zurücksetzen für das nächste Paar
					}
				}
			}

 // Das API-Feld

        // if (csvRow[sourceField]) {
        //     apiData[targetField] = csvRow[sourceField];
        //     console.log(`Mapping - Source: ${sourceField}, Target: ${targetField}, Value: ${csvRow[sourceField]}`);
        // }
		
    	});
		console.log("csv",csvInfo);
    	return apiData;
	};

function onChangeHandler(event) {
	let selectedEntity: number;
	selectedEntity = event.target.value;

	let searchEntity = entities.find(entity => entity.id == selectedEntity)

	dataset.EntityTemplateId = selectedEntity;
	dataset.MetadataStructureId = searchEntity.metadataStructure.id;
	// console.log("dataset", dataset)

}
function create()
{
	createAllDatasets(dataset.MetadataStructureId, dataset.EntityTemplateId);
}

async function createAllDatasets (metadataStructureId: number, entityTemplateId: number, dataStructureId: number = 0)
{

    try {
        const mappedData = validData.map(row => {
            const mappedRow = mapToApiFormat(row, mappingJson);
            // console.log("Mapped Row:", mappedRow);  // Ausgabe der gemappten Zeile
      
        });

		let res: string;

		let ds = {
			Title: "",
			Description: "",
			DataStructureId: dataStructureId,
			MetadataStructureId: metadataStructureId,
			EntityTemplateId: entityTemplateId
		};


		for(const data of csvInfo)
		{
			// console.log("title",data.title)
			// console.log("",data[0][1])
			
				ds.Title = data.title;
				ds.Description = data.description;
				res = await apiCalls.createDataset(ds);
				console.log("datasets",ds);			
		}

        // console.log("Mapped Data:", mappedData); // Alle gemappten Daten

        // await Promise.all(mappedData.map(dataset => apiCalls.createDataset(dataset)));

        console.log("Alle Datensätze wurden erstellt!");
    } catch (error) {
        console.error("Fehler beim Erstellen der Datensätze:", error);
    }
};

	
	function countCommas(data: string): number {
		return data.split(',').length - 1;
	}

	// sort data in 2 different files, 1 with vaild data and 1 with invalid data
	function sortData(columns: any, data: any, refColumn: string) {
		let columnErrors: { [key: string]: any[] } = {};
		let cellError: errorItem[] = [];

		data.forEach((row: any, rowIndex: number) => {
			cellError = [];
			const ref: string = row[refColumn];
			if (ref) {
				if (countCommas(ref) == 0) {
					validData.push(row);
				} else {
					let numberOfCommas: number = countCommas(ref);
					let valid: boolean = true;

					for (let i = 0; i < columns.length; i++) {
						if (numberOfCommas != countCommas(row[columns[i]])) {
							cellError.push({
								column: columns[i],
								errorMsg: 'Number of entries dose not match with Data URL'
							});
							valid = false;

							if (!columnErrors[columns[i]]) {
								columnErrors[columns[i]] = [];
							}
							columnErrors[columns[i]].push(rowIndex);
						}
					}

					if (!valid) {
						let contains: boolean = false;
						for (let i = 0; i < errors.length; i++) {
							if (rowIndex == errors[i].rowIndex) {
								errors[i].cellErrors.concat(cellError);
								contains = true;
								break;
							}
						}
						if (!contains) {
							invalidDataCounter++;
							invalidData.push(row);
							errors.push({ rowIndex: rowIndex, cellErrors: cellError });
						}
					} else {
						validData.push(row);
					}
				}
			}
		});

		columns.forEach((columnHeader: string) => {
			if (!columnCfg[columnHeader]) {
				// WenncolumnsWithError eine Spalte bereits existiert, füge den renderComponent hinzu
				columnCfg[columnHeader] = {
					...(columnCfg[columnHeader] || {}),
					disableFiltering: true,
					instructions: {
						renderComponent: error // Setzt den error renderComponent für diese Spalte
					}
				};
			}
		});
	}

	// parse json back to csv and create download links
	function createDownloadLinks() {
		const validCSVString = Papa.unparse(validData, { delimiter: ',' }); // Ensure comma as separator
		const blobValid = new Blob([validCSVString], { type: 'text/csv' });
		downloadLinkValidData = URL.createObjectURL(blobValid);

		// fs.writeFile('valid_data', validData)
		const validDataJson = JSON.stringify(validData);
		const blobJson = new Blob([validDataJson], { type: 'application/json' });
		downloadLinkValidDataAsJson = URL.createObjectURL(blobJson);

		const invalidCSVString = Papa.unparse(invalidData, { delimiter: ',' }); // Ensure comma as separator
		const blobInvalid = new Blob([invalidCSVString], { type: 'text/csv' });
		downloadLinkInvalidData = URL.createObjectURL(blobInvalid);

		isLoading = false;
	}

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
	<button on:click={create}>Alle Datasets erstellen</button>


	<div class="flex gap-5 w-full">
		<div id="fileLabel" class="w-16">Entity :</div>
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

	{#if validData.length > 0 || invalidData.length > 0}
		<div class="card p-2">
			{#if validData.length > 0}
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

			{#if invalidData.length > 0}
				<div class="w-full">
					<div class="text-left card variant-ghost-warning w-full flex gap-5 p-2 my-1">
						<div class="w-full align-middle">{invalidDataCounter} rows failed to import</div>
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
							{#each errors as error}
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
</Page>

<!-- C:\Users\xi68neg\Desktop\Bexis\repo\Console\BExIS.Web.Shell\Areas\RPM\BExIS.Modules.Rpm.UI.Svelte\src\routes\unit\+page.svelte -->
