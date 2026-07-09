<script lang="ts">
	import Show from "$lib/components/datadescription/Show.svelte";
import type { DataDescriptionModel } from "$models/DataDescription";
	import { getHookStart } from "$services/HookCaller";
	import { onMount } from "svelte";


	export let id = 0;
	export let version = 1;
	export let hook;

 let model: DataDescriptionModel;
 $:model;

 onMount(async () => {
  
		model = await getHookStart(hook.start, id, version);
		
	});

</script>

<h3 class="h3">Data Description</h3>

<div class="card p-5 mb-5">
 {#if model && model.isStructured}
 <Show {...model} />
 {/if}
 

</div>