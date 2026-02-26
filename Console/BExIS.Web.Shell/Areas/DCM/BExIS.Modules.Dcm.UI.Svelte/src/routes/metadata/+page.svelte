<script lang="ts">
	import { onMount } from 'svelte';

	import { NumberInput, Page } from '@bexis2/bexis2-core-ui';
	import MetadataEdit from './components/edit/MetadataEdit.svelte';
	import MetadataView from './components/view/MetadataView.svelte';
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';

	//export let schemaId: number = 3;
	export let datasetId: number = 2;
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

	<h1 class="h1">Metadata</h1>
	<div class="mb-4 text-sm text-surface-700" >
		Manage the metadata of the dataset here. Please fill in all required fields.
	</div>

	<!-- show dataset id-->
	<div class=" flex mb-4 text-sm text-surface-700 w-1/2" >
		<NumberInput 
			id="datasetidinput"
			label="dataset id"
			bind:value={datasetId}
	 />
	</div>
	<!-- switch between edit and view modes -->
	<RadioGroup bind:value={mode}>
		<RadioItem bind:group={mode} name="justify" title="view" label="view" value="view">VIEW</RadioItem>
		<RadioItem bind:group={mode} name="justify" title="edit" label="edit" value="edit">EDIT</RadioItem>
</RadioGroup>

 {#if datasetId === 0}
		<div><b>please select a id</b></div>
	{:else}
 {#key datasetId}
		{#if mode	=== 'view'}
			<MetadataView bind:datasetId={datasetId} />
		{:else}
			<MetadataEdit	bind:datasetId={datasetId} />
		{/if}
 {/key}
 {/if}
</Page>
