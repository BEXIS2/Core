<script lang="ts">
	import Show from "$lib/components/datadescription/Show.svelte";
  import type { DataDescriptionModel } from "$models/DataDescription";
	import { getHookStart } from "$services/HookCaller";
	import { onMount } from "svelte";
	import Fa from "svelte-fa";
	import { faMaximize } from "@fortawesome/free-solid-svg-icons";
	import { goTo } from "$services/BaseCaller";
	import PlaceHolderHookContent from "../edit/placeholder/PlaceHolderHookContent.svelte";
	import { ErrorMessage } from '@bexis2/bexis2-core-ui';

	export let id = 0;
	export let version = 0;
	export let tag = 0;
	export let hook;

  let model: DataDescriptionModel;
  $:model;

  onMount(async () => {
 		model = await getHookStart(hook.start, id, version);
 	});
</script>

{#if model && model.isStructured}
	<div class="flex justify-between items-center">
		<h3 class="h3">Data Description</h3>
		<!-- svelte-ignore a11y-missing-attribute -->
		<a on:click={()=> goTo('/dcm/view/datadescription?id='+id+'&version='+version, true)} class="badge text-lg cursor-pointer"><Fa icon="{faMaximize}"/></a>
	</div>
	<Show {...model} />
{/if}