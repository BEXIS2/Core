<script lang="ts">
	import { GetMetadataOverview } from "$services/MetadataCaller";
	import { Api } from "@bexis2/bexis2-core-ui";
	import { onMount } from "svelte";


export let id = 0;
export let version = 0;
export let tag = 0;
let lastmodified = '';
let lastchanger = '';
let useTags:Boolean = false;

$:{
 lastmodified;
 lastchanger;
 useTags;
 id;
 version;
}



onMount(async () => {

 const res = await GetMetadataOverview(id, version, tag);
 if (res) { 
  
  lastmodified = res.lastModified;
  lastchanger = res.lastChanger;
  useTags = res.useTags;
  version = res.version;
  id= res.id;
  console.log("🚀 ~ onMount ~ useTags:", useTags)
  console.log("🚀 ~ onMount ~ lastmodified:", lastmodified)
  console.log("🚀 ~ onMount ~ lastchanger:", lastchanger)

  
 }

})


</script>

<div class="flex flex-col gap-2">

 {#if useTags}
 <p>Current tag: {tag}</p>
 {:else}
 <p>Current version: {version}</p>
 {/if}
 <p>Dataset ID: {id}</p>
 <p>Last modified: {lastmodified}</p>
 <p>Modified by: {lastchanger}</p>
</div>

