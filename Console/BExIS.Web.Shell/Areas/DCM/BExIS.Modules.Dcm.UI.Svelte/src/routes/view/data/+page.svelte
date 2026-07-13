<script lang="ts">

	import { Page, pageContentLayoutType } from "@bexis2/bexis2-core-ui";
	import PrimaryData from "$lib/components/data/PrimaryData.svelte";
	import { onMount } from "svelte";
	import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";
	import Fa from "svelte-fa";
	import Back from "$lib/components/utils/Back.svelte";
 


export let id = 0;
export let version = 1;
export let	versionId	= -1;

	onMount(async () => {

	 let container;
  container = document.getElementById('data');
		id = container?.getAttribute('dataset');
		version = container?.getAttribute('version');
		versionId = container?.getAttribute('versionId');


	});


</script>

<Page title="Data of entity ({id})" contentLayoutType={pageContentLayoutType.full}>

	<Back	/>
 
	{#if id	> 0}
		<PrimaryData {id} {version} />
	{:else}
		<p class="text-center text-lg">No data available</p>
	{/if}

</Page>