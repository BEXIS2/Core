<script lang="ts">
	import { FileUploader, ErrorMessage } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';
	import { onMount, createEventDispatcher } from 'svelte';
	import FileOverview from '$lib/components/fileupload/FileOverview.svelte';
	import TimeDuration from '$lib/components/utils/TimeDuration.svelte';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import type { FileUploadModel } from '$models/FileUpload';

	export let id = 0;
	export let version = 1;
	export let hook;

	// action for fileupload
	let start = '';
	let save = '/dcm/attachmentupload/saveFileDescription';
	let remove = '/dcm/attachmentupload/removefile';
	let submit = '/dcm/attachmentupload/upload';
	let context = 'attachment';

	let model: FileUploadModel;
	$: model;

	onMount(async () => {
		load();
	});

	$: loading = false;
	$: existError = false;

	const dispatch = createEventDispatcher();

	async function load() {
		model = await getHookStart(hook.start, id, version);
		start = hook.start;

		loading = false;
	}

	async function reload(e) {
		/* load data*/
		load();
	}

	function success(e) {
		console.log('success');
		reload(e);
		dispatch('success', { text: e.detail.text });
	}

	function warning(e) {
		console.log('warning');
		reload(e);
		dispatch('warning', { text: e.detail.text });
	}
</script>

{#await load()}
	<div class="text-surface-800">
		<Spinner label="loading File Uploader" />
	</div>
{:then result}
	{#if model.lastModification}
		<TimeDuration milliseconds={new Date(model.lastModification)} />
	{/if}
	<FileUploader
		{id}
		{version}
		{context}
		data={model.fileUploader}
		{start}
		{submit}
		on:submited={load}
		on:submit={() => (loading = true)}
		on:error
		on:success
	/>
	{#if model.fileUploader.existingFiles && model.fileUploader.existingFiles.length > 0}
		<div class="pt-4">
			<b>Uploaded File(s)</b>
		</div>
		<FileOverview
			{id}
			files={model.fileUploader.existingFiles}
			descriptionType={model.fileUploader.descriptionType}
			{save}
			{remove}
			on:success
		/>
	{/if}
{:catch error}
	<ErrorMessage {error} />
{/await}
