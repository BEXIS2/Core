<script lang="ts">
	import { onMount, createEventDispatcher } from 'svelte';

	import { FileUploader, Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';

	import FileOverview from '$lib/components/fileupload/FileOverview.svelte';

	import FileReaderInformation from '$lib/components/fileupload/FileReaderInformation.svelte';

	import { latestFileUploadDate, latestFileReaderDate,latestSubmitDate } from '../../routes/edit/stores';

	import type { FileUploadModel } from '$models/FileUpload';

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

	let model: FileUploadModel;
	$: model;


	let fileReaderSelectedFile=""

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
		console.log('reload fileupload', $latestFileUploadDate);

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
		fileReaderSelectedFile="";
	}

</script>

<div class="space-y-2">
	{#await load()}
		<div class="text-surface-800">
			<Spinner label="loading File Uploader" position="{positionType.start}" />
		</div>
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

		{#if model.fileUploader.existingFiles}
			<FileOverview
				{id}
				files={model.fileUploader.existingFiles}
				descriptionType={model.fileUploader.descriptionType}
				{save}
				{remove}
				on:success={success}
				on:warning={warning}
			/>
		{/if}

		<FileReaderInformation
			{id}
			bind:target = {fileReaderSelectedFile}
			bind:readableFiles={model.fileUploader.existingFiles}
			bind:asciiFileReaderInfo={model.asciiFileReaderInfo}
		/>
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</div>
