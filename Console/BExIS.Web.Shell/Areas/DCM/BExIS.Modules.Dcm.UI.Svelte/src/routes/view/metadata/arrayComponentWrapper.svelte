<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';
	import { getNodeByPath } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { slide } from 'svelte/transition';
	import { hideStore, metadataStore } from '$lib/components/utils/metadata/stores';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';
	import Header from './MetadataComponentHeader.svelte';

	export let arrayComponent: any;
	export let path: string;
	export let backgroundClass: string = '';

	let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
	let requiredList =
		arrayComponent.items && arrayComponent.items.type === 'object' && arrayComponent.items.required
			? arrayComponent.items.required
			: [];

	let value = getNodeByPath(path) == undefined ? [] : getNodeByPath(path);
	// $: value = ($metadataStore, getNodeByPath(path) == undefined ? [] : getNodeByPath(path));
	let render: boolean = false;

	$: depth = Math.max(0, path.split('.').length - 1);
 	$: leftIndentPx = depth * 12;
</script>

{#if arrayComponent.items}
	<div class="" id={path}>
		{#key render}
			{#if arrayComponent.items.type === 'object' && arrayComponent.items.properties && !arrayComponent.items.properties['#text']}
				<div class="grid  grid-cols-1 gap-0">
				 <!-- <b>array : {$activeStore.includes(path)}</b> -->
					{#if arrayComponent.items.anyOf || arrayComponent.items.allOf}

						<ChoiceComponent choiceComponent={arrayComponent} {path} />
					{:else}

					{#if !$hideStore.includes(path) }
				
					<!-- <Header	path={path} required={requiredList.includes(label)} /> -->

						<div in:slide out:slide id={path}>	
		
						{#if value && value.length > 0}
							{#each value as item, index}
								<div in:slide out:slide>
									<!-- <div class="grid pt-2 grid-cols-2 gap-2"> -->
									<div class="grid pt-2 grid-cols-2 gap-2">
									{#if index == 0}
									<Header
										path={path }
										required={requiredList.includes(label)}
										p={path}
										description={arrayComponent.items.description}
									/>
									{/if}		
									</div>
									
									 <div in:slide out:slide class="bg-gray-50 dark:bg-gray-900/50 rounded-xl flex flex-col border border-gray-200 pr-2	" style={`margin-left: ${leftIndentPx}px`}>
										<ComplexComponent
											complexComponent={arrayComponent.items}
											path={path + '.' + index}
											required={requiredList.includes(label)}
											backgroundClass={"bg-gray-50 dark:bg-gray-900/50"}
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
						<div in:slide out:slide >
							<div class="flex flex-col md:flex-row md:items-center gap-2">
								<div class="flex-1  min-w-[100px]">
									<SimpleComponent
										simpleComponent={arrayComponent.items}
										path={path}
										value={value.map(item => item["#text"]).join(", ")}
										{label}
										required={requiredList.includes(label)}
									/>
								</div>
							</div>
						</div>
				{/if}
			{/if}
		{/key}
	</div>
{/if}
