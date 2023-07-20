<script lang="ts">

import type { missingValueType } from "$models/StructureSuggestion";

 export let type;
 export let missingValues:any[];

 console.log("missing values",missingValues);
 let mv = ""

</script>


{#if type==="string" && missingValues}
 <b>String detected: </b>
  If not correct, please check if placeholder for missing values provided correctly.<br>
  {#if missingValues}
   <b>Missing values: </b> {missingValues.map(m=>m.displayName).join(', ')}
  {/if}
{/if}