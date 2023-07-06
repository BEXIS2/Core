<script lang="ts">
	import Fa from 'svelte-fa/src/fa.svelte';

	import { createEventDispatcher } from 'svelte';

	import { faTrash, faPen } from '@fortawesome/free-solid-svg-icons';
	import { onMount } from 'svelte';

	// ui components
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import Card from './Card.svelte';
	import { Modal, modalStore } from '@skeletonlabs/skeleton';

	//services
	import { setApiConfig, positionType } from '@bexis2/bexis2-core-ui';
	import { deleteEntityTemplate } from '../../services/EntityTemplateCaller';

	//types
	import type { EntityTemplateModel } from '../../models/EntityTemplate';
	import type { ModalSettings } from '@skeletonlabs/skeleton';

	const dispatch = createEventDispatcher();

	export let entitytemplates: EntityTemplateModel[];

	function edit(id) {
		console.log('edit', id);
		dispatch('edit', id);
	}

	// when bt for remove is trigger a conformation is needed
	function deletionConfirmation(index, id) {
		const modal: ModalSettings = {
			type: 'confirm',
			title: 'Please Confirm',
			body: 'Are you sure you wish to remove?',
			response: (r: boolean) => {
				remove(index, id);
			}
		};

		modalStore.trigger(modal);
	}

	// call delete entity template on server
	async function remove(index, id) {
		console.log(index, id);
		//remove in backend
		const res = await deleteEntityTemplate(id);
		console.log(res);
		if (res === true) {
			//remove list
			entitytemplates = entitytemplates.filter((id, idx) => {
				return idx !== index;
			});
		}
	}
</script>

{#if entitytemplates}
	<div class="py-5 w-full grid sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
		{#each entitytemplates as item, i (item.id)}
			<Card {...item}>
				<button class="btn variant-filled-primary" on:click={() => edit(item.id)}
					><Fa icon={faPen} /></button
				>
				<button
					class="btn variant-filled-warning"
					disabled={item.linkedSubjects.length > 0}
					on:click={() => deletionConfirmation(i, item.id)}><Fa icon={faTrash} /></button
				>
			</Card>
		{/each}
	</div>
{:else}
	<div class="text-surface-800">
		<Spinner position={positionType.center} label="loading entity templates" />
	</div>
{/if}

<Modal />
