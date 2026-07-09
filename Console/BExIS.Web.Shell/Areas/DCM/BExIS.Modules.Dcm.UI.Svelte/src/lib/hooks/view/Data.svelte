<script lang="ts">
	import type { DataModel } from "$models/Data";
	import { getHookStart } from "$services/HookCaller";
	import { onMount } from "svelte";
	import PlaceHolderHookContent from "../edit/placeholder/PlaceHolderHookContent.svelte";
	import PrimaryData from "$lib/components/data/PrimaryData.svelte";
	import Files from "$lib/components/data/Files.svelte";


 export let id = 0;
	export let version = 1;
	export let hook;
 
	let model: DataModel;
 
	onMount(async () => {
  load();
	});

 async function load() {
		model = await getHookStart(hook.start, id, version);
 }

</script>

<h3 class="h3">Data</h3> 

<div class="card p-5 mb-5">
 {#await load()}
		<PlaceHolderHookContent />
	{:then result}
		{#if model.hasStructure}
			<PrimaryData id={model.id} />
		{:else}
			<Files
				id={model.id}
				bind:files={model.existingFiles}
				bind:deletedFiles={model.deleteFiles}
				bind:descriptionType={model.descriptionType}
			/>
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</div>