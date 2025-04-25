<script lang="ts">
	import { onMount } from 'svelte';
	import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
	import ComplexComponent from './complexComponentWraper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';
	import ArrayComponent from './arrayComponentWrapper.svelte';

	export let complexComponent: any;
    export let path: string;
	
	let label: string = path.split('.').length > 1  ? path.split('.')[path.split('.').length - 1] : path;
</script>
	{#if complexComponent.type === 'object' && complexComponent.properties}
		{#each Object.entries(complexComponent.properties) as [key, value]}
			{@const path= path ? path + '.' + key : key}
			{#if value.type === 'object' && value.properties && !value.properties['#text']}
			<div class="pb-3" id="{path}">
				<h3 class="h3">{key}</h3>
				<ComplexComponent complexComponent={value} path={path}  />
			</div>
			{:else if value.type === 'object' && value.properties['#text']}
				<div class="pb-3">
				<SimpleComponent simpleComponent={value} path={path}/>
				</div>
			{:else if value.type === 'array' && value.items}
				<ArrayComponent arrayComponent={value} path={path} />
			{/if}
		{/each}
	{/if}


