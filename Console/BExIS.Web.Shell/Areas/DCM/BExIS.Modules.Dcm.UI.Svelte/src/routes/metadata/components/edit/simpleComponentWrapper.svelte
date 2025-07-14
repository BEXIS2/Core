<script lang="ts">
	import { onMount } from 'svelte';
	import { TextInput, NumberInput, TextArea, DropdownKVP, helpStore, CodeEditor } from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	
	import { getValueByPath, setValueByPath,  updateMetadataStore} from '../../utils';
	
	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;

	let label: string = !path ? "" : path.split('.').length > 1  ? path.split('.')[path.split('.').length - 1] : path;

	let metadata: any;

	let value = getValueByPath(path + '.#text');
	$: updateMetadataStore(path, value);

</script>

{#if path && simpleComponent.properties}
	<div class="px-5" id={path}>
		{#if simpleComponent.properties['#text'].type === 'string'}
			<TextInput id={path} label={label} required={required} bind:value={value}/>
		{:else if simpleComponent.properties['#text'].type === 'number'}
			<NumberInput id={path} label={label} required={required} bind:value={value}/>
		{:else if simpleComponent.properties['#text'].type === 'boolean'}
			<SlideToggle name={label} value={false} bind:checked={value}>{label}</SlideToggle>
		{/if}
	</div>
{/if}