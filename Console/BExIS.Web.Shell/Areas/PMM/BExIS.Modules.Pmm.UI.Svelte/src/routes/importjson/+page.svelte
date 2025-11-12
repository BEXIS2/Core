<script lang="ts">
	/* -------------------- IMPORTS -------------------- */
	import { Page, DropdownKVP, TextInput, CheckboxKVPList, notificationStore, notificationType } from '@bexis2/bexis2-core-ui';
	import { FileButton } from '@skeletonlabs/skeleton';
	import {
		faArrowUpFromBracket,
		faChevronDown,
		faChevronUp
	} from '@fortawesome/free-solid-svg-icons';
	import { onMount } from 'svelte';
	import Fa from 'svelte-fa';
	import { slide } from 'svelte/transition';
	import JsonTree from 'svelte-json-tree';

	import mapping from './mapping.json';
	import type { validatedData, datasetType, issueType, fillDatasetType } from './models';
	import * as apiCalls from './services/apiCalls';
	import { applyTransformations } from './transformations';

	/* -------------------- VARIABLES -------------------- */
	// --- File & DOI ---
	let filename: string = '';
	let manualDOI: string = '';

	// --- Data & validation ---
	let validatedData: validatedData = {
		data: [],
		MissingFields: [],
		TypeMessage: ''
	};
	let DataArray: any[] = [];
	let showDataTree: boolean = false;

	// --- Entities / dropdown ---
	let entities: any[] = [];
	let transformedArray: any[] = [];
	let target: number;
	let EntityTemplateId: number;
	let MetadataStructureId: number;

	// --- Issues & checkbox ---
	let showErrorMsg: boolean = false;
	let hasIssuesCalled: boolean = false;
	let IssueRow: issueType = { Index: 0, errorType: '', msg: '' };
	let issueBlock: any[] = [];
	let groupedIssues: any[] = [];
	let currentIndex: number = 0;

	let selectAll: { key: number; value: string }[] = [{ key: -1, value: 'Select all' }];
	
	// let _prevSelectAllTarget: number[] = [];

	let initialized = false;
	let selectAllTarget: number[] = [];
	$: selectAllBoxes(selectAllTarget);
	$: deselectSelectBox(groupedIssues);

 function deselectSelectBox(issues: typeof groupedIssues) {
    const allFilled = issues.every(issue => issue.selected.length > 0);

    if (allFilled) {
      if (!initialized) {
      initialized = true;
      return; // 🚫 beim ersten Lauf: nichts tun
    }
	  
      selectAllTarget = [-1];
    } else {
      console.log("⚠️ Mindestens ein selected ist leer");
      selectAllTarget = [];
    }
  }
	/* -------------------- ON MOUNT -------------------- */

	onMount(async () => {
		entities = await apiCalls.getEntityTemplateList();
		transformedArray = entities.map((entity) => ({
			id: entity.id,
			text: entity.name
		}));
	});

	/* -------------------- HANDLERS -------------------- */

	function onChangeHandler(event: any) {
		let selectedEntity: number;
		selectedEntity = event.target.value;
		let searchEntity = entities.find((entity) => entity.id == selectedEntity);
		EntityTemplateId = +selectedEntity;
		MetadataStructureId = searchEntity.metadataStructure.id;
		console.log('Selected Entity Template ID:', EntityTemplateId);
		console.log('Selected Metadata Structure ID:', MetadataStructureId);
	}

	// handle dois - get metadata
	async function handleManualDOI() {
		if (!manualDOI || manualDOI.trim() === '') {
			let msg = "Please enter at least on DOI"
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: msg
			})
			
			// alert("Please enter at least one DOI.");
			return;
		}

		// Eingabe aufteilen anhand von Kommas oder Leerzeichen
		const doiList = manualDOI
			.split(/[,\s]+/) // trennt an Kommas oder Leerzeichen
			.map((doi) => doi.trim())
			.filter((doi) => doi.length > 0);

		if (doiList.length === 0) {
			alert('No valid DOIs entered.');
			return;
		}

		console.log('Manual DOIs entered:', doiList);

		DataArray = [];
		validatedData = { data: [], MissingFields: [], TypeMessage: '' };
		showDataTree = false;

		await grabMetadata(doiList);
	}

	// let json: any; //für debugging -> wenn gelöscht wird bei handleFileUpload const json =JSON.parse...

	async function handleFileUpload(event: Event) {
		const input = event.target as HTMLInputElement;
		if (input.files && input.files.length > 0) {
			DataArray = [];
			validatedData = { data: [], MissingFields: [], TypeMessage: '' };
			showDataTree = false;

			filename = input.files[0].name;
			const file = input.files[0];
			const reader = new FileReader();

			reader.onload = async (e) => {
				try {
					const json = JSON.parse(e.target?.result as string);
					// json = JSON.parse(e.target?.result as string);
					console.log('Uploaded JSON:', json);
					await grabDOI(json);
				} catch (err) {
					console.error('Invalid JSON file');
				}
			};

			reader.readAsText(file);
		}
	}

	async function grabDOI(json: any) {
		let DOIs: string[] = [];

		if (json.message) {
			DOIs.push(json.message.DOI);
			console.log('Grabbed DOI:', json.message.DOI);
		} else if (json.data && Array.isArray(json.data)) {
			json.data.forEach((item: any, i: number) => {
				DOIs.push(item?.message?.DOI);
				console.log(`Grabbed DOI [${i}]:`, item?.message?.DOI);
			});
		} else {
			console.warn('No DOI found!');
		}

		await grabMetadata(DOIs);
	}

	async function grabMetadata(dois: string[]) {
		let Metadata: any[] = [];
		for (const doi of dois) {
			try {
				const res = await apiCalls.getWorkByDOI(doi);
				// const res = json;
				Metadata.push(res?.message);
				console.log(`Metadata for DOI ${doi}:`, res?.message);
			} catch (e) {
				console.error(`Failed to fetch metadata for DOI ${doi}:`, e);
			}
		}
		validate(Metadata);
	}

	// validation
	function validate(Metadata: any[]) {
		console.log('Starting validation for metadata:', Metadata);
		Metadata.forEach((data) => {
			console.log('Validating data:', data);
			// validatedData.data = data;
			checkMandatoryFields(data);
			validateTypes(data);
			const transformedData = applyTransformations(data);
			console.log("transformierte daten", transformedData)
			validatedData.data = filterData(transformedData, mapping);

			DataArray.push({ ...validatedData }); // Kopie pushen
			validatedData = { data: [], MissingFields: [], TypeMessage: '' };
		});
		hasIssues(DataArray);
		showDataTree = true;
		console.log('All validated data:', DataArray);
	}

	function checkMandatoryFields(data: Record<string, any>): void {
		validatedData.MissingFields ??= [];
		for (const map of mapping.Mappings) {
			const source = map.Source?.trim();
			if (source === '') continue;
			const value =
				getNestedValue(data, source) ??
				getNestedValue(data.message, source) ??
				findValueByKeyRecursive(data, source) ??
				findValueByKeyRecursive(data.message, source);

			if (isEmptyValue(value)) {
				validatedData.MissingFields.push(source);
			}
		}
	}

function getNestedValue(obj: any, path: string): any {
	if (!obj || typeof path !== 'string') return undefined;
	const parts = path.split('.');
	let cur = obj;

	for (const p of parts) {
		if (cur == null || typeof cur !== 'object') return undefined;

		// Finde key case-insensitive
		const key = Object.keys(cur).find((k) => k.toLowerCase() === p.toLowerCase());
		if (!key) return undefined;

		cur = cur[key];
	}
	return cur;
}

	function findValueByKeyRecursive(obj: any, key: string): any {
		if (obj == null || typeof obj !== 'object') return undefined;
		if (Object.prototype.hasOwnProperty.call(obj, key)) return obj[key];
		for (const k of Object.keys(obj)) {
			const v = obj[k];
			if (typeof v === 'object' && v !== null) {
				const found = findValueByKeyRecursive(v, key);
				if (found !== undefined) return found;
			}
		}
		return undefined;
	}

	function isEmptyValue(value: any): boolean {
		if (value === null || value === undefined) return true;
		if (typeof value === 'string') return value.trim() === '';
		if (Array.isArray(value)) return value.length === 0 || value.every(isEmptyValue);
		if (typeof value === 'object') {
			const keys = Object.keys(value);
			return keys.length === 0 || keys.every((k) => isEmptyValue(value[k]));
		}
		return false;
	}

	function validateTypes(data: any) {
		console.log('Validating types for data:', data);
		if (data['update-to']?.[0]?.type === 'retraction') {
			validatedData.TypeMessage = 'This data has type "retraction". Do you really want to import?';
		}
	}

	function filterData(data: any, mapping: any) {
		// Alle relevanten Quellen aus der Mapping-Datei extrahieren
		const sources = mapping.Mappings.map((m: any) => m.Source?.trim()).filter(
			(s: string) => s && s.length > 0
		); // Nur nicht-leere Sources

		const filtered: Record<string, any> = {};

		for (const key of sources) {
			const value = getNestedValue(data, key);
			if (value !== undefined) {
				filtered[key] = value;
			}
		}

		return filtered;
	}

	function hasIssues(dataArray: any[]) {
		dataArray.forEach((data) => {
			if (data.MissingFields.length > 0 || data.TypeMessage) {
				hasIssuesCalled = true;
			}
		});

		if (hasIssuesCalled) {
			IssueInformation(dataArray);
		}
	}

	function IssueInformation(dataArray: any[]) {
		dataArray.forEach((data, index) => {
			if (data.MissingFields.length > 0) {
				data.MissingFields.forEach((field: string) => {
					IssueRow = {
						Index: index + 1,
						errorType: 'Error',
						msg: 'Missing Fields: ' + field
					};
					issueBlock.push({ ...IssueRow }); // Kopie pushen
					IssueRow = { Index: 0, errorType: '', msg: '' };
				});
			}
			if (data.TypeMessage) {
				IssueRow = {
					Index: index + 1,
					errorType: 'Warning',
					msg: 'Type Message: ' + data.TypeMessage
				};
				issueBlock.push({ ...IssueRow }); // Kopie pushen
				IssueRow = { Index: 0, errorType: '', msg: '' };
			}
		});
		console.log('Issue Information:', issueBlock);
		for (const issue of issueBlock) {
			let group = groupedIssues.find((g: any) => g.Index === issue.Index);
			if (!group) {
				group = {
					Index: issue.Index,
					issues: [],
					checkboxValue: [{ key: issue.Index, value: '' }],
					selected: []
				};
				groupedIssues.push(group);
			}
			group.issues.push({ errorType: issue.errorType, msg: issue.msg });
		}
	}

	// create datasets in bexis
	async function createDatasets(Array: any) {
		console.log('Creating datasets with validated data:', Array);
		const fillDatasetArray:any[] = [];
		for (const data of Array) {
			const mapped = mapToApiFormat(data);
			const res = await apiCalls.createDataset(mapped);
			console.log('Dataset created:', res);
			const fillDataset: fillDatasetType = {id: res.id, data: data.data}
			fillDatasetArray.push(fillDataset);
		}
		console.log('All datasets created!');
		fillDatasets(fillDatasetArray);
	}

	function mapToApiFormat(data: any) {
		console.log('Mapping data to API format:', data);
		const dataset: datasetType = {
			Title: '',
			Description: '',
			DataStructureId: 0,
			MetadataStructureId: MetadataStructureId,
			EntityTemplateId: EntityTemplateId
		};

		dataset.Title = data.data.title?.toString() ?? '';
		dataset.Description = data.data.abstract?.toString() ?? '';

		console.log('Mapped dataset:', dataset);
		return dataset;
	}

	async function fillDatasets(data: any) {
		console.log("Diese Datasets werden füllen", data);

		for (const dataset of data) {
			// Metadaten abrufen
			const getMetadata = await apiCalls.GetMetadata(dataset.id);
			let metadata = getMetadata; // wird überschrieben

			console.log("Ursprüngliche Metadaten:", metadata);

			// JSON-Daten aus deinem DOI-Metadaten-Array holen
			const json = dataset.data;

			// Über Mapping iterieren
			for (const map of mapping.Mappings) {
				const source = map.Source?.trim();
				const target = map.Target?.trim();

				// Source leer → überspringen
				if (!source) continue;

				// Wert aus JSON holen
				const value =
					getNestedValue(json, source) ??
					json[source] ??
					undefined;

				if (value === undefined) continue;

				// Typ anpassen
				const normalized = normalizeValue(value, "string");

				// Wert in Metadaten setzen
				setNestedValue(metadata, target, normalized);
			}

			console.log("Gefüllte Metadaten:", metadata);
			await apiCalls.putMetadata(dataset.id, metadata);

			// Optional: API call zum Speichern
			// await apiCalls.UpdateMetadata(dataset.id, metadata);
		}
	}

// Hilfsfunktion: Pfad im Objekt setzen (z.B. $.publication.Title → metadata.publication.Title)
	function setNestedValue(obj: any, path: string, value: any) {
		path = path.replace(/^\$\./, '');
		const parts = path.split('.');
		let cur = obj;

		for (let i = 0; i < parts.length - 1; i++) {
			const part = parts[i];

			// Case-insensitive lookup
			let key = Object.keys(cur).find((k) => k.toLowerCase() === part.toLowerCase());
			if (!key) {
				key = part;
				cur[key] = {};
			}

			cur = cur[key];
		}

		const lastKeyPart = parts[parts.length - 1];
		let lastKey = Object.keys(cur).find((k) => k.toLowerCase() === lastKeyPart.toLowerCase()) ?? lastKeyPart;

		const isArrayTarget = Array.isArray(cur[lastKey]);

		// FALL 1: Ziel ist ein Array → erstelle pro Wert ein Objekt mit #text
		if (isArrayTarget) {
			let values = Array.isArray(value) ? value : [value];

			cur[lastKey] = values.map((v) => ({
				"@ref": "",
				"@partyid": "",
				"#text": typeof v === "object" ? JSON.stringify(v) : String(v),
			}));
			return;
		}

		// FALL 2: Source ist ein Array, aber Ziel ist kein Array → prüfe ob Ziel-Template ein Array ist
		if (Array.isArray(value)) {
			// Wenn aktuelles Ziel-Objekt ein Array sein sollte (z. B. Author in Metadaten)
			if (cur[lastKey] && typeof cur[lastKey] === "object" && cur[lastKey]["#text"] !== undefined) {
				// Ein einzelnes Objekt vorhanden → in Array umwandeln
				cur[lastKey] = value.map((v) => ({
					"@ref": "",
					"@partyid": "",
					"#text": typeof v === "object" ? JSON.stringify(v) : String(v),
				}));
				return;
			}
		}

		// FALL 3: Einfacher Wert (Standard)
		if (typeof cur[lastKey] !== "object" || cur[lastKey] === null) {
			cur[lastKey] = { "#text": "" };
		} else if (!("#text" in cur[lastKey])) {
			cur[lastKey]["#text"] = "";
		}

		cur[lastKey]["#text"] = Array.isArray(value)
			? value.join(", ")
			: typeof value === "object"
			? JSON.stringify(value)
			: String(value);
	}



// Wandelt JSON-Wert auf erwarteten Typ um (einfach gehalten)
	function normalizeValue(value: any, expectedType: string): any {
		if (value === null || value === undefined) return "";
		if (Array.isArray(value)) {
			// Wenn Array von Objekten → JSON-String, sonst join
			if (value.every(v => typeof v === "object")) {
				return JSON.stringify(value);
			}
			return value.join(", ");
		}
		if (typeof value === "object") {
			// Objekt in JSON-String umwandeln
			return JSON.stringify(value);
		}
		if (typeof value === "boolean" || typeof value === "number") {
			return String(value);
		}
		return value.toString();
	}




	// checkbox stuff
	function checkCheckboxes() {
		// Finde alle Indizes, deren selected leer ist (diese sollen raus)
		const removeIndices = groupedIssues
			.filter((g: any) => g.selected.length === 0)
			.map((g: any) => g.Index - 1);

		// Erzeuge ein NEUES Array ohne die zu entfernenden Einträge
		const filteredDataArray = DataArray.filter((_, i) => !removeIndices.includes(i));

		console.log('Gefiltertes DataArray:', filteredDataArray);
		console.log("groupoed Issues:", groupedIssues)
		createDatasets(filteredDataArray);
	}


	// Reaktiver Watcher: läuft automatisch, wenn selectAllTarget sich ändert
 //if (!arraysEqual(_prevSelectAllTarget, selectAllTarget)) {
	function selectAllBoxes(selectAll: any[]){
 	// update prev
		if(selectAll.length == 1 && selectAllTarget[0] == -1){
			console.log('✅ Checkbox ist angeklickt:', selectAll);

			groupedIssues = groupedIssues.map((group: any) => ({
				...group,
				selected: [group.Index] // neue Array-Referenz
			}));	
			//_prevSelectAllTarget = [];
		}
		else{
			console.log('❌ Checkbox ist nicht angeklickt', selectAll);

			groupedIssues = groupedIssues.map((group: any) => ({
				...group,
				selected: [] // neue Array-Referenz
			}));
			//_prevSelectAllTarget = ["-1"];
		}
		console.log('Checkbox ist :', selectAll);
	}
		// console.log('lalelu',_prevSelectAllTarget);
		// führe handler aus
		//onSelectAllChanged(selectAllTarget);
	//}

	function next() {
		if (currentIndex < DataArray.length - 1) currentIndex++;
	}

	function prev() {
		if (currentIndex > 0) currentIndex--;
	}

	function toggleErrorMsg() {
		showErrorMsg = !showErrorMsg;
	}

	function jumpTo(index: number) {
		console.log('das ist der index', index);
		currentIndex = index - 1;
	}

	function checkSingle(){
		console.log("aktueller index", currentIndex)
		console.log("grouped Issues", groupedIssues)
		console.log("absenden", ) 

	}
</script>

<Page>
	<h1 class="h1">Import Publications</h1>

	<!-- Dropdown -->
	<div class="flex gap-5 w-full">
		<div id="fileLabel" class="w-36 mt-3">Entity Template :</div>
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

	<!-- Datei-Upload -->
	<div class="flex gap-5 w-full mt-4">
		<div id="fileLabel" class="w-16">File :</div>
		<div id="fileName" class="overflow-clip w-full">{filename}</div>
		<div id="fileButton" class="w-16">
			<FileButton
				id="uploadJSON"
				title="Upload JSON"
				button="btn variant-filled-primary h-9 w-16 shadow-md"
				name="uploadJSON"
				accept=".json,application/json"
				on:change={handleFileUpload}
			>
				<Fa icon={faArrowUpFromBracket} />
			</FileButton>
		</div>
	</div>

	<!--  Manuelle DOI-Eingabe -->
	<div class="flex gap-5 w-full mt-4">
		<div id="manualDOILabel" class="w-36 mt-3">Manual DOI :</div>
		<div class="over-clip w-full">
			<TextInput id="name" required={false} bind:value={manualDOI} />
		</div>
		<button class="btn variant-filled-primary h-9 shadow-md mt-2" on:click={handleManualDOI}>
			Fetch DOI
		</button>
	</div>

	{#if hasIssuesCalled}
		<div class="w-full">
			<div class="text-left card variant-ghost-warning w-full flex gap-5 p-2 my-1">
				<div class="w-full align-middle">Issues</div>
				<div class="w-9">
					{#if !showErrorMsg}
						<button class="chip" on:click={toggleErrorMsg}><Fa icon={faChevronDown}></Fa></button>
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
					<div class="font-bold">Selection</div>
					<div class="font-bold">Index</div>
					<div class="font-bold">Issue Type</div>
					<div class="font-bold">Message</div>

					{#each groupedIssues as issueGroup}
						<div class="col-span-5"><hr /></div>
						<div>
							<CheckboxKVPList
								id="issueSelect"
								key=""
								description=""
								title=""
								source={issueGroup.checkboxValue}
								bind:target={issueGroup.selected}
							/>
						</div>
						<div>{issueGroup.Index}</div>

						{#each issueGroup.issues as singleIssue, i}
							{#if i > 0}
								<div class="col-span-2"></div>
							{/if}
							<div>{singleIssue.errorType}</div>
							<div class="text-red-500">{singleIssue.msg}</div>
							{#if i > 0}
								<div></div>
							{:else}
								<div class="text-right">
									<button
										on:click={() => jumpTo(issueGroup.Index)}
										class="badge variant-filled-primary shadow-md w-16 text-xs"
									>
										Jump to
									</button>
								</div>
							{/if}
						{/each}
					{/each}
					<div>
						<CheckboxKVPList
							id="selectAll"
							key=""
							description=""
							title=""
							source={selectAll}
							bind:target={selectAllTarget}
						/>
					</div>
				</div>
			{/if}
		</div>
	{/if}

	<!-- Dataset-Erstellung -->
	{#if showDataTree && DataArray.length > 0}
		<div class="mt-6 flex flex-col items-center gap-4">
			<h2 class="text-xl font-bold mb-2">
				Your JSON import {currentIndex + 1} / {DataArray.length}
			</h2>

			<!-- JSON Tree -->
			<div
				class="w-full max-w-4xl text-gray-100 p-4 rounded-xl shadow-md overflow-auto max-h-[500px]"
			>
				<JsonTree
					value={DataArray[currentIndex].data}
					defaultExpandedLevel={700}
					shouldShowPreview={false}
				/>
			</div>

			<!-- Navigation Buttons -->
			<div class="flex justify-between items-center w-full max-w-4xl mt-3">
				<div class="flex gap-3">
					<button
						class="btn variant-filled-primary shadow-md disabled:opacity-50"
						on:click={prev}
						disabled={currentIndex === 0}
					>
						Prev
					</button>

					<button
						class="btn variant-filled-primary shadow-md disabled:opacity-50"
						on:click={next}
						disabled={currentIndex === DataArray.length - 1}
					>
						Next
					</button>
				</div>
				<div>
					{#if EntityTemplateId}
						<button class="btn variant-filled-primary shadow-md" on:click={checkSingle}>
							Confirm
						</button>
						<button class="btn variant-filled-primary shadow-md" on:click={checkCheckboxes}>
							Confirm all
						</button>
					{:else}
						<button class="btn variant-filled-primary shadow-md disabled" disabled>
							Confirm
						</button>
						<button class="btn variant-filled-primary shadow-md disabled" disabled>
							Confirm all
						</button>
					{/if}
				</div>
				
			</div>
		</div>
	{/if}
</Page>
