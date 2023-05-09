<script lang="ts">

 export let label:string = "";
 export let required:boolean;
 export let feedback:string;
 
 
 </script>
 
 <label class="label">
  <span>{label} 
   {#if required} <span class="text-sm text-red-600">*</span> {/if}
  </span>
   <slot/>
   {#if feedback}
    <span class="text-sm text-red-600">{feedback}</span>
   {/if}
 </label>
 
 
 