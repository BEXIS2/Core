<script lang="ts">

	/* ============================================================
	   IMPORTS
	   ============================================================ */
	import {
		Page,
		DropdownKVP,
		TextArea,
		CheckboxKVPList,
		notificationStore,
		notificationType
	} from '@bexis2/bexis2-core-ui';

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
	import type {
		validatedData,
		datasetType,
		issueType,
		fillDatasetType
	} from './models';

	import * as apiCalls from './services/apiCalls';
	import { applyTransformations } from './transformations';



	/* ============================================================
	   GLOBAL STATE VARIABLES
	   ============================================================ */

	// --- FILE & DOI INPUT ---
	let filename: string = "";             // name of uploaded JSON/TXT file
	let manualDOI: string = "";            // DOI(s) typed manually

	// --- VALIDATION RESULT STRUCTURE ---
	let validatedData: validatedData = {
		data: [],
		MissingFields: [],
		TypeMessage: "",
		imported: false
	};

	let DataArray: any[] = [];             // stores validated results for each DOI
	let showDataTree: boolean = false;     // toggles preview

	// --- ENTITY TEMPLATE SELECTION (BEXIS) ---
	let entities: any[] = [];              // list of all entity templates from backend
	let transformedArray: any[] = [];      // dropdown-compatible structure
	let target: number;                    // dropdown selected ID
	let EntityTemplateId: number;          // selected template ID
	let MetadataStructureId: number;       // metadata schema ID for import

	// --- ISSUE HANDLING ---
	let showErrorMsg: boolean = false;     // toggle for issue components
	let hasIssuesCalled: boolean = false;
	let IssueRow: issueType = { Index: 0, errorType: "", msg: "" };
	let issueBlock: any[] = [];            // flat list of all issues
	let groupedIssues: any[] = [];         // issues grouped by dataset index
	let currentIndex: number = 0;          // for step navigation

	// --- "SELECT ALL" CHECKBOX HANDLING ---
	let selectAll = [{ key: -1, value: "Select all" }];
	let initialized = false;               // ensures select-all does not auto-trigger on first run
	let selectAllTarget: number[] = [];

	// Reactive Svelte statements
	$: selectAllBoxes(selectAllTarget);
	$: deselectSelectBox(groupedIssues);



	/* ============================================================
	   SELECT-ALL LOGIC
	   ============================================================ */

	// Automatically deactivate select-all when not all entries are selected
	function deselectSelectBox(issues: typeof groupedIssues) {
		const allFilled = issues.every(issue => issue.selected.length > 0);

		if (allFilled) {
			if (!initialized) {
				initialized = true; // prevent first-run auto-trigger
				return;
			}
			selectAllTarget = [-1]; // enable select-all
		} else {
			selectAllTarget = [];   // disable select-all
		}
	}



	/* ============================================================
	   LIFECYCLE: OnMount
	   ============================================================ */
	onMount(async () => {
		// Load available entity templates for metadata import
		entities = await apiCalls.getEntityTemplateList();

		// Convert to dropdown format
		transformedArray = entities.map(entity => ({
			id: entity.id,
			text: entity.name
		}));
	});



	/* ============================================================
	   HANDLERS: ENTITY SELECTION
	   ============================================================ */
	function onChangeHandler(event: any) {
		const selectedEntity = Number(event.target.value);

		const searchEntity = entities.find(e => e.id === selectedEntity);

		EntityTemplateId = selectedEntity;
		MetadataStructureId = searchEntity.metadataStructure.id;

		console.log("Selected Entity Template ID:", EntityTemplateId);
		console.log("Selected Metadata Structure ID:", MetadataStructureId);
	}



	/* ============================================================
	   TXT FILE UPLOAD — PARSE DOI LIST
	   ============================================================ */
	async function handleTxtUpload(event: Event) {
		const input = event.target as HTMLInputElement;
		if (!input.files?.length) return;

		filename = input.files[0].name;
		const file = input.files[0];

		const reader = new FileReader();
		reader.onload = async e => {
			const content = e.target?.result as string;

			// Split by comma, semicolon, whitespace, line breaks
			const doiList = content
				.split(/[,\n\r; ]+/)
				.map(d => d.trim())
				.filter(d => d.length > 0);

			if (!doiList.length) {
				notificationStore.showNotification({
					notificationType: notificationType.error,
					message: "No valid DOIs found in text file."
				});
				return;
			}

			console.log("DOIs from TXT:", doiList);

			// Reset UI state
			DataArray = [];
			validatedData = { data: [], MissingFields: [], TypeMessage: "", imported: false };
			showDataTree = false;

			await grabMetadata(doiList);
		};

		reader.readAsText(file);
	}



	/* ============================================================
	   MANUAL DOI INPUT
	   ============================================================ */
	async function handleManualDOI() {
		if (!manualDOI.trim()) {
			notificationStore.showNotification({
				notificationType: notificationType.error,
				message: "Please enter at least one DOI"
			});
			return;
		}

		// Split by comma or whitespace
		const doiList = manualDOI
			.split(/[,\s]+/)
			.map(d => d.trim())
			.filter(d => d.length > 0);

		if (!doiList.length) {
			alert("No valid DOIs entered.");
			return;
		}

		console.log("Manual DOIs:", doiList);

		// Reset previous results
		DataArray = [];
		validatedData = { data: [], MissingFields: [], TypeMessage: "", imported: false };
		showDataTree = false;

		await grabMetadata(doiList);
	}



	/* ============================================================
	   JSON FILE UPLOAD — EXTRACT DOI(s)
	   ============================================================ */
	async function handleFileUpload(event: Event) {
		const input = event.target as HTMLInputElement;
		if (!input.files?.length) return;

		DataArray = [];
		validatedData = { data: [], MissingFields: [], TypeMessage: "", imported: false };
		showDataTree = false;

		filename = input.files[0].name;
		const file = input.files[0];

		const reader = new FileReader();
		reader.onload = async e => {
			try {
				const json = JSON.parse(e.target?.result as string);
				console.log("Uploaded JSON:", json);
				await grabDOI(json);
			} catch {
				console.error("Invalid JSON file");
			}
		};

		reader.readAsText(file);
	}



	/* ============================================================
	   EXTRACT DOI(S) FROM JSON
	   ============================================================ */
	async function grabDOI(json: any) {
		const DOIs: string[] = [];

		if (json.message) {
			// JSON contains single item
			DOIs.push(json.message.DOI);
		}
		else if (json.data && Array.isArray(json.data)) {
			// JSON contains list of items
			json.data.forEach((item: any, i: number) => {
				DOIs.push(item?.message?.DOI);
			});
		}
		else {
			console.warn("No DOI found!");
		}

		await grabMetadata(DOIs);
	}



	/* ============================================================
	   FETCH METADATA FOR EACH DOI
	   ============================================================ */
	async function grabMetadata(dois: string[]) {
		const Metadata: any[] = [];

		for (const doi of dois) {
			try {
				const res = await apiCalls.getWorkByDOI(doi);
				Metadata.push(res?.message);
			} catch (e) {
				console.error(`Failed to fetch metadata for DOI ${doi}:`, e);
			}
		}

		validate(Metadata);
	}



	/* ============================================================
	   VALIDATION: MANDATORY FIELDS & TYPE CHECKING
	   ============================================================ */
	function validate(Metadata: any[]) {
		Metadata.forEach(data => {

			checkMandatoryFields(data);
			validateTypes(data);

			// apply custom rules from transformations.ts
			const transformedData = applyTransformations(data);

			// Filter data based on mapping.json
			validatedData.data = filterData(transformedData, mapping);

			// Push a *copy* to DataArray
			DataArray.push({ ...validatedData });

			// Reset for next iteration
			validatedData = {
				data: [],
				MissingFields: [],
				TypeMessage: "",
				imported: false
			};
		});

		// Check for issues
		hasIssues(DataArray);
		showDataTree = true;

		console.log("Validated data:", DataArray);
	}



	/* ============================================================
	   VALIDATION: CHECK REQUIRED FIELDS
	   ============================================================ */
	function checkMandatoryFields(data: Record<string, any>) {
		validatedData.MissingFields ??= [];

		for (const map of mapping.Mappings) {
			const source = map.Source?.trim();
			if (!source) continue;

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



	/* ============================================================
	   HELPERS: DEEP LOOKUP TO EXTRACT VALUES BY PATH
	   ============================================================ */
	function getNestedValue(obj: any, path: string): any {
		if (!obj || typeof path !== "string") return undefined;

		const parts = path.split(".");
		let cur = obj;

		for (const p of parts) {
			if (cur == null || typeof cur !== "object") return undefined;

			// Case-insensitive key match
			const key = Object.keys(cur)
				.find(k => k.toLowerCase() === p.toLowerCase());

			if (!key) return undefined;
			cur = cur[key];
		}

		return cur;
	}

	function findValueByKeyRecursive(obj: any, key: string): any {
		if (!obj || typeof obj !== "object") return undefined;

		if (Object.prototype.hasOwnProperty.call(obj, key)) {
			return obj[key];
		}

		for (const k of Object.keys(obj)) {
			const v = obj[k];
			if (typeof v === "object" && v !== null) {
				const found = findValueByKeyRecursive(v, key);
				if (found !== undefined) return found;
			}
		}

		return undefined;
	}

	function isEmptyValue(value: any): boolean {
		if (value == null) return true;

		if (typeof value === "string") return value.trim() === "";
		if (Array.isArray(value)) return value.length === 0 || value.every(isEmptyValue);

		if (typeof value === "object") {
			const keys = Object.keys(value);
			return keys.length === 0 || keys.every(k => isEmptyValue(value[k]));
		}

		return false;
	}



	/* ============================================================
	   VALIDATION: SPECIAL TYPE CHECKS (EX: RETRACTION)
	   ============================================================ */
	function validateTypes(data: any) {
		if (data["update-to"]?.[0]?.type === "retraction") {
			validatedData.TypeMessage =
				'This data has type "retraction". Do you really want to import?';
		}
	}



	/* ============================================================
	   FILTER DATA ACCORDING TO MAPPING.JSON
	   ============================================================ */
	function filterData(data: any, mapping: any) {
		const sources = mapping.Mappings
			.map(m => m.Source?.trim())
			.filter(s => s);

		const filtered: Record<string, any> = {};

		for (const key of sources) {
			const value = getNestedValue(data, key);
			if (value !== undefined) {
				filtered[key] = value;
			}
		}

		return filtered;
	}



	/* ============================================================
	   ISSUE DETECTION & GROUPING
	   ============================================================ */
	function hasIssues(dataArray: any[]) {
		dataArray.forEach(data => {
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

			// Missing fields
			data.MissingFields.forEach(field => {
				IssueRow = {
					Index: index + 1,
					errorType: "Error",
					msg: "Missing Fields: " + field
				};
				issueBlock.push({ ...IssueRow });
			});

			// Type warnings
			if (data.TypeMessage) {
				IssueRow = {
					Index: index + 1,
					errorType: "Warning",
					msg: "Type Message: " + data.TypeMessage
				};
				issueBlock.push({ ...IssueRow });
			}
		});

		console.log("Issue Information:", issueBlock);

		// Group issues by dataset index
		for (const issue of issueBlock) {
			let group = groupedIssues.find(g => g.Index === issue.Index);

			if (!group) {
				group = {
					Index: issue.Index,
					issues: [],
					checkboxValue: [{ key: issue.Index, value: "" }],
					selected: []
				};
				groupedIssues.push(group);
			}

			group.issues.push({
				errorType: issue.errorType,
				msg: issue.msg
			});
		}
	}



	/* ============================================================
	   CREATE DATASETS IN BEXIS
	   ============================================================ */
	async function createDatasets(Array: any) {
		console.log("Creating datasets…", Array);

		const fillDatasetArray: any[] = [];

		for (const data of Array) {
			const mapped = mapToApiFormat(data);

			// Create empty dataset in BEXIS
			const res = await apiCalls.createDataset(mapped);

			console.log("Dataset created:", res);

			// Store ID + JSON data for filling
			fillDatasetArray.push({
				id: res.id,
				data: data.data
			});
		}

		// Now fill metadata for each dataset
		fillDatasets(fillDatasetArray);
	}



	/* ============================================================
	   MAP VALIDATED DATA TO BEXIS API FORMAT
	   ============================================================ */
	function mapToApiFormat(data: any) {
		const dataset: datasetType = {
			Title: "",
			Description: "",
			DataStructureId: 0,
			MetadataStructureId: MetadataStructureId,
			EntityTemplateId: EntityTemplateId
		};

		dataset.Title = data.data.title?.toString() ?? "";
		dataset.Description = data.data.abstract?.toString() ?? "";

		return dataset;
	}



	/* ============================================================
	   FILL DATASET METADATA ACCORDING TO MAPPING.JSON
	   ============================================================ */
	async function fillDatasets(data: any[]) {

		for (const dataset of data) {

			// Fetch existing metadata (empty template)
			const getMetadata = await apiCalls.GetMetadata(dataset.id);
			let metadata = getMetadata;

			console.log("Original Metadata:", metadata);

			const json = dataset.data;

			// Apply mapping.json rules
			for (const map of mapping.Mappings) {
				const source = map.Source?.trim();
				const target = map.Target?.trim();

				if (!source) continue;

				const value =
					getNestedValue(json, source) ??
					json[source] ??
					undefined;

				if (value === undefined) continue;

				const normalized = normalizeValue(value, "string");
				setNestedValue(metadata, target, normalized);
			}

			console.log("Filled metadata:", metadata);

			// PUT metadata to server
			await apiCalls.putMetadata(dataset.id, metadata);
		}
	}



	/* ============================================================
	   UTILITY: SET VALUE INSIDE NESTED METADATA STRUCTURE
	   ============================================================ */
	function setNestedValue(obj: any, path: string, value: any) {
		path = path.replace(/^\$\./, "");
		const parts = path.split(".");
		let cur = obj;

		for (let i = 0; i < parts.length - 1; i++) {
			const part = parts[i];

			let key = Object.keys(cur)
				.find(k => k.toLowerCase() === part.toLowerCase());

			if (!key) {
				key = part;
				cur[key] = {};
			}

			cur = cur[key];
		}

		const lastKeyPart = parts[parts.length - 1];
		let lastKey =
			Object.keys(cur)
				.find(k => k.toLowerCase() === lastKeyPart.toLowerCase())
			?? lastKeyPart;

		const isArrayTarget = Array.isArray(cur[lastKey]);

		// Case 1: Target is array → create list of {#text} items
		if (isArrayTarget) {
			const values = Array.isArray(value) ? value : [value];

			cur[lastKey] = values.map(v => ({
				"@ref": "",
				"@partyid": "",
				"#text": typeof v === "object"
					? JSON.stringify(v)
					: String(v)
			}));

			return;
		}

		// Case 2: Source is array but target is single → try coercing
		if (Array.isArray(value)) {
			if (cur[lastKey] && typeof cur[lastKey] === "object" && cur[lastKey]["#text"] !== undefined) {
				cur[lastKey] = value.map(v => ({
					"@ref": "",
					"@partyid": "",
					"#text": typeof v === "object"
						? JSON.stringify(v)
						: String(v)
				}));
				return;
			}
		}

		// Case 3: Simple #text value
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



	/* ============================================================
	   UTILITY: NORMALIZE VALUE TYPES
	   ============================================================ */
	function normalizeValue(value: any, expectedType: string): any {
		if (value == null) return "";

		if (Array.isArray(value)) {
			if (value.every(v => typeof v === "object")) {
				return JSON.stringify(value);
			}
			return value.join(", ");
		}

		if (typeof value === "object") {
			return JSON.stringify(value);
		}

		if (typeof value === "boolean" || typeof value === "number") {
			return String(value);
		}

		return value.toString();
	}



	/* ============================================================
	   CONFIRM SELECTION — ONLY IMPORT SELECTED DATASETS
	   ============================================================ */
function confirmSelectedDatasets() {
	// Indices, die der Nutzer abgewählt hat → überspringen
	const removeIndices = groupedIssues
		.filter(g => g.selected.length === 0)
		.map(g => g.Index - 1);

	// Filtere DataArray: nur ausgewählte & noch nicht importierte
	const filteredDataArray = DataArray.filter((item, i) => {
		return !removeIndices.includes(i) && !item.imported;
	});

	if (!filteredDataArray.length) {
		console.warn("No new or selected datasets to import.");
		return;
	}

	// markiere alle als importiert
	filteredDataArray.forEach(item => {
		item.imported = true;
	});

	// entferne importierte Datensätze aus groupedIssues
	groupedIssues = groupedIssues.filter(g => {
		// Index anpassen: group.Index - 1 ist der DataArray-Index
		const dataIndex = g.Index - 1;
		return !filteredDataArray.some((_, i) => i === dataIndex);
	});

	// optional: Erfolgsmeldung
	showMessage(`${filteredDataArray.length} Datensatz${filteredDataArray.length > 1 ? 'e' : ''} importiert.`);

	// Importvorgang
	createDatasets(filteredDataArray);
}




	/* ============================================================
	   TOGGLE SELECT-ALL BEHAVIOR
	   ============================================================ */
	function selectAllBoxes(selectAll: any[]) {

		if (selectAll.length === 1 && selectAll[0] === -1) {
			// Select everything
			groupedIssues = groupedIssues.map(group => ({
				...group,
				selected: [group.Index]
			}));
		}
		else {
			// Deselect everything
			groupedIssues = groupedIssues.map(group => ({
				...group,
				selected: []
			}));
		}
	}



	/* ============================================================
	   NAVIGATION FUNCTIONS
	   ============================================================ */
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
		currentIndex = index - 1;
	}



	/* ============================================================
	   IMPORT ONLY ONE DATASET ("CHECK SINGLE")
	   ============================================================ */
	function checkSingle() {
		const nextIndex = currentIndex;

		// remove its grouped issue entry
		const hasNext = groupedIssues.some(g => g.Index === nextIndex + 1);

		if (hasNext) {
			groupedIssues = groupedIssues.filter(g => g.Index !== nextIndex + 1);
		}

		// mark as imported
		if (!DataArray[currentIndex]) {
			console.warn("No entry found for import.");
			return;
		}

		DataArray[currentIndex].imported = true;

		// create a filtered array with only the selected dataset
		const newArray = [...DataArray];
		const filtered = newArray.filter((_, i) => i === currentIndex);

		createDatasets(filtered);
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
		<div id="fileLabel" class="w-32">File  (.json):</div>
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

		<div class="flex gap-5 w-full mt-4">
		<div id="fileLabel" class="w-96">DOI List  (.txt, comma seperated):</div>
		<div id="fileName" class="overflow-clip w-full">{filename}</div>
		<div id="fileButton" class="w-16">
			<FileButton
				id="uploadTXT"
				title="Upload TXT"
				button="btn variant-filled-primary h-9 w-16 shadow-md"
				name="uploadTXT"
				accept=".txt"
				on:change={handleTxtUpload}
			>
				<Fa icon={faArrowUpFromBracket} />
			</FileButton>
		</div>
	</div>


	<!--  Manuelle DOI-Eingabe -->
	<div class="flex gap-5 w-full mt-4">
		<div id="manualDOILabel" class="w-36 mt-3">Manual DOI :</div>
		<div class="over-clip w-full">
			<!-- <TextInput id="name" required={false} bind:value={manualDOI} /> -->
			<TextArea
			id="name"
			required={false}
			placeholder="Enter one or more DOIs, separated by commas or spaces"
			bind:value={manualDOI}
		/>
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
			{#if DataArray[currentIndex].imported === true}
				<div class="w-96">
					<div class="text-center card variant-ghost-primary w-full flex gap-5 p-2 my-1">
						<div class="w-full align-middle">You have already imported this Dataset</div>
					</div>
				</div>
				{:else if DataArray[currentIndex].TypeMessage !== '' || DataArray[currentIndex].MissingFields.length > 0  }
				<div class="w-96">
					<div class="text-center card variant-ghost-error w-full flex gap-5 p-2 my-1">
						<div class="w-full align-middle">This Dataset has issues</div>
					</div>
				</div>
			{/if}

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
				{#if DataArray.length > 1}
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
				{/if}
				<div class="text-right w-full">
					{#if EntityTemplateId}
						{#if DataArray[currentIndex].imported === false}
							<button class="btn variant-filled-primary shadow-md" on:click={checkSingle}>
								Confirm
							</button>
						{:else}
						<button class="btn variant-filled-primary shadow-md disabled" disabled>
							Confirm
						</button>
						{/if}
						{#if DataArray.length > 1}
							<button class="btn variant-filled-primary shadow-md" on:click={confirmSelectedDatasets}>
								Confirm selected
							</button>
						{/if}
					{:else}
						<button class="btn variant-filled-primary shadow-md disabled" disabled>
							Confirm
						</button>
						{#if DataArray.length > 1}
							<button class="btn variant-filled-primary shadow-md disabled" disabled>
								Confirm all
							</button>
						{/if}
					{/if}
				</div>
				
			</div>
		</div>
	{/if}
</Page>
