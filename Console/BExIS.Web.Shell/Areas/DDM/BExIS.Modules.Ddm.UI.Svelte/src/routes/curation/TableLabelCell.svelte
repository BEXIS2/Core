<script lang="ts">
	import { getContrastColor } from './CurationEntries';
	import {
		CurationStatus,
		CurationStatusLabels,
		getCurationStatusFromBoolean,
		type CurationLabel
	} from './types';

	export let value;
	export let column;

	export let row;
	export let dispatchFn;
	row;
	dispatchFn;

	let status: CurationStatus | undefined = undefined;
	let labels: CurationLabel[] | undefined;

	if (column.id === 'labels' && Array.isArray(value)) {
		labels = value.map(
			(v) =>
				({
					name: v
						.match(/^\S*\s/)
						?.toString()
						.trim(),
					color: v
						.match(/\s#[0-9a-fA-F]+$/)
						?.toString()
						.slice(1, 8)
				}) as CurationLabel
		);
	} else if (column.id === 'curationStatus') {
		status = getCurationStatusFromBoolean(value.userIsDone, value.isApproved);
	}
</script>

{#if labels && labels.length > 0}
	<div class="flex max-w-80 flex-wrap gap-1">
		{#each labels as l}
			<div
				class="text-nowrap rounded-full px-2 py-0.5"
				style="background-color: {l.color}; color: {getContrastColor(l.color)};"
			>
				{l.name}
			</div>
		{/each}
	</div>
{:else if column.id === 'curationStatus'}
	{#if !value.curationStarted}
		<div>Not Curated</div>
	{:else if status !== undefined}
		<div
			class="w-fit text-nowrap rounded-full px-2 py-0.5"
			style="background-color: {CurationStatusLabels[status].bgColor}; color: {CurationStatusLabels[
				status
			].fontColor}"
		>
			{CurationStatusLabels[status].name}
		</div>
	{/if}
{/if}
