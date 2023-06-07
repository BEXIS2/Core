<script lang="ts">
	import { onMount } from 'svelte';

	import { getView } from './services';
	import { getViewStart } from '../../services/HookCaller';
	import { setApiConfig, Spinner } from '@bexis2/bexis2-core-ui';

	import Header from './Header.svelte';
	import TabContent from './Tab.svelte';
	import Debug from '../../lib/components/Debug.svelte';
	import { TabGroup, Tab } from '@skeletonlabs/skeleton';

	//types
	import type { ViewModel, Hook } from '../../models/View';

	let title = '';

	let container;
	let id: number;
	let version: number = 0;
	let model: ViewModel;

	// tabs
	let activeTab = 'metadata';

	let tabSet = 0;

	let hookList: Hook[];
	$: hooks = hookList;

	// for test ui
	$: testPage = '';

	onMount(async () => {
		// get data from parent
		container = document.getElementById('view');
		id = container?.getAttribute('dataset');
		version = container?.getAttribute('version');

		console.log('start view', id, version);
		//setup api
		setApiConfig('https://localhost:44345', 'davidschoene', '123456');

		// load data from server
		model = await getView(id);

		console.log('onmount', model);

		hooks = model.hooks;
		title = model.title;
		version = model.version;
		id = model.id;

		console.log(model);
		console.log('hooks', hooks);

		//test ui as html
		// const resTestPage = await fetch('dcm/view/test');
		// testPage = await resTestPage.text();
	});

	async function loadTab(action, id, version) {
		let test = await getViewStart(action, id, version);
		return test;
	}
</script>

<div>
	<!-- Header -->
	<Header {id} {version} {title} />

	{#if hooks}
		<TabGroup>
			<!-- nav -->
			{#each hooks as hook, i}
				{#if hook.status == 2}
					<Tab bind:group={tabSet} name={hook.name} value={i}>{hook.name}</Tab>
				{/if}
			{/each}

			<!-- content -->

			<!-- Tab Panels --->
			<svelte:fragment slot="panel">
				{#each hooks as hook, i}
					{#if hook.status == 2}
						<TabContent {id} {version} {...hook} active={tabSet == i} />
					{/if}
				{/each}
			</svelte:fragment>
		</TabGroup>
	{:else}
		<div>
			<Spinner label="loading dataset {title}" />
		</div>
	{/if}

	<Debug {hooks} />
</div>
