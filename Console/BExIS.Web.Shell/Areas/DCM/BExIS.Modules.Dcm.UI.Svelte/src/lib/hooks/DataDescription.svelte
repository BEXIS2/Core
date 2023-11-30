<script lang="ts">
	import { getHookStart } from '$services/HookCaller';
	import { latestFileUploadDate, latestDataDescriptionDate } from '../../routes/edit/stores';
	import { onMount, createEventDispatcher } from 'svelte';

	import TimeDuration from '$lib/components/utils/TimeDuration.svelte';
	import Generate from '$lib/components/datadescription/Generate.svelte';
	import Show from '$lib/components/datadescription/Show.svelte';
	import { Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';

	import type { DataDescriptionModel } from '$models/DataDescription';
	import PlaceholderHook from './placeholder/PlaceholderHook.svelte';
	import PlaceHolderHookContent from './placeholder/PlaceHolderHookContent.svelte';

	export let id = 0;
	export let version = 1;
	export let hook;

	let model: DataDescriptionModel;
	$: model;
	$: loading = false;

	$: $latestFileUploadDate, reloadByFileUpdate();
	$: $latestDataDescriptionDate, reload();

	const dispatch = createEventDispatcher();

	onMount(async () => {
		load();
	});

	async function load() {
		model = await getHookStart(hook.start, id, version);

		dispatch('dateChanged', { lastModification: model.lastModification });
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
	<!-- <div class="w-full h-full text-surface-600">
		<Spinner label="loading data description" position={positionType.start} />
	</div> -->
	<PlaceHolderHookContent />
{:then a}
	<!--if structure not exist go to generate view otherwise show structure-->
	{#if model && model.structureId > 0}
		<!--show-->
		<Show {...model} on:error />
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
