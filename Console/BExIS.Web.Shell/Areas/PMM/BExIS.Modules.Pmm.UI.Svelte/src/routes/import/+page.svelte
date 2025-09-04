<script lang="ts">
	import { DropdownKVP, Page } from '@bexis2/bexis2-core-ui';
	import Papa from 'papaparse';
	import Fa from 'svelte-fa';
	import { tick } from 'svelte';
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
	import { invalidTableStore, validTableStore } from './stores';
	import type { errorArray, errorItem, tableErrorItem, ValidationType } from './models';
	import { onMount } from 'svelte';
	import * as apiCalls from './services/apiCalls';
	import mappingPublication from './mappingPublication.json';
	import mappingResource from './mappingResource.json';

	type dataSetType = {
		Title: string;
		Description: string;
		DataStructureId: number;
		MetadataStructureId: number;
		EntityTemplateId: number;
	};

	let filename: string = '';

	let entities: any[] = [];
	let transformedArray: any[] = [];
	let target: number;
	let tempTitle: string | null = null;
	let dataset: dataSetType = {
		Title: '',
		Description: '',
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
		transformedArray = entities.map((entity) => ({
			id: entity.id,
			text: entity.name
		}));
	});

	function clear() {
		invalidDataCounter = 0;
		invalidData = [];
		validData = [];
		errors = [];
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

				validateRows(jsonData, codeColumns, dataColumns);

				fillInvalidTableStore(invalidData);
				showInvalid = invalidData.length > 0 ? true : false;
				validTableStore.set(validData);

				createDownloadLinks();
			}
		});
	}

	function applyAvailableUponRequestRules(row: any) {
		function syncColumns(presentKey: string, targetKeys: string[], triggerValue: string) {
			if (!row[presentKey]) return;

			const presentValues = row[presentKey]
				.toString()
				.split(',')
				.map((v: string) => v.trim());

			for (const targetKey of targetKeys) {
				// Stelle sicher, dass die Zielzelle existiert (sonst leeres Array zum Füllen)
				let targetValues = (row[targetKey] || '')
					.toString()
					.split(',')
					.map((v: string) => v.trim());

				// Länge angleichen (falls Zielspalte kürzer ist)
				while (targetValues.length < presentValues.length) {
					targetValues.push('');
				}

				// Werte synchronisieren
				for (let i = 0; i < presentValues.length; i++) {
					if (presentValues[i].toLowerCase() === triggerValue.toLowerCase()) {
						targetValues[i] = triggerValue;
					}
				}

				row[targetKey] = targetValues.join(', ');
			}
		}

		// Regel 1: available upon request
		syncColumns('Data present', ['Data License', 'Data Publisher'], 'available upon request');
		syncColumns('Code present', ['Code License', 'Code Publisher'], 'available upon request');

		// Regel 2: no access (nur für Data)
		syncColumns(
			'Data present',
			[
				'Data License',
				'Data Publisher',
				'Data URL',
				'Data URL resolves',
				'Data DOI',
				'Data DOI resolves'
			],
			'no access'
		);

		syncColumns(
			'Code present',
			[
				'Code License',
				'Code Publisher',
				'Code URL',
				'Code URL resolves',
				'Code DOI',
				'Code DOI resolves'
			],
			'no access'
		);
		return row;
	}

	function validateRows(data: any[], codeColumns: string[], dataColumns: string[]) {
		let columnErrors: { [key: string]: any[] } = {};

		data.forEach((row: any, rowIndex: number) => {
			if (!row || Object.values(row).every((val) => (val ?? '').toString().trim() === '')) {
			} else {
				let validationType: ValidationType = validateRow(row, codeColumns, dataColumns);

				validationType.cellError.forEach((ce) => {
					if (!columnErrors[ce.column]) {
						columnErrors[ce.column] = [];
					}
					columnErrors[ce.column].push(rowIndex);
				});

				if (validationType.valid) {
					row = applyAvailableUponRequestRules(row);
					validData.push(row);
				} else {
					invalidDataCounter++;
					invalidData.push(row);
					errors.push({ rowIndex, cellErrors: validationType.cellError });
				}
				// console.log("valid", validData)
			}
		});

		// Fehlerhafte Spalten konfigurieren
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

	function validateRow(row: any, codeColumns: string[], dataColumns: string[]): ValidationType {
		let cellError: errorItem[] = [];
		let rowValid: boolean = true;
		const checkColumns = [
			{ columns: codeColumns, refColumn: 'Code URL' },
			{ columns: dataColumns, refColumn: 'Data URL' }
		];

		checkColumns.forEach(({ columns, refColumn }) => {
			const ref = row[refColumn];
			if (!ref) return;

			const commaCount = countCommas(ref);

			if (commaCount === 0) return; // gilt als valide

			columns.forEach((col) => {
				if (commaCount !== countCommas(row[col])) {
					cellError.push({
						column: col,
						errorMsg: `Number of entries does not match with ${refColumn}`
					});
					rowValid = false;
				}
			});
		});
		let validationType: ValidationType = { valid: rowValid, cellError: cellError };
		return validationType;
	}

	function countCommas(data: string): number {
		return data.split(',').length - 1;
	}

	function fillInvalidTableStore(data: any) {
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
	}

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

	function create() {
		// console.log('click');
		createAllDatasets(dataset.MetadataStructureId, dataset.EntityTemplateId);
	}

	let uploadedCount: number = 0;
	$: totalUploads = $validTableStore.length;

	async function createAllDatasets(
		metadataStructureId: number,
		entityTemplateId: number,
		dataStructureId: number = 0
	) {
		try {
			let dss: dataSetType[] = [];

			for (const row of validData) {
				dss.push(mapToApiFormat(row, mappingPublication));
			}

			uploadedCount = 0; // Reset beim Start

			for (let index = 0; index < dss.length; index++) {
				const ds = dss[index];
				const csvId = index;

				ds.MetadataStructureId = metadataStructureId;
				ds.EntityTemplateId = entityTemplateId;
				ds.DataStructureId = dataStructureId;

				let res = await apiCalls.createDataset(ds);
				getDatasets(res, csvId);

				uploadedCount++;

				// Update alle 100 oder beim letzten
				if (uploadedCount % 100 === 0 || uploadedCount === validData.length) {
					await tick(); // sorgt für UI-Update
				}
			}

			for (const map of idMapping) {
				const rowIndex = map[0];
				const metadataId = map[1];

				let metadata = await apiCalls.GetMetadata(metadataId);
				let MetadataScheema: any = null;
				if (MetadataScheema == null) {
					MetadataScheema = await apiCalls.GetMetadataScheema(metadata['@id']);
				}

				// console.log('metadata', metadata);

				applyMappingToMetadata(metadata, validData[rowIndex], mappingPublication.Mappings);
				console.log('finaldata', metadata);
				await apiCalls.putMetadata(metadataId, metadata);
				await apiCalls.GetMetadata(metadataId);
			}
		} catch (error) {
			console.error('Fehler beim Erstellen der Datensätze:', error);
		}
	}

	async function applyMappingToMetadata(metadata: any, row: any, mapping: any[]) {
		for (const map of mapping) {
			const source = map.Source;
			const rawTargetPath = map.Target;
			const value = row[source];
			if (value === undefined || value === null) continue;

			// "$." entfernen → nur die Pfadteile
			const cleanPath = rawTargetPath.replace(/^\$\./, '');
			const pathParts = cleanPath.split('.');

			let current = metadata;
			let parent = null;
			let key = null;

			for (let i = 0; i < pathParts.length; i++) {
				const part = pathParts[i];

				if (i === pathParts.length - 1) {
					parent = current;
					key = part;
				} else {
					if (!(part in current) || typeof current[part] !== 'object') {
						current[part] = {};
					}
					current = current[part];
				}
			}

			// Zielstruktur prüfen
			const targetStructure = parent?.[key];
			const isArray =
				Array.isArray(targetStructure) ||
				rawTargetPath.endsWith('[]') ||
				Array.isArray(row[source]);

			let values: string[] = [];

			// Wenn es ein Array ist oder ein String mit Trennzeichen
			if (Array.isArray(value)) {
				values = value;
			} else if (typeof value === 'string' && value.includes(';')) {
				values = value.split(';').map((v) => v.trim());
			} else {
				values = [value];
			}

			if (isArray) {
				parent[key] = values.map((v) => ({ '#text': v }));
			} else {
				parent[key] = { '#text': values[0] };
			}
		}
		let Resource: any = [];
		Resource = applyResourceMapping(metadata, row, mappingResource.Mappings);
		// console.log('Resource', Resource);
	}

	async function applyResourceMapping(metadata: any, row: any, mapping: any[]) {
		const resourceTemplate = metadata.Resource?.[0] || {};
		const filledResources = [];

		function setValueAtPath(obj: any, rawPath: string, value: any) {
			let cleanPath = rawPath;
			if (rawPath.startsWith('$.Resource.')) {
				cleanPath = rawPath.replace(/^\$\.Resource\./, '');
			} else if (rawPath.startsWith('$.')) {
				cleanPath = rawPath.replace(/^\$\./, '');
			}

			const pathParts = cleanPath.split('.');
			let current = obj;

			for (let i = 0; i < pathParts.length - 1; i++) {
				const part = pathParts[i];
				// Suche Property case-insensitiv
				const realKey =
					Object.keys(current).find((k) => k.toLowerCase() === part.toLowerCase()) || part;

				if (Array.isArray(current[realKey])) {
					current[realKey].forEach((item) => {
						setValueAtPath(item, pathParts.slice(i + 1).join('.'), value);
					});
					return;
				}

				if (!(realKey in current) || typeof current[realKey] !== 'object') {
					current[realKey] = {};
				}
				current = current[realKey];
			}

			const finalPart = pathParts[pathParts.length - 1];
			const realFinalKey =
				Object.keys(current).find((k) => k.toLowerCase() === finalPart.toLowerCase()) || finalPart;

			// Dynamische Array-Erkennung
			let values: string[] = [];
			if (Array.isArray(value)) {
				values = value;
			} else if (typeof value === 'string' && value.includes(';')) {
				values = value.split(';').map((v) => v.trim());
			} else {
				values = [value];
			}

			const isArray =
				rawPath.endsWith('[]') || Array.isArray(value) || Array.isArray(current[realFinalKey]);

			if (isArray) {
				current[realFinalKey] = values.map((v) => ({ '#text': v }));
			} else {
				current[realFinalKey] = { '#text': values[0] };
			}
		}
		function deepCopy(obj: any) {
			return JSON.parse(JSON.stringify(obj));
		}

		function splitAndTrim(value: string | undefined): string[] {
			return (value || '')
				.split(',')
				.map((s) => s.trim())
				.filter(Boolean);
		}

		// Schritt 1: Allgemeine Metadaten (nicht Resource-spezifisch)
		for (const map of mapping) {
			const source = map.Source;
			const rawTargetPath = map.Target;
			const value = row[source];
			if (value === undefined || value === null) continue;

			// Prüfe, ob das Mapping auf alle Resource-Objekte angewendet werden soll
			if (rawTargetPath.startsWith('$.Resource.')) {
				if (Array.isArray(metadata.Resource)) {
					metadata.Resource.forEach((resourceObj: any) => {
						setValueAtPath(resourceObj, rawTargetPath.replace('$.Resource.', ''), value);
					});
				}
				continue;
			}

			// Sonst wie gehabt
			setValueAtPath(metadata, rawTargetPath, value);
		}

		// Schritt 2: Ressourcen (Data und Code)
		const resourceTypes = ['Data', 'Code'];

		for (const type of resourceTypes) {
			// Hole alle Mapping-Einträge für diesen Typ
			const groupMappings = mapping.filter((m) => m.Source.startsWith(type));

			// Extrahiere die Werte für alle Spalten dieses Typs
			const groupedValues: Record<string, string[]> = Object.fromEntries(
				groupMappings.map((m) => [m.Source, splitAndTrim(row[m.Source])])
			);

			const urls = groupedValues[`${type} URL`] || [];
			const typeText = type === 'Data' ? 'Dataset' : 'Software';

			for (let i = 0; i < urls.length; i++) {
				const newResource = deepCopy(resourceTemplate);
				// newResource.Name = urls[i];

				// Setze den Ressourcentyp
				if (
					newResource.Resources_Type &&
					typeof newResource.Resources_Type === 'object' &&
					'#text' in newResource.Resources_Type
				) {
					newResource.Resources_Type['#text'] = typeText;
				} else {
					newResource.Resources_Type = { '#text': typeText };
				}

				// Fülle alle anderen Felder gemäß Mapping
				for (const map of groupMappings) {
					const valueList = groupedValues[map.Source];
					if (!valueList || !valueList[i]) continue;
					setValueAtPath(newResource, map.Target, valueList[i]);
				}
				console.log('newResource', newResource);
				filledResources.push(newResource);
			}
		}
		console.log('filled', filledResources);
		metadata.Resource = filledResources;

		return filledResources;
	}

	let idMapping: [number, number][] = []; // Initial leer

	function getDatasets(response: any, csvId: number) {
		idMapping.push([csvId, response.id]);
	}

	function mapToApiFormat(csvRow: any, mapping: any) {
		let ds: dataSetType = {
			Title: '',
			Description: '',
			DataStructureId: 0,
			MetadataStructureId: 0,
			EntityTemplateId: 0
		};

		for (let index = 0; index < 2; index++) {
			const map = mapping.Mappings[index];
			let sourceField = map.Source;

			if (sourceField) {
				if (index % 2 === 0) {
					// Jeder 1. Durchgang (Titel)
					ds.Title = csvRow[sourceField];
				} else if (ds.Title !== null) {
					// Jeder 2. Durchgang (Beschreibung)
					ds.Title = ds.Title;
					ds.Description = csvRow[sourceField];
				} else {
					tempTitle = null; // Zurücksetzen für das nächste Paar
				}
			}
		}

		return ds;
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

	function onChangeHandler(event) {
		let selectedEntity: number;
		selectedEntity = event.target.value;

		let searchEntity = entities.find((entity) => entity.id == selectedEntity);

		dataset.EntityTemplateId = selectedEntity;
		metadataStructureId = dataset.MetadataStructureId = searchEntity.metadataStructure.id;
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
	<div class="flex gap-5 w-full">
		<div id="datasetCounter">
			{#if validData.length > 0}
				<p class="card variant-ghost-primary gap-5 p-2 my-1 align-middle">
					{uploadedCount} of {totalUploads} datasets created
				</p>
			{/if}
		</div>
		<div id="progressBar" class="overflow-clip w-full flex items-center">
			{#if validData.length > 0}
				<progress max={totalUploads} value={uploadedCount}></progress>
			{/if}
		</div>
		<div id="importButton" class="w-24 flex items-center">
			{#if validData.length > 0 && dataset.EntityTemplateId}
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
