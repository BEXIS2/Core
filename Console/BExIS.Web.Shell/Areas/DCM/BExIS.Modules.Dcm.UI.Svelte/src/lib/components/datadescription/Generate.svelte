<script lang="ts">
	import { goTo } from '../../../services/BaseCaller';
	import { MultiSelect, Spinner } from '@bexis2/bexis2-core-ui';
	import type { listItemType } from '@bexis2/bexis2-core-ui';
	import { onMount, createEventDispatcher} from 'svelte';

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
		goTo('/rpm/datastructure/create?entityId=' + id);
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

	let selected;
	$: selected;

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
	// it will go to the generator or select a exiting structure
	async function change(e) {
		let item = e.detail;
		//console.log("select item",item)

		if (item.group === 'other options') {
			//console.log("go to create a datastructure")
			goToCreate();
		} else if (item.group === 'create new based on file') {
			if (item.text === 'upload first a file to create a new data structure based on it') {
				// do nothing
				// trigger clear selection
				selected = null;
				return;
			}
			loading = true;
			goToGenerate(e.detail.text, model.id);
		} else if (item.group === 'data structures') {
			console.log('select a structure', id, item.id);
			loading = true;
			await setStructure(model.id, item.id);
			dispatch('selected');
		}
	}

	// list is a combination of options, already existing data structures and files
	function setList(files, structureList) {
		list = [];

		if (files && model.isRestricted == false) {
			// if user is not restricted by selection of the structures, then add option to create from file
			if (files.length > 0){
				files.forEach((i) => list.push({
					id: i.name, text: i.name, group: 'create new based on file',
					description: ''
				}));
			}
			else{
				// if no files are available, we can not create from file
				list.push({
					id: 0, text: 'upload first a file to create a new data structure based on it', group: 'create new based on file',
					description: ''
				});
			}
		}
	
		if (model.isRestricted == false) {
			// if user is not restricted by selection of the structures, then add option to vcreate a new one
			// get max id to avoid id conflict
			let maxId = 0;
			list.forEach((i) => {
				if (i.id > maxId) maxId = i.id;
			});
			list.push({
				id: maxId + 1, text: 'create a new data structure manually', group: 'other options',
				description: '',
			});
		}

		if (structureList) {
			//structureList!== null && structureList != undefined)
			// change group from structure to existing structure
			structureList.forEach((s) => (s.group = 'data structures'));
			list = [...list, ...structureList];
		}
	}
</script>

{#if list && structures}
	<div class="flex">
		<MultiSelect
			id="SelectDataStructure"
			title="Create new (uploaded file or manually) or select an existing data structure"
			placeholder="Select data structure option"
			itemId="id"
			itemLabel="text"
			itemGroup="group"
			bind:source={list}
			on:change={change}
			complexSource={true}
			complexTarget={true}
			isMulti={false}
			{loading}
			bind:target={selected}
	
		/>
	</div>
{/if}
