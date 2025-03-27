<script lang="ts">
 import { onMount, createEventDispatcher } from 'svelte';
 import FileOverview from '../fileupload/FileOverview.svelte';
 import FileOverviewItem  from '../fileupload/FileOverviewItem.svelte';
 import type {fileInfoType} from '@bexis2/bexis2-core-ui';
 import { latestDataDate } from '../../../routes/edit/stores';
 import {revertFile} from './services';
	// import FileDeleteItem from './FileDeleteItem.Svelte'	import { getPatchDetailsFromCliString } from 'patch-package/dist/PackageDetails';
;

import { faUndo } from '@fortawesome/free-solid-svg-icons';
// for files
let remove = '/dcm/data/removefile';
let revert = '/dcm/data/revertfile';
let save = '/dcm/data/saveFileDescription';

export let id = 0;
export let files:fileInfoType[] = [];
$:files;
export let deletedFiles:any[] = [];
$:deletedFiles;
export let descriptionType: number = 0;
$:descriptionType;

const dispatch = createEventDispatcher();

function handleSave(e){
  console.log("handleSave",e);
  latestDataDate.set(Date.now());
  dispatch('success', { text: e.detail.text });
}
function handleRemove(e){
  console.log("handleRemove",e.detail.file);
  files = files.filter(f => f.name !== e.detail.file.name);
  
  console.log("🚀 ~ handleRemove ~ deletedFiles:", deletedFiles)
  if(!deletedFiles) deletedFiles = [];
  deletedFiles = [...deletedFiles, e.detail.file];

  latestDataDate.set(Date.now());
  const text ="File removed: "+e.detail.file.name;

  dispatch('warning', { text });	
}

function handleRevert(e){
  console.log("handleRemove",e.detail.file);
  deletedFiles = deletedFiles.filter(f => f.name !== e.detail.file.name);
  
  console.log("🚀 ~ handleReverted ~ deletedFiles:", deletedFiles)
  if(!files) files = [];
  files = [...files, e.detail.file];

  latestDataDate.set(Date.now());
  const text ="File reverted: "+e.detail.file.name;

  dispatch('warning', { text });	
}

//  async function handleRevert(e, index){
//   console.log("handleRevert",e.detail.file);
//   deletedFiles = deletedFiles.filter(f => f.name !== e.detail.file.name);

//   const res = await revertFile(id,e.detail.file)
 
//   if(res.status == 200){
    
//     if(!files) files = [];
//     files = [...files, e.detail.file];
//     const text ="File reverted: "+e.detail.file.name;

//     dispatch('warning', {text} );	
//   }
//   else
//   {
//     dispatch('error', {text:"revert file was not succesful."} );	
//   }
// }

</script>

{#if files.length>0}

<div>
 <div class="pt-4">
  <b>Existing File(s)</b>
 </div>
 <FileOverview
    {id}
    bind:files={files}
    {descriptionType}
    {remove}
    {save}
    on:success={handleSave}
    on:warning={handleRemove}
   />
</div>
{/if}

{#if deletedFiles && deletedFiles.length>0}

<div>
 <div class="pt-4">
  <b>File(s) to delete</b>
 </div>

 <div class="grid gap-2 divide-y-2 pb-3 overflow-auto">

    {#each deletedFiles as file, index }

    <FileOverviewItem
         {id}
         {file}
         {...file}
         remove = {revert}
         {descriptionType}
         faIcon={faUndo}
         on:removed={handleRevert}
      />

    {/each}
   </div>
</div>
{/if}
