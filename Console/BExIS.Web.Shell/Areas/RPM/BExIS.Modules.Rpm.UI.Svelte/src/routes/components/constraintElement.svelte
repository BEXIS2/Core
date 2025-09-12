<script lang="ts">
	import { helpStore, type helpItemType } from '@bexis2/bexis2-core-ui';

	export let value: string;

	let helpitem: helpItemType = {
		id: 'Constreint',
		name: 'Formal Description',
		description: value
	};
</script>

<!-- svelte-ignore a11y-no-static-element-interactions -->
<div
	class="break-words w-modal line-clamp-2"
	title={value}
	on:mouseover={() => {
		helpStore.showHelpItem(helpitem);
	}}
	on:focus={() => {
		helpStore.showHelpItem(helpitem);
	}}
>
	{value}
</div>
