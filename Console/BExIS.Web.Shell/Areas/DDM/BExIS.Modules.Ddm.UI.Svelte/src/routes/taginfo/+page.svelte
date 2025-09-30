<script lang="ts">
	import { Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import type { linkType } from '@bexis2/bexis2-core-ui';

	import TagInfoEdit from './TagInfoEdit.svelte';
	import TagInfoView from './TagInfoView.svelte';

	let container;
	let id;
	let date = Date.now();

	container = document.getElementById('taginfo');
	id = Number(container?.getAttribute('dataset'));

	let links: linkType[] = [{ label: 'Manual', url: '/home/docs/MyData#dataset-versioning' }];
</script>

<Page
	title="Release Tag Management"
	note="Manage and view release tags of the dataset."
	contentLayoutType={pageContentLayoutType.center}
	{links}
>
	<TagInfoEdit on:reload={() => (date = Date.now())} />

	<TagInfoView {id} bind:date />
</Page>
