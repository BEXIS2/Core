<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';
	import ArrayComponent from './arrayComponentWrapper.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';

	import { slide, fade } from 'svelte/transition';
	import { activeStore, hideStore, metadataStore } from '$lib/components/utils/metadata/stores';
	import Header from './MetadataComponentHeader.svelte';
	import { getValueByPath, hasValue, hasValueAtPath } from '$lib/components/utils/metadata/metadataComponentUtils';

	export let complexComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let backgroundClass: string = '';

	let label: string =
		path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;

	let requiredList =
		complexComponent && complexComponent.type === 'object' && complexComponent.required
			? complexComponent.required
			: [];

	$: propertyEntries = Object.entries(complexComponent?.properties ?? {}) as [string, any][];

</script>


{#if complexComponent && complexComponent.type === 'object' && complexComponent.properties}
	{#each propertyEntries as [key, value]}
		{@const p = path ? path + '.' + key : key}
		{@const l = key}
		{#if value.type === 'object' && value.properties && !value.properties['#text'] && hasValueAtPath(p) } 
		<!--&& hasValueAtPath(p)-->
			<div
		
			>
				{#if value.oneOf || value.anyOf || value.allOf}
					<ChoiceComponent choiceComponent={value} path={p} />
				{:else}
					<!--<div class="grid grid-cols-1 card  gap-0 ">-->
					<!--only show if childs do have values-->

					<Header {required} path={p} {p} description={value.description} />
					{#if !$hideStore.includes(p) && $activeStore.includes(p)}
						<div class="pl-2  dark:bg-gray-900/50 rounded-sm flex flex-col" id={p}>
							<ComplexComponent
								complexComponent={value}
								path={p}
								required={requiredList.includes(key)}
							/>
						</div>
					{/if}
				{/if}
			</div>
		{:else if value.type === 'object' && value.properties['#text']}
			{#if hasValueAtPath(p)}
				<div class="pl-2">
					<div class=" md:items-center gap-2">
						<div class="cont">
							<SimpleComponent
								simpleComponent={value}
								path={p}
								required={requiredList.includes(key)}
								backgroundClass={backgroundClass}
							/>
						</div>
					</div>
				</div>
			{/if}
		{:else if value.type === 'array' && value.items}
			<div
				class="pl-2 dark:bg-gray-900/50  "
			>
				<ArrayComponent arrayComponent={value} path={p} backgroundClass={backgroundClass} />
			</div>
		{/if}
	{/each}
{/if}

<style>
.cont {
  margin-left: 1em;
}
</style>