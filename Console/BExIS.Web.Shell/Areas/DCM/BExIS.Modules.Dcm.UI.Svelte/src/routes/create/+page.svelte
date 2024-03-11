<script lang="ts">
	import List from './List.svelte';
	import Form from './Form.svelte';
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	import type { linkType } from '@bexis2/bexis2-core-ui';

	import { onMount } from 'svelte';
	import { fade } from 'svelte/transition';

	import { Spinner } from '@bexis2/bexis2-core-ui';
	import { getEntityTemplateList } from './services';
	import { goTo } from '$services/BaseCaller';

	import type { EntityTemplateModel } from '$models/EntityTemplate';

	let entitytemplate: EntityTemplateModel;

	$: entitytemplates = [];
	// $:systemkeys= [];
	$: selected = entitytemplate;

	onMount(async () => {
		entitytemplates = await getEntityTemplateList();
	});

	$: isOpen = false;

	function handleSelect(e) {
		console.log('on select');
		//remove form from dom
		isOpen = false;

		// reopen form with new object
		setTimeout(async () => {
			let index = e.detail;
			selected = entitytemplates[index];
			console.log(selected);
			isOpen = true;

			console.log('on isOpen', isOpen);
		}, 500);
	}

	function onSaveHandler(e) {
		//e.detail == id of teh new created dataset
		goTo('/dcm/edit?id=' + e.detail);
	}

	let links: linkType[] = [
		{
			label: 'manual',
			url: 'https://github.com/BEXIS2/Documents/blob/master/Manuals/DCM/Manual.md'
		}
	];

	//console.log(links)
</script>

<Page
	title="Create a Dataset"
	note="On this page you can create a dataset based on a template. please select on template and fill out the form."
	{links}
	contentLayoutType={pageContentLayoutType.full}
>
	<div in:fade={{ delay: 500 }} out:fade={{ delay: 500 }}>
		{#if entitytemplates}
			<div class="grid grid-cols-1 lg:grid-cols-2 gap-3">
				<List items={entitytemplates} on:select={handleSelect} />

				{#if selected && isOpen}
					<Form
						bind:id={selected.id}
						on:cancel={() => (isOpen = false)}
						on:save={(e) => onSaveHandler(e)}
					/>
				{/if}
			</div>
		{:else}
			<Spinner />
		{/if}
	</div>
</Page>
