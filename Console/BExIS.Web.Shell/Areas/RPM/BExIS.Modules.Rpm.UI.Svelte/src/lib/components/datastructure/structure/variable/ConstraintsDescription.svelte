<script lang="ts">
	import type { listItemType } from '@bexis2/bexis2-core-ui';

	export let list: listItemType[];
</script>

<div class="flex flex-col gap-2 mt-2">
	{#if list}
		{#each list as i}
			<div class="flex flex-col gap-1">
				<span class="text-xs font-semibold">{i.text}</span>
				<span class="text-xs">- {i.description}</span>
			</div>
		{/each}
	{/if}
</div>
