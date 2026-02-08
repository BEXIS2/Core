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
	import { getValueBySchemaPath, setValueByPath, updateMetadataStore, schemaToJson, toggleShow } from '../../../../lib/components/utils/metadata/metadataComponentUtils';
	import { faPlus, faChevronUp, faChevronDown, faTrash } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { slide, fade } from 'svelte/transition';
	import { hideStore } from '../../../../lib/components/utils/metadata/stores';
	import { convertDisplayName } from './../../metadataShared';

	export let arrayComponent: any;
	export let path: string;

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let requiredList =
		arrayComponent.items && arrayComponent.items.type === 'object' && arrayComponent.items.required
			? arrayComponent.items.required
			: [];

	let value = getValueBySchemaPath(path) == undefined ? [] : getValueBySchemaPath(path);
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
		console.log('insertItemAt', value);
		render = !render;
	}
</script>

{#if arrayComponent.items}
	<div class="" id={path}>
		{#key render}
			{#if arrayComponent.items.type === 'object' && arrayComponent.items.properties && !arrayComponent.items.properties['#text']}
				<div class="grid grid-cols-1 gap-0">
					<div class="card bg-primary-300 dark:bg-primary-800 px-5 py-2 grid grid-cols-2">
						<div class="text-left w-4/5">						
							<h3 class="h3">{convertDisplayName(label, true)}</h3>
						</div>
						<div class="text-right">
							{#if !$hideStore.includes(path)}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {convertDisplayName(label, true)}"
									on:click={() => toggleShow(path)}><Fa icon={faChevronUp} /></button
								>
							{:else}
								<button
									class="h-9 w-10 text-right"
									title="Open or close {convertDisplayName(label, true)}"
									on:click={() => toggleShow(path)}><Fa icon={faChevronDown} /></button
								>
							{/if}
						</div>
					</div>
					{#if !$hideStore.includes(path)}
						<div in:slide out:slide class="card px-5 py-4" id={path}>
						{#if value && value.length > 0}
							{#each value as item, index}
								<div in:slide out:slide class="p-5 card mb-2">
									<div class="grid grid-cols-2 gap-2">
									<div>
										<h3 class="h3 text-primary-500">{convertDisplayName(label, true)} {index+1}</h3>
									</div>
									<div class="text-right w-full pr-2">
										<button
											class="chip variant-filled-primary"
											class:disabled={value.length >= maxItems}
											disabled={value.length >= maxItems}
											on:click={() => insertItemAt(index + 1)}
										>
											<Fa icon={faPlus} />
										</button>
										<button
											class="chip variant-filled-primary"
											class:disabled={index <= 0}
											disabled={index <= 0}
											on:click={() => itemUp(index)}
										>
											<Fa icon={faChevronUp} />
										</button>
										<button
											class="chip variant-filled-primary"
											class:disabled={index >= value.length - 1}
											disabled={index >= value.length - 1}
											on:click={() => itemDown(index)}
										>
											<Fa icon={faChevronDown} />
										</button>
										<button
											class="chip variant-filled-primary"
											class:disabled={value.length <= minItems}
											disabled={value.length <= minItems}
											on:click={() => removeItem(index)}
										>
											<Fa icon={faTrash} />
										</button>
									</div>
									</div>
									<div>
										<ComplexComponent
											complexComponent={arrayComponent.items}
											path={path + '.' + index}
											required={requiredList.includes(label)}
										/>
									</div>
								</div>
							{/each}
						{/if}
						</div>
					{/if}
				</div>
			{:else if arrayComponent.items.type === 'object' && arrayComponent.items.properties['#text']}
				{#if value && value.length > 0}
					{#each value as item, index}
						<div in:slide out:slide class="py-2">
							<div class="flex flex-col md:flex-row md:items-center gap-2">
								<div class="flex-1 min-w-[100px]">
									<SimpleComponent
										simpleComponent={arrayComponent.items}
										path={path + '.' + index}
										value={getValueBySchemaPath(path + '.' + index + '.#text')}
										{label}
										required={requiredList.includes(label)}
									/>
								</div>
								<div class="flex shrink-0 gap-1 justify-end pr-4">
								<button
									class="chip variant-filled-primary"
									class:disabled={value.length >= maxItems}
									disabled={value.length >= maxItems}
									on:click={() => insertItemAt(index + 1)}
								>
									<Fa icon={faPlus} />
								</button>
								<button
									class="chip variant-filled-primary"
									class:disabled={index <= 0}
									disabled={index <= 0}
									on:click={() => itemUp(index)}
								>
									<Fa icon={faChevronUp} />
								</button>
								<button
									class="chip variant-filled-primary"
									class:disabled={index >= value.length - 1}
									disabled={index >= value.length - 1}
									on:click={() => itemDown(index)}
								>
									<Fa icon={faChevronDown} />
								</button>
								<button
									class="chip variant-filled-primary"
									class:disabled={value.length <= minItems}
									disabled={value.length <= minItems}
									on:click={() => removeItem(index)}
								>
									<Fa icon={faTrash} />
								</button>
							</div>
							</div>
						</div>
					{/each}
				{/if}
			{/if}
		{/key}
	</div>
{/if}
