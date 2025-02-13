<script lang="ts">
	import { onMount, createEventDispatcher } from 'svelte';

	import { FileUploader, Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';

	import FileOverview from '$lib/components/fileupload/FileOverview.svelte';

	import FileReaderInformation from '$lib/components/fileupload/FileReaderInfo.svelte';

	import {
		latestFileUploadDate,
		latestFileReaderDate,
		latestSubmitDate,
		latestDataDescriptionDate
	} from '../../routes/edit/stores';

	import type { FileUploadModel } from '$models/FileUpload';
	import PlaceholderHook from './placeholder/PlaceholderHook.svelte';
	import PlaceHolderHookContent from './placeholder/PlaceHolderHookContent.svelte';

	export let id = 0;
	export let version = 1;
	export let hook;

	// action for fileupload
	let start = '';
	let save = '/dcm/fileupload/saveFileDescription';
	let remove = '/dcm/fileupload/removefile';
	let submit = '/dcm/fileupload/upload';
	let context = 'fileupload';
	let error = '';

	$: $latestFileReaderDate, load();
	$: $latestSubmitDate, load();
	$: $latestDataDescriptionDate, load();

	let model: FileUploadModel;
	$: model;

	let fileReaderSelectedFile = '';

	onMount(async () => {
		load();

	});

	$: loading = false;

	const dispatch = createEventDispatcher();

	async function load() {
		model = await getHookStart(hook.start, id, version);
		start = hook.start;

		loading = false;

		dispatch('dateChanged', { lastModification: model.lastModification });
	}

	async function reload() {
		/*update store*/
		latestFileUploadDate.set(Date.now());

		/* load data*/
		load();
	}

	function success(e) {
		console.log('success');
		reload();
		dispatch('success', { text: e.detail.text });
	}

	function warning(e) {
		console.log('warning');
		reload();
		dispatch('warning', { text: e.detail.text });
		fileReaderSelectedFile = '';
	}
</script>

<div class="space-y-2">
	{#await load()}
		<PlaceHolderHookContent />
	{:then result}
	<FileUploader
			{id}
			{version}
			{context}
			data={model.fileUploader}
			{start}
			{submit}
			on:submited={reload}
			on:submit={() => (loading = true)}
			on:error
			on:success
		/>
		{#if model.fileUploader.existingFiles.length}
	<FileOverview
				{id}
				files={model.fileUploader.existingFiles}
				descriptionType={model.fileUploader.descriptionType}
				{save}
				{remove}
				on:success={success}
				on:warning={warning}
			/>
 	{#if model.allFilesReadable && model.hasStructure}

			<FileReaderInformation
				{id}
				bind:target={fileReaderSelectedFile}
				bind:readableFiles={model.fileUploader.existingFiles}
				bind:asciiFileReaderInfo={model.asciiFileReaderInfo}
			/>
			{/if}
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</div>
