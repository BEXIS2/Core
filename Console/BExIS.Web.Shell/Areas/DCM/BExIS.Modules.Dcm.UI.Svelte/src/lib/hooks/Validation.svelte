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
	} from '../../routes/edit/stores';
	
	import { hooksStatus } from '../../routes/edit/stores';
	import { onMount } from 'svelte';

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

	onMount(async () => {
		latestFileUploadDate.subscribe((s) => {
			if (s > 0) {
				console.log(
					'🚀 ~ file: Validation.svelte:37 ~ onMount ~ latestFileUploadDate:',
					$latestFileUploadDate
				);
				reload('latestFileUploadDate');
			}
		});
		latestDataDescriptionDate.subscribe((s) => {
			if (s > 0) {
				console.log(
					'🚀 ~ file: Validation.svelte:41 ~ onMount ~ latestDataDescriptionDate:',
					$latestDataDescriptionDate
				);
				reload('latestDataDescriptionDate');
			}
		});
		latestFileReaderDate.subscribe((s) => {
			if (s > 0) {
				console.log(
					'🚀 ~ file: Validation.svelte:45 ~ onMount ~ latestFileReaderDate:',
					$latestFileReaderDate
				);
				reload('latestFileReaderDate');
			}
		});
		latestSubmitDate.subscribe((s) => {
			if (s > 0) {
				console.log(
					'🚀 ~ file: Validation.svelte:49 ~ onMount ~ latestSubmitDate:',
					$latestSubmitDate
				);
				//reload('latestSubmitDate');
			}
		});
	});

	async function reload(type) {
	
			model = null;
			model = await getHookStart(start, id, version);
			console.log('validation end', model?.isValid);
			latestValidationDate.set(Date.now());
		
	}
</script>

{#await reload('await')}
	<PlaceHolderHookContent />
{:then a}
	{#if model && model.fileResults}
		{#each model.fileResults as fileResult}
			<ValidationResult bind:sortedErrors={fileResult.sortedErrors} bind:file={fileResult.file} />
		{/each}
	{/if}
{:catch error}
	<ErrorMessage {error} />
{/await}
