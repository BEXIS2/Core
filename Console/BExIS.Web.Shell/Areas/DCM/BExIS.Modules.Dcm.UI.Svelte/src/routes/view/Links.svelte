<script lang="ts">
	import type { ReferenceModel } from '../../models/View';


 export let links:ReferenceModel[] = [];

 const types = links.map(link => link.linkType).filter((value, index, self) => self.indexOf(value) === index);

</script>

{#if types}
 {#each types as type}
 <div class="mb-4">
   <h2 class="h3">{type.toUpperCase()}</h2>
   <div class="flex mt-2 gap-2">
   {#each links.filter(link => link.linkType === type) as link}
     <span>{link.referenceType}</span><a class="chip variant-outline-tertiary mb-2" href="/dcm/view?id={link.target.id}&version={link.target.version}" target="_blank">{link.target.title} </a>
   {/each}
  </div>
 </div>
 {/each}
{/if} 
