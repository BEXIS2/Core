<script lang="ts">
	import { onMount } from 'svelte';
	import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
	import FormGenerator from './formGenerator.svelte';
	import ArrayFormGenerator from './arrayFormGenerator.svelte';

	export let schema: any;
    export let path: string;
	export let colorToggle: boolean = false;
	
	let bgColor: string = colorToggle ? 'bg-primary-200' : 'bg-primary-400';
	let label: string = path.split('.').length > 1  ? path.split('.')[path.split('.').length - 1] : path;

	console.log('label',label);
</script>

<div>
	{#if schema.type === 'object' && schema.properties}
		{#each Object.entries(schema.properties) as [key, value]}
			{@const path= path ? path + '.' + key : key}
			<div class="p-2 {bgColor}" id="{path}">
			{#if value.type === 'object' && value.properties}
				{key}
				<FormGenerator schema={value} path={path} colorToggle={!colorToggle}  />
			{:else if !key.startsWith('@')}
				<div class="p-2 bg-surface-100">
					{#if value.type === 'string'}
						<TextInput id={path} label={label} />
					{:else if value.type === 'number'}
						<NumberInput id={path} label={label} />
					{:else if value.type === 'array'}
						<ArrayFormGenerator schema={value} path={path} colorToggle={!colorToggle}  />						
					{:else}
						fail
					{/if}
				</div>
			{/if}
			</div>
		{/each}
	{/if}
</div>

