<script lang="ts">
	import { onMount } from 'svelte';
	import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
	
	export let simpleComponent: any;
	export let path: string;

	let label: string = !path ? "" : path.split('.').length > 1  ? path.split('.')[path.split('.').length - 1] : path;

	console.log('simpleComponent', simpleComponent);
</script>

{#if path && simpleComponent.properties}
	<div class="">
		{#if simpleComponent.properties['#text'].type === 'string'}
			<TextInput id={path} label={label} value={path} />
		{:else if simpleComponent.properties['#text'].type === 'number'}
			<NumberInput id={path} label={label} value={path.length} />
		{/if}
	</div>
{/if}