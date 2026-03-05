<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';
	import { setValueByPath, updateMetadataStore, schemaToJson, toggleShow, getNodeByPath } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { faPlus, faChevronUp, faChevronDown, faTrash } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { slide, fade } from 'svelte/transition';
	import { hideStore } from '$lib/components/utils/metadata/stores';
	import { convertDisplayName } from '../metadataShared';
	import Header from './MetadataComponentHeader.svelte';

	export let arrayComponent: any;
	export let path: string;

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let requiredList =
		arrayComponent.items && arrayComponent.items.type === 'object' && arrayComponent.items.required
			? arrayComponent.items.required
			: [];

	let value = getNodeByPath(path) == undefined ? [] : getNodeByPath(path);
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
					{#if arrayComponent.items.anyOf || arrayComponent.items.allOf}
					 <ChoiceComponent choiceComponent={arrayComponent} {path} />
					{:else}

					<Header	path={path} required={requiredList.includes(label)} />
					{#if !$hideStore.includes(path)}
						<div in:slide out:slide class="card pl-5 py-2" id={path}>						
						{#if value && value.length > 0}
							{#each value as item, index}
								<div in:slide out:slide class="pl-5 py-5 card mb-2">
									<div class="grid grid-cols-2 gap-2">
									<div>
										<h4 class="h4 text-primary-500">{convertDisplayName(label, true)} {index+1}</h4>
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
										value={getNodeByPath(path + '.' + index + '.#text')}
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
