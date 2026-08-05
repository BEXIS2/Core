<script lang="ts">
	import {
		TextInput,
		NumberInput,
		TextArea,
		DropdownKVP,
		Dropdown,
		helpStore,
		CodeEditor,
		MultiSelect
	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { onMount } from 'svelte';
	import { getConfigStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { customComponentsCatalog } from '$lib/components/customComponents/componentCatalog';
	import type { SimpleComponentData } from '$lib/components/utils/metadata/models';
	import SveltyPicker from 'svelty-picker';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';

	//import { en, de } from 'svelty-picker/dist/i18n';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let value: any;
	export let label: string;
	export let backgroundClass: string = '';

	$: depth = Math.max(0, path.split('.').length - 1);
	$: leftIndentPx = depth * 12;
	$: lastPathPart = path.split('.').pop() ?? '';

	let date: Date = undefined as unknown as Date;
	// load form result object
	let config: any;
	let isAnchor: boolean = false;
	let isVisible: boolean = true;
	let customComponent: any;
	let min: number | undefined = -10000000;
	let max: number | undefined = 1000000;

	onMount(async () => {
		// checks for date
		if (
			simpleComponent.properties['#text'].format === 'date' ||
			simpleComponent.properties['#text'].format === 'datetime' ||
			simpleComponent.properties['#text'].format === 'date and time' ||
			simpleComponent.properties['#text'].format === 'time'
		) {
			// console.log("date format detected, set date value", value, value as Date);
			date = value !== undefined || value == '' ? (value as Date) : (Date.now() as unknown as Date);
			// console.log("date format detected, set date", date);
		}

		// numeric - set min and max if exist	in schema
		if (simpleComponent.properties['#text'].minimum !== undefined) {
			min = simpleComponent.properties['#text'].minimum;
		}
		if (simpleComponent.properties['#text'].maximum !== undefined) {
			max = simpleComponent.properties['#text'].maximum;
		}

		//#### CONFIGURATION	 ####
		config = getConfigStore();
		// check if this component is an anchor point
		//console.log("check for anchorpoin", config)
		for (const component of config.components) {
			//console.log("ghjgJ", component.globalSettings.anchorpoint, path)
			if (component.globalSettings.anchorpoint == path) {
				isAnchor = true;
				customComponent = customComponentsCatalog[component.meta.component_name].component;
			}
			for (const variable of component.mode.variables.variable) {
				if (variable.JSONPath == path && variable.is_visible == false) {
					isVisible = false;
				}
			}
		}
	});
</script>

<!-- Simple Component Rendering [200px_1fr] [188px_1fr] -->
{#if isVisible && !isAnchor}
	<div 
  class="grid grid-cols-[var(--indent-width)_1fr] gap-4 py-1" 
  style="--indent-width: calc(200px - {leftIndentPx}px);"
>
		{#if path.split('.').length > 1 && !isNaN(parseInt(lastPathPart))}
			{#if lastPathPart === '0'}
				<span class="text-sm font-medium text-gray-500" style={`padding-left: ${leftIndentPx -8 }px`}
					>{convertDisplayName(label)}</span
				>
			{:else}
				<span class="text-sm font-medium text-gray-500" style={`padding-left: ${leftIndentPx - 8}px`}
				></span>
			{/if}
		{:else}
			<span class="text-sm font-medium text-gray-500" style={`padding-left: ${leftIndentPx}px`}
				>{convertDisplayName(label)}</span
			>
		{/if}
		<span class="text-sm text-gray-900 font-semibold">{value}</span>
	</div>
{:else if isAnchor}
	<div class="" id={path}>
		<svelte:component this={customComponent} anchor={path} label={convertDisplayName(label)} />
	</div>
{/if}
