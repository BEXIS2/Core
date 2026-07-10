<script lang="ts">
	import Show from "$lib/components/datadescription/Show.svelte";
 import type { DataDescriptionModel } from "$models/DataDescription";
	import { getHookStart } from "$services/HookCaller";
	import { onMount } from "svelte";
	import Fa from "svelte-fa";
	import { faMaximize } from "@fortawesome/free-solid-svg-icons";
	import { goTo, goToEntity } from "$services/BaseCaller";


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


<div class="flex justify-between items-center">
	<h3 class="h3">Data Description</h3>
	<div class="flex justify-end">
		<a href="datadescription?id={id}" class="badge text-lg"><Fa	icon="{faMaximize}"/></a>
	</div>
</div>

<div class="card p-5 mb-5">
 
	
		{#if model && model.isStructured}
			<Show {...model} />
		{/if}
</div>