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
	import SimpleComponent from './simpleComponent.svelte';
	import { getValueByPath, setValueByPath, updateMetadataStore, schemaToJson } from '../../utils';
	import { faPlus, faChevronUp, faChevronDown, faTrash } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { slide, fade } from 'svelte/transition';

	export let arrayComponent: any;
	export let path: string;

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let requiredList =
		arrayComponent.items && arrayComponent.items.type === 'object' && arrayComponent.items.required
			? arrayComponent.items.required
			: [];

	let value = getValueByPath(path) == undefined ? [] : getValueByPath(path);
	let render: boolean = false;

	let maxItems: number = arrayComponent.maxItems ? arrayComponent.maxItems : 2147483647;
	let minItems: number = arrayComponent.minItems ? arrayComponent.minItems : 1;

	function addItem(idx: number) {
		value.push(schemaToJson(arrayComponent.items));
		render = !render;
	}

	function removeItem(idx: number) {
		value.splice(idx, 1);
		render = !render;
	}

	function itemUp(idx: number) {
		if (idx > 0) {
			let temp = value[idx];
			value[idx] = value[idx - 1];
			value[idx - 1] = temp;
			render = !render;
		}
	}

	function itemDown(idx: number) {
		if (idx < value.length - 1) {
			let temp = value[idx];
			value[idx] = value[idx + 1];
			value[idx + 1] = temp;
			render = !render;
		}
	}

	function insertItemAt(index: number) {
		value.splice(index, 0, schemaToJson(arrayComponent.items));
		render = !render;
	}
</script>

{#if arrayComponent.items}
	<div class="p-2" id={path}>
		{#key render}
			{#if value && value.length > 0}
				{#each value as item, index}
					<div in:slide out:slide class="p-5">
						<div class="text-right w-full">
							{#if value.length < maxItems}
								<button class="chip variant-filled-primary" on:click={() => insertItemAt(index + 1)}
									><Fa icon={faPlus} /></button
								>
							{:else}
								<button class="chip variant-filled-primary disabled" disabled
									><Fa icon={faPlus} /></button
								>
							{/if}
							{#if index > 0}
								<button class="chip variant-filled-primary" on:click={() => itemUp(index)}
									><Fa icon={faChevronUp} /></button
								>
							{:else}
								<button class="chip variant-filled-primary disabled" disabled
									><Fa icon={faChevronUp} /></button
								>
							{/if}
							{#if index < value.length - 1}
								<button class="chip variant-filled-primary" on:click={() => itemDown(index)}
									><Fa icon={faChevronDown} /></button
								>
							{:else}
								<button class="chip variant-filled-primary disabled" disabled
									><Fa icon={faChevronDown} /></button
								>
							{/if}
							{#if value.length > minItems}
								<button class="chip variant-filled-primary" on:click={() => removeItem(index)}
									><Fa icon={faTrash} /></button
								>
							{:else}
								<button class="chip variant-filled-primary disabled" disabled
									><Fa icon={faTrash} /></button
								>
							{/if}
						</div>
						{#if arrayComponent.items.type === 'object' && arrayComponent.items.properties && !arrayComponent.items.properties['#text']}
							<ComplexComponent
								complexComponent={arrayComponent.items}
								path={path + '.' + index}
								required={requiredList.includes(label)}
							/>
						{:else if arrayComponent.items.type === 'object' && arrayComponent.items.properties['#text']}
							<SimpleComponent
								simpleComponent={arrayComponent.items}
								path={path + '.' + index}
								value={getValueByPath(path + '.' + index + '.#text')}
								{label}
								required={requiredList.includes(label)}
							/>
						{/if}
					</div>
				{/each}
			{/if}
		{/key}
	</div>
{/if}
