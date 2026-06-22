<script lang="ts">
	import { getHookStart } from '$services/HookCaller';
	import { latestFileUploadDate, latestDataDescriptionDate } from '../../routes/edit/stores';
	import { onMount, createEventDispatcher } from 'svelte';

	import Generate from '$lib/components/datadescription/Generate.svelte';
	import Show from '$lib/components/datadescription/Show.svelte';
	import { Spinner, ErrorMessage, positionType } from '@bexis2/bexis2-core-ui';

	import type { DataDescriptionModel } from '$models/DataDescription';
	import PlaceHolderHookContent from './placeholder/PlaceHolderHookContent.svelte';

	export let id = 0;
	export let version = 1;
	export let hook;

	let model: DataDescriptionModel;
	$: model;
	$: loading = false;
	let isMounted = false; // Flag to track if the component has been mounted

	// Reactive statement: Only runs if isMounted is true AND the store changes
    $: if (isMounted && $latestFileUploadDate > 0) {
        reloadByFileUpdate();
    }

    $: if (isMounted && $latestDataDescriptionDate > 0) {
        load();
    }

	let errorMessage: any = null;
	const dispatch = createEventDispatcher();

	onMount(async () => {
		await load();
		isMounted = true;
	});

	async function load() {
		loading = true;
		try {
            model = await getHookStart(hook.start, id, version);
            dispatch('dateChanged', { lastModification: model.lastModification });
        } catch (error) {
            console.error("Failed to fetch data description:", error);
			errorMessage = error;
        } finally {
            loading = false;
        }
    }

    async function reloadByFileUpdate() {
        if (model && model.structureId === 0) {
            await load();
        }
	}

</script>

{#if loading}
	<!-- <div class="w-full h-full text-surface-600">
		<Spinner label="loading data description" position={positionType.start} />
	</div> -->
	<PlaceHolderHookContent />
{:else if model}
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
{:else if errorMessage}
	<ErrorMessage error={errorMessage} />
{/if}
