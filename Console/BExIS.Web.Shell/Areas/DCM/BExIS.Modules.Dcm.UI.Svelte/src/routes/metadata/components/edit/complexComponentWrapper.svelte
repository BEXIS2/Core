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
	import ArrayComponent from './arrayComponentWrapper.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';
	import { faPlus, faChevronUp, faChevronDown } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { slide, fade } from 'svelte/transition';
	import { hideStore } from '../../../../lib/components/utils/metadata/stores';
	import { toggleShow } from '../../../../lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from './../../metadataShared';

	export let complexComponent: any;
	export let path: string;
	export let required: boolean = false;

	let label: string =
		path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let requiredList =
		complexComponent && complexComponent.type === 'object' && complexComponent.required
			? complexComponent.required
			: [];
</script>

{#if complexComponent && complexComponent.type === 'object' && complexComponent.properties}
	{#each Object.entries(complexComponent.properties) as [key, value]}
		{@const p = path = path ? path + '.' + key : key}
		{@const l = label = key}
		{#if value.type === 'object' && value.properties && !value.properties['#text']}
			{#if value.oneOf || value.anyOf || value.allOf}
				<ChoiceComponent choiceComponent={value} {path} />
			{:else}
				<div class="grid grid-cols-1 gap-0 ">
					<div class="card bg-primary-300 dark:bg-primary-800 pl-5 py-2 grid grid-cols-2">
						<div class="text-left w-4/5">
							{#if required}
								<h4 class="h4">{convertDisplayName(label, true)} *</h4>
							{:else}
								<h4 class="h4">{convertDisplayName(label, true)}</h4>
							{/if}
			
						</div>
						<div class="text-left flex justify-end w-2">
							{#if !$hideStore.includes(path)}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {convertDisplayName(label, true)}"
									on:click={() => toggleShow(p)}><Fa icon={faChevronUp} /></button
								>
							{:else}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {convertDisplayName(label, true)}"
									on:click={() => toggleShow(p)}><Fa icon={faChevronDown} /></button
								>
							{/if}
						</div>
					</div>
					{#if !$hideStore.includes(path)}
						<div in:slide out:slide class="card pl-5 py-4" id={path}>
							<ComplexComponent
								complexComponent={value}
								{path}
								required={requiredList.includes(key)}
							/>
						</div>
					{/if}
				</div>
			{/if}
		{:else if value.type === 'object' && value.properties['#text']}
			<div class="mb-2">
				<div class="flex flex-col md:flex-row md:items-center gap-2">
					<div class="flex-1 min-w-[100px]">
						<SimpleComponent simpleComponent={value} {path} required={requiredList.includes(key)} />
					</div>
				</div>
			
			</div>
		{:else if value.type === 'array' && value.items}
			<ArrayComponent arrayComponent={value} {path} />
		{/if}
	{/each}
	
{/if}
