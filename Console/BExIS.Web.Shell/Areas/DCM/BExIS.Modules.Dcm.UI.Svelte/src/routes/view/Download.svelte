<script lang="ts">
	import { downloadZip } from "./services";
	import Fa from "svelte-fa";
 import { faDownload, faSave } from "@fortawesome/free-solid-svg-icons";


 export let id;
 export let version;
 export let versionId:number;

 export let downloadAccess = false; // user has access to download the dataset
 export let hasDatastructure = false; // dataset has a datastructure
 export let hasData = false; // dataset has data

 export let isPublic = false; // dataset is public
 export let data_aggrement = ""; //"data policy" or "terms_and_conditions"
 export let total: number; // number of rows in dataset

 let withUnits = false;
 let exceptAgreement = false;
 $:exceptAgreement = !isPublic || (isPublic && data_aggrement === "none") ? true : exceptAgreement;
 let excelMaxRows = 1048576;
 let selectedFormat = "";

 const downloadFormats = [
  {
   'label': 'application/xlsx',
   'value': 'application/xlsx',
  },
  {
   'label': 'text/csv',
   'value': 'text/csv',
  },
  {
   'label': 'text/tsv',
   'value': 'text/tsv',
  },
  {
   'label': 'text/plain',
   'value': 'text/plain',
  }
];

async function downloadDatasetFn()
{
  const res = await downloadZip(id, null, versionId);
  console.log("🚀 ~ downloadDatasetFn ~ res:", res)
  sendDataTo(res,"dataset_test_name.zip", "application/zip");
}

async function downloadDatasetWithFormatFn(event)
{
  alert(selectedFormat)
  const format = selectedFormat;
  if(format === 'application/xlsx' && total > excelMaxRows){
    alert(`The dataset has ${total} rows, which exceeds the maximum number of rows for Excel (${excelMaxRows}). Please choose another format.`);
    return;
  }

  const res = await downloadZip(id, format, versionId, withUnits);
  
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

}



</script>

{#if downloadAccess}
 {#if hasDatastructure} 
 <div class="">
 <div class="flex ">
  <h4 class="h4 grow">Download</h4> 
</div>

  <div class="input-group input-group-divider grid-cols-[1fr_auto]">
    <select class="select" bind:value={selectedFormat} >
      <option value="" disabled selected hidden>- Select a format -</option>
      {#each downloadFormats as d}
        <option value={d.value}>{d.label}</option>
      {/each}
    </select>
    <button class="variant-filled-secondary" disabled={!exceptAgreement || selectedFormat == ''} on:click={downloadDatasetWithFormatFn}><Fa icon={faDownload} /></button>
  </div>
</div>
<div class="padding-top-5 position-releative flex flex-col gap-2">
    <span>
        <input class="checkbox"  type="checkbox" id="withUnits" bind:checked="{withUnits}" />
        <span>with units</span>

    </span>

    {#if data_aggrement === "data policy"}
 
     <div class="data-aggreement">
         <input type="checkbox" class="checkbox" id="data-policy" bind:checked="{exceptAgreement}"/>
         <b>
             I accept the public regulations from the
             <a class="a" href="/footer/policy" target="_blank">privacy policy</a>.
         </b>
     </div>
 
    {:else if data_aggrement === "terms and conditions"}
    
        <div class="data-aggreement">
            <input type="checkbox" class="checkbox" id="terms-and-conditions" bind:checked="{exceptAgreement}"/>
            <b>
                I accept the public regulations from the
                <a class="a" href="/footer/termsandconditions" target="_blank">terms and conditions</a>.
            </b>
        </div>
    
    {/if}
</div>


  {:else} <!-- // download package with files -->

  <button class="btn variant-filled-primary" disabled={!exceptAgreement} on:click={() => downloadDatasetFn()}>
   <!-- svelte-ignore missing-declaration -->
   <Fa icon={faSave} />
   <span class="padding-left-5">Download</span>
  </button>
 
 {/if}



{/if}