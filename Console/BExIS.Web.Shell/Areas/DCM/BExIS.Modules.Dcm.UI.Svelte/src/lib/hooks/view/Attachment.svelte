<script lang="ts">
	import { FileUploader, ErrorMessage } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';
	import { onMount, createEventDispatcher } from 'svelte';
	import FileOverview from '$lib/components/fileupload/FileOverview.svelte';
	import TimeDuration from '$lib/components/utils/TimeDuration.svelte';
	import { Spinner } from '@bexis2/bexis2-core-ui';
	import type { FileUploadModel } from '$models/FileUpload';
	import FilesView from '$lib/components/data/FilesView.svelte';
	import PlaceHolderHookContent from '../edit/placeholder/PlaceHolderHookContent.svelte';
	import type { AttachmentsViewModel } from '$models/View';

	export let id = 0;
	export let version = 1;
	export let hook;

	let model: AttachmentsViewModel;
	$: model;

	async function load() {
		model = await getHookStart(hook.start, id, version);
		console.log("🚀 ~ Attachments ~ model:", model)
	}


</script>
<div class="flex justify-between items-center">
 <h3 class="h3">Attachments</h3> 
</div>
<div class="card p-5 mb-5">
{#await load()}
			<PlaceHolderHookContent />
{:then result}
		<FilesView
			{id}
			files={model.files}
		/>

{:catch error}
	<ErrorMessage {error} />
{/await}
</div>

