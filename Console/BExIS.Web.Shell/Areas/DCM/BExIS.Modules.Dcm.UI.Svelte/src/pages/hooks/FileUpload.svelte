<script>
import {FileUploader} from '@bexis2/svelte-bexis2-core-ui';
import {getHookStart}  from '../../services/Caller'


import { onMount }from 'svelte'
import { Button, Spinner } from 'sveltestrap';

import FileOverview from '../../components/fileupload/FileOverview.svelte'

export let id=0;
export let version=1;
export let hook;

$:model = null;

onMount(async () => {
  load();
})

// for fileUploader
let start="";
let submit="";
let saveDescription="";
let removeFile="";


async function load()
{
  model = await getHookStart(hook.start,id,version);
  start = hook.start;
  submit = "/dcm/fileupload/upload";



  console.log(model);
  console.log(start);
  console.log(submit);
}

</script>

{#if model}

<FileUploader {id} {version} data={model} {start} {submit} on:submit={load}/>

{#if model.ExistingFiles}
  <FileOverview {id} files={model.ExistingFiles} {saveDescription} {removeFile} />
{/if}

{/if}