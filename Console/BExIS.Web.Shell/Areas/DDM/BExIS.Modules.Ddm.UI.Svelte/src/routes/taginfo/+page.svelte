<script lang="ts">

	import { Page, pageContentLayoutType} from "@bexis2/bexis2-core-ui";
	import TagInfoEdit from "./TagInfoEdit.svelte";
	import TagInfoView from "./TagInfoView.svelte";

	let container;
 let id;	
	let date = Date.now();

	container = document.getElementById('taginfo');
	id = Number(container?.getAttribute('dataset'));

</script>
<Page 
	title="Tag Overview" 
	note="Tag Infomations about a version"
	contentLayoutType={pageContentLayoutType.center}
>
	<TagInfoEdit on:reload={()=>date = Date.now()}/>

	<TagInfoView {id} bind:date={date}/>

</Page>