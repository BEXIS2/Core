<script lang="ts">
	import { onMount } from 'svelte';
	import {
		TextInput,
		NumberInput,
		TextArea,
		DropdownKVP,
		helpStore,
		CodeEditor
	} from '@bexis2/bexis2-core-ui';
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';

	export let choiceComponent: any;
	export let path: string;
	export let required: boolean = false;

	console.log('path', path);
	console.log('required', required);

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
</script>

{#if choiceComponent.items}
	<h4 class="h4">{label} +|-</h4>
	<div class="p-2" id={path}>
		{#if choiceComponent.items.type === 'object' && choiceComponent.items.properties && !choiceComponent.items.properties['#text']}
			<ComplexComponent complexComponent={choiceComponent.items} {path} />
		{:else if choiceComponent.items.type === 'object' && choiceComponent.items.properties['#text']}
			<SimpleComponent simpleComponent={choiceComponent.items} {path} />
		{/if}
	</div>
{/if}
