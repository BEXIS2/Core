<script lang="ts" >
	import type { CitationDataModel } from "$models/View";


 export let model:CitationDataModel


</script>

<div class="flex flex-col gap-2">
{#if model}
 <h2 class="h2">{model.title}</h2>
{:else}
 <p>Title is not available.</p>
{/if}

<div>{model.authors}</div>

</div>