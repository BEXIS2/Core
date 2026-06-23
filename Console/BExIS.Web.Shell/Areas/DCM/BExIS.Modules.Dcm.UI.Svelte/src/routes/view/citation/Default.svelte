<script lang="ts" >
	import type { CitationDataModel } from "$models/View";


 export let model:CitationDataModel


</script>

{#if model}
 <h2 class="h2">{model.title}</h2>
{:else}
 <p>Title is not available.</p>
{/if}