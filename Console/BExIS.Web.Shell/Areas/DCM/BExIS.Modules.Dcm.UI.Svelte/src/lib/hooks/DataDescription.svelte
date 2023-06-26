<script lang="ts">

 import {getHookStart}  from '../../services/HookCaller'
 import { latestFileUploadDate, latestDataDescriptionDate } from '../../routes/edit/stores';

 import { onMount }from 'svelte'

import TimeDuration from '../../lib/components/utils/TimeDuration.svelte'
import Generate from '../../lib/components/datadescription/Generate.svelte'
import Show from '../../lib/components/datadescription/Show.svelte'
import {Spinner} from '@bexis2/bexis2-core-ui'

import type {DataDescriptionModel} from '../../models/DataDescription'


 
export let id=0;
export let version=1;
export let hook;
 
let model:DataDescriptionModel;
$:model;
$:loading = false;

$:$latestFileUploadDate, reload()
$:$latestDataDescriptionDate, reload()
 
onMount(async () => {
  load();
})

async function load()
{
  console.log("datadscription",hook);
  model = await getHookStart(hook.start,id,version);
}

async function reload()
{
  console.log("reload datadscription");
  load();
} 
 

 
 </script>
 
 {#if model}
  {#if model.allFilesReadable==true}

    {#if model.lastModification}
    
    <TimeDuration milliseconds={Number(new Date(model.lastModification))}/>
    
    {/if}

    <!--if structure not exist go to generate view otherwise show structure-->
    {#if model && model.structureId > 0} 
      <!--show-->
      <Show {...model}></Show>
    {:else}
      <!--generate-->
      <Generate bind:files={model.readableFiles} {...model} on:selected={()=> latestDataDescriptionDate.set(Date.now())}></Generate>
    {/if}

    {#if loading}
      <Spinner/>
    {/if}
  {:else}
    <span>not available</span>
  {/if}
 {/if}