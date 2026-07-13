<script lang="ts">

import Fa from "svelte-fa";
import {
 faCreativeCommons, 
 faCreativeCommonsBy,
 faCreativeCommonsSa,
 faCreativeCommonsNd,
 faCreativeCommonsNc,
} from "@fortawesome/free-brands-svg-icons";

export let license = "";
const l = license.toUpperCase();


// switch (license) {
//   case "CC BY":
//     license = "Creative Commons Attribution";
//     break;
//   case "CC BY-SA":
//     license = "Creative Commons Attribution-ShareAlike";
//     break;
//   case "CC BY-ND":
//     license = "Creative Commons Attribution-NoDerivs";
//     break;
//   case "CC BY-NC":
//     license = "Creative Commons Attribution-NonCommercial";
//     break;
//   case "CC BY-NC-SA":
//     license = "Creative Commons Attribution-NonCommercial-ShareAlike";
//     break;
//   case "CC BY-NC-ND":
//     license = "Creative Commons Attribution-NonCommercial-NoDerivs";
//     break;
//   default:
//     license = "";
// }


</script>



<div title={license} class="badge variant-filled-secondary"> 
<span>{l}</span> 
{#if l === 'CC BY'}
	 <Fa icon={faCreativeCommonsBy} />
{:else if l === 'CC BY-SA'}
 <Fa icon={faCreativeCommonsSa} />
{:else if l === 'CC BY-ND'}
 <Fa icon={faCreativeCommonsNd} />
  {:else if l === 'CC BY-NC'}
 <Fa icon={faCreativeCommonsNc} />
  {:else if l === 'CC BY-NC-SA'}
 <Fa icon={faCreativeCommons}  />
  {:else if l === 'CC BY-NC-ND'}
 <Fa icon={faCreativeCommons}  />
  {:else if l === 'CC'}
 <Fa icon={faCreativeCommons} />
{/if}
</div>