<script lang="ts">
	import type { Hook } from "../../models/View";
 import { goTo } from '../../services/BaseCaller';

 export let id: number;
 export let version: number;
 export let hooks: Hook[];

</script>

<div class="flex-col gap-3 border-solid border-2 border-gray-200 rounded-md p-3 mb-5 overflow-x-auto">	
 
{#if hooks && hooks.length > 0}
  <div class="pb-2"><b>more details</b></div>
  <div class="flex flex-wrap gap-2">
   {#each hooks as hook}
   <div class="pb-1">
    <button class="badge variant-filled-primary" title="{hook.description}" on:click={()=> goTo(hook.start+ '?id=' + id + '&version=' + version, false) }>{hook.name}</button>
   </div>
   {/each}
  </div>
{:else}
 <p>No hooks available.</p>
{/if}
</div>
