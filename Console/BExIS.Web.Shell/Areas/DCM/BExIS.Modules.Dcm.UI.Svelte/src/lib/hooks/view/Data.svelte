<script lang="ts">
	import type { DataModel } from "$models/Data";
	import { getHookStart } from "$services/HookCaller";
	import { onMount } from "svelte";
	import PlaceHolderHookContent from "../edit/placeholder/PlaceHolderHookContent.svelte";
	import PrimaryData from "$lib/components/data/PrimaryData.svelte";
	import FilesView from "$lib/components/data/FilesView.svelte";
	import Fa from "svelte-fa";
	import { faMaximize } from "@fortawesome/free-solid-svg-icons";


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

 <div class="flex justify-between items-center">
 <h3 class="h3">Data</h3> 

 <div class="flex justify-end">
			<a href="data?id={id}&version={version}" title="Open data in new window" class="badge text-lg"><Fa	icon="{faMaximize}"/></a>
	</div>

</div>
<div class="card p-5 mb-5">
 {#await load()}
		<PlaceHolderHookContent />
	{:then result}
		{#if model.hasStructure}

		
			<PrimaryData id={model.id} />
		{:else}
			<FilesView
				id={model.id}
				bind:files={model.existingFiles}
				bind:descriptionType={model.descriptionType}
			/>
		{/if}
	{:catch error}
		<ErrorMessage {error} />
	{/await}
</div>