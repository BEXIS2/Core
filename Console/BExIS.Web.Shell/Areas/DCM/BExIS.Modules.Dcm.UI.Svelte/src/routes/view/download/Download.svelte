<script lang="ts">
	import { downloadZip } from "../services";
	import Fa from "svelte-fa";
 import { faDownload, faSave } from "@fortawesome/free-solid-svg-icons";
	import Request from "./Request.svelte";
	import { positionType, Spinner } from "@bexis2/bexis2-core-ui";
	import { ProgressRadial } from "@skeletonlabs/skeleton";
  import {scope} from '../../view/stores';
	import { get } from "svelte/store";


 export let id;
 export let version;
 export let versionId:number;

 export let downloadAccess = false; // user has access to download the dataset
 export let hasDatastructure = false; // dataset has a datastructure
 export let hasData = false; // dataset has data

// for requests
export let requestAble: boolean = false; // user can request the dataset
export let hasRequestRight: boolean = false; // user has rights to request the dataset
export let requestExist: boolean = false; // user has already requested the dataset

 export let isPublic = false; // dataset is public
 export let data_aggrement = ""; //"data policy" or "terms_and_conditions"
 export let total: number; // number of rows in dataset

 $:isDownloading = false;
 $:exceptAgreement = !isPublic || (isPublic && data_aggrement === "none") ? true : exceptAgreement;

 let excelMaxRows = 1048576;
 let selectedFormat = "";

 const baseFormats = [
  'application/xlsx',
  'text/csv',
  'text/tsv',
  'text/plain'
 ];

 $: downloadFormats = baseFormats.flatMap(f => [
  { label: f, value: f + '|false', format: f, withUnits: false },
  { label: f + ' (with units)', value: f + '|true', format: f, withUnits: true }
 ]);

 $: selected = downloadFormats.find(d => d.value === selectedFormat);
 $: withUnits = selected?.withUnits ?? false;
 $: selectedMimeType = selected?.format ?? '';
 



async function downloadDatasetFn()
{
  isDownloading = true;
  const res = await downloadZip(id, null, versionId);
  if(res)
  {
    // get name
    const cd = res.headers['content-disposition'];
    let fileName = 'download';
    
    if (cd) {
      const match = cd.match(/filename\*?=(?:UTF-8'')?"?([^\";]+)"?/i);
      if (match?.[1]) fileName = decodeURIComponent(match[1]);
    }

    sendDataTo(res.data,fileName, "application/zip");
  }
}



async function downloadDatasetWithFormatFn(event)
{

  const format = selectedMimeType;
  if(format === 'application/xlsx' && total > excelMaxRows){
    alert(`The dataset has ${total} rows, which exceeds the maximum number of rows for Excel (${excelMaxRows}). Please choose another format.`);
    return;
  }

  isDownloading = true;

  let f = false;
  const s = get(scope);

  if(s && ((s.filter && s.filter.length > 0) || (s.sort && s.sort.length > 0 )))
  {
    f = true;
  }
  else
  {
    f = false;
  }

  const res = await downloadZip(id, format, versionId, f , withUnits,s);
  
  if(res)
  {
    const file = res.data;

    // get name
    const cd = res.headers['content-disposition'];
    let fileName = 'download';
    
    if (cd) {
      const match = cd.match(/filename\*?=(?:UTF-8'')?"?([^\";]+)"?/i);
      if (match?.[1]) fileName = decodeURIComponent(match[1]);
    }

    sendDataTo(file, fileName, "application/zip");
  }
 
}

function sendDataTo(data, name, type)
{

  if(data) {
	
					const blob = new Blob([data], { type: type });
					const url = window.URL.createObjectURL(blob);

					const a = document.createElement('a');
					a.href = url;
					a.download = name;
					document.body.appendChild(a);
					a.click();

					a.remove();
					window.URL.revokeObjectURL(url);
			}

      isDownloading = false;

}



</script>

{#if downloadAccess}
 {#if hasDatastructure} 
 <div class="">
 <div class="flex ">
  <h4 class="h4 grow">Download</h4> 
</div>
   
   <div class="input-group input-group-divider grid-cols-[1fr_auto]" >
    
    <select class="select" bind:value={selectedFormat}>
      <option value="" disabled selected hidden>- Select a format -</option>
      {#each downloadFormats as d}
        <option value={d.value}>{d.label}</option>
      {/each}
    </select>
    
    <button class:variant-filled-primary={selectedFormat !== ''} class:variant-ghost-primary={selectedFormat === ''} disabled={!exceptAgreement || selectedFormat == ''} on:click={downloadDatasetWithFormatFn}>
    {#if isDownloading}
      <!-- <Spinner position="{positionType.center}" label="Downloading..." /> -->
       <ProgressRadial width="w-6"  stroke={60}  meter="stroke-tertiary-500" track="stroke-primary-500/30" strokeLinecap="round"/>
    {:else}
      <Fa icon={faDownload} />    
    {/if}    
    </button>
   
  </div>
 </div>


  {:else} <!-- // download package with files -->

   <button class="btn" class:variant-filled-primary={exceptAgreement} class:variant-ghost-primary={!exceptAgreement}  disabled={!exceptAgreement} on:click={() => downloadDatasetFn()}>
    {#if isDownloading}<ProgressRadial width="w-6"  stroke={60}  meter="stroke-tertiary-500" track="stroke-primary-500/30" strokeLinecap="round"/>{:else}<Fa icon={faDownload} />{/if}
    <span class="padding-left-5">Download</span>
   </button>

 {/if}

{#if isPublic &&  data_aggrement === "data policy"}

<div class="data-aggreement">
    <input type="checkbox" class="checkbox" id="data-policy" bind:checked="{exceptAgreement}"/>
    <b>
        I accept the public regulations from the
        <a class="a" href="/footer/policy" target="_blank">privacy policy</a>.
    </b>
</div>

{:else if isPublic &&  data_aggrement === "terms and conditions"}

  <div class="data-aggreement">
      <input type="checkbox" class="checkbox" id="terms-and-conditions" bind:checked="{exceptAgreement}"/>
      <b>
          I accept the public regulations from the
          <a class="a" href="/footer/termsandconditions" target="_blank">terms and conditions</a>.
      </b>
  </div>

{/if}


{:else if   requestAble && hasRequestRight}
  <Request {id} exist={requestExist}/>
{:else if !requestAble && hasRequestRight}

  <button class="btn variant-filled-primary" disabled title="This dataset is currently not requestable.">
   <Fa icon={faSave} />
   <span class="padding-left-5">Currently not available</span>
  </button>
{:else if requestAble && !hasRequestRight}

  <button class="btn variant-filled-primary" disabled title="You do not have the right to request this dataset.">
   <Fa icon={faSave} />
   <span class="padding-left-5">Currently not available</span>
  </button>
{:else}

  <button class="btn variant-filled-primary" disabled on:click={() => downloadDatasetFn()}>
   <!-- svelte-ignore missing-declaration -->
   <Fa icon={faSave} />
   <span class="padding-left-5">Download</span>
  </button>
{/if}