<script lang="ts">
	import {
		TextInput,
		NumberInput,
		TextArea,
		DropdownKVP,
		helpStore,
		CodeEditor
	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';

	import { getValueByPath, setValueByPath, updateMetadataStore } from '../../utils';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let value: any;
	export let label: string;

	$: updateMetadataStore(path, value);
</script>

{#if path && simpleComponent.properties}
	<div class="px-5" id={path}>
		{#if simpleComponent.properties['#text'].type === 'string'}
			<TextInput id={path} {label} {required} bind:value />
		{:else if simpleComponent.properties['#text'].type === 'number'}
			<NumberInput id={path} {label} {required} bind:value />
		{:else if simpleComponent.properties['#text'].type === 'boolean'}
			<SlideToggle id={path} name={label} value={false} bind:checked={value}>{label}</SlideToggle>
		{/if}
	</div>
{/if}
