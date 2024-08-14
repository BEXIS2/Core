<script lang="ts">
	import Structure from '$lib/components/datastructure/structure/EditStructure.svelte';

	import {
		Spinner,
		Page,
		ErrorMessage,
		type helpItemType,
		helpStore
	} from '@bexis2/bexis2-core-ui';
	import {
		generate,
		save,
		get,
		getDisplayPattern,
		getStructures
	} from '$lib/components/datastructure/services';

	import type { DataStructureEditModel } from '$lib/components/datastructure/types';
	import {
		displayPatternStore,
		structureStore,
		isTemplateRequiredStore,
		isMeaningRequiredStore,
		setByTemplateStore,
		enforcePrimaryKeyStore,
		changeablePrimaryKeyStore
	} from '$lib/components/datastructure/store';
	import { pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	//help
	import { dataStructureHelp } from '../help';
	let helpItems: helpItemType[] = dataStructureHelp;

	// load attributes from div
	let container;
	let datastructureId: number = 6;

	let model: DataStructureEditModel;
	$: model;

	let init: boolean = true;
	let dataExist: boolean = false;

	async function start() {
		helpStore.setHelpItemList(helpItems);
		// get data from parent
		//<div id="datastructure" structure="9" dataexist="False" istemplaterequired="False" ismeaningrequired="False" setbytemplate="True" enforceprimarykey="True" changeableprimarykey="False"></div>

		container = document.getElementById('datastructure');
		datastructureId = Number(container?.getAttribute('structure'));
		dataExist = container?.getAttribute('dataExist')?.toLocaleLowerCase() === 'true';
		console.log(
			"🚀 ~ file: +page.svelte:32 ~ start ~ container?.getAttribute('dataExist'):",
			container?.getAttribute('dataExist')
		);

		console.log('🚀 ~ file: +page.svelte:32 ~ start ~ dataExist:', dataExist);

		// get isTemplateRequired from settings and add it to store
		// is used by validation
		const isTemplateRequired =
			container?.getAttribute('isTemplateRequired')?.toLocaleLowerCase() == 'true' ? true : false;
		isTemplateRequiredStore.set(isTemplateRequired);

		// get isTemplateRequired from settings and add it to store
		// is used by validation
		const isMeaningRequired =
			container?.getAttribute('isMeaningRequired')?.toLocaleLowerCase() == 'true' ? true : false;
		console.log('🚀 ~ file: +page.svelte:57 ~ start ~ isMeaningRequired:', isMeaningRequired);
		isMeaningRequiredStore.set(isMeaningRequired);

		// get setByTemplate from settings and add it to store
		// is used by createion of variables
		const setByTemplate =
			container?.getAttribute('setByTemplate')?.toLocaleLowerCase() == 'true' ? true : false;
		setByTemplateStore.set(setByTemplate);

		// get enforcePrimaryKey from settings and add it to store
		// save structure only if pk is set
		const enforcePrimaryKey =
			container?.getAttribute('enforcePrimaryKey')?.toLocaleLowerCase() == 'true' ? true : false;
		enforcePrimaryKeyStore.set(enforcePrimaryKey);

		// get changeablePrimaryKey from settings and add it to store
		// save structure only if pk is set
		const changeablePrimaryKey =
			container?.getAttribute('changeablePrimaryKey')?.toLocaleLowerCase() == 'true' ? true : false;
		changeablePrimaryKeyStore.set(changeablePrimaryKey);

		console.log('edit structure', datastructureId);

		// copy structure
		model = await get(datastructureId);

		// load sturctures for validation against existings
		const structures = await getStructures();
		structureStore.set(structures);

		// load display pattern onces for all edit types
		const displayPattern = await getDisplayPattern();
		displayPatternStore.set(displayPattern);

		// console.log('model', model);
	}

	function back() {
		init = false;
	}
</script>
<Page
	title="Data Structure"
	note="This page allows you to create and edit data structures."
	contentLayoutType={pageContentLayoutType.full}
	help={true}
	footer={false}
>

	{#await start()}
		<Spinner label="the data structure is loading" />
	{:then}
		{#if model}
			<Structure {model} {dataExist} on:back={back} />
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>
