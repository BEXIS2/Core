 <script lang="ts">
	import { Page } from '@bexis2/bexis2-core-ui';
	import Papa from 'papaparse';
	import Fa from 'svelte-fa';
	import { faFileArrowDown, faArrowUpFromBracket, faPlus, faChevronDown, faChevronUp } from '@fortawesome/free-solid-svg-icons';
	import { FileButton } from '@skeletonlabs/skeleton';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import { Table } from '@bexis2/bexis2-core-ui';
	import lineClamp from './components/lineClamp.svelte';
	import error from './components/error.svelte';
	import { invalidTableStore, validTableStore } from './stores';
	import type { errorItem, tableErrorItem } from './models';
	
	let csvData: any;
	let jsonData: any[] = [];
	let filename: string = '';
	// let dataStore = writable([]);

	let validData: any[] = [];
	let validDataCounter: any = null;
	let showValid = false;

	let invalidData: any[] = [];
	let invalidDataCounter: any = null;
	let showInvalid: boolean = true;


	let downloadLinkValidData = '';
	let downloadLinkInvalidData = '';

	let isLoading: boolean = false;

	$: validTableStore.set(validData);

	$: fillInvalidTableStore(invalidData);
	
	let errors: any[] = [];
	let showErrorMsg: boolean = false;

	let columnCfg: any = {
							"Author": {
								minWidth: 300,
								instructions: {
									renderComponent: lineClamp
									
								},
							},
							"Title": {
								minWidth: 300,
								instructions: {
									renderComponent: lineClamp
								},
							},
							"Abstract Note": {
								minWidth: 600,
								instructions: {
									renderComponent: lineClamp
								},
							}
						};

    function fillInvalidTableStore (data :any) {
		invalidTableStore.set(data);
		let tableErrorItem: tableErrorItem;
		let errorItem: errorItem;
		for(let i :number = 0; i < errors.length; i++)
		{
			console.log('errors row '+i,errors[i]);
			console.log(errors[i].length);
			for (let j :number = 0; j < errors[i].length ; j++)	{
				errorItem = errors[i][j];
				console.log("errorItem.column",errorItem.column);
				//console.log($invalidTableStore[i][errorItem.column]);
				tableErrorItem = {errorType: 'missmatch', errorMsg: errorItem.errorMsg, value: $invalidTableStore[i][errorItem.column]}
				$invalidTableStore[i][errorItem.column] = tableErrorItem;
			}
		}
	}
	// upload file and parse to json
	function handleFileUpload(event: any) {
		validDataCounter = 0;
		invalidDataCounter = 0;
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
		Papa.parse(data, {
			header: true,
			complete: function (results) {
				jsonData = results.data;
				sortData(jsonData);
				createDownloadLinks();
			}
		});
	}

	// count commas
	function countCommas(data: string): number {
		return data.split(',').length - 1;
	}
	
	// sort data in 2 different files, 1 with vaild data and 1 with invalid data
	function sortData(data: any) {
		validData = [];
		invalidData = [];
		let columnErrors: { [key: string]: any[] } = {};
		let cellError: errorItem[] = [];

console.log(data);
		let relevantColumns = Object.keys(data[0]).filter(
			(key) => key.startsWith('Data') || key.startsWith('Code')
		);
		console.log(relevantColumns);
		data.forEach((row: any, rowIndex: number) => {
			cellError = []; 
			const dataURL = row['Data URL'];
			if (dataURL) {
				if (countCommas(dataURL) === 0) {
					validData.push(row);
					validDataCounter += 1;
				} else {
					let numberOfCommas: number = countCommas(dataURL) ;
					let valid: boolean = true;
					

					for (let i = 0; i < relevantColumns.length; i++) {
						
						if (numberOfCommas != countCommas(row[relevantColumns[i]])) {
							cellError.push({row: rowIndex, column : relevantColumns[i], errorMsg: 'Number of entries dose not match with Data URL'})
							valid = false;

							if (!columnErrors[relevantColumns[i]]) {
                            columnErrors[relevantColumns[i]] = [];
                        }
                        columnErrors[relevantColumns[i]].push(rowIndex);
						}
					
					}
					if (!valid) {
						invalidDataCounter += 1;
						invalidData.push(row);
						errors.push(cellError);
						
					} else {
						validDataCounter += 1;
						validData.push(row);
					}
				}
			}
		});

		relevantColumns.forEach((columnHeader: string) => {
			if (!columnCfg[columnHeader]) {			// WenncolumnsWithError eine Spalte bereits existiert, füge den renderComponent hinzu
					columnCfg[columnHeader] = {
						...(columnCfg[columnHeader] || {}),
						disableFiltering: true,
						instructions: {
							renderComponent: error // Setzt den error renderComponent für diese Spalte
						}
					};
				}
		});
		createDownloadLinks();
	}

	// parse json back to csv and create download links
	function createDownloadLinks() {
		const validCSVString = Papa.unparse(validData, { delimiter: ',' }); // Ensure comma as separator
		const blobValid = new Blob([validCSVString], { type: 'text/csv' });
		downloadLinkValidData = URL.createObjectURL(blobValid);

		const invalidCSVString = Papa.unparse(invalidData, { delimiter: ',' }); // Ensure comma as separator
		const blobInvalid = new Blob([invalidCSVString], { type: 'text/csv' });
		downloadLinkInvalidData = URL.createObjectURL(blobInvalid);

		isLoading = false;
	}

	function downloadValidData() {
		const link = document.createElement('a');
		link.href = downloadLinkValidData;
		link.download = 'valid__data.csv';
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
			<div id="fileLabel" class="w-16">
				Entity :
			</div>
			<div id="fileName" class="overflow-clip w-full">
				Entity name
			</div>
			<div id="fileButton" class="w-16">
				<FileButton
					button="btn variant-filled-primary h-9 w-16 shadow-md"
					name="chooseEntity"
					><Fa icon={faPlus} />
				</FileButton>
			</div>
		</div>
		<div class="flex gap-5 w-full">
			{#if isLoading}
			<div class="w-full">
			<Spinner label="...loading"></Spinner>
			</div>
			<div class="w-16 placeholder h-9"/>
			{:else}
			<div id="fileLabel" class="w-16">
				File :
			</div>
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

		<!-- <div>
			<button class="btn variant-filled-primary" on:click={downloadValidData}
				><Fa icon={faFileArrowDown}></Fa></button>
		</div> -->
		
		<div class="card p-2">
			{#if validDataCounter != undefined}
				<div class="grid gap-5 w-full">
					<div class="text-left card variant-ghost-primary w-full flex gap-5 p-2 my-1">
						{#if !showValid}
							<div class="w-full align-middle">Show Valid Data</div>
						{:else}
							<div class="w-full align-middle">Hide Valid Data</div>
						{/if}
						<div class="w-8">
							<button class="chip variant-filled-primary" on:click={downloadValidData}
							><Fa icon={faFileArrowDown}></Fa></button>
						</div>
						<div class="w-9">
							{#if !showValid}
								<button class="chip" on:click={toggleValidForm}><Fa icon={faChevronDown}></Fa></button>
							{:else}
								<button class="chip" on:click={toggleValidForm}><Fa icon={faChevronUp}></Fa></button>
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
								"Author": {
									minWidth: 300,
									instructions: {
										renderComponent: lineClamp
										
									},
								},
								"Title": {
									minWidth: 300,
									instructions: {
										renderComponent: lineClamp
									},
								},
								"Abstract Note": {
									minWidth: 600,
									instructions: {
										renderComponent: lineClamp
									},
								},
							}
						}}
						/>
					{/if}	
				</div>
			{/if}

			{#if invalidDataCounter != undefined}
				<div class="w-full ">						
						<div class="text-left card variant-ghost-warning w-full flex gap-5 p-2 my-1">
							<div class="w-full align-middle">{invalidDataCounter} rows failed to import</div>
							<div class="w-9">
								{#if !showErrorMsg}
									<button class="chip" on:click={toggleErrorMsg}><Fa icon={faChevronDown}></Fa></button>
								{:else}
									<button class="chip" on:click={toggleErrorMsg}><Fa icon={faChevronUp}></Fa></button>
								{/if}
							</div>
						</div>
						{#if showErrorMsg}
							<div class="grid grid-cols-5 gap-1 w-full card variant-ringed-warning p-5 text-xs"  in:slide={{ delay: 400 }} out:slide>
								<div class="font-bold">Row</div>
								<div class="font-bold">Column</div>
								<div class="font-bold col-span-3">Error Messages</div>
								{#each errors as erRow}
									{#each erRow as error, i}
										{#if i === 0}
											<div class="col-span-5"><hr/></div>
											<div>{error.row}</div>
										{:else}
											<div></div>
										{/if}
										<div>{error.column}</div>
										<div class="text-red-500 col-span-3">{error.errorMsg}</div>
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
								><Fa icon={faFileArrowDown}></Fa></button>
							</div>
							<div class="w-9">
								{#if !showInvalid}
									<button class="chip" on:click={toggleInvalidForm}><Fa icon={faChevronDown}></Fa></button>
								{:else}
									<button class="chip" on:click={toggleInvalidForm}><Fa icon={faChevronUp}></Fa></button>
								{/if}
							</div>
						</div>
							
						{#if showInvalid}
							<Table
							config={{
								height: 600,
								id: 'Invalid',
								data: invalidTableStore,
								columns: columnCfg,
							}}
							/>
							
						{/if}	
				</div>
			{/if}
		</div>
</Page>

 <!-- C:\Users\xi68neg\Desktop\Bexis\repo\Console\BExIS.Web.Shell\Areas\RPM\BExIS.Modules.Rpm.UI.Svelte\src\routes\unit\+page.svelte --> 

 
