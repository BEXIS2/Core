<script lang="ts">
import { onMount, createEventDispatcher }from 'svelte'

import {FileUploader, Spinner, ErrorMessage} from '@bexis2/bexis2-core-ui';
import {getHookStart}  from '$services/HookCaller'

import FileOverview from '$lib/components/fileupload/FileOverview.svelte'
import TimeDuration from '$lib/components/utils/TimeDuration.svelte'
import FileReaderInformation from '$lib/components/fileupload/FileReaderInformation.svelte'


import { latestFileUploadDate } from '../../routes/edit/stores';

import type { FileUploadModel } from '$models/FileUpload'

export let id=0;
export let version=1;
export let hook;


// action for fileupload
let start="";
let save='/dcm/fileupload/saveFileDescription';
let remove='/dcm/fileupload/removefile';
let submit = "/dcm/fileupload/upload";
let context = "fileupload";
let error = "";

let model:FileUploadModel;
$:model;

onMount(async () => {
  load();
})



$:loading = false;
$:existError = false;

const dispatch = createEventDispatcher();

async function load()
{
  model = await getHookStart(hook.start,id,version);
  start = hook.start;

  loading = false;

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

{#await load()}
  <div class="text-surface-800">
    <Spinner label="loading File Uploader" />
  </div>

{:then result}

{#if model.lastModification}
  <TimeDuration milliseconds={new Date(model.lastModification)}/>
{/if}

  <FileUploader {id} {version} {context} data={model.fileUploader} {start} {submit} on:submited={reload} on:submit={()=>loading=true } on:error on:success />

{#if model.fileUploader.existingFiles}
  <FileOverview {id} files={model.fileUploader.existingFiles} descriptionType={model.fileUploader.descriptionType} {save} {remove} on:success={success} on:warning={warning}/>
{/if}

<FileReaderInformation {id} readableFiles={model.fileUploader.existingFiles} asciiFileReaderInfo = {model.asciiFileReaderInfo}/>

{:catch error} 
  <ErrorMessage {error}/>
{/await}