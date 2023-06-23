<script>

import {TextInput} from "@bexis2/bexis2-core-ui";
import Fa from "svelte-fa/src/fa.svelte";
import {faEye} from '@fortawesome/free-solid-svg-icons'

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
    <div class="text-right col-span-3">
      <button class="btn variant-ringed-secondary" on:click="{()=> window.open("/ddm/data/Showdata?id="+id)}"> <Fa icon="{faEye}"/></button>
    </div>
</div> 