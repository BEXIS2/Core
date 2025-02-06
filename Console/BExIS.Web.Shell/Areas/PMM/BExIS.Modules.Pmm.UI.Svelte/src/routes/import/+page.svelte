<script lang="ts">
	import { Page } from '@bexis2/bexis2-core-ui';
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
		Mapping,
		MappingEntry
	} from './models';
	import jMapping from './mapping.json';
	import { onMount } from 'svelte';

	import * as apiCalls from './services/apiCalls';

	let filename: string = '';

	let schema: any;

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
		schema = await apiCalls.GetMetadataScheema(6);
		console.log(schema);
	});

	function fillInvalidTableStore(data: any) {
		console.time('fillInvalidTableStore');
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
		console.timeEnd('fillInvalidTableStore');
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

				processMappings(jMapping.Mappings, Sources);
				jsonData = jsonData.map((row: any) => {
					const filteredRow: any = {};
					Sources.forEach((header) => {
						if (header in row) {
							filteredRow[header] = row[header];
						}
					});
					return filteredRow;
				});

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

				console.timeEnd('start sort');
				
				const instance = generateJSONFromSchema(schema);
				// const sortedJson = clearDescriptions(instance)
				console.log('jSchema', JSON.stringify(instance));
				console.log('jSchema', instance);
				createDownloadLinks();
			}
		});
	}

	let SourceToTarget: any[] = [];

	function processMappings(mappings: Mapping[], Sources: any) {
		let nestedEntries: any;
		let entries: any;

		for (const mapping of mappings) {
			for (const key in mapping) {
				entries = mapping[key as keyof Mapping];
				if (Array.isArray(entries)) {
					for (const entry of entries) {
						// Direkte Zuordnung von Source zu Target
						if (entry.Source) {
							Sources.add(entry.Source);
						}

						if (entry.Source && entry.Target) {
							let mapEntry = new Set<string>();
							mapEntry.add(entry.Source);
							mapEntry.add(entry.Target);
							SourceToTarget.push(mapEntry);
						}

						// Dynamische Überprüfung aller Schlüssel im entry-Objekt
						for (const nestedKey in entry) {
							nestedEntries = entry[nestedKey as keyof MappingEntry];

							// Wenn der Wert ein Array ist, rufe processMappings rekursiv auf
							if (Array.isArray(nestedEntries)) {
								processMappings([{ publication: nestedEntries }], Sources);
							}
						}
					}
				}
			}
		}
		return Sources;
	}

	// count commas
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

	// function generateJSONFromSchema(schema: any): any {
	// 	const result: { [key: string]: any } = {};

	// 	Object.keys(schema.properties).forEach((key) => {
	// 		const property = schema.properties[key];
	// 		if (property.type === 'string') {
	// 			result[key] = '';
	// 		} else if (property.type === 'boolean') {
	// 			result[key] = false;
	// 		} else if (property.type === 'integer') {
	// 			result[key] = 0;
	// 		} else if (property.type === 'array') {
	// 			result[key] = [];
	// 		} else if (property.type === 'object') {
	// 			result[key] = generateJSONFromSchema(property);
	// 		}
	// 	});

	// 	return result;
	// }

// function generateJSONFromSchema(schema: any): any {
// 	let result: { [key: string]: any } = {};

//   Überprüfe, ob es eine properties-Eigenschaft gibt
//   if (schema.properties) {
//     Object.keys(schema.properties).forEach(key => {
//       const property = schema.properties[key];

//       Wenn es ein "title" und "description" gibt, füge es zum Resultat hinzu
//       if (property.title && property.description !== undefined) {
//         result[property.title] = property.description;
//       }

	  
// 	  if (property.description !== undefined) {
//         property.description = '';  // Description auf leeren String setzen
//       }

//       Falls es verschachtelte Eigenschaften gibt, rekursiv weiter verarbeiten
//       if (property.properties) {
//         const nestedResult = generateJSONFromSchema(property);
//         result = { ...result, ...nestedResult };

		
//       }
//     });
//   }

//   return result;
// }

interface PropertySchema {
  type: string;
  properties?: Record<string, PropertySchema>;
}

interface Schema {
  properties: Record<string, PropertySchema>;
}

interface Result {
  [key: string]: any;
}

function generateJSONFromSchema(schema: Schema): Result {
  const result: Result = {};

  // Rekursive Funktion zur Verarbeitung der Schema-Properties
  function processProperties(properties: Record<string, PropertySchema>): Record<string, any> {
    const obj: Record<string, any> = {};

    for (const key in properties) {
      const property = properties[key];

      // Wenn die Property vom Typ 'object' ist, erstellen wir ein Objekt mit 'description'
      if (property.type === 'object') {
        obj[key] = { description: "" };
      }
      // Wenn die Property vom Typ 'array' ist, erstellen wir ein Array mit einem leeren Objekt mit 'description'
      else if (property.type === 'array') {
        obj[key] = [{}]; // Ein Array mit einem leeren Objekt
      }
      // Für alle anderen Typen setzen wir 'description' auf einen leeren String
      else {
        obj[key] = { description: "" };
      }
    }
    return obj;
  }

  // Verarbeitung des gesamten Schemas und Erstellen der JSON-Struktur
  for (const key in schema.properties) {
    result[key] = [processProperties(schema.properties[key].properties || {})];
  }

  return result;
}


// function generateJSONFromSchema(schema: any): any {
//   let result: { [key: string]: any } = {};

//   // Überprüfe, ob es eine properties-Eigenschaft gibt
//   if (schema.properties) {
//     Object.keys(schema.properties).forEach(key => {
//       const property = schema.properties[key];

//       // Wenn es ein "title" und "description" gibt, füge es zum Resultat hinzu
//       if (property.title && property.description !== undefined) {
//         result[property.title] = property.description;
//       }

//       // Setze description auf leeren String
//       if (property.description !== undefined) {
//         property.description = '';  // Description auf leeren String setzen
//       }

//       // Falls es verschachtelte Eigenschaften gibt, rekursiv weiter verarbeiten
//       if (property.properties) {
//         // Rekursiv die Eigenschaften durchgehen und dort descriptions ebenfalls leeren
//         const nestedResult = generateJSONFromSchema(property);
//         result = { ...result, ...nestedResult };
//       }
//     });
//   }

//   return result;
// }

function clearDescriptions(json: any) {
	console.log ('json instance', json)
	for (let key in json) {
		console.log(json[key])
//   if (json.hasOwnProperty(key) && (json[key] === "" || json[key] == null || (Array.isArray(json[key]) && json[key].length === 0))) {
//     console.log(json[key])
	// json[key] = "";  // Setze den Wert auf einen leeren String
  }
// }
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

	<div class="flex gap-5 w-full">
		<div id="fileLabel" class="w-16">Entity :</div>
		<div id="fileName" class="overflow-clip w-full">Entity name</div>
		<div id="fileButton" class="w-16">
			<FileButton button="btn variant-filled-primary h-9 w-16 shadow-md" name="chooseEntity"
				><Fa icon={faPlus} />
			</FileButton>
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
