<script lang="ts">
	import { onMount } from 'svelte';
	import type { ConstraintListItem, DatasetInfo } from '../models';
	import { GetDatasetsByConstreint } from '../services/apiCalls';
	import Fa from 'svelte-fa';
	import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';

	export let constraint: ConstraintListItem;
	let ds: DatasetInfo[] = [];
	$: datasets = ds;
	$: console.log('Datasets', datasets);

	onMount(async () => {
		ds = await GetDatasetsByConstreint(constraint.id);
	});
</script>

<Fa
	class="text-warning-500 shadow-md h3 m-1"
	title="Changing the Contsraint may cause inconsistencies on Datasets."
	icon={faTriangleExclamation}
/>
<div class="max-h-40 overflow-scroll">
	<div class="w-full mb-1 p-3 variant-ghost-warning card">
		<p class="h4">Changing the Contsraint may cause inconsistencies on following Datasets.</p>
		<ul class="text-sm mt-1">
			{#each datasets as dataset}
				<li>
					{dataset.name}, {dataset.id}
				</li>
			{/each}
		</ul>
	</div>
</div>
