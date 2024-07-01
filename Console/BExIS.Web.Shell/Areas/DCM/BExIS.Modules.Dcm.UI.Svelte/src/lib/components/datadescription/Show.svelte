<script lang="ts">
	import type { VariableModel } from '$models/DataDescription';
	import Header from './ShowHeader.svelte';
	import { Table } from '@bexis2/bexis2-core-ui';
	import type { TableConfig } from '@bexis2/bexis2-core-ui';
	import { dataset_dev } from 'svelte/internal';
	import { writable } from 'svelte/store';
	import NameTableCol from './NameTableCol.svelte';

	export let id; //enityid
	export let title;
	export let description;
	export let structureId;
	export let fileReaderExist; // if filereader not exist, need to set
	export let readableFiles; // if file reader not exist, select from this files to generate a suggestion
	export let hasData;
	export let enableEdit;

	export let variables: VariableModel[] = [];

	const variableStore = writable<VariableModel[]>([]);

	variableStore.set(variables);
	console.log('🚀 ~ variables:', variables);

	const variableConfig: TableConfig<VariableModel> = {
		id: 'variables',
		data: variableStore,
		height: 225,
		columns: {
			id: {
				fixedWidth: 100
			},
			name: {
				instructions: {
					renderComponent: NameTableCol
				}
			},
			isKeys: {
				exclude: true,
				disableFiltering: true
			}
		}
	};
</script>

<div class="flex-col space-y-2">
	{#if readableFiles}
		<Header
			{id}
			{structureId}
			{title}
			{description}
			{fileReaderExist}
			{readableFiles}
			{hasData}
			{enableEdit}
			on:error
		/>
	{/if}

	{#if variables}
		<Table config={variableConfig} />
	{/if}
</div>
