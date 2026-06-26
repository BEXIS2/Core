<script lang="ts">
	import { onMount } from "svelte";
	import { getVersions } from "../services";

	import type { versionListItemType } from "../types";
import {fade} from "svelte/transition";

export let id: number;
export let version: number;


let currentVersion: versionListItemType | undefined = undefined;
let versions: versionListItemType[] = [];
let  showVersions: boolean = false;


onMount(async () => {
 console.log('id', id);
 console.log('version', version);

 const res = await getVersions(id, version);
 versions = res;
 console.log("🚀 ~ versions:", versions)


 currentVersion = versions.find(v => v.id === version);

});


</script>




<div class="card p-5  flex flex-col gap-2">
 <h4 class="h4">Versions</h4>

 <div class="flex">
  <b class="grow" title={currentVersion?.description}>Version {currentVersion?.id}</b>
  {currentVersion?.date}
 </div>

 <div class="flex text-right">
   <div class="grow"></div>
  <button class="chip p-0" on:click={() => showVersions = !showVersions}>Show other versions</button>
 </div>

{#if showVersions}
 	<div class="flex flex-col gap-2" transition:fade >
 		{#each versions.filter(v => v.id !== currentVersion?.id) as v, i}
 			<div class="flex justify-between">
 				<div title={v.description}>
      <a href="/dcm/view?id={id}&version={v.id}" target="_blank">
 					Version {v.id}
      </a>
 				</div>
 				<div>
 					{v.date}
 				</div>
       
 			</div>
 		{/each}
 	</div>

 {/if}

</div>