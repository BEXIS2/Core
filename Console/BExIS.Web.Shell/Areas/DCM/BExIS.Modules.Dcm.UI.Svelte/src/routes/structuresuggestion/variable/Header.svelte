<script>

 import {SlideToggle} from '@skeletonlabs/skeleton'

 export let index;
 export let name;
 export let isKey;
 export let isOptional;

 </script>

<div class="grid grid-cols-6">

 <div class="col-span-4"><h1>{name} ({index})</h1></div>

 <div>
    {#if isKey && isOptional}
      <aside class="alert variant-filled-warning">
        <!-- Message -->
        <div class="alert-message">
            <p>If variables are defined as part of the primary key and are optional, there may be complications when uploading.</p>
        </div>
    </aside>  
    
    {/if}
    <div style="float:right">
      <SlideToggle name="isKey" bind:checked={isKey}>Mark as part of primary keys</SlideToggle>
      <SlideToggle name="isOptional" bind:checked={isOptional}>Value can be optional</SlideToggle>
    </div>
  </div>
</div>