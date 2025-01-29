<script lang="ts">
	import { onMount } from 'svelte';
	import type { ConstraintListItem, DatasetInfo, Info } from '../models';
	import { GetDatasetsByConstraint, GetMeaningsByConstraint } from '../services/apiCalls';
	import Fa from 'svelte-fa';
	import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';

	export let constraint: ConstraintListItem;
	let ds: DatasetInfo[] = [];
	let ms: Info[] = [];
	$: datasets = ds;
	$: meanings = ms;
	$: reload(constraint);


	onMount(async () => {
		await reload(constraint);
	});

	async function reload(c:ConstraintListItem): Promise<void> {
		datasets = [];
		meanings = [];
		if (c.inUseByVariable) {
			ds = await GetDatasetsByConstraint(c.id);
		}
		if (c.inUseByMeaning) {
			ms = await GetMeaningsByConstraint(c.id);
		}
	}
</script>

<Fa class="text-warning-500 shadow-md h3 m-1" icon={faTriangleExclamation} />
<div class="max-h-40 overflow-y-auto">
	<div class="w-full mb-1 p-3 variant-ghost-warning card">
		<p class="h4" title="Changing the constraint may cause inconsistencies.">
			Changing the constraint may cause inconsistencies.
		</p>
		{#if datasets && datasets.length > 0}
			<p class="h5">The constraint is used by the following datasets:</p>
			<p class="text-sm mt-1">
				{#each datasets as dataset}
					{#if dataset === datasets[datasets.length - 1]}
						{dataset.name} ({dataset.id})
					{:else}
						{dataset.name} ({dataset.id}),&nbsp
					{/if} 
				{/each}
			</p>
		{/if}
		{#if meanings && meanings.length > 0}
			<p class="h5">The constraint is used by the following meanings:</p>
			<p class="text-sm mt-1">
				{#each meanings as meaning}
					{#if meaning === meanings[meanings.length - 1]}
						{meaning.name} ({meaning.id})
					{:else}
						{meaning.name} ({meaning.id}),&nbsp
					{/if} 
				{/each}
			</p>
		{/if}
	</div>
</div>
