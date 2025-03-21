<script lang="ts">

	import Search from "$lib/components/Search.svelte";
		import {type linkType, Page, pageContentLayoutType } from "@bexis2/bexis2-core-ui";
	
	
	const controller = 'publicsearch';
	
		const links:linkType[] = [
			{ label: 'Manual', url: '/home/docs/configuration#search' }
		];
	
	
	</script>
	
	<Page
		title="Public Search"
		note="Search over the public data in this system."
		contentLayoutType={pageContentLayoutType.full}
		{links}
	>
	
		<Search {controller} />
	
	</Page>