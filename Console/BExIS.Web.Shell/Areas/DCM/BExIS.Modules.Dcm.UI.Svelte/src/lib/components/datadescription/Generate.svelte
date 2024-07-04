<script lang="ts">
	import { goTo } from '../../../services/BaseCaller';
	import { MultiSelect, Spinner } from '@bexis2/bexis2-core-ui';
	import type { listItemType } from '@bexis2/bexis2-core-ui';
	import { onMount, createEventDispatcher } from 'svelte';

	import { availableStructues, setStructure } from '$services/DataDescriptionCaller';
	import { getHookStart } from '$services/HookCaller';

	import type { DataDescriptionModel } from '$models/DataDescription';

	import { latestFileUploadDate, latestDataDescriptionDate } from '../../../routes/edit/stores';

	const dispatch = createEventDispatcher();

	function goToGenerate(file, id) {
		// if its possible the file will be used to start structure analyze
		goTo('/rpm/datastructure/create?entityId=' + id + '&file=' + file);
	}

	function goToCreate() {
		// if its possible the file will be used to start structure analyze
		goTo('/rpm/datastructure/create?');
	}

	export let id;
	export let version;
	export let hook;
	export let model: DataDescriptionModel;
	$: model;

	let list: listItemType[];
	$: list;

	let loading: boolean;
	$: loading;

	let structures = [];
	$: structures;

	onMount(async () => {
		reload();
		setList(model.readableFiles, structures);
		latestFileUploadDate.subscribe((s) => {
			if (s > 0) {
				reload();
			}
		});
		latestDataDescriptionDate.subscribe((s) => {
			if (s > 0) {
				reload();
			}
		});
	});

	async function reload() {
		loading = true;
		console.log('reload generated data descritoon generate');
		structures = await availableStructues(id);
		model = await getHookStart(hook.start, id, version);
		setList(model.readableFiles, structures);
		loading = false;
	}

	// after select a value from the dropdown
	// it will go to the generator or selet a exiting structure
	async function change(e) {
		let item = e.detail;
		//console.log("select item",item)

		if (item.group === 'options') {
			//console.log("go to create a datastructure")
			goToCreate();
		} else if (item.group === 'file') {
			loading = true;
			goToGenerate(e.detail.text, model.id);
		} else if (item.group === 'structure') {
			console.log('select a structure', id, item.id);
			loading = true;
			await setStructure(model.id, item.id);
			dispatch('selected');
		}
	}

	// list is a comibnation of options, already existing datastructures and files
	function setList(files, structureList) {
		list = [];

		if (model.isRestricted == false) {
			// if user is not restricted by selection of the structures, then add option to vcreate a new one
			list.push({ id: 0, text: 'create new', group: 'options' });
		}

		if (files && model.isRestricted == false) {
			// if user is not restricted by selection of the structures, then add option to create from file
			files.forEach((i) => list.push({ id: i.name, text: i.name, group: 'file' }));
		}

		if (structureList) {
			//structureList!== null && structureList != undefined)
			list = [...list, ...structureList];
		}
	}
</script>

{#if list && structures}
	<div class="flex">
		<MultiSelect
			id="SelectDataStructure"
			title="Generate based on an uploaded file, select an existing one, or create a new"
			itemId="id"
			itemLabel="text"
			itemGroup="group"
			bind:source={list}
			on:change={change}
			complexSource={true}
			complexTarget={true}
			isMulti={false}
			{loading}
		/>
	</div>
{/if}
