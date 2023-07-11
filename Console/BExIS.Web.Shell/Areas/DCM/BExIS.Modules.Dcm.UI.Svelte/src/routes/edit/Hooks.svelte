<script>
import HookContainer from '$lib/components/HookContainer.svelte';
import Hook from '$lib/components//Hook.svelte';
import Attachments from '$lib/hooks/Attachment.svelte'

import {Spinner} from '@bexis2/bexis2-core-ui';
import {onMount} from 'svelte'
	import { construct_svelte_component } from 'svelte/internal';


 export let id;
 export let version;
 export let hooks=[];

 $:seperateHooks(hooks);

let attachmentHook;

$:addtionalhooks= [];

function seperateHooks(hooks)
{
  console.log("h",hooks);
  hooks.forEach(element => {
  
     if(element.name == "attachments"){ attachmentHook = element;} 
     else
     {
      // console.log(element.name)
      addtionalhooks.push(element);
     }
    });
}

let hookColor = "bg-surface-200"
 
</script>

<div class="{hookColor}">
 {#if addtionalhooks} <!-- if hooks list is loaded render hooks -->

 <HookContainer {...attachmentHook}  let:errorHandler let:successHandler  color="{hookColor}" >
  <div slot="view">
    <Attachments {id} {version} hook = {attachmentHook}  on:error={(e)=> errorHandler(e)} on:success={(e)=> successHandler(e)} />
  </div>
</HookContainer>

 {#each addtionalhooks as hook}
  <HookContainer {...hook} color="{hookColor}">
    <div slot="view">
      <Hook {id} {version} {...hook} />
    </div>
  </HookContainer>
 {/each}
 
 {:else} <!-- while data is not loaded show a loading information -->
    <Spinner/>
 {/if}

</div> 

