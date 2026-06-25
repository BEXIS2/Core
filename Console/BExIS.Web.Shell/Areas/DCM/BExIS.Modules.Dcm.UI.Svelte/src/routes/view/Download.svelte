<script lang="ts">
	import type { ViewSettings } from "$models/View";
	import { MultiSelect } from "@bexis2/bexis2-core-ui";
	import { downloadZip } from "./services";
	import Fa from "svelte-fa";
 import { faSave } from "@fortawesome/free-solid-svg-icons";


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
 let withFilters = false;
 let exceptAgreement = false;
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
// with units
// with filter


async function downloadDatasetFn()
{
  const res = await downloadZip(id, null, versionId);
  console.log("🚀 ~ downloadDatasetFn ~ res:", res)
  sendDataTo(res,"dataset_test_name.zip", "application/zip");
}

async function downloadDatasetWithFormatFn(event)
{
  console.log('downloadDatasetWithFormatFn', event.detail);
  const format = event.detail.value;
  if(format === 'application/xlsx' && total > excelMaxRows){
    alert(`The dataset has ${total} rows, which exceeds the maximum number of rows for Excel (${excelMaxRows}). Please choose another format.`);
    return;
  }

  const res = await downloadZip(id, selectedFormat, versionId, withUnits, withFilters);
  sendDataTo(res, `dataset_test_name`, "application/zip");
 
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
 
 {#if data_aggrement === "data policy"}
 
     <div class="data-aggreement">
         <input type="checkbox" id="data-policy" bind:checked="{exceptAgreement}"/>

         <b>
             I accept the public regulations from the
             <a href="/footer/policy" target="_blank">privacy policy</a>.
         </b>
     </div>
 
 {:else if data_aggrement === "terms and conditions"}
 
     <div class="data-aggreement">
         <input type="checkbox" id="terms-and-conditions" bind:checked="{exceptAgreement}"/>
         <b>
             I accept the public regulations from the
             <a href="/footer/termsandconditions" target="_blank">terms and conditions</a>.
         </b>
     </div>
 
 {/if}

 {#if hasDatastructure} 
  <div class="padding-top-5 position-releative ">
    <span>
        <input type="checkbox" id="withFilter" bind:checked="{withFilters}" />
        use filter
    </span>

    <span>
        <input class="form-check-input" type="checkbox" id="withUnits" bind:checked="{withUnits}" />
        add units
    </span>
  </div>

  <MultiSelect
    id="download-format"
    title="Download dataset with"
    source={downloadFormats}
    target={selectedFormat}
    itemId="value"
    itemLabel="label"
    isMulti={false}
    clearable={false}
    on:change={downloadDatasetWithFormatFn}
   />

  {:else} <!-- // download package with files -->

  <button class="btn variant-filled-primary" on:click={() => downloadDatasetFn()}>
   <!-- svelte-ignore missing-declaration -->
   <Fa icon={faSave} />
   <span class="padding-left-5">Download</span>
  </button>
 
 {/if}



{/if}