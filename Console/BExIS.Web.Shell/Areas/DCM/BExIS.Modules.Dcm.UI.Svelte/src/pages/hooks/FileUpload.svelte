<script>
import {FileUploader} from '@bexis2/svelte-bexis2-core-ui';
import {FileInfo} from '@bexis2/svelte-bexis2-core-ui'
import Fa from 'svelte-fa/src/fa.svelte'

import { hosturl } from '../../stores/store.js'
import { faTrash } from '@fortawesome/free-solid-svg-icons'
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


async function load()
{
  let url = hosturl+hook.start+"?id="+id+"&version="+version;

  // load menu froms server
  const res = await fetch(url);
  model = await res.json();

  start = hosturl+hook.start;
  submit = hosturl+"dcm/fileupload/upload";

  console.log(model);
}

</script>

{#if model}

<FileUploader {id} {version} data={model} {start} {submit} on:submit={load}/>

<FileOverview {id} files={model.ExistingFiles} removeAction="fileupload/removefile" saveAction="fileupload/saveFileDescription"/>

{/if}