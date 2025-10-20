<script lang="ts">
	import { Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';
	import { submit } from '../../routes/edit/services';
	import type { SubmitModel, submitResponceType } from '$models/SubmitModels';
	import GoToView from '$lib/components/submit/GoTo.svelte';

	import { Modal, getModalStore } from '@skeletonlabs/skeleton';
	const modalStore = getModalStore();
	import type { ModalSettings } from '@skeletonlabs/skeleton';

	import {
		latestFileUploadDate,
		latestDataDescriptionDate,
		latestFileReaderDate,
		latestSubmitDate,
		latestValidationDate,
		latestDataDate
	} from '../../routes/edit/stores';

	import { onMount, createEventDispatcher } from 'svelte';
	import { goto } from '$app/navigation';
	import PlaceHolderHookContent from './placeholder/PlaceHolderHookContent.svelte';

	export let id = 0;
	export let version = 1;
	export let status = 0;
	export let displayName = '';
	export let start = '';
	export let description = '';

	const dispatch = createEventDispatcher();

	let model: SubmitModel;
	$: model;

	let canSubmit: boolean = false;
	$: canSubmit;

	let isSubmitting: boolean = false;

	latestFileUploadDate.subscribe((s) => {
			if (s > 0) {
				reload();
			}
		});
		latestDataDescriptionDate.subscribe((s) => {
			if (s > 0) {
				reload();
			}
		});
		latestFileReaderDate.subscribe((s) => {
			if (s > 0) {
				reload();
			}
		});
		latestValidationDate.subscribe((s) => {
			if (s > 0) {
				reload();
			}
		});

		latestDataDate.subscribe((s) => {
			console.log('🚀 ~ latestDataDate.subscribe ~ s:', s);

			if (s > 0) {
				reload();
			}
		});

	onMount(async () => {
		
	});

	async function reload() {
		//console.log('reload submit', start, id, version);
		//console.log('latestDataDate', latestDataDate);

		canSubmit = false;
		//console.log(' before hook');

		model = await getHookStart(start, id, version);
		//console.log(' before activateSubmit', canSubmit);

		canSubmit = activateSubmit();
		//console.log(' after activateSubmit', canSubmit);

		//console.log('reload submit', model);

		return model;
	}

	const confirm: ModalSettings = {
		type: 'confirm',
		title: 'Submit',
		body: 'Are you sure you wish to submit the data?',
		// TRUE if confirm pressed, FALSE if cancel pressed
		response: (r: boolean) => {
			if (r === true) {
				submitBt();
				modalStore.trigger(next);
				dispatch('success', { text: 'The import of your data has been started.' });
			}
		}
	};

	const next: ModalSettings = {
		type: 'alert',
		title: 'Import started',
		body: 'Editing will be disabled until the upload is complete. If you are uploading a large amount of data, the upload will take a while, and you will be notified by email when it is complete. Please check the data you have uploaded. ',
		buttonTextCancel: 'Ok'
		// TRUE if confirm pressed, FALSE if cancel pressed
	};

	async function submitBt() {
		isSubmitting = true;
		canSubmit = false;
		const res: submitResponceType = await submit(id);

		//console.log("submit",res);

		if (!res.success) {
			dispatch('error', { messages: res.errors.map((e) => e.issue) });
		} else {
			if (res.asyncUpload) {
				dispatch('success', { text: res.asyncUploadMessage });
			}
			// update store
			latestSubmitDate.set(Date.now());
			isSubmitting = false;
		}
	}

	// return a boolean value for 2 different cases for submit
	//1. upload files only
	//2. updload data with data structure
	function activateSubmit() {
		//check use case 1
		//console.log('🚀 ~ activateSubmit ~ model:', model);
		if (model.hasStructrue == false && model.files.length > 0) {
			return true;
		}

		//check use case 2
		if (model.hasStructrue == false && model.modifiedFiles?.length > 0) {
			return true;
		}

		//check use case 3
		console.log(
			'🚀 ~ activateSubmit ~ model.hasStructrue:',
			model.hasStructrue,
			model.deletedFiles
		);
		if (model.hasStructrue == false && model.deleteFiles?.length > 0) {
			return true;
		}

		//check use case 4
		if (
			model.hasStructrue == true &&
			model.files.length > 0 &&
			model.allFilesReadable &&
			model.isDataValid
		) {
			return true;
		}

		return false;
	}
</script>

{#await reload()}
	<PlaceHolderHookContent />
{:then m}
	<div class="flex gap-3 items-center">
		<button
			type="button"
			class="btn variant-filled-primary"
			disabled={!canSubmit || isSubmitting}
			on:click={() => modalStore.trigger(confirm)}>Submit</button
		>
		{#if isSubmitting}
			<div class="flex-none">
				<Spinner />
			</div>
		{/if}
	</div>
{:catch error}
	<ErrorMessage {error} />
{/await}
