<script lang="ts">
import { onMount, createEventDispatcher }from 'svelte'

import {FileUploader, Spinner} from '@bexis2/bexis2-core-ui';
import {getHookStart}  from '../../services/HookCaller'

import FileOverview from '$lib/components/fileupload/FileOverview.svelte'
import TimeDuration from '$lib/components/utils/TimeDuration.svelte'

import { latestFileUploadDate } from '../../routes/edit/stores';

import type { fileUploaderModel } from '@bexis2/bexis2-core-ui'

export let id=0;
export let version=1;
export let hook;

let model:fileUploaderModel;
$:model;

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

  console.log("FileUploadHook",model)
  
}

async function reload(e) 
{
  /*update store*/
  latestFileUploadDate.set(Date.now());

  /* load data*/
  load();
}

function success(e)
{
  console.log("success");
  reload(e);
  dispatch("success",{text:e.detail.text})
}

function warning(e)
{
  console.log("warning");
  reload(e);
  dispatch("warning",{text:e.detail.text})
}


</script>

{#if model != undefined}
  {#if model.lastModification}

  <TimeDuration milliseconds={new Date(model.lastModification).getTime()}/>

  {/if}
    <FileUploader {id} {version} {context} data={model} {start} {submit} on:submited={reload} on:submit={()=>loading=true} on:error on:success/>
  {#if model.existingFiles}
    <FileOverview {id} files={model.existingFiles} descriptionType={model.descriptionType} {save} {remove} on:success={success} on:warning={warning} />
  {/if}

  {#if loading}
    <Spinner />
  {/if}

{/if}