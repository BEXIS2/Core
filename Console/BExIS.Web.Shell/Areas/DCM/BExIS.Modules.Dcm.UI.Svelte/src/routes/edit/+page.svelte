<script lang="ts">
	import { onMount } from 'svelte';
	import { fade } from 'svelte/transition';

	import { getEdit, getHooks } from './services';
	import { Spinner, Page, ErrorMessage, pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	import Header from './Header.svelte';
	import Data from './Data.svelte';
	import Hooks from './Hooks.svelte';
	import Message from './MessagesContainer.svelte';
	import Debug from '$lib/components/Debug.svelte';

	import {
		latestFileUploadDate,
		latestDataDescriptionDate,
		latestFileReaderDate,
		hooksStatus
	} from './stores';

	import type { EditModel, HookModel, ViewModel } from './types';
	import { isEditModel } from './types';
	import Hook from '$lib/components/Hook.svelte';

	// load attributes from div
	let container;
	let id: number = 0;

	let version: number;

	let title = '';

	let model: EditModel = {
		id: 0,
		versionId: 0,
		version: 0,
		title: '',
		hooks: [],
		views: []
	};

	let hookStatusList: { [key: string]: number };
	$: hookStatusList;
	// hooks
	let hooks: HookModel[];
	$: hooks = [];

	let datasethooks: HookModel[];
	$: datasethooks = [];

	let addtionalhooks: HookModel[];
	$: addtionalhooks = [];

	// views
	let views: ViewModel[];
	$: views = [];

	let additionalViews: ViewModel[];
	$: additionalViews = [];

	let messageView: ViewModel;
	$: messageView;

	//$:$latestFileUploadDate, updateHookStatus();

	latestFileUploadDate.subscribe((e) => {
		console.log('latestFileUploadDate');
		updateHookStatus();
	});

	// latestDataDescriptionDate.subscribe((e) => {
	// 	console.log('latestDataDescriptionDate');
	// 	updateHookStatus();
	// });

	// latestFileReaderDate.subscribe((e) => {
	// 	console.log('latestFileReaderDate');
	// 	updateHookStatus();
	// });

	async function load() {
		// get data from parent
		container = document.getElementById('edit');
		id = Number(container?.getAttribute('dataset'));
		version = Number(container?.getAttribute('version'));

		// load model froms server
		model = await getEdit(id);
		console.log('edit model ', model);
		hooks = model.hooks;
		views = model.views;
		title = model.title;
		version = model.version;

		// there is a need for a time delay to update the hook status
		// if not exit, the first run faild because the hooks are not
		setTimeout(async () => {
			console.log('HOOKS ', model.hooks);

			// update store
			updateStatus(model.hooks);

			// seperate dcm hooks from other hooks
			seperateHooks(hooks);

			// get resultView
			seperateViews(views);
		}, 1000); /* <--- If this is enough greater than transition, it doesn't happen... */

		console.log('model and hooks', model, hookStatusList);
	}

	async function updateHookStatus() {
		let x = await getHooks(id);
		setTimeout(async () => {
			console.log('updateHookStatus', x);

			model.hooks = x;
			if (model.hooks) {
				updateStatus(model.hooks);
			}
		}, 1000);
	}

	// seperate dcm hooks from other hooks
	// known hooks - metadata, fileupload, validation
	function seperateHooks(hooks: HookModel[]) {
		datasethooks = [];
		addtionalhooks = [];

		hooks.forEach((element) => {
			if (
				element.name == 'metadata' ||
				element.name == 'fileupload' ||
				element.name == 'validation' ||
				element.name == 'submit' ||
				element.name == 'datadescription'
			) {
				datasethooks.push(element);
			} else {
				addtionalhooks.push(element);
			}
		});
	}

	// seperate known views from other views
	// known view - resultMessages
	function seperateViews(views) {
		views.forEach((element) => {
			if (element.name == 'messages') {
				messageView = element;
			} else {
				additionalViews.push(element);
			}
		});

		// console.log(messageView)
		// console.log(additionalViews)
	}

	function updateStatus(_hooks: HookModel[]) {
		console.log('updateStatus', _hooks);
		let dic: { [key: string]: number } = { ['']: 0 };

		if (_hooks !== undefined) {
			_hooks.forEach((hook) => {
				dic[hook.name] = hook.status;
			});

			//console.log("cuurent Hookstatus",$hooksStatus);
			hooksStatus.set(dic);
			//console.log("update Hookstatus",$hooksStatus);
		}

		hookStatusList = $hooksStatus;
	}

	// debug infos
	let visible = false;
</script>

<Page title="Edit dataset: {model.title}" contentLayoutType={pageContentLayoutType.full}>
	{#await load()}
		<div class="w-full h-full text-surface-600">
			<Spinner label="loading edit page" />
		</div>
	{:then a}
		{#if model && hookStatusList}
			<!--if the model == true, load page-->
			<!-- Header -->
			<Header {id} {version} {title} />

			<!-- Data Module Hooks -->
			<Data bind:hooks={datasethooks} {id} {version} />

			<hr class="!border-dashed" />

			<Hooks bind:hooks={addtionalhooks} {id} {version} />
		{:else}
			<!-- access denied -->
			<div class="h-screen">
				<Spinner textCss="text-surface-800" label="loading edit page" position="center" />
			</div>
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>
