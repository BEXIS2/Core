<script lang="ts">
	import { getEdit, getHooks } from './services';
	import { Page, ErrorMessage, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import { Modal } from '@skeletonlabs/skeleton';

	import type { linkType } from '@bexis2/bexis2-core-ui';

	import Header from './Header.svelte';
	import Data from './Data.svelte';
	import Hooks from './Hooks.svelte';


	import {
		latestFileUploadDate,
		latestDataDescriptionDate,
		latestFileReaderDate,
		latestSubmitDate,
		hooksStatus,
		latestValidationDate,
		latestDataDate
	} from './stores';

	import type { EditModel, HookModel, ViewModel } from './types';
	import Placeholder from './PlaceholderPage.svelte';

	// load attributes from div
	let container;
	let id: number = 0;

	let version: number;

	let title = '';

	$: title;

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

	latestFileUploadDate.subscribe((e) => {
		updateHookStatus();
	});

	latestDataDescriptionDate.subscribe((e) => {
		updateHookStatus();
	});

	latestFileReaderDate.subscribe((e) => {
		updateHookStatus();
	});

	latestValidationDate.subscribe((e) => {
		updateHookStatus();
	});

	latestSubmitDate.subscribe((e) => {
		updateHookStatus();
	});

	latestDataDate.subscribe((e) => {
		updateHookStatus();
	});
	async function load() {
		console.log('LOAD EDIT', Date.now);

		// get data from parent
		container = document.getElementById('edit');
		id = Number(container?.getAttribute('dataset'));
		version = Number(container?.getAttribute('version'));

		// load model froms server
		model = await getEdit(id);
		hooks = model.hooks;
		views = model.views;
		title = model.title;
		version = model.version;

		// // there is a need for a time delay to update the hook status
		// // if not exit, the first run faild because the hooks are not
		// setTimeout(async () => {

		console.log('🚀 ~ file: +page.svelte:89 ~ load ~ hooks:', hooks);
		// update store
		if (model.hooks) {
			updateStatus(model.hooks);

			// seperate dcm hooks from other hooks
			seperateHooks(model.hooks);

			// get resultView
			seperateViews(views);
		}

		// }, 1000); /* <--- If this is enough greater than transition, it doesn't happen... */
	}

	async function updateHookStatus() {
		let wait = false;
		let time = 1000;

		if (id > 0) {
			do {
				// get status of hooks,
				const r = await getHooks(id);

				model.hooks = r;
				if (model.hooks) {
					updateStatus(model.hooks);

					if (model.hooks.filter((h) => h.status == 6).length > 0) {
						wait = true;
					} else {
						wait = false;
					}

					if (time <= 10000) {
						time = time * 2;
					}
				}

				await sleep(time);
				console.log('test');
			} while (wait);
		}
	}

	function sleep(milliseconds) {
		return new Promise((resolve) => setTimeout(resolve, milliseconds));
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
				element.name == 'datadescription' ||
				element.name == 'data'
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
	}

	function updateStatus(_hooks: HookModel[]) {
		let dic: { [key: string]: number } = { ['']: 0 };

		if (_hooks !== undefined) {
			_hooks.forEach((hook) => {
				dic[hook.name] = hook.status;
			});

			hooksStatus.set(dic);
		}

		hookStatusList = $hooksStatus;
	}

	// debug infos
	let visible = false;

	const links:linkType[] = [
			{
			label: 'Manual',
			url: '/home/docs/Datasets#dataset-edit-page' }
		];

</script>

<Page title="Edit: ({id} | {title})" contentLayoutType={pageContentLayoutType.full} {links}>
	<Header {id} {version} {title} />
	{#await load()}
		<Placeholder />
	{:then a}
		{#if model && model.hooks && datasethooks && addtionalhooks && hookStatusList}
			<!-- Data Module Hooks -->
			<Data bind:hooks={datasethooks} {id} {version} />

			<hr class="!border-dashed" />

			<Hooks bind:hooks={addtionalhooks} {id} {version} />
		{:else}
			<Placeholder />
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</Page>

<Modal />
