<script lang="ts">
	import { Api, Spinner } from '@bexis2/bexis2-core-ui';
	import { onMount } from 'svelte';
	import DiffNode from './DiffNode.svelte';

	export let datasetId: number | null = null;

	// TODO: Fetch available datasets from API if needed
	let datasets = datasetId ? [{ id: datasetId, name: 'Current Dataset' }] : [];

	async function fetchDataset() {
		try {
			let response = await Api.get('/api/dataset/' + datasetId);
			return { maxVersion: response.data.version };
		} catch (error) {
			return { error };
		}
	}

	async function fetchMetadataVersion(versionNumber: number) {
		try {
			let response = await Api.get(
				`/api/metadata/${datasetId}/version_number/${versionNumber}?simplifiedJson=1`
			);
			return response.data;
		} catch (error) {
			return { error };
		}
	}

	let datasetResponse: { maxVersion?: number; error?: any } = {};

	onMount(async () => {
		datasetResponse = await fetchDataset();
	});

	$: versions = Array.from({ length: datasetResponse.maxVersion ?? 0 }, (_, i) => i + 1);

	let selectedDataset1: { id: number; name: string } | null = null;
	let selectedVersion1: number | null = null;
	let metadata1: any = null;
	let loading1 = false;

	let selectedDataset2: { id: number; name: string } | null = null;
	let selectedVersion2: number | null = null;
	let metadata2: any = null;
	let loading2 = false;

	// TODO: Fetch datasets from API if multiple datasets are to be compared
	$: selectedDataset1 =
		datasets.length > 0 && selectedDataset1 === null ? datasets[0] : selectedDataset1;
	$: selectedDataset2 = selectedDataset1;

	$: selectedVersion1 =
		versions.length > 0 && selectedVersion1 === null ? (versions.at(-2) ?? null) : selectedVersion1;
	$: selectedVersion2 =
		versions.length > 0 && selectedVersion2 === null ? (versions.at(-1) ?? null) : selectedVersion2;

	$: if (selectedVersion1 !== null) {
		metadata1 = null;
		loading1 = true;
		fetchMetadataVersion(selectedVersion1).then((data) => {
			metadata1 = data;
			loading1 = false;
		});
	}

	$: if (selectedVersion2 !== null) {
		metadata2 = null;
		loading2 = true;
		fetchMetadataVersion(selectedVersion2).then((data) => {
			metadata2 = data;
			loading2 = false;
		});
	}
</script>

{#if datasetResponse.error}
	<p class="text-red-600">Error loading dataset: {datasetResponse.error.message}</p>
{:else if datasetResponse.maxVersion !== undefined}
	<div class="mx-4 mb-4 flex justify-around gap-x-8 gap-y-4">
		<div class="flex flex-wrap gap-2">
			<label>
				<span>Dataset 1:</span>
				<select
					class="select min-w-40"
					id="dataset1"
					bind:value={selectedDataset1}
					disabled={datasets.length <= 1}
				>
					{#each datasets as dataset}
						<option value={dataset}>
							{dataset.name}
						</option>
					{/each}
				</select>
			</label>
			<label>
				<span>Version 1:</span>
				<select class="select min-w-40" id="version1" bind:value={selectedVersion1}>
					{#each versions as version}
						<option value={version}>
							Version {version}{version === datasetResponse.maxVersion ? ' (Latest)' : ''}
						</option>
					{/each}
				</select>
			</label>
		</div>

		<div class="flex flex-wrap gap-2">
			<label>
				<span>Dataset 2:</span>
				<select
					class="select min-w-40"
					id="dataset2"
					bind:value={selectedDataset2}
					disabled={datasets.length <= 1}
				>
					{#each datasets as dataset}
						<option value={dataset}>
							{dataset.name}
						</option>
					{/each}
				</select>
			</label>
			<label>
				<span>Version 2:</span>
				<select class="select min-w-40" id="version2" bind:value={selectedVersion2}>
					{#each versions as version}
						<option value={version}>
							Version {version}{version === datasetResponse.maxVersion ? ' (Latest)' : ''}
						</option>
					{/each}
				</select>
			</label>
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
