<script lang="ts">
	import { Page, DropdownKVP, TextInput }  from '@bexis2/bexis2-core-ui';
	import { FileButton } from '@skeletonlabs/skeleton';
	import { onMount } from 'svelte';
	import Fa from 'svelte-fa';
	import { faArrowUpFromBracket } from '@fortawesome/free-solid-svg-icons';

	import mapping from './mapping.json';
	import type { validatedData, datasetType } from './models';
	import * as apiCalls from './services/apiCalls';

	let filename: string = '';
	let manualDOI: string = ''; // ✅ neu

	let validatedData: validatedData = {
		data: [],
		MissingFields: [],
		TypeMessage: "",
	};
	let DataArray: any[] = [];

	let target: number;
	let entities: any[] = [];
	let transformedArray: any[] = [];

	let EntityTemplateId: number;
	let MetadataStructureId: number;

	onMount(async () => {
		entities = await apiCalls.getEntityTemplateList();
		transformedArray = entities.map((entity) => ({
			id: entity.id,
			text: entity.name
		}));
	});

	function onChangeHandler(event: any) {
		let selectedEntity: number;
		selectedEntity = event.target.value;
		let searchEntity = entities.find((entity) => entity.id == selectedEntity);
		EntityTemplateId = selectedEntity;
		MetadataStructureId = searchEntity.metadataStructure.id;
		console.log("Selected Entity Template ID:", EntityTemplateId);
		console.log("Selected Metadata Structure ID:", MetadataStructureId);
	}

	async function handleFileUpload(event: Event) {
		const input = event.target as HTMLInputElement;
		if (input.files && input.files.length > 0) {
			filename = input.files[0].name;
			const file = input.files[0];
			const reader = new FileReader();

			reader.onload = async (e) => {
				try {
					const json = JSON.parse(e.target?.result as string);
					console.log('Uploaded JSON:', json);
					await grabDOI(json);
				} catch (err) {
					console.error('Invalid JSON file');
				}
			};

			reader.readAsText(file);
		}
	}

	// ✅ neue Funktion – DOI manuell eingeben und abfragen
	async function handleManualDOI() {
		if (!manualDOI || manualDOI.trim() === "") {
			alert("Please enter a DOI.");
			return;
		}

		console.log("Manual DOI entered:", manualDOI);
		await grabMetadata([manualDOI.trim()]);
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
				Metadata.push(res?.message);
				console.log(`Metadata for DOI ${doi}:`, res?.message);
			} catch (e) {
				console.error(`Failed to fetch metadata for DOI ${doi}:`, e);
			}
		}
		validate(Metadata);
	}

	function validate(Metadata: any[]) {
		console.log("Starting validation for metadata:", Metadata);
		Metadata.forEach((data) => {
			console.log("Validating data:", data);
			validatedData.data = data;
			checkMandatoryFields(data);
			validateTypes(data);

			DataArray.push({ ...validatedData }); // ✅ Kopie pushen
			validatedData = { data: [], MissingFields: [], TypeMessage: "" };
		});
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
</script>

<Page>
	<h1 class="h1">Import Publications</h1>

	<!-- Dropdown -->
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

	<!-- ✅ Manuelle DOI-Eingabe -->
	<div class="flex gap-3 w-full mt-6 items-center">
        <div class="pb-10">
	        <TextInput id="name" label="Name" required={false} bind:value={manualDOI} />
        </div>
		<!-- <input
			type="text"
			placeholder="Enter DOI manually..."
			bind:value={manualDOI}
			class="input input-bordered w-full"
		/> -->
		<button
			class="btn variant-filled-primary h-9 shadow-md"
			on:click={handleManualDOI}
		>
			Fetch DOI
		</button>
	</div>

	<!-- Dataset-Erstellung -->
	<div class="mt-6">
		<button class="btn variant-filled-success" on:click={createDatasets}>
			Create
		</button>
	</div>
</Page>
