<script lang="ts">
	import { onMount } from 'svelte';

	import Fa from 'svelte-fa';
	import { faSave } from '@fortawesome/free-solid-svg-icons';

	import { Page, notificationType, notificationStore, helpStore, pageContentLayoutType } from '@bexis2/bexis2-core-ui';
	import { ListBox, ListBoxItem } from '@skeletonlabs/skeleton';

	let list:string[]	= ["1","2","3","4","5","6","7","8","9","10"];	



</script>

<Page title="help" contentLayoutType="{pageContentLayoutType.full}">

<div slot="left">

		<nav class="list-nav">
			<!-- (optionally you can provide a label here) -->
			<ul>
				{#each list as item}
						<li>
							<a class="flex-auto">{item}</a>
						</li>
					{/each}
				</ul>
		</nav>
		

</div>

<div class="w-full h-full variant-filled-secondary-secondary text-justify">test</div>

</Page>
