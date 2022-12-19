<script>
import {FileUploader} from '@bexis2/bexis2-core-ui/src/lib/index';
import {getHookStart}  from '../../services/Caller'
import { onMount, createEventDispatcher }from 'svelte'
import FileOverview from '../../components/fileupload/FileOverview.svelte'
import {Spinner, Alert} from 'sveltestrap'
import TimeDuration from '../../components/utils/TimeDuration.svelte'

import { latestFileUploadDate } from '../../stores/editStores';

export let id=0;
export let version=1;
export let hook;

$:model = null;

onMount(async () => {
  load();
})

// action for fileupload
let start="";
let save='/dcm/fileupload/saveFileDescription';
let remove='/dcm/fileupload/removefile';
let submit = "/dcm/fileupload/upload";
let context = "fileupload";
let error = "";

$:loading = false;
$:existError = false;

const dispatch = createEventDispatcher();

async function load()
{
  model = await getHookStart(hook.start,id,version);
  start = hook.start;

  loading = false;
  
}

async function reload() 
{
  /*update store*/
  latestFileUploadDate.set(Date.now());

  /* load data*/
  load();

}


</script>

{#if model}
  {#if model.lastModification}

  <TimeDuration milliseconds={new Date(model.lastModification)}/>

  {/if}
    <FileUploader {id} {version} {context} data={model} {start} {submit} on:submited={reload} on:submit={()=>loading=true} on:error on:success/>
  {#if model.existingFiles}
    <FileOverview {id} files={model.existingFiles} descriptionType={model.descriptionType} {save} {remove} on:success={reload} />
  {/if}

  {#if loading}
    <Spinner color="info" size="sm" type ="grow" text-center />
  {/if}

{/if}