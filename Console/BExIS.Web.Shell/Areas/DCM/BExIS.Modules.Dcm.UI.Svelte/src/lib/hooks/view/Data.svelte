<script lang="ts">
	import type { DataModel } from "$models/Data";
	import { getHookStart } from "$services/HookCaller";
	import { onMount } from "svelte";
	import PlaceHolderHookContent from "../edit/placeholder/PlaceHolderHookContent.svelte";
	import PrimaryData from "$lib/components/data/PrimaryData.svelte";
	import FilesView from "$lib/components/data/FilesView.svelte";
	import Fa from "svelte-fa";
	import { faMaximize } from "@fortawesome/free-solid-svg-icons";
	import { goTo } from "$services/BaseCaller";
	import { ErrorMessage } from '@bexis2/bexis2-core-ui';


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

{#await load()}
	<PlaceHolderHookContent />
{:then result}
	{#if model.hasStructure}
		<div class="flex justify-between items-center">
			<h3 class="h3">Data</h3>
			<!-- svelte-ignore a11y-missing-attribute -->
			<a on:click={()=> goTo('/dcm/view/data?id='+id+'&version='+version, true)} title="Open data in new window" class="badge text-lg cursor-pointer"><Fa icon="{faMaximize}"/></a>
		</div>
		<PrimaryData id={model.id} />
	{:else if model.existingFiles && model.existingFiles.length > 0}
		<div class="flex justify-between items-center">
			<h3 class="h3">Data</h3>
		</div>
		<FilesView
			id={model.id}
			versionId={model.versionId}
			bind:files={model.existingFiles}
			bind:descriptionType={model.descriptionType}
			downloadMode="data"
		/>
	{/if}
{:catch error}
	<ErrorMessage {error} />
{/await}