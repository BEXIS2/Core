<script>
import {FileUploader} from '@bexis2/svelte-bexis2-core-ui';
import {getHookStart}  from '../../services/Caller'
import { onMount, createEventDispatcher }from 'svelte'
import FileOverview from '../../components/fileupload/FileOverview.svelte'
import {Spinner, Alert} from 'sveltestrap'

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

</script>

{#if model}
  <FileUploader {id} {version} {context} data={model} {start} {submit} on:submited={load} on:submit={()=>loading=true} on:error on:success/>
{#if model.existingFiles}
  <FileOverview {id} files={model.existingFiles} descriptionType={model.descriptionType} {save} {remove} on:success />
{/if}

{#if loading}
  <Spinner color="info" size="sm" type ="grow" text-center />
{/if}

{/if}