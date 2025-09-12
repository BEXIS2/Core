<script lang="ts">
	import Search from '$lib/components/Search.svelte';
	import { type linkType, Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	const controller = 'publicsearch';

	const links: linkType[] = [{ label: 'Manual', url: '/home/docs/Search%20and%20Download%20Data' }];
</script>

<Page
	title="Public Search"
	note="Search within public available data in this system."
	contentLayoutType={pageContentLayoutType.full}
	{links}
>
	<Search {controller} />
</Page>
