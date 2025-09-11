<script lang="ts">
	import { fade } from 'svelte/transition';

	import { positionType, ErrorMessage } from '@bexis2/bexis2-core-ui';
	import type { linkType } from '@bexis2/bexis2-core-ui';

	// ui components
	import Fa from 'svelte-fa';
	import { faPlus } from '@fortawesome/free-solid-svg-icons';
	import Overview from './Overview.svelte';
	import Edit from './Edit.svelte';

	// services
	import { Page, Spinner } from '@bexis2/bexis2-core-ui';

	import {
		getEntities,
		getDataStructures,
		getMetadataStructures,
		getGroups,
		getHooks,
		getFileTypes,
		getEntityTemplateList,
		getExtensions
	} from '../../services/EntityTemplateCaller';

	// types
	import type { EntityTemplateModel } from '../../models/EntityTemplate';

	// Store
	import { entityTemplatesStore } from './store';

	let hooks = [];
	let metadataStructures = [];
	let dataStructures = [];
	let entities = [];
	let groups = [];
	let filetypes = [];
	let extensions = [];

	let isOpen = false;
	let header = '';

	$: selectedEntityTemplate = 0;

	async function load() {
		hooks = await getHooks();
		console.log('hooks', hooks);
		metadataStructures = await getMetadataStructures();
		dataStructures = await getDataStructures();
		entities = await getEntities();
		groups = await getGroups();
		filetypes = await getFileTypes();
		extensions	= await getExtensions();

		entityTemplatesStore.set(await getEntityTemplateList());
	}

	async function refresh(e: any) {
		const newEntityTemplate = e.detail;
		//console.log(newEnityTemplate);

		//remove object from list & add to list again
		$entityTemplatesStore = $entityTemplatesStore.filter((e) => {
			return e.id !== newEntityTemplate.id;
		});

		$entityTemplatesStore = [...$entityTemplatesStore, newEntityTemplate];

		// // update store
		// entityTemplatesStore.set(entitytemplates);

		// close the form to reset
		isOpen = false;
	}

	// open form as new
	async function create() {
		// set Form header
		header = 'Create a new entity template';

		// set id
		selectedEntityTemplate = 0;
		isOpen = !isOpen;
	}

	// open form in edit with id in e.detail
	async function edit(e: any) {
		// set Form header
		header = 'Edit a new entity template (' + e.detail + ')';

		//remove form from dom
		isOpen = false;

		// reopen form with new object
		setTimeout(async () => {
			selectedEntityTemplate = e.detail;
			isOpen = true;
		}, 500);
	}

	const toggle = () => (isOpen = !isOpen);

	const links: linkType[] = [
		{
			label: 'Manual',
			url: '/home/docs/Data%20Description#entity-templates'
		}
	];
</script>

<Page
	title="Entity Templates"
	note="This page allows you to edit the entity templates and create new ones."
	help={true}
	{links}
>
	<svelte:fragment>
		{#await load()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading entity templates" />
			</div>
		{:then result}
			{#if isOpen}
				<Edit
					id={selectedEntityTemplate}
					{hooks}
					{metadataStructures}
					{dataStructures}
					{entities}
					{groups}
					{filetypes}
					{extensions}
					on:save={refresh}
					on:cancel={() => (isOpen = false)}
				/>
			{:else}
				<div class="w-screen">
					<button title="create" type="button" on:click={create} class="btn variant-filled-primary"
						><Fa icon={faPlus} /></button
					>
				</div>
			{/if}

			<Overview bind:entitytemplates={$entityTemplatesStore} on:edit={edit} />
		{:catch error}
			<ErrorMessage {error} />
		{/await}
	</svelte:fragment>
</Page>
