<script lang="ts">
	import Search from '$lib/components/Search.svelte';
	import { type linkType, Page, pageContentLayoutType } from '@bexis2/bexis2-core-ui';

	const controller = 'search';




</script>

<Page>
							<div class="flex gap-4 items-center grow">
								<button
									title="Apply selected search criteria"
									class="btn variant-filled-primary"
									on:click|preventDefault={async () => {}}>Search</button
								>
							</div>

</Page>