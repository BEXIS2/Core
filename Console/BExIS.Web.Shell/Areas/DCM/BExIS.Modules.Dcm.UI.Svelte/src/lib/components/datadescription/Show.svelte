<script lang="ts">
	import type { VariableModel } from '$models/DataDescription';
	import Header from './ShowHeader.svelte';
	import { Table } from '@bexis2/bexis2-core-ui';
	import type { TableConfig } from '@bexis2/bexis2-core-ui';
	import { writable } from 'svelte/store';

	export let id; //enityid
	export let title;
	export let description;
	export let structureId;
	export let fileReaderExist; // if filereader not exist, need to set
	export let readableFiles; // if file reader not exist, select from this files to generate a suggestion
	export let hasData;

	export let variables: VariableModel[] = [];

	const variableStore = writable<VariableModel[]>([]);

	variableStore.set(variables);

	const variableConfig: TableConfig<VariableModel> = {
		id: 'variables',
		data: variableStore,
		height: 225
	};



</script>

<div class="flex-col space-y-2">
{#if readableFiles}

	<Header {id} {structureId} {title} {description} {fileReaderExist} {readableFiles} {hasData} on:error/>
{/if}

{#if variables}
	<Table config={variableConfig} />
{/if}
</div>