<script lang="ts">
	import { onMount } from "svelte";
	import { getCitation } from "./services";
	import { ReadCitationFormat, type CitationDataModel, type CitationModel } from "$models/View";
 import Default from "./citation/Default.svelte";
 import APA from "./citation/APA.svelte";
 import Text from "./citation/Text.svelte";

	export let id;
	export let version;
	export	let tag;

 let data:CitationDataModel;
 let citationComponent;


	onMount(async () => {
		
		const res:CitationModel = await getCitation(id, version, tag);
		console.log('citation', res);
		console.log('format', res.format, ReadCitationFormat.Default);

  data = res.data;

  if(res.format == ReadCitationFormat.Default){
    citationComponent = Default;
  }else if(res.format == ReadCitationFormat.APA){
    citationComponent = APA;  
  }else if(res.format == ReadCitationFormat.Text){
    citationComponent = Text;  
  }

		
	});
</script>

{#key citationComponent}
 <svelte:component this={citationComponent} model={data}  />
{/key}

