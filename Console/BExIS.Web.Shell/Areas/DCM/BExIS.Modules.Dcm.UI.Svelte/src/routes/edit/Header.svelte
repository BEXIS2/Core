<script>

import {TextInput} from "@bexis2/bexis2-core-ui";

 export let id;
 export let version; 
 export let title = "";

 </script>
 <!-- Header -->
 <div class="grid p-5 m-2 grid-cols-6" >
    <div class="flex gap-2 text-center">
      <h4 class="h4">Title</h4>
    </div>
    <div class="col-span-2">
      <TextInput bind:value={title} />
    </div>
</div> 


 
 