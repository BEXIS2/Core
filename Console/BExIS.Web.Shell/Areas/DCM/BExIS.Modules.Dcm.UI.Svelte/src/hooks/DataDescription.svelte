<script>

 import {getHookStart}  from '../../services/Caller'
 import { latestFileUploadDate, latestDataDescriptionDate } from '../../stores/editStores';

 import { onMount }from 'svelte'

import TimeDuration from '../../components/utils/TimeDuration.svelte'
import Generate from '../../components/datadescription/Generate.svelte'
import Show from '../../components/datadescription/Show.svelte'
import {Spinner} from 'sveltestrap'


 
export let id=0;
export let version=1;
export let hook;
 
$:model = null;
$:loading = false;

$:$latestFileUploadDate, reload()
$:$latestDataDescriptionDate, reload()
 
onMount(async () => {
  load();
})

async function load()
{
  //console.log("datadscription",hook);
  model = await getHookStart(hook.start,id,version);
}

async function reload()
{
  //console.log("reload datadscription");
  load();
} 
 
 
 </script>
 
 {#if model}
  {#if model.allFilesReadable==true}

    {#if model.lastModification}
    
    <TimeDuration milliseconds={new Date(model.lastModification)}/>
    
    {/if}

    <!--if structure not exist go to generate view otherwise show structure-->
    {#if model && model.structureId > 0} 
      <!--show-->
      <Show {...model}></Show>
    {:else}
      <!--generate-->
      <Generate bind:files={model.readableFiles} {...model}></Generate>
    {/if}

    {#if loading}
      <Spinner color="info" size="sm" type ="grow" text-center />
    {/if}
  {:else}
    <span>not available</span>
  {/if}
 {/if}