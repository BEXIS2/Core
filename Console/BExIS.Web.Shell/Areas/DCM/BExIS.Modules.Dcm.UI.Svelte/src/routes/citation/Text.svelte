<script lang="ts">
 import type { CitationDataModel } from "$models/View";

 export let model:CitationDataModel


 function copyDivContentToClipboard(divId) {
      const div = document.getElementById(divId);
      // You can copy text or HTML content, change as needed
      const content = div.innerText; // Use innerHTML if you want to copy HTML formatting

      navigator.clipboard.writeText(content)
          .then(() => {
              alert('Citation copied to clipboard!');
          })
          .catch(err => {
              alert('Error copying citation: ' + err);
          });
  }


 </script>


<div id="citation-container"><b>Citation:</b>
 <div id="citation-title">
  {model.authors} 
  ({model.year}): 
  <b>{model.title}</b>. 
  Version {model.version}. 
  {model.publisher}. 
  {model.entityName}. 
  {model.uRL}  
  <span id="citation-copy" class="bx bx-copy" on:click="{() => copyDivContentToClipboard('citation-title')}"></span>
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