<script lang="ts">

	import { Page, pageContentLayoutType} from "@bexis2/bexis2-core-ui";
	import TagInfoEdit from "./TagInfoEdit.svelte";
	import TagInfoView from "./TagInfoView.svelte";

	


</script>
<Page 
	title="Tag Overview" 
	note="Tag Infomations about a version"
	contentLayoutType={pageContentLayoutType.center}
>
<TagInfoEdit  />
<TagInfoView  />



</Page>