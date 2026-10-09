<script lang="ts">
 import type { CitationDataModel } from "$models/View";
 import Fa from "svelte-fa";
 import {faCopy} from "@fortawesome/free-solid-svg-icons";

 export let model:CitationDataModel


 function copyDivContentToClipboard(divId) {

      const div = document.getElementById(divId);
      // You can copy text or HTML content, change as needed
      const content = div?.innerText || ""; // Use innerHTML if you want to copy HTML formatting

      navigator.clipboard.writeText(content)
          .then(() => {
              alert('Citation copied to clipboard!');
          })
          .catch(err => {
              alert('Error copying citation: ' + err);
          });
  }


 </script>

<div id="citation-container">
 <div id="citation-title" class="flex h4 flex-wrap gap-1">
  {model.authors} 
  ({model.year}): 
  <b>{model.title}</b>. 
  Version {model.version}. 
  {model.publisher}. 
  {model.entityName}. 
  {model.url}  
 </div>
 <div on:click={()=>  copyDivContentToClipboard('citation-title')} on:keydown={()=>  copyDivContentToClipboard('citation-title')}
    id="citation-copy" title="Copy citation to clipboard" role="button" tabindex="0">
    <Fa icon={faCopy} />
</div>

   
</div>

<style>
    #citation-container {
        display: flex;
        flex: auto;
        gap: 10px;
    }

    #citation-copy {
        cursor: pointer;
    }

    #citation-copy:hover {
        color: var(--bexis2-gray-3);
    }

</style>