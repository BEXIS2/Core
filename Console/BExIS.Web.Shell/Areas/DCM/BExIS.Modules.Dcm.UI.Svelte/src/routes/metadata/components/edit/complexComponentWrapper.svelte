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
	import { hideStore } from '../../stores';

	export let complexComponent: any;
	export let path: string;
	export let required: boolean = false;

	let label: string =
		path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let requiredList =
		complexComponent && complexComponent.type === 'object' && complexComponent.required
			? complexComponent.required
			: [];

	function toggleShow(path: string) {
		let hideStoreValue: string[] = $hideStore;
		if (hideStoreValue.includes(path)) {
			let idx = hideStoreValue.findIndex((x) => x == path);
			if (idx > -1) hideStoreValue.splice(idx, 1);
		} else {
			hideStoreValue.push(path);
		}
		hideStore.set(hideStoreValue);
	}
</script>

{#if complexComponent && complexComponent.type === 'object' && complexComponent.properties}
	{#each Object.entries(complexComponent.properties) as [key, value]}
		{@const p = path = path ? path + '.' + key : key}
		{@const l = label = key}
		{#if value.type === 'object' && value.properties && !value.properties['#text']}
			{#if value.oneOf || value.anyOf || value.allOf}
				<ChoiceComponent choiceComponent={value} {path} />
			{:else}
				<div class="grid grid-cols-1 gap-0 m-2">
					<div class="card bg-primary-300 dark:bg-primary-800 px-5 py-2 grid grid-cols-2">
						<div class="text-left w-4/5">
							{#if required}
								<h3 class="h3">{label} *</h3>
							{:else}
								<h3 class="h3">{label}</h3>
							{/if}
						</div>
						<div class="text-right">
							{#if !$hideStore.includes(path)}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {label}"
									on:click={() => toggleShow(p)}><Fa icon={faChevronUp} /></button
								>
							{:else}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {label}"
									on:click={() => toggleShow(p)}><Fa icon={faChevronDown} /></button
								>
							{/if}
						</div>
					</div>
					{#if !$hideStore.includes(path)}
						<div in:slide out:slide class="card px-5 py-4" id={path}>
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
			<div class="px-5 py-2">
				<SimpleComponent simpleComponent={value} {path} required={requiredList.includes(key)} />
			</div>
		{:else if value.type === 'array' && value.items}
			<ArrayComponent arrayComponent={value} {path} />
		{/if}
	{/each}
{/if}
