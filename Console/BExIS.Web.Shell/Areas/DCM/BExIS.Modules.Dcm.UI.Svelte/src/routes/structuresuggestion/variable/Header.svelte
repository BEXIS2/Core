<script>

 import {SlideToggle} from '@skeletonlabs/skeleton'
 import {Alert} from '@bexis2/bexis2-core-ui'

 export let index;
 export let name;
 export let isKey;
 export let isOptional;

 </script>

<div class="grid grid-cols-10">

 <div class="col-span-3"><h1 class="h1">{name}</h1></div>

    <div  class="col-span-4">
    {#if isKey && isOptional}
      <Alert cssClass="variant-filled-warning" deleteBtn={false}>
        If variables are defined as part of the primary key and are optional, there may be complications when uploading.
     </Alert>
    
    {/if}
   </div>
    <div class="col-span-3 text-right">
      <SlideToggle size="sm" name="isKey" bind:checked={isKey} active="bg-primary-500">Mark as part of primary keys</SlideToggle>
      <SlideToggle size="sm" name="isOptional" active="bg-primary-500" bind:checked={isOptional}>Value can be optional</SlideToggle>
    </div>

</div>