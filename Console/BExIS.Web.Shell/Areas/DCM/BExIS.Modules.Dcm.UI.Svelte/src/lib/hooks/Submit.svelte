<script lang="ts">
	import { Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';
	import { getHookStart } from '$services/HookCaller';
	import { submit } from '../../routes/edit/services';
	import type { SubmitModel, submitResponceType } from '$models/SubmitModels';

	import { getModalStore } from '@skeletonlabs/skeleton';
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

	export let id = 0;
	export let version = 1;
	export let start = '';

	const dispatch = createEventDispatcher();

	let model: SubmitModel;
	$: model;

	// boolean to control if submit button should be enabled based on the content of the submit model
	let canSubmit: boolean = false;
	$: canSubmit;

	// boolean to control if submit is in progress to disable button and show loading state
	let isSubmitting: boolean = false;

	// boolean to prevent the "Done..." message from showing a second time after the store reload runs
	let hasSubmitted: boolean = false;

	// text for submit button and confirm modal, will be set based on the content of the submit model in the activateSubmit function
	let submitText = '';
	let confirmText = 'Please confirm if you wish to proceed.';

	// only run reactive reloads after initial load has completed
	let mounted = false;

	// loading state for initial load and reloads, to show spinner and avoid showing stale data while loading
	let loading = true;

	// timeout reference for debounced reloads to avoid multiple reloads in quick succession when multiple stores are updated
	let reloadTimeout: NodeJS.Timeout;

	// array of store values to trigger reactive reload when any of them changes, used in combination with the mounted flag to only trigger reloads after initial load is done
	$: storeTriggers = [
		$latestFileUploadDate,
		$latestDataDescriptionDate,
		$latestFileReaderDate,
		$latestValidationDate,
		$latestDataDate
	];


	$: if (mounted && storeTriggers.some((date) => date > 0)) {
		debouncedReload();
	}

	// function to debounce reloads when multiple store values are updated in quick succession, to avoid multiple reloads and only reload once after all updates are done
	function debouncedReload() {

		console.log('Sumbit debouncedReload..');

		// Clear any previous scheduled reload
		clearTimeout(reloadTimeout);

		// Schedule a new reload 50ms from now
		reloadTimeout = setTimeout(() => {
			console.log('Debounced reload triggered');
			reload();
		}, 50);
	}

	onMount(async () => {
		mounted = true;
		console.log("Sumbit 🚀 ~ onMount ~ mounted:", mounted)
		
	});

	// function to load the submit model from the server based on the hook start action and update the canSubmit state based on the content of the model
	async function reload() {
		//console.trace('reload submit', start, id, version);

		loading = true;
		//console.log('reload submit', start, id, version);
		//console.log('latestDataDate', latestDataDate);

		// Clear the model to show loading state (null is not working)
		model = {} as SubmitModel;

		canSubmit = false;
		// isSubmitting = false;
		//console.log(' before hook');

		model = await getHookStart(start, id, version);
		//console.log(' before activateSubmit', canSubmit);

		canSubmit = await activateSubmit();
		if (canSubmit) {
			hasSubmitted = false;
		}
		//console.log(' after activateSubmit', canSubmit);

		loading = false;
		// return model;
	}

	// function to open a confirm modal before submitting, the modal will call the submitBt function if the user confirms
	function openConfirmModal() {
		const confirm: ModalSettings = {
			type: 'confirm',
			title: submitText,
			body: confirmText,

			// TRUE if confirm pressed, FALSE if cancel pressed
			response: (r: boolean) => {
				if (r === true) {
					submitBt();
				}
			}
		};
		modalStore.trigger(confirm);
	}

	// function to call the submit API and handle the response, showing success or error messages based on the result and updating the stores to trigger reloads of the data after submit
	async function submitBt() {
		isSubmitting = true;
		canSubmit = false;
		hasSubmitted = false; // reset in case of retry
		const res: submitResponceType = await submit(id);

		//console.log("submit",res);

		if (!res.success) {
			dispatch('error', { messages: res.errors.map((e) => e.issue) });
		} else {
			if (res.asyncUpload) {
				dispatch('success', { text: res.asyncUploadMessage });
			}
			isSubmitting = false;
			setTimeout(() => {
				hasSubmitted = true;
				// update store
				latestSubmitDate.set(Date.now());
				// reload to update view after submit
			}, 1000);
		}
	}

	// function to determine if submit button should be enabled and set the submit button text based on the content of the submit model, this is a simplified example and should be adapted to the actual use cases and model content
	async function activateSubmit() {
		var returnValue = false;
		// File Upload without structure
		console.log('🚀 ~ activateSubmit ~ model:', model);
		if (model.hasStructrue == false && model.files.length > 0) {
			submitText = 'Import File(s)';
			returnValue = true;
		}

		// Update Description of a file
		if (model.hasStructrue == false && model.modifiedFiles?.length > 0) {
			if (submitText.includes('Import File(s)')) {
				submitText = 'Import and Update Description';
			} else {
				submitText = 'Update Description';
				returnValue = true;
			}
		}

		// Delete Files
		if (model.hasStructrue == false && model.deleteFiles?.length > 0) {
			if (submitText.includes('Import File(s)') || submitText.includes('Update Description')) {
				submitText += ' and Delete File(s)';
			} else {
				submitText = 'Delete File(s)';
			}
			return true;
		}

		// File Upload with structure
		if (
			model.hasStructrue == true &&
			model.files.length > 0 &&
			model.allFilesReadable &&
			model.isDataValid
		) {
			if (
				submitText.includes('Import File(s)') ||
				submitText.includes('Update Description') ||
				submitText.includes('Delete File(s)')
			) {
				submitText += ' and Start import/update of tabular data';
			} else {
				submitText = 'Start adding/update of tabular data';
			}
			((confirmText =
				'Editing will be disabled until the import is complete. If you are importing a large amount of tabular data it may take a while. You will be notified by email when it is complete. <br><br> Once the import is complete, please check the imported data. '),
				(returnValue = true));
		}

		return returnValue;
	}
</script>

{#if loading}
	<!-- <PlaceHolderHookContent /> remove to avoid to much layout shift-->
{:else}
	{#if !isSubmitting && canSubmit}
		<div class="mb-2">
			{#if canSubmit && model.modifiedFiles?.length > 0}
				<div
					class="flex items-center gap-1 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200"
					role="status"
				>
					<span class="sr-only">Info:</span>
					Description of
					{#each model.modifiedFiles as file, index}
						<b>{file.name}</b>{#if index < model.modifiedFiles.length - 1},
						{/if}
					{/each}
					has been modified.
				</div>
			{/if}
		</div>
		<div class="mb-2">
			{#if model.hasStructrue == false && model.files.length > 0}
				<div
					class="flex items-center gap-1 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200"
					role="status"
				>
					<span class="sr-only">Info:</span>
					The following file(s) are ready for import:
					{#each model.files as file, index}
						<b>{file.name}</b>{#if index < model.files.length - 1},
						{/if}
					{/each}
				</div>
			{/if}
		</div>
		<div class="mb-2">
			{#if model.hasStructrue == false && model.deleteFiles?.length > 0}
				<div
					class="flex items-center gap-1 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200"
					role="status"
				>
					<span class="sr-only">Warning:</span>
					The following file(s) will be deleted:
					{#each model.deleteFiles as file, index}
						<b>{file.name}</b>{#if index < model.deleteFiles.length - 1},
						{/if}
					{/each}
				</div>
			{/if}
		</div>
		<div class="mb-2">
			{#if model.hasStructrue == true && model.files.length > 0 && model.allFilesReadable && model.isDataValid}
				<div
					class="flex items-center gap-1 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200"
					role="status"
				>
					<span class="sr-only">Info:</span>
					Data from the following file(s) will be added or updated based on the primary key defined in
					the data structure:

					{#each model.files as file, index}
						<b>{file.name}</b>{#if index < model.files.length - 1},
						{/if}
					{/each}
				</div>
			{/if}
		</div>
	{/if}
	<div class="flex gap-3 items-center">
		{#if !isSubmitting && !canSubmit && !hasSubmitted}
			<div
				class="pt-2 variant-ghost-warning warning border-l-4 border-warning-500 p-2 text-warning-800 dark:text-warning-200"
				role="status"
			>
				<span class="sr-only">Info:</span>
				<b>Info:</b> Done. Please wait for the view to update.
			</div>
		{:else if !hasSubmitted}
			<button
				type="button"
				class="btn variant-filled-primary"
				disabled={!canSubmit || isSubmitting}
				on:click={openConfirmModal}>{submitText}</button
			>
		{/if}
		{#if isSubmitting && !canSubmit}
			<div class="flex-none">
				<Spinner />
			</div>
		{/if}
	</div>
{/if}
