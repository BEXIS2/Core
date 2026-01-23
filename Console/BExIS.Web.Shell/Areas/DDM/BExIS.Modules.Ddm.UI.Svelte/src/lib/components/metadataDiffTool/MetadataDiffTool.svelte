<script lang="ts">
	import { Api, Spinner, DropdownKVP, MultiSelect } from '@bexis2/bexis2-core-ui';
	import { onMount } from 'svelte';
	import DiffNode from './DiffNode.svelte';
	import { SlideToggle } from '@skeletonlabs/skeleton';

	export let datasetIdStart: number | null = null;
	let datasetId: number | null = null;
	// TODO: Fetch available datasets from API if needed
	let datasets = []; // datasetId ? [{ id: datasetId, name: 'Current Dataset' }] : [];

	let datasetResponse1: { maxVersion?: number; error?: any } = {};
	let datasetResponse2: { maxVersion?: number; error?: any } = {};

	onMount(async () => {
		// read id from URL if not provided (id is provided in route)
		const urlSearchParams = new URLSearchParams(window.location.search);
		const urlDatasetId = urlSearchParams.get('id');
		if (urlDatasetId) {
			datasetIdStart = parseInt(urlDatasetId);
			console.log('Dataset ID from URL:', datasetIdStart);
		}

		datasets = datasets.length > 0 ? datasets : await fetchAllDatasets();
		console.log(datasets);
		if (datasetIdStart !== null) {
			datasetId = datasetIdStart;
			selectedDataset1 =
				datasets.find((ds: { id: number; text: string }) => ds.id === datasetId) || null;
			selectedDataset2 =
				datasets.find((ds: { id: number; text: string }) => ds.id === datasetId) || null;
		} else {
			selectedDataset1 = datasets.length > 0 ? datasets[0] : null;
			selectedDataset2 = datasets.length > 0 ? datasets[0] : null;
		}

		console.log('Selected Datasets:', selectedDataset1, selectedDataset2);
		onChangeSelectedDataset1(new Event('init'));
		onChangeSelectedDataset2(new Event('init'));
	});

	async function fetchAllDatasets() {
		try {
			let response = await Api.get('/api/dataset');

			return response.data
				.map((ds: any) => ({ id: ds.Id, text: ds.Id + ' ' + ds.Title }))
				.sort((a: any, b: any) => b.id - a.id);
		} catch (error) {
			return { error };
		}
	}

	async function fetchDataset(id: number) {
		try {
			let response = await Api.get('/api/dataset/' + id);
			console.log('Fetched dataset info:', response.data);
			return { maxVersion: response.data.version };
		} catch (error) {
			return { error };
		}
	}

	async function fetchMetadataVersion(versionNumber: number, id: number) {
		try {
			let response = await Api.get(
				`/api/metadata/${id}/version_number/${versionNumber}?simplifiedJson=1`
			);
			return response.data;
		} catch (error) {
			return { error };
		}
	}

	onMount(async () => {});

	let versions1: number[] = [];
	let versions2: number[] = [];

	let selectedDataset1: { id: number; text: string } | null = null;
	let selectedVersion1: number | null = null;
	let metadata1: any = null;
	let loading1 = false;

	let selectedDataset2: { id: number; text: string } | null = null;
	let selectedVersion2: number | null = null;
	let metadata2: any = null;
	let loading2 = false;

	// TODO: Fetch datasets from API if multiple datasets are to be compared

	// update datasetId when selection changes and refetch versions
	function onChangeSelectedDataset1(event: Event) {
		selectedVersion1 = null;

		fetchDataset(selectedDataset1.id).then((response) => {
			datasetResponse1 = response;
			console.log('Dataset Response 1:', datasetResponse1);
			versions1 = Array.from({ length: datasetResponse1.maxVersion ?? 0 }, (_, i) => i + 1);
			selectedVersion1 = datasetResponse1.maxVersion ?? null;
			onChangeSelectedVersion1(new Event('init'));
		});
	}

	function onChangeSelectedDataset2(event: Event) {
		selectedVersion2 = null;

		fetchDataset(selectedDataset2.id).then((response) => {
			datasetResponse2 = response;
			versions2 = Array.from({ length: datasetResponse2.maxVersion ?? 0 }, (_, i) => i + 1);
			selectedVersion2 = datasetResponse2.maxVersion ?? null;
			onChangeSelectedVersion2(new Event('init'));
		});
	}

	// $: selectedDataset2 = selectedDataset1;

	$: selectedVersion1 = null;
	$: selectedVersion2 = null;

	function onChangeSelectedVersion1(event: Event) {
		if (syncSelections && selectedDataset1 != selectedDataset2) {
			selectedDataset2 = selectedDataset1;
			onChangeSelectedDataset2(new Event('init'));
		}
		metadata1 = null;
		loading1 = true;
		if (selectedVersion1 === null) {
			return;
		}
		console.log('Fetching metadata for version 1:', selectedVersion1);
		fetchMetadataVersion(selectedVersion1, selectedDataset1.id).then((data) => {
			metadata1 = data;
			loading1 = false;
		});
	}

	function onChangeSelectedVersion2(event: Event) {
			if (syncSelections && selectedDataset1 != selectedDataset2) {
			selectedDataset1 = selectedDataset2;
			onChangeSelectedDataset1(new Event('init'));
		}
		metadata2 = null;
		loading2 = true;
		if (selectedVersion2 === null) {
			return;
		}
		console.log('Fetching metadata for version 2:', selectedVersion2);
		fetchMetadataVersion(selectedVersion2, selectedDataset2.id).then((data) => {
			metadata2 = data;
			loading2 = false;
		});
	}

	let syncSelections: boolean = true;
</script>


<h2 class="m-4 text-2xl font-bold">Metadata Diff Tool</h2>
<p class="mx-4 mb-2 text-sm ">Select datasets and versions to compare their metadata.</p>

{#if datasetResponse1.error || datasetResponse2.error}
	<p class="text-red-600">Error loading dataset: {datasetResponse1.error.message}</p>
{:else if datasetResponse1.maxVersion !== undefined}
	<!-- toggle to sync selections -->
<div class="mx-4 mb-2 text-sm ">
		
	<SlideToggle
		active="bg-secondary-500"
		size="sm"
		id="to_index"
		name={"syncSelections"}
		bind:checked={syncSelections}
		on:change={!syncSelections
			? () => {}
			: () => {
					selectedDataset2 = selectedDataset1;
					onChangeSelectedDataset2(new Event('init'));
			  }}
		>Sync selection</SlideToggle
	>
</div>
	<div class="mx-4 mb-4 flex justify-around gap-x-8 gap-y-4">
		<div class="flex flex-wrap gap-2 w-1/2">
			<div class="w-full mb-2">
				<MultiSelect
					id="dataset1"
					title="Select Dataset 1"
					bind:source={datasets}
					itemId="id"
					itemLabel="text"
					itemGroup="group"
					complexSource={true}
					complexTarget={true}
					bind:target={selectedDataset1}
					isMulti={false}
					placeholder="-- Please select --"
					clearable={false}
					on:change={onChangeSelectedDataset1}
				/>
			</div>
			<div class="w-full font-bold">{selectedDataset1 ? selectedDataset1.text : 'None'}</div>
			<div class="w-full">
				<label>
					<span>Version 1:</span>
					<select
						class="select min-w-40"
						id="version1"
						bind:value={selectedVersion1}
						on:change={onChangeSelectedVersion1}
					>
						{#each versions1 as version}
							<option value={version}>
								Version {version}{version === datasetResponse1.maxVersion ? ' (Latest)' : ''}
							</option>
						{/each}
					</select>
				</label>
			</div>
		</div>
		<div class="flex flex-wrap gap-2 w-1/2">
			<div class="w-full mb-2">
				<MultiSelect
					id="dataset2"
					title="Select Dataset 2"
					bind:source={datasets}
					itemId="id"
					itemLabel="text"
					itemGroup="group"
					complexSource={true}
					complexTarget={true}
					bind:target={selectedDataset2}
					isMulti={false}
					placeholder="-- Please select --"
					clearable={false}
					on:change={onChangeSelectedDataset2}
				/>
			</div>
			<div class="w-full font-bold">{selectedDataset2 ? selectedDataset2.text : 'None'}</div>
			<div class="w-full">
				<label>
					<span>Version 2:</span>
					<select
						class="select min-w-40"
						id="version2"
						bind:value={selectedVersion2}
						on:change={onChangeSelectedVersion2}
					>
						{#each versions2 as version}
							<option value={version}>
								Version {version}{version === datasetResponse2.maxVersion ? ' (Latest)' : ''}
							</option>
						{/each}
					</select>
				</label>
			</div>
		</div>
	</div>
	{#if selectedVersion1 && selectedVersion2}
		{#if loading1 || loading2}
			<div class="wrap mx-4 flex gap-4">
				<div class="m-4 flex grow flex-col items-center gap-2 text-surface-800">
					{#if loading1}
						<Spinner />
						<p>Loading metadata 1...</p>
					{/if}
				</div>
				<div class="m-4 flex grow flex-col items-center gap-2 text-surface-800">
					{#if loading2}
						<Spinner />
						<p>Loading metadata 2...</p>
					{/if}
				</div>
			</div>
		{:else if metadata1 && metadata2}
			{#key `${selectedVersion1}\n\n---\n\n${selectedVersion2}`}
				<DiffNode value1={metadata1} value2={metadata2} />
			{/key}
		{/if}
	{/if}
{:else}
	<div class="m-4 flex flex-col items-center gap-2 text-surface-800">
		<Spinner />
		<p>Loading dataset information...</p>
	</div>
{/if}
