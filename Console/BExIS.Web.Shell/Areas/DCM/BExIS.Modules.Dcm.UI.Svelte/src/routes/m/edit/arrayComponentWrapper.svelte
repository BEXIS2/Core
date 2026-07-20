<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';
	import { schemaToJson, getNodeByPath, getByPath} from '$lib/components/utils/metadata/metadataComponentUtils';
	import { faPlus, faChevronUp, faChevronDown, faTrash } from '@fortawesome/free-solid-svg-icons';
	import Fa from 'svelte-fa';
	import { slide, fade } from 'svelte/transition';
	import { activeStore, hideStore, validationStore } from '$lib/components/utils/metadata/stores';
	import { convertDisplayName } from '../../../lib/components/utils/metadata/metadataShared';
	import Header from './MetadataComponentHeader.svelte';


	export let arrayComponent: any;
	export let path: string;
	export let required: boolean = false;

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;

	let value = getNodeByPath(path) == undefined ? [] : getNodeByPath(path);
	let render: boolean = false;

	let maxItems: number = arrayComponent.maxItems ? arrayComponent.maxItems : 2147483647;
	let minItems: number = arrayComponent.minItems ? arrayComponent.minItems : 1;

 
	function addItem(idx: number) {
		value.push(schemaToJson(arrayComponent.items));
		render = !render;
	}

function removeFromValidationStore(path: string) {
	validationStore.update(store => {
		if (!store) {
			return store;
		}

		return {
			...store,
			simpleTypeValidationItems: store.simpleTypeValidationItems.filter(item => !item.path.startsWith(path)),
			complexTypeValidationItems: store.complexTypeValidationItems.filter(item => !item.path.startsWith(path))
		};
	});
}

	function removeItem(idx: number) {
		value.splice(idx, 1);
		render = !render;
		// Remove stale validation entries for this array branch.
		removeFromValidationStore(path);
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
					
					 <Header	path={path} {required} />

						{#if !$hideStore.includes(path) && $activeStore.includes(path)}
								<div in:slide out:slide class="card pl-5 py-1" id={path}>						
								{#if value && value.length > 0}
									{#each value as item, index}
									{@const p = path + '.'+ index}
										<div in:slide out:slide class="pl-5 py-5 card mb-2">
											<div class="grid grid-cols-2 gap-2">
											<div>
												<h4 class="h4 text-primary-500">{index+1}.&nbsp;{convertDisplayName(label, true)}</h4>
											</div>
											<div class="text-right w-full pr-2">
												<button
													class="chip variant-filled-primary"
													class:disabled={value.length >= maxItems}
													disabled={value.length >= maxItems}
													on:click={() => insertItemAt(index + 1)}
													title="Add a new {convertDisplayName(label, true)} entry after this entry"
												>
													<Fa icon={faPlus} />
												</button>
												<button
													class="chip variant-filled-primary"
													class:disabled={index <= 0}
													disabled={index <= 0}
													on:click={() => itemUp(index)}
													title="Move {convertDisplayName(label, true)} entry up"
												>
													<Fa icon={faChevronUp} />
												</button>
												<button
													class="chip variant-filled-primary"
													class:disabled={index >= value.length - 1}
													disabled={index >= value.length - 1}
													on:click={() => itemDown(index)}
													title="Move {convertDisplayName(label, true)} entry down"
												>
													<Fa icon={faChevronDown} />
												</button>
												<button
													class="chip variant-filled-primary"
													class:disabled={value.length <= minItems}
													disabled={value.length <= minItems}
													on:click={() => removeItem(index)}
													title="Remove {convertDisplayName(label, true)} entry"
												>
													<Fa icon={faTrash} />
												</button>
											</div>
											</div>
											<div>		
							
												<ComplexComponent
													complexComponent={arrayComponent.items}
													path={path + '.' + index}
													required={required}
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
				{#if arrayComponent.items.properties['#text'].enum	!= undefined}

				<SimpleComponent
										simpleComponent={arrayComponent.items}
										path={path}
										value={getNodeByPath(path)}
										{label}
										required={required}
										isMulti={true}							
									/>
				{:else}

				
					{#each value as item, index}
						<div in:slide out:slide class="py-1">
							<div class="flex flex-col md:flex-row md:items-center gap-2">
								<div class="flex-1 min-w-[100px]">
									<SimpleComponent
										simpleComponent={arrayComponent.items}
										path={path + '.' + index}
										value={getNodeByPath(path + '.' + index + '.#text')}
										{label}
										required={required}
									/>
								</div>
								<div class="flex shrink-0 gap-1 justify-end pr-4">
								<button
									class="chip variant-filled-primary"
									class:disabled={value.length >= maxItems}
									disabled={value.length >= maxItems}
									on:click={() => insertItemAt(index + 1)}
									title="Add a new {convertDisplayName(label, true)} entry after this entry"
								>
									<Fa icon={faPlus} />
								</button>
								<button
									class="chip variant-filled-primary"
									class:disabled={index <= 0}
									disabled={index <= 0}
									on:click={() => itemUp(index)}
									title="Move {convertDisplayName(label, true)} entry up"
								>
									<Fa icon={faChevronUp} />
								</button>
								<button
									class="chip variant-filled-primary"
									class:disabled={index >= value.length - 1}
									disabled={index >= value.length - 1}
									on:click={() => itemDown(index)}
									title="Move {convertDisplayName(label, true)} entry down"
								>
									<Fa icon={faChevronDown} />
								</button>
								<button
									class="chip variant-filled-primary"
									class:disabled={value.length <= minItems}
									disabled={value.length <= minItems}
									on:click={() => removeItem(index)}
									title="Remove {convertDisplayName(label, true)} entry"
								>
									<Fa icon={faTrash} />
								</button>
							</div>
							</div>
						</div>
					{/each}
					{/if}
				{/if}
			{/if}
		{/key}
	</div>
{/if}
