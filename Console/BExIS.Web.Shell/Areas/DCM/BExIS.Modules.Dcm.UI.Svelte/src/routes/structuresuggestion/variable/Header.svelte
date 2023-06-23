<script>

 import {SlideToggle} from '@skeletonlabs/skeleton'

 export let index;
 export let name;
 export let isKey;
 export let isOptional;

 </script>

<div class="grid grid-cols-10">

 <div class="col-span-3"><h1 class="h1">{name} ({index})</h1></div>

    <div  class="col-span-4">
    {#if isKey && isOptional}
      <aside class="alert variant-filled-warning">
        <!-- Message -->
        <div class="alert-message">
            <p>If variables are defined as part of the primary key and are optional, there may be complications when uploading.</p>
        </div>
     </aside>  
    
    {/if}
   </div>
    <div class="col-span-3 text-right">
      <SlideToggle name="isKey" bind:checked={isKey}>Mark as part of primary keys</SlideToggle>
      <SlideToggle name="isOptional" bind:checked={isOptional}>Value can be optional</SlideToggle>
    </div>

</div>