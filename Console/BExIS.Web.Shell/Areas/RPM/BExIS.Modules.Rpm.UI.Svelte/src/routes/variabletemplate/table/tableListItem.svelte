<script lang="ts">
	import type { listItemType } from '@bexis2/bexis2-core-ui';

	export let value: listItemType;
</script>

{#if value != undefined && value.text != undefined}
	{value.text}
{/if}
