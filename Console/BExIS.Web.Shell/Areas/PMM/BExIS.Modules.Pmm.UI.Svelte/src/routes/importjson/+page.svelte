<script lang="ts">
/* -------------------- IMPORTS -------------------- */
	import { Page, DropdownKVP, TextInput, CheckboxKVPList } from '@bexis2/bexis2-core-ui';
	import { FileButton } from '@skeletonlabs/skeleton';
	import { faArrowUpFromBracket, faChevronDown, faChevronUp } from '@fortawesome/free-solid-svg-icons';
	import { onMount } from 'svelte';
	import Fa from 'svelte-fa';
	import { slide } from 'svelte/transition';
	import JsonTree from 'svelte-json-tree';

	import mapping from './mapping.json';
	import type { validatedData, datasetType, issueType } from './models';
	import * as apiCalls from './services/apiCalls';
	

/* -------------------- VARIABLES -------------------- */
	// --- File & DOI ---
	let filename: string = '';
	let manualDOI: string = '';
	let noValidDOi: boolean = false;

	// --- Data & validation ---
	let validatedData: validatedData = {
		data: [],
		MissingFields: [],
		TypeMessage: "",
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
	let IssueRow: issueType = { Index: 0, errorType: "", msg: "" };
	let issueBlock: any[] = [];
	let groupedIssues: any[] = [];
	let currentIndex: number = 0;

	let selectAll: { key: number; value: string }[] = [
		{ key: -1, value: "Select all" }
	];
	$: selectAllTarget = [];
	let _prevSelectAllTarget: string[] = [];



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
		console.log("Selected Entity Template ID:", EntityTemplateId);
		console.log("Selected Metadata Structure ID:", MetadataStructureId);
	}




	// handle dois - get metadata
	async function handleManualDOI() {
		if (!manualDOI || manualDOI.trim() === "") {
			noValidDOi = true;
			// alert("Please enter at least one DOI.");
			return;
		}

		// Eingabe aufteilen anhand von Kommas oder Leerzeichen
		const doiList = manualDOI
			.split(/[,\s]+/) // trennt an Kommas oder Leerzeichen
			.map((doi) => doi.trim())
			.filter((doi) => doi.length > 0);

		if (doiList.length === 0) {
			alert("No valid DOIs entered.");
			return;
		}

		console.log("Manual DOIs entered:", doiList);

		DataArray = [];
		validatedData = { data: [], MissingFields: [], TypeMessage: "" };
		showDataTree = false;

		await grabMetadata(doiList);
	}



	// let json: any; //für debugging -> wenn gelöscht wird bei handleFileUpload const json =JSON.parse...

	async function handleFileUpload(event: Event) {
		const input = event.target as HTMLInputElement;
		if (input.files && input.files.length > 0) {

			DataArray = [];
			validatedData = { data: [], MissingFields: [], TypeMessage: "" };
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
			console.log("Grabbed DOI:", json.message.DOI);
		} else if (json.data && Array.isArray(json.data)) {
			json.data.forEach((item: any, i: number) => {
				DOIs.push(item?.message?.DOI);
				console.log(`Grabbed DOI [${i}]:`, item?.message?.DOI);
			});
		} else {
			console.warn("No DOI found!");
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
		console.log("Starting validation for metadata:", Metadata);
		Metadata.forEach((data) => {
			console.log("Validating data:", data);
			// validatedData.data = data;
			checkMandatoryFields(data);
			validateTypes(data);
			validatedData.data = filterData(data, mapping);

			DataArray.push({ ...validatedData }); // Kopie pushen
			validatedData = { data: [], MissingFields: [], TypeMessage: "" };
		});
		hasIssues(DataArray);
		showDataTree = true;
		console.log("All validated data:", DataArray);
	}

 

	function checkMandatoryFields(data: Record<string, any>): void {
		validatedData.MissingFields ??= [];
		for (const map of mapping.Mappings) {
			const source = map.Source?.trim();
			if (source === "") continue;
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
			if (cur == null) return undefined;
			cur = cur[p];
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
			return keys.length === 0 || keys.every(k => isEmptyValue(value[k]));
		}
		return false;
	}



	function validateTypes(data: any) {
		console.log("Validating types for data:", data);
		if (data['update-to']?.[0]?.type === "retraction") {
			validatedData.TypeMessage = 'This data has type "retraction". Do you really want to import?';
		}
	}



	function filterData(data: any, mapping: any) {
		// Alle relevanten Quellen aus der Mapping-Datei extrahieren
		const sources = mapping.Mappings
			.map((m: any) => m.Source?.trim())
			.filter((s: string) => s && s.length > 0); // Nur nicht-leere Sources

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
						errorType: "Error",
						msg: "Missing Fields: " + field
					};
					issueBlock.push({ ...IssueRow}); // Kopie pushen
					IssueRow = { Index: 0, errorType: "", msg: "" };
				});
			}
			if (data.TypeMessage) {
				IssueRow = {
					Index: index + 1,
					errorType: "Warning",
					msg: "Type Message: " + data.TypeMessage
				};
				issueBlock.push({ ...IssueRow}); // Kopie pushen
				IssueRow = { Index: 0, errorType: "", msg: "" };
			}
		});
		console.log("Issue Information:", issueBlock);
		for (const issue of issueBlock) {
			let group = groupedIssues.find((g: any) => g.Index === issue.Index);
			if (!group) {
			group = { 
				Index: issue.Index,
				issues: [],
				checkboxValue: [{ key: issue.Index, value: ""}],
				selected: [] 
			};
			groupedIssues.push(group);
			}
    		group.issues.push({ errorType: issue.errorType, msg: issue.msg });
  		}
	}



// create datasets in bexis
	async function createDatasets() {
		console.log("Creating datasets with validated data:", DataArray);
		for (const data of DataArray) {
			const mapped = mapToApiFormat(data);
			const res = await apiCalls.createDataset(mapped);
			console.log("Dataset created:", res);
		}
		console.log("All datasets created!");
	}



	function mapToApiFormat(data: any) {
		console.log("Mapping data to API format:", data);
		const dataset: datasetType = {
			Title: '',
			Description: '',
			DataStructureId: 0,
			MetadataStructureId: MetadataStructureId,
			EntityTemplateId: EntityTemplateId
		};

		dataset.Title = data.data.title?.toString() ?? "";
		dataset.Description = data.data.abstract?.toString() ?? "";

		console.log("Mapped dataset:", dataset);
		return dataset;
	}



// checkbox stuff
	function checkCheckboxes() {
		// Finde alle Indizes, deren selected leer ist (diese sollen raus)
		const removeIndices = groupedIssues
			.filter((g: any) => g.selected.length === 0)
			.map((g: any) => g.Index - 1);

		// Erzeuge ein NEUES Array ohne die zu entfernenden Einträge
		const filteredDataArray = DataArray.filter((_, i) => !removeIndices.includes(i));

		console.log("Gefiltertes DataArray:", filteredDataArray);
	}



	// Hilfsfunktion für tiefen Array-Vergleich (einfach)
	function arraysEqual(a: any[], b: any[]) {
		if (a === b) return true;
		if (a.length !== b.length) return false;
		for (let i = 0; i < a.length; i++) {
		if (a[i] !== b[i]) return false;
		}
		return true;
	}

	// Reaktiver Watcher: läuft automatisch, wenn selectAllTarget sich ändert
	$: if (!arraysEqual(_prevSelectAllTarget, selectAllTarget)) {
		// update prev
		_prevSelectAllTarget = [...selectAllTarget];
		// führe handler aus
		onSelectAllChanged(selectAllTarget);
	}



	function onSelectAllChanged(current: string[]) {
		if (current.length > 0) {
		console.log("✅ Checkbox ist angeklickt:", current);

		groupedIssues = groupedIssues.map((group: any) => ({
		...group,
		selected: [group.Index] // neue Array-Referenz
		}));
		} else {
		console.log("❌ Checkbox ist nicht angeklickt", current);

			groupedIssues = groupedIssues.map((group: any) => ({
		...group,
		selected: [] // neue Array-Referenz
		}));
		}
	}



	function closeNoDOIAlert() {
		noValidDOi = false;
	}

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
		console.log("das ist der index", index);
		currentIndex = index -1;
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
		<!-- <input
			type="text"
			placeholder="Enter DOI manually..."
			bind:value={manualDOI}
			class="input input-bordered w-full"
		/> -->
		<button
			class="btn variant-filled-primary h-9 shadow-md mt-2"
			on:click={handleManualDOI}
		>
			Fetch DOI
		</button>

	</div>


			{#if noValidDOi}
		<aside class="alert variant-ghost-error w-80">
			<div class="alert-message">
				<p>Please enter at least one DOI.</p>
			</div>
			<div class="alert-actions">
				<button on:click={closeNoDOIAlert} class="btn hover:text-primary-100" title="Close alert"><svg class="svelte-fa svelte-fa-base undefined svelte-bvo74f" viewBox="0 0 384 512" aria-hidden="true" role="img" xmlns="http://www.w3.org/2000/svg"><g transform="translate(192 256)" transform-origin="96 0"><g transform="translate(0,0) scale(1,1)"><path d="M342.6 150.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0L192 210.7 86.6 105.4c-12.5-12.5-32.8-12.5-45.3 0s-12.5 32.8 0 45.3L146.7 256 41.4 361.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0L192 301.3 297.4 406.6c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L237.3 256 342.6 150.6z" fill="currentColor" transform="translate(-192 -256)"></path></g></g></svg></button>
			</div>
		</aside>
	{/if}

	{#if hasIssuesCalled}
		<div class="w-full">
			<div class="text-left card variant-ghost-warning w-full flex gap-5 p-2 my-1">
				<div class="w-full align-middle">Issues</div>
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
			<div class="grid grid-cols-5 gap-1 w-full card variant-ringed-warning p-5 text-xs" in:slide={{ delay: 400 }} out:slide>
				<div class="font-bold">Selection</div>
				<div class="font-bold">Index </div>
				<div class="font-bold">Issue Type</div>
				<div class="font-bold ">Message</div>

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
							<button on:click={() => jumpTo(issueGroup.Index)} class="badge variant-filled-primary shadow-md w-16 text-xs">
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
			<h2 class="text-xl font-bold mb-2">Your JSON import {currentIndex + 1} / {DataArray.length}</h2>

			<!-- JSON Tree -->
			<div class="w-full max-w-4xl text-gray-100 p-4 rounded-xl shadow-md overflow-auto max-h-[500px]">
				<JsonTree value={DataArray[currentIndex].data} defaultExpandedLevel={700} shouldShowPreview={false} />
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
			<button class="btn variant-filled-primary shadow-md" on:click={checkCheckboxes}>
				Confirm
			</button>
		</div>
	</div>
{/if}

	

</Page>
