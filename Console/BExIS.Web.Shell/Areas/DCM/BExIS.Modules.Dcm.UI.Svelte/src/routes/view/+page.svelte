<script lang="ts">

	import { getApiDataset, getView } from './services';
	import { ErrorMessage, type linkType, Page, pageContentLayoutType, positionType, setApiConfig, Spinner } from '@bexis2/bexis2-core-ui';

	import Header from './Header.svelte';


	//types
	import type { ViewModel, Hook, ApiDatasetModel } from '../../models/View';
	import { fade } from 'svelte/transition';
	import Hooks from './Hooks.svelte';
	import Links from './Links.svelte';

	import Versions from './version/Versions.svelte';
	import Keywords from './Keywords.svelte';
	import Funding from './Funding.svelte';
	import Tags from './version/Tags.svelte';
	import Download from './Download.svelte';


	let title = '';

	let container;
	let id: number;
	let version: number = 0;
	let model: ViewModel;

	let isPartOfCollection: boolean = false;



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

		// check if dataset is part of a collection
		isPartOfCollectionFunc();

	}

	function isPartOfCollectionFunc() {
		if(model.links.to.filter(link => link.referenceType === 'Collection').length > 0){
			isPartOfCollection = true;
		}
	}


</script>
<Page title="Edit: ({id} | {title})" contentLayoutType={pageContentLayoutType.center} {links}>

<div class="flex flex-col gap-2" in:fade={{ delay: 500 }} out:fade={{ delay: 500 }}>

	{#await load()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading entity templates" />
			</div>
		{:then result}
		
		<Header	{id} {version} {title} labels = {model.labels} license = {model.additionalInformations['license']} {isPartOfCollection} hasEditRight={model.hasEditRight}/>

		<div class="flex">
				<div class="flex-grow card	p-5 w-3/4">
						{model.description}
				</div>
				<div class="flex flex-col ml-5 gap-3 w-1/4">
						<Download {id} {version}
						 versionId={model.versionId}
							downloadAccess= {model.downloadAccess}
							hasDatastructure= {model.dataStructureId	!== undefined && model.dataStructureId > 0}
							hasData= {model.hasData}
							isPublic= {model.isPublic}
							data_aggrement = {model.settings.dataAggrement}
							total = {model.count}
						/> 
						{#if model.settings.useTags}
						 <Tags  {id} {version}  tag={model.tag}/>
							{:else}
							<Versions	{id} {version} />	
						{/if}

						<Funding f={model.additionalInformations['funder']}  />
						<Keywords k={model.additionalInformations['keyword']} />
				</div>
		</div>


		<Links	links={model.links.to} />

	 <Hooks	{id} {version} hooks={hooks} />
	
		{:catch error}
			<ErrorMessage {error} />
		{/await}

</div>

</Page>

