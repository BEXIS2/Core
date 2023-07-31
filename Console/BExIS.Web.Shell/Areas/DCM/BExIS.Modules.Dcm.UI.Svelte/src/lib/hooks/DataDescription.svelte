<script lang="ts">
	import { getHookStart } from '$services/HookCaller';
	import { latestFileUploadDate, latestDataDescriptionDate } from '../../routes/edit/stores';
	import { onMount } from 'svelte';

	import TimeDuration from '$lib/components/utils/TimeDuration.svelte';
	import Generate from '$lib/components/datadescription/Generate.svelte';
	import Show from '$lib/components/datadescription/Show.svelte';
	import { Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';

	import type { DataDescriptionModel } from '$models/DataDescription';

	export let id = 0;
	export let version = 1;
	export let hook;

	let model: DataDescriptionModel;
	$: model;
	$: loading = false;

	$: $latestFileUploadDate, reloadByFileUpdate();
	$: $latestDataDescriptionDate, reload();

	onMount(async () => {
		load();
	});

	async function load() {
		model = await getHookStart(hook.start, id, version);
  console.log("DataDescriptionModel",model);
		
	}

	async function reload() {
		load();
	}

	async function reloadByFileUpdate() {
		// only when strutcure is not set update model
		if (model && model.structureId == 0) {
			load();
		}
	}
</script>

{#await getHookStart(hook.start, id, version)}
	<div class="w-full h-full text-surface-600">
		<Spinner label="loading data description" position="{positionType.start}" />
	</div>
{:then a}
	{#if model && model.lastModification}
		<TimeDuration milliseconds={Number(new Date(model.lastModification))} />
	{/if}

	<!--if structure not exist go to generate view otherwise show structure-->
	{#if model && model.structureId > 0}
		<!--show-->
		<Show {...model} />
	{:else if model && model.allFilesReadable == true}
		<!--generate-->
		<!-- <Generate bind:files={model.readableFiles} {...model} on:selected={()=> latestDataDescriptionDate.set(Date.now())} isRestricted={model.isRestricted}></Generate> -->
		<Generate
			{id}
			{version}
			{model}
			{hook}
			on:selected={() => latestDataDescriptionDate.set(Date.now())}
		/>
	{:else}
		<span>not available</span>
	{/if}
{:catch error}
	<ErrorMessage {error} />
{/await}
