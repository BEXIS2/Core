<script lang="ts">
	import { Spinner, ErrorMessage } from '@bexis2/bexis2-core-ui';
	import ValidationResult from '$lib/components/validation/ValidationResult.svelte';

	import { getHookStart } from '../../services/HookCaller';
	import { latestFileUploadDate, latestDataDescriptionDate, latestFileReaderDate } from '../../routes/edit/stores';
	import { onMount } from 'svelte';

	import type { ValidationModel } from '$models/ValidationModels';


	export let id = 0;
	export let version = 1;
	export let status = 0;
	export let displayName = '';
	export let start = '';
	export let description = '';

	let model: ValidationModel;
	$: model;

	$: $latestFileUploadDate, reload();
	$: $latestDataDescriptionDate, reload();
	$: $latestFileReaderDate, reload();

	onMount(async () => {
		reload();
	});

	async function reload() {
		//const res = await fetch(url);
		model = await getHookStart(start, id, version);
		console.log('validation', model);
	}


</script>

{#await reload()}
	<div class="w-full h-full text-surface-600">
		<Spinner label="...validate data" />
	</div>
{:then a}
	{#if model &&  model.fileResults}
		{#each model.fileResults as fileResult}
			<ValidationResult bind:sortedErrors={fileResult.sortedErrors} bind:file={fileResult.file} />
		{/each}
	{/if}
{:catch error}
	<ErrorMessage {error} />
{/await}
