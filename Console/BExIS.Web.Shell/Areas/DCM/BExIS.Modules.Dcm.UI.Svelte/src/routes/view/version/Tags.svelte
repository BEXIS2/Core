<script lang="ts">
	import { onMount } from "svelte";
	import { getTags } from "../services";
	import type { TagInfoViewModel } from "../types";


export let id: number;
export let version: number;
export let tag:number;
export let tags: TagInfoViewModel[] = [];

let currentTag:TagInfoViewModel|undefined = undefined;
let showTags:boolean = false;
$:showTags
onMount(async () => {
 console.log('id', id);
 console.log('version', version);

 const res = await getTags(id, version);
 tags = res;

 console.log("🚀 ~ tags:", tags)


 currentTag = tags.find(t => t.version === tag);

 if(currentTag === undefined && tags.length > 0){
 	currentTag = tags[0];
 }


});

</script>
<div class="card p-5 flex flex-col gap-2">
 <h4 class="h4">Tags</h4>

 <div class="flex">
  <b class="grow" title="{currentTag?.releaseNotes.join(', ')}">Tag {currentTag?.version}</b>
  {currentTag?.releaseDate ? new Date(currentTag.releaseDate).toLocaleDateString() : 'N/A'}
 </div>

  <div class="flex text-right">
   <div class="grow"></div>
   <button class="chip p-0" on:click={() => showTags = !showTags}>Show other tags</button>
 </div>
{#if showTags}
 	<div class="flex flex-col gap-2">
 		{#each tags.filter(v => v.version !== currentTag?.version) as v, i}
 			<div class="flex justify-between">
 				<div title={v.releaseNotes.join(', ')}>
      <a href="/dcm/view?id={id}&tag={v.version}" target="_blank">
 					Tag {v.version}
      </a>
 				</div>
 				<div>
 					{new Date(v.releaseDate).toLocaleDateString()}
 				</div>
 			</div>
 		{/each}
 	</div>

 {/if}



</div>