<script lang="ts">
	import { onMount } from 'svelte';
	import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';
	import ArrayComponent from './arrayComponentWrapper.svelte';

	export let complexComponent: any;
    export let path: string;
	export let required: boolean = false;
	
	let label: string = path.split('.').length > 1  ? path.split('.')[path.split('.').length - 1] : path;

	let requiredList = complexComponent && complexComponent.type === 'object' && complexComponent.required ? complexComponent.required : [];

</script>
	{#if complexComponent && complexComponent.type === 'object' && complexComponent.properties}
		{#each Object.entries(complexComponent.properties) as [key, value]}
			{@const p = path = path ? path + '.' + key : key}
			{@const l = label = key}
			{#if value.type === 'object' && value.properties && !value.properties['#text']}	
			{#if required}
				<h3 class="h3">{label} *</h3>
			{:else}
			   	<h3 class="h3">{label}</h3>
			{/if}
			<div class="px-5" id="{path}">
				{#if value.oneOf || value.anyOf || value.allOf}					
					<p>Choise</p>
				{:else}
					<ComplexComponent complexComponent={value} path={path} required={requiredList.includes(key)}  />
				{/if}
			</div>
			{:else if value.type === 'object' && value.properties['#text']}
				<SimpleComponent simpleComponent={value} path={path} required={requiredList.includes(key)}/>
			{:else if value.type === 'array' && value.items}
				<ArrayComponent arrayComponent={value} path={path} required={requiredList.includes(key)}/>
			{/if}
		{/each}
	{/if}


