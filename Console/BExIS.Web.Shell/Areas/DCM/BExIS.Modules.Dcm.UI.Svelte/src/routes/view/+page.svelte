<script lang="ts">

	import { getApiDataset, getView } from './services';
	import { ErrorMessage, type linkType, Page, pageContentLayoutType, positionType, setApiConfig, Spinner } from '@bexis2/bexis2-core-ui';

	import Header from './Header.svelte';


	//types
	import type { ViewModel, Hook, ApiDatasetModel } from '../../models/View';
	import { fade } from 'svelte/transition';
	import Hooks from './Hooks.svelte';
	import Links from './Links.svelte';

	let title = '';

	let container;
	let id: number;
	let version: number = 0;
	let model: ViewModel;




	let hookList: Hook[];
	$: hooks = hookList;

 const links: linkType[] = [
		{
			label: 'Manual',
			url: '/home/docs/Datasets#dataset-view-page'
		}
	];

	async function load () {
		// get data from parent
		container = document.getElementById('view');
		id = container?.getAttribute('dataset');
		version = container?.getAttribute('version');

		console.log('start view', id, version);
		//setup api
		// setApiConfig('https://localhost:44345', 'davidschoene', '123456');

		// load data from server
		model = await getView(id);

		hooks = model.settings.hooks;
		title = model.title;
		version = model.version;
		id = model.id;

		console.log('model',model);
		console.log('hooks', hooks);


	}

</script>
<Page title="Edit: ({id} | {title})" contentLayoutType={pageContentLayoutType.center} {links}>

<div in:fade={{ delay: 500 }} out:fade={{ delay: 500 }}>

	{#await load()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading entity templates" />
			</div>
		{:then result}

		<Header	{id} {version} {title} labels = {model.labels}/>

		<div class="flex-col mb-2">
						<div class="font-bold mr-2">Author : {model.additionalInformations['author'] ? model.additionalInformations['author'] : 'n/a'} </div>
						<div class="font-bold mr-2">License : {model.additionalInformations['license'] ? model.additionalInformations['license'] : 'n/a'} </div>
			</div>


		<div class="flex">
				<div class="flex-grow card	mb-5 p-5">
						{model.description}
				</div>
				<div class="ml-5 card	mb-5 p-5 w-auto">
						test
				</div>
		</div>

<div class="flex-col w-1/2	mb-5 p-5 card">
	<div class="h3 mb-5">Additional Information Overview</div>
			
		<div class="flex-col mb-2">
			{#if model.additionalInformations}
				{#each  Object.entries(model.additionalInformations)	as info}
								<div class="font-bold mr-2">{info[0]}:{info[1]}</div>
				{/each}
				{/if}
		</div>
		</div>

		<Links	links={model.links.to} />

	 <Hooks	{id} {version} hooks={hooks} />
		

		{:catch error}
			<ErrorMessage {error} />
		{/await}

</div>

</Page>

