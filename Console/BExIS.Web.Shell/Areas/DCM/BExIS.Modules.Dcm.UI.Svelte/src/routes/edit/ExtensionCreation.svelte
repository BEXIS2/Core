<script lang="ts">
	import List from '../../lib/components/entityTemplate/List.svelte';
	import Form from '../../lib/components/entityTemplate/Form.svelte';
	import {ErrorMessage, notificationStore, notificationType} from '@bexis2/bexis2-core-ui';
	import { fade } from 'svelte/transition';

	import { Spinner, positionType } from '@bexis2/bexis2-core-ui';
	import { createExtensionLink, getExtensionEntityTemplateList } from './services';
	import { goTo } from '$services/BaseCaller';

	import type { EntityTemplateModel } from '$models/EntityTemplate';

	let entitytemplate: EntityTemplateModel;
	export let id;

	$: entitytemplates = [];
	// $:systemkeys= [];
	$: selected = entitytemplate;

	$: isOpen = false;

	async function load() {
		entitytemplates = await getExtensionEntityTemplateList(id);
		console.log('🚀 ~ load ~ entitytemplates:', entitytemplates);
	}
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

	async function onSaveHandler(e) {
		//e.detail == id of teh new created dataset
		console.log('saved', e.detail, id);
		//create link between main entity and extension entity
		const res =	await createExtensionLink(id,e.detail);
		notificationStore.showNotification({
				notificationType: notificationType.success,
				message: 'Extension created and linked successfully'
			})
	
		goTo('/dcm/edit?id=' + id);
	}


	//console.log(links)


</script>


	<div in:fade={{ delay: 500 }} out:fade={{ delay: 500 }}>
		{#await load()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading entity templates" />
			</div>
		{:then result}
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
		{:catch error}
			<ErrorMessage {error} />
		{/await}
	</div>

