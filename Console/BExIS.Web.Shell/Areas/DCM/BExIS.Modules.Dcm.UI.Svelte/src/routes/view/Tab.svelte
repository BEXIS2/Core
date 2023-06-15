<script lang="ts">
import { onMount } from 'svelte'; 
import {getHookStart}  from '../../services/HookCaller'

import { Spinner } from '@bexis2/bexis2-core-ui';


//entity infos
export let id;
export let version;

//entity hook infos
export let name;
export let displayName;
export let start

// tab pane properties
export let active:boolean;



$:content="";

onMount(async () => {

console.log(name);

// content = await getHookStart(start,id,version);

})

</script>

{#if active}
{displayName}
<div id="{name}" > 
{#if content}
 <div >
  <!-- {@html content} -->
  {displayName}
 </div>
 {:else} <!-- while data is not loaded show a loading information -->

  <Spinner />

{/if}
</div>
{/if}


