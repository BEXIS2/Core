<script lang="ts">

import Search from "$lib/components/Search.svelte";
	import {type linkType, Page, pageContentLayoutType } from "@bexis2/bexis2-core-ui";


const controller = 'search';

	const links:linkType[] = [
		{ label: 'Public Search', url: '/ddm/publicsearch' },
		{ label: 'Manual', url: '/home/docs/configuration#search' }
	];


</script>

<Page
	title="Search"
	note="Search over the data in this system."
	contentLayoutType={pageContentLayoutType.full}
	{links}
>

	<Search {controller} />

</Page>