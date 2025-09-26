<script lang="ts">
	import { CurationEntryStatusColorPalettes } from '$lib/models/CurationEntry';
	import { curationStore } from '$lib/stores/CurationStore';

	const { statusColorPalette } = curationStore;
</script>

<label class="flex">
	<span class="mr-1">Color palette for the entry status:</span>
	<select
		bind:value={$statusColorPalette}
		title="Change color palette of entry status"
		class="rounded-l py-0.5 text-sm"
	>
		{#each CurationEntryStatusColorPalettes as colorPalette}
			<option value={colorPalette}>{colorPalette.name}</option>
		{/each}
	</select>
	<div
		class="flex items-center gap-x-1 overflow-hidden rounded-r border-y border-r border-surface-500 px-2"
	>
		{#each $statusColorPalette.colors as c}
			<div class="inline size-2 rounded-full" style="background-color: {c}">&nbsp;</div>
		{/each}
	</div>
</label>
