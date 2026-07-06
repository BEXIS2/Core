<script lang="ts">
import Fa from "svelte-fa";
import { faDownload, faCopy } from "@fortawesome/free-solid-svg-icons";
import {CitationFormat} from "$models/View";
	import { getCitationOptions, getCitationText } from "../services";
	import fileDownload from "js-file-download";
	import { onMount } from "svelte";

export let id = 0;
export let version = 0;;
export let tag = 0;

 let selectedFormat:number = -1;
 $:selectedFormat, console.log("🚀 ~ selectedFormat:", selectedFormat, downloadAccess), downloadAccess = selectedFormat !== ''?true:false;

 let filename = `entity_${id}_${version}.txt`;

 let downloadFormats = [
  {
   'label': 'APA',
   'format': 'apa',
   'value': CitationFormat.APA,
  },
  {
   'label': 'RIS',
   'format': 'ris',
   'value': CitationFormat.RIS,
  },
  {
   'label': 'Text',
   'format': 'txt',
   'value': CitationFormat.Text,
  },
  {
   'label': 'BibTeX',
   'format': 'bib',
   'value': CitationFormat.Bibtex,
  }
];
$:downloadFormats, console.log("🚀 ~ downloadFormats:", downloadFormats)

let downloadAccess = selectedFormat !== -1?true:false; // user has access to download the dataset
console.log("🚀 ~ downloadAccess:", downloadAccess)

onMount(async() => {
  console.log("🚀 ~ downloadAccess:", downloadAccess)
  const res = await getCitationOptions(id, version, tag);
  console.log("🚀 ~ res:", res)
  
  downloadFormats = res.formats;
  filename = res.fileName;

});


async function downloadCitationFn()
{
  const res = await getCitationText(id, version, tag, selectedFormat);

  navigator.clipboard.writeText(res)
    .then(() => {
      console.log('Text copied successfully!');

      const format = downloadFormats.find(f => f.value === selectedFormat)?.format || 'txt';

      fileDownload(res, `${filename}.${format}`);

    })
    .catch(err => {
      console.error('Failed to copy text: ', err);
    });
}




</script>

{#if downloadFormats.length > 0}
<div class="">
  <h4 class="h4">Citation</h4>

  <div class="input-group input-group-divider grid-cols-[1fr_auto_auto]">
    <select class="select" bind:value={selectedFormat} >
      <option value={-1} disabled selected hidden>- Select a format -</option>
      {#each downloadFormats as d}
        <option value={d.value}>{d.label}</option>
      {/each}
    </select>
    <button class:variant-filled-primary={downloadAccess} disabled={!downloadAccess} on:click={downloadCitationFn}><Fa icon={faDownload} /></button>
    
  </div>
</div>
{/if}