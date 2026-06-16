<script lang="ts">
	import { onMount } from 'svelte';

	import { NumberInput, Page } from '@bexis2/bexis2-core-ui';


	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';

	//export let schemaId: number = 3;
	export let datasetId: number = 3;

 $:datasetId;

	let mode: 'edit' | 'view' = 'edit';
 $:mode;

	onMount(() => {
				// read id from url
				//datasetId = Number(new URLSearchParams(window.location.search).get('id'));
			console.log('Loading metadata for datasetId:', datasetId);
		});


</script>

<Page>


</Page>
