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

	console.log('ChoiceComponent', choiceComponent);

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
</script>

{#if choiceComponent.anyOf}
	<h4 class="h4">{label} +|-</h4>
	{#each choiceComponent.anyOf as item}
		<div>
			{item['$ref'].split('/').length - 1}
		</div>
	{/each}
{:else if choiceComponent.oneOf}
	<h4 class="h4">{label} +|-</h4>
	{#each choiceComponent.oneOf as item}
		<div>
			{item['$ref'].split('/').length - 1}
		</div>
	{/each}
{/if}
