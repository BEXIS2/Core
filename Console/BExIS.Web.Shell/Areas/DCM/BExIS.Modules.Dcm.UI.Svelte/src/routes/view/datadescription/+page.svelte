<script lang="ts">
	import Show from "$lib/components/datadescription/Show.svelte";
 import type { DataDescriptionModel } from "$models/DataDescription";
	import { onMount } from "svelte";
	import { getDataDescription } from "../services";
	import { Page } from "@bexis2/bexis2-core-ui";
	import Fa from "svelte-fa";
	import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";
	import Back from "$lib/components/utils/Back.svelte";


	export let id = 0;
	export let version = 1;


 let model: DataDescriptionModel;
 $:model;

 onMount(async () => {


		 
	 let container;
  container = document.getElementById('datadescription');
		id = container?.getAttribute('dataset');
		alert(id +	" " + version)
		model = await getDataDescription(id, version);
		console.log("🚀 ~ model:", model)
	});

</script>

<Page title="Data Description of entity ({id})">

 <Back	/>

 {#if model && model.isStructured}
   <Show {...model} />
 {/if}
 
</Page>