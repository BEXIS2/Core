<script lang="ts">
	import type { ReferenceModel } from '../../../models/View';


 export let links:ReferenceModel[] = [];
 console.log("🚀 ~ links:", links)

 const types = links.map(link => link.linkType).filter((value, index, self) => self.indexOf(value) === index);

</script>


{#if types}
<h3 class="h3">Related Work</h3>
<div class="card p-5 mb-5">
 {#if types.length === 0}
  <p>No related work found.</p>
 {/if}
 
 {#each types as type}

 <div class="mb-4">
   <h4 class="h4">{type.toUpperCase()}</h4>
   <div class="flex flex-col mt-2 gap-2">
   {#each links.filter(link => link.linkType === type) as link}
     <div class="flex gap-4"><span>{link.referenceType}</span>
        <span>{link.target.type}</span>
          <a class="chip variant-outline-tertiary mb-2" href="/dcm/view?id={link.target.id}&version={link.target.version}" target="_blank">{link.target.title} </a>
        <span>{link.context}</span>
    </div>
   {/each}
  </div>
 </div>
 {/each}
 </div>
{/if} 
