<script lang="ts">
	import { onMount } from 'svelte';

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
	let datamodel: ApiDatasetModel



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
		datamodel = await getApiDataset(id, version);
		
		console.log('onmount', model);

		hooks = model.hooks;
		title = model.title;
		version = model.version;
		id = model.id;

		console.log('model',model);
		console.log('hooks', hooks);
		console.log('datamodel', datamodel);

		//test ui as html
		// const resTestPage = await fetch('dcm/view/test');
		// testPage = await resTestPage.text();
	};

</script>
<Page title="Edit: ({id} | {title})" contentLayoutType={pageContentLayoutType.center} {links}>

<div in:fade={{ delay: 500 }} out:fade={{ delay: 500 }}>

	{#await load()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading entity templates" />
			</div>
		{:then result}

		<Header	{id} {version} {title} labels = {model.labels}/>

		<div class="flex">
				<div class="flex-grow card	mb-5 p-5">
						{datamodel.description}
				</div>
				<div class="ml-5 card	mb-5 p-5 w-auto">
						test
				</div>
		</div>

		<Links	links={datamodel.links.to} />

	 <Hooks	{id} {version} hooks={hooks} />
		

		{:catch error}
			<ErrorMessage {error} />
		{/await}

</div>

</Page>

