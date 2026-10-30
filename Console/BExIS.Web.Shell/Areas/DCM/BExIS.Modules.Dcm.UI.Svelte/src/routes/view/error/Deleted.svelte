<script lang="ts">
	import { onMount } from 'svelte';
	import { getDeleted } from '../services.js';

	import type { DeletedModel } from '../types.js';
	import Link from '$lib/hooks/view/Link.svelte';

	export let id: number;

	let model: DeletedModel;
	$: model;
	onMount(async () => {
		const res = await getDeleted(id);
		console.log('🚀 ~ res:', res);

		model = res;
	});
</script>

{#if model}
	<div class="flex gap-4 items-start mb-5">
		<h2 class="h2">{model.title}</h2>
		<div class="chip variant-filled-error">Deleted</div>
	</div>

	<div>
		<p class="variant-ghost-error p-5">
			The dataset has been withdrawn. Please see the 'Related Work' section to see whether a new version of the dataset is available. 
			available.

			{#if model.reason && model.reason != 'Delete'}
				<br />
        <br />
				<strong>Reason:</strong>
				{model.reason}
			{/if}
		</p>
	</div>

	<Link links={model.links.to} />
{/if}
