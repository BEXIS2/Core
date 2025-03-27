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
	note="Manage and view tags of the dataset."
	contentLayoutType={pageContentLayoutType.center}
>
	<TagInfoEdit on:reload={()=>date = Date.now()}/>

	<TagInfoView {id} bind:date={date}/>

</Page>