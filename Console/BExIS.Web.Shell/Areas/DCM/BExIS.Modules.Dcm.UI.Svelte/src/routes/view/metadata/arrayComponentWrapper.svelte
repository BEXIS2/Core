<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponent.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';
	import { getNodeByPath } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { slide } from 'svelte/transition';
	import { hideStore, metadataStore } from '$lib/components/utils/metadata/stores';
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
	$: value = ($metadataStore, getNodeByPath(path) == undefined ? [] : getNodeByPath(path));
	let render: boolean = false;

	$: depth = Math.max(0, path.split('.').length - 1);
 	$: leftIndentPx = depth * 8;
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
					

						{#if !$hideStore.includes(path)}
							<div in:slide out:slide id={path}>
								{#if value && value.length > 0}
									{#each value as item, index}
										<div in:slide out:slide>
											<div in:slide out:slide class="arr bg-gray-50 dark:bg-gray-900/50 rounded-sm flex flex-col border border-gray-200 mb-2">
												<ComplexComponent
													complexComponent={arrayComponent.items}
													path={path + '.' + index}
													required={requiredList.includes(label)}
													backgroundClass={backgroundClass || "bg-gray-50 dark:bg-gray-900/50"}
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
								<div class="cont flex-1  min-w-[100px]">
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

<style>
.cont {
  margin-left: 1em;
}
.arr:not(:last-child) {
  padding-bottom: 0.5em;
  border-bottom: 1px solid black;
}
</style>