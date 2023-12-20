<script lang="ts">
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import type { fileInfoType } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faTrash, faEdit } from '@fortawesome/free-solid-svg-icons';

	import { removeStructure } from '$services/DataDescriptionCaller';
	import { latestDataDescriptionDate } from '../../../routes/edit/stores';

	import { getModalStore } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();
	import type { ModalSettings } from '@skeletonlabs/skeleton';
	import { createEventDispatcher } from 'svelte';
	import { goTo } from '$services/BaseCaller';


	export let id;
	export let structureId;
	export let title;
	export let description;
	export let fileReaderExist;
	export let hasData;
	export let enableEdit;
	export let readableFiles: fileInfoType[] = [];

	let loading = false;
	const dispatch = createEventDispatcher();

	const modal: ModalSettings = {
		type: 'confirm',
		title: 'Copy',
		body: 'Are you sure you wish to remove the structure?',
		// TRUE if confirm pressed, FALSE if cancel pressed
		response: (r: boolean) => {
			if (r === true) {
				remove();
			}
		}
	};

	async function remove() {
		loading = true;
		console.log('remove');
		try {
			const res = await removeStructure(id);
			console.log('remove', res);

			if (res == true) {
				// update store
				latestDataDescriptionDate.set(Date.now());
			} else {
				let messages: string[] = [];
				messages.push('remove failed');
				//show message
				dispatch('error', { messages });
			}
		} catch (err) {
			let messages: string[] = [];
			messages.push('remove failed');
			//show message
			dispatch('error', { messages });
		}

		loading = false;
	}

	function goToEdit() {
		goTo("/rpm/datastructure/edit?structureId="+id)
	}
</script>

<div class="show-datadescription-header-container flex">
	<div class="flex-col gap-3 grow">
		<h4 class="h4">{title} ({structureId})</h4>
		{#if description} <p>{description}</p> {/if}
		{#if loading}
			<div>
				<Spinner textCss="text-surface-500" />
			</div>
		{/if}
	</div>
	<div>
		<div class="flex gap-2 text-end flex-auto"></div>
		{#if enableEdit}
			<button title="edit" class="chip variant-filled-secondary" on:click={goToEdit}
				><Fa icon={faEdit} /></button
			>
		{/if}
		{#if hasData === false}
			
				<button
					title="remove"
					class="chip variant-filled-error"
					on:click={() => modalStore.trigger(modal)}><Fa icon={faTrash} /></button
				>

		{/if}
	</div>
</div>
