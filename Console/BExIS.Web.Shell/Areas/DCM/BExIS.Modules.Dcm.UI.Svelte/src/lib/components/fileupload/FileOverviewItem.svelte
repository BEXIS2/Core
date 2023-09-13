<script>
	import { Fa } from 'svelte-fa/src/index.js';

	import { FileInfo, Spinner, TextInput } from '@bexis2/bexis2-core-ui';
	import { faTrash } from '@fortawesome/free-solid-svg-icons';

	import { removeFile, saveFileDescription } from '../../../routes/edit/services';

	import { createEventDispatcher, onMount } from 'svelte';

	export let id;
	export let file;
	export let type;
	export let description;
	export let withDescription;
	const dispatch = createEventDispatcher();

	// action to save description
	export let save;
	// action to remove file
	export let remove;

	// set if its possible to generate a structure based on that file
	export let generateAble = false;

	let loading = false;
	let fileNameSpan = 'col-span-7';
	onMount(async () => {
		fileNameSpan = withDescription ? 'col-span-4' : 'col-span-7';
	});

	async function handleRemoveFile(e) {
		loading = true;

		//remove from server
		const res = await removeFile(remove, id, file);
		if (res == true) {
			let message = file + ' removed.';
			dispatch('removed', { text: message });
		}
		console.log('remove loading');
		loading = false;
	}

	async function handleSaveFileDescription() {
		const res = await saveFileDescription(save, id, file, description);
		if (true) {
			let message = 'Description of ' + file + ' is updated.';
			dispatch('saved', { text: message });
		}
	}
</script>

{#if type}
	<div class="flex gap-5">
		<div class="self-center flex-none"><FileInfo {type} size="x-large" /></div>

		<div class="{fileNameSpan} self-center grow">
			{file}
		</div>

		{#if withDescription}
			<div class="{fileNameSpan} self-center grow">
				<TextInput bind:value={description} on:change={(e) => handleSaveFileDescription()} />
			</div>
		{:else}
			<div />
		{/if}

		<div class="text-right">
			{#if loading}
				<Spinner textCss="text-surface-500" />
			{:else}
				<button class="btn" on:click={(e) => handleRemoveFile(e)}><Fa icon={faTrash} /></button>
			{/if}
		</div>
	</div>
{/if}
