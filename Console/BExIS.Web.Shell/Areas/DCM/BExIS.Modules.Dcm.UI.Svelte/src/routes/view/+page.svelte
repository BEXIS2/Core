<script lang="ts">

	import { getView } from './services';
	import { ErrorMessage, type linkType, Page, pageContentLayoutType, positionType,  Spinner } from '@bexis2/bexis2-core-ui';

	import Header from './Header.svelte';


	//types
	import type { ViewModel, Hook, ApiDatasetModel } from '../../models/View';
	import { fade } from 'svelte/transition';
	import Hooks from './Hooks.svelte';


	import Versions from './version/Versions.svelte';
	import Keywords from './Keywords.svelte';
	import Funding from './Funding.svelte';
	import Tags from './version/Tags.svelte';
	import Download from './download/Download.svelte';
	import Forbidden from './error/Forbidden.svelte';
	import Deleted from './error/Deleted.svelte';
	import InProcess from './error/InProcess.svelte';
	import NotExist from './error/NotExist.svelte';
	import InternalServer from './error/InternalServer.svelte';
	import CitationDownload from './citation/CitationDownload.svelte';
	import type { HookModel } from '$models/Hook';
	import Metadata from '$lib/hooks/view/Metadata.svelte';
	import DataDescription from '$lib/hooks/view/DataDescription.svelte';
	import Data from '$lib/hooks/view/Data.svelte';
	import Link from '$lib/hooks/view/Link.svelte';
	import Back from '$lib/components/utils/Back.svelte';
	import Attachment from '$lib/hooks/view/Attachment.svelte';

	let title = '';

	let container;
	let id: number;
	let version: number = 0;
	let tag: number = 0;
	let model: ViewModel;

	let isPartOfCollection: boolean = false;

	let metadataHook;
	let dataDescriptionHook;
	let linkHook;
	let dataHook;
	let attachmentsHook;
	let entityName;

	let useTags: boolean = false;

	let addtionalhooks: HookModel[];
	$: addtionalhooks = [];

	let hooks: Hook[];


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
		tag = container?.getAttribute('tag');

		console.log('start view', id, version, tag);
		//setup api
		// setApiConfig('https://localhost:44345', 'davidschoene', '123456');

		// load data from server
		const res = await getView(id,	version, tag);

		if(res?.status==200)
		{
			model = res?.data;
			hooks = model.settings.hooks;
			title = model.title;
			version = model.version;
			id = model.id;
			tag = model.tag;
			entityName = model.entityName;

			console.log('model',model);
			console.log('hooks', hooks);

			// check if dataset is part of a collection
			isPartOfCollectionFunc();

			if(model.settings.hooks	&& model.settings.hooks.length > 0)
			{
				 seperateHooks(model.settings.hooks);
				
			}

		}
		

	}

	function isPartOfCollectionFunc() {
		if(model.links.to.filter(link => link.referenceType === 'Collection').length > 0){
			isPartOfCollection = true;
		}
	}

 // seperate dcm hooks from other hooks
	// known hooks - metadata, data, datadescription
	function seperateHooks(hooks: HookModel[]) {
		addtionalhooks = [];

		hooks.forEach((element) => {
			if (element.name == 'metadata')
				{
					metadataHook = element;
				}
				else	if (element.name == 'datadescription') {
					dataDescriptionHook = element;
				}
				else if (element.name == 'data') {
					dataHook = element;
				}
				else if (element.name == 'link') {
					linkHook = element;
				}
				else if (element.name == 'attachments') {
					attachmentsHook = element;
				}
				else {
					addtionalhooks.push(element);
				}
				
		});
	}


</script>
<Page title="Edit: ({id} | {title})" contentLayoutType={pageContentLayoutType.center} {links}>

<div class="flex flex-col gap-2" in:fade={{ delay: 500 }} out:fade={{ delay: 500 }}>

	{#await load()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading" />
			</div>
		{:then result}

	 <Header	
			{id} 
			{version} 
			{tag}
			{title} 
			{entityName}
			labels = {model.labels} 
			license = {model.additionalInformations['license']} 
			{isPartOfCollection} 
			hasEditRight={model.hasEditRight}
			isPublic={model.isPublic}
			publicationDate={model.publicationDate}

			/>

		<div class="flex">

				{#if metadataHook}
						<div class="flex-grow flex flex-col gap-3">
										<Metadata {id} {version} {tag} hook={metadataHook} description={model.description} />
						</div>
					{/if}

					{#if entityName?.toLowerCase()!='extension'}

						<div class="flex flex-col ml-5 gap-3 w-1/4">

								<CitationDownload	{id} {version} {tag} {useTags} />

								<Download {id} {version}
									versionId={model.versionId}
									downloadAccess= {model.downloadAccess}
									hasDatastructure= {model.dataStructureId	!== undefined && model.dataStructureId > 0}
									hasData= {model.hasData}
									isPublic= {model.isPublic}
									data_aggrement = {model.settings.dataAggrement}
									total = {model.count}
									requestAble = {model.requestAble}
									hasRequestRight = {model.hasRequestRight}
									requestExist = {model.requestExist}
								/> 
								{#if model.settings.useTags}
									<Tags  {id} {version}  tag={model.tag}/>
									{:else}
									<Versions	{id} {version} />	
								{/if}

								<Funding f={model.additionalInformations['funder']}  />
								<Keywords k={model.additionalInformations['keyword']} />
						</div>
						{/if}
		</div>


		{#if linkHook && entityName?.toLowerCase()!='extension' }

				<Link	links={model.links.to} />

		{/if}

	

		{#if dataDescriptionHook && model.dataStructureId	!== undefined && model.dataStructureId > 0}
			 <DataDescription	{id} {version} {tag} hook={dataDescriptionHook}/>
		{/if}

		{#if dataHook	&& model.hasData}

			<Data {id} {version} hook={dataHook}/>
		{/if}

		{#if attachmentsHook}
			<Attachment {id} {version} hook={attachmentsHook}/>
		{/if}

  {#if model.downloadAccess && addtionalhooks	&& addtionalhooks.length > 0	}
			<Hooks	{id} {version} hooks={addtionalhooks} />
		{/if}

		{:catch error}
			{#if error.status === 403	}
				<Forbidden/>
				{:else if error.status === 404}
				<NotExist />
			{:else if error.status === 410}
				<Deleted {id}/>
			{:else if error.status === 423}
				<InProcess />
			{:else if error.status === 500}
				<InternalServer />
			{:else}
				<ErrorMessage {error} />
			{/if}
		
		{/await}

</div>

</Page>

