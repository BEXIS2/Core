<script>
// import {FileUploader} from '@bexis2/bexis2-core-ui/src/lib/index';
import {getHookStart}  from '../../services/HookCaller'
import { onMount }from 'svelte'
import FileOverview from '../../lib/components/fileupload/FileOverview.svelte'
import TimeDuration from '../../lib/components/utils/TimeDuration.svelte'
import {Spinner} from 'sveltestrap'

export let id=0;
export let version=1;
export let hook;

$:model = null;
$:loading = false;

onMount(async () => {
  load();
})

// action for fileupload
let start="";
let save='/dcm/attachmentupload/saveFileDescription';
let remove='/dcm/attachmentupload/removefile';
let submit = "/dcm/attachmentupload/upload";
let context = "attachment"
async function load()
{

  model = await getHookStart(hook.start,id,version);
  start = hook.start;
  loading = false;
}


</script>

{#if model}
{#if model.lastModification}

<TimeDuration milliseconds={new Date(model.lastModification)}/>

{/if}

<!-- <FileUploader {id} {version} {context} data={model} {start} {submit} on:submited={load} on:submit={()=>loading=true } on:error on:success /> -->

{#if model.existingFiles}
  <!-- <FileOverview {id} files={model.existingFiles} descriptionType={model.descriptionType} {save} {remove} on:success/> -->
{/if}

{#if loading}
  <Spinner color="info" size="sm" type ="grow" text-center />
{/if}

{/if}