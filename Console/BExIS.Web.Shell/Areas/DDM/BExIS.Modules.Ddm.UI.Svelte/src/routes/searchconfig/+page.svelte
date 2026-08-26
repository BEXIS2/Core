<script lang="ts">
	import SearchConfig from '$lib/components/SearchConfig/SearchConfig.svelte';
	import { type linkType, Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faTrash, faPen } from '@fortawesome/free-solid-svg-icons';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { ErrorMessage, positionType, TextInput } from '@bexis2/bexis2-core-ui';
	import {
		getEntityTemplate,
		getEntityTemplateList,
		getMeanings,
		LoadConfig,
		SaveConfig
	} from '$lib/services/searchConfigServices';
	import { onMount } from 'svelte';
	import type {
		CalcBlockListItem,
		SearchConfigSchema,
		MeaningModel,
	} from '$lib/components/SearchConfig/SearchConfigModel.ts';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { faSave, faXmark } from '@fortawesome/free-solid-svg-icons/index';
	import TableOption from '$lib/components/SearchConfig/tableOptions.svelte';
	import TableElements from '$lib/components/SearchConfig/tableElements.svelte';
	import { writable, type Writable } from 'svelte/store';

	// load example data
	import exampleData from '$lib/components/SearchConfig/SearchConfigExample.json';
	

	let entitytemplates: any[] = [];
	let meanings: MeaningModel[] = [];
	// use temporary example data -> later replace with API call
	// initialize once so child components can mutate it via binding
	let searchConfigData: SearchConfigSchema = JSON.parse(
		JSON.stringify(exampleData)
	) as SearchConfigSchema;

	async function load() {
		searchConfigData = await LoadConfig();
		entitytemplates = await getEntityTemplateList();
		meanings = await getMeanings();
		// dataTypes = await getDataTypes();
	}

	const controller = 'search';

	const links: linkType[] = [
		{ label: 'Search Configuration', url: '/ddm/searchconfig' },
		{ label: 'Manual', url: '/home/docs/Search%20and%20Download%20Data/' }
	];
	// validation
	import suite from './searchConfigValidation';

	// validation
	let res = suite.get();

	// flag to enable submit button
	$: disabled = !res.isValid();

	onMount(async () => {
		await load();

		suite.reset();

		setTimeout(async () => {
			res = suite(searchConfigData, undefined);
		}, 100);
	});

	async function submit() {
		console.log('submit', searchConfigData);

		await SaveConfig(searchConfigData);
		// download JSON file
		const dataStr =
			'data:text/json;charset=utf-8,' + encodeURIComponent(JSON.stringify(searchConfigData));
		const downloadAnchorNode = document.createElement('a');
		downloadAnchorNode.setAttribute('href', dataStr);
		downloadAnchorNode.setAttribute('download', 'search_config.json');
		document.body.appendChild(downloadAnchorNode); // required for firefox
		downloadAnchorNode.click();
		downloadAnchorNode.remove();
	}

	function onCancel() {
		// reset to example data -> replace with original data later
		searchConfigData = JSON.parse(JSON.stringify(exampleData)) as SearchConfigSchema;
	}
	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		console.log('input changed', e);
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite(searchConfigData, e.target.id);
		}, 10);

		console.log('res after change', res);
	}
</script>

<Page
	title="Search Configuration"
	note="Configuration of global and local search settings."
	{links}
>
	<svelte:fragment>
		{#await load()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading entity templates" />
			</div>
		{:then result}
			{#if entitytemplates && meanings && searchConfigData}
				<h2 class="text-2xl font-semibold mb-4">Search Configuration</h2>
				{#if !res.isValid()}
					<div class="text-red-600 mb-4">
						There are validation errors in the form. Please check your input.
					</div>
				{/if}
				<form on:submit|preventDefault={submit}>
					<div class="grow text-right gap-2">
						<button
							title="cancel"
							type="button"
							class="btn variant-filled-warning"
							on:click={onCancel}><Fa icon={faXmark} /></button
						>
						<button title="save" type="submit" class="btn variant-filled-primary" {disabled}
							><Fa icon={faSave} /></button
						>
					</div>

					<SearchConfig
						bind:searchConfigData
						{entitytemplates}
						{meanings}
						{onChangeHandler}
						{res}
					/>
				</form>
			{:else}
				<div class="text-surface-800">
					<Spinner position={positionType.center} label="loading entity templates" />
				</div>
			{/if}
		{:catch error}
			<ErrorMessage {error} />
		{/await}
	</svelte:fragment>
</Page>
