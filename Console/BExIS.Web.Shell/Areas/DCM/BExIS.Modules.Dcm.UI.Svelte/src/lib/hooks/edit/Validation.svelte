<script lang="ts">
	import { Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';

	import ValidationResult from '$lib/components/validation/ValidationResult.svelte';

	import { getHookStart } from '$services/HookCaller';
	import {
		latestFileUploadDate,
		latestDataDescriptionDate,
		latestFileReaderDate,
		latestSubmitDate,
		latestValidationDate
	} from '../../../routes/edit/stores';

	import { hooksStatus } from '../../../routes/edit/stores';
	import { onMount, onDestroy } from 'svelte';

	import type { ValidationModel } from '$models/ValidationModels';
	import PlaceHolderHookContent from './placeholder/PlaceHolderHookContent.svelte';
	import { get } from 'svelte/store';

	export let id = 0;
	export let version = 1;
	export let status = 0;
	export let displayName = '';
	export let start = '';
	export let description = '';

	let model: ValidationModel | null;
	$: model;

	let validationPromise: Promise<any>;

	const unsubFileUpload = latestFileUploadDate.subscribe((s) => {
			if (s > 0) {
				console.log(
					'🚀 ~ file: Validation.svelte:37 ~ onMount ~ latestFileUploadDate:',
					$latestFileUploadDate
				);
				validationPromise = reload('latestFileUploadDate');
			}
		});

	const unsubDataDescription = latestDataDescriptionDate.subscribe((s) => {
			if (s > 0) {
				console.log(
					'🚀 ~ file: Validation.svelte:41 ~ onMount ~ latestDataDescriptionDate:',
					$latestDataDescriptionDate
				);
				validationPromise = reload('latestDataDescriptionDate');
			}
		});

	const unsubFileReader = latestFileReaderDate.subscribe((s) => {
			if (s > 0) {
				console.log(
					'🚀 ~ file: Validation.svelte:45 ~ onMount ~ latestFileReaderDate:',
					$latestFileReaderDate
				);
				validationPromise = reload('latestFileReaderDate');
			}
		});

	onMount(async () => {
		validationPromise = reload('await');
	});

	onDestroy(() => {
		unsubFileUpload();
		unsubDataDescription();
		unsubFileReader();
	});

	async function reload(type) {
		model = null;
		model = await getHookStart(start, id, version);
		console.log('validation end', model);
		latestValidationDate.set(Date.now());
	}
</script>

{#if validationPromise}
	{#await validationPromise}
		<PlaceHolderHookContent />
	{:then a}
		{#if model && model.fileResults}
			{#each model.fileResults as fileResult}
				<ValidationResult
					bind:sortedErrors={fileResult.sortedErrors}
					bind:sortedWarnings={fileResult.sortedWarnings}
					bind:file={fileResult.file}
				/>
			{/each}
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
{/if}
