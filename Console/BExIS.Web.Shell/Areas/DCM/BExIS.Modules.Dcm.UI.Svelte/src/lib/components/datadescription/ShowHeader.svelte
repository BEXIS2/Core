<script lang="ts">
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import type { fileInfoType } from '@bexis2/bexis2-core-ui';

	import Fa from 'svelte-fa';
	import { faTrash } from '@fortawesome/free-solid-svg-icons';

	import { removeStructure } from '$services/DataDescriptionCaller';
	import { latestDataDescriptionDate } from '../../../routes/edit/stores';

	import { modalStore } from '@skeletonlabs/skeleton';
	import type { ModalSettings } from '@skeletonlabs/skeleton';

	export let id;
	export let structureId;
	export let title;
	export let description;
	export let fileReaderExist;
	export let hasData;
	export let readableFiles: fileInfoType[] = [];

	let loading = false;


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
		const res = await removeStructure(id);

		console.log('remove', res);

		if (res == true) {
			// update store
			latestDataDescriptionDate.set(Date.now());
		} else {
			//show message
		}

		loading = false;
	}
</script>

<div class="show-datadescription-header-container flex">
	<div class="flex-col gap-3 grow">
		<h2 class="h2">{title} ({structureId})</h2>
		<p>{description}</p>
		{#if loading}
			<div>
				<Spinner textCss="text-surface-500" />
			</div>
		{/if}
	</div>
	<div>

		{#if hasData ===false}
		<div class="text-end flex-auto">
			<button class="btn bg-warning-500" on:click={()=>modalStore.trigger(modal)}><Fa icon={faTrash} /></button>
		</div>
		{/if}
	</div>
</div>
