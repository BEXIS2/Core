<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';
	import ArrayComponent from './arrayComponentWrapper.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';

	import { slide, fade } from 'svelte/transition';
	import { activeStore, hideStore, metadataStore } from '$lib/components/utils/metadata/stores';
	import Header from './MetadataComponentHeader.svelte';
	import { getValueByPath, hasValue } from '$lib/components/utils/metadata/metadataComponentUtils';

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

<!--<section class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl p-6 shadow-sm flex flex-col gap-4">
    
    <h3 class="text-lg font-bold text-gray-800 dark:text-gray-100 border-b border-gray-150 pb-2">
        Creator
    </h3>
    
    <div class="grid grid-cols-[200px_1fr] gap-4 py-1">
        <span class="text-sm font-semibold text-gray-500">Organization name</span>
        <span class="text-sm text-gray-900 font-medium">qweqwe</span>
    </div>

    <div class="mt-2 ml-4 pl-4 border-l-2 border-blue-500/40 bg-gray-50/50 dark:bg-gray-900/30 rounded-r-lg p-3">
        <h4 class="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3">
            Individual Name
        </h4>
        
        <div class="flex flex-col gap-3">
            <div class="grid grid-cols-[180px_1fr] gap-4">
                <span class="text-sm font-medium text-gray-500">Given name</span>
                <span class="text-sm text-gray-900 font-semibold">Franziska</span>
            </div>
            <div class="grid grid-cols-[180px_1fr] gap-4">
                <span class="text-sm font-medium text-gray-500">Sur name</span>
                <span class="text-sm text-gray-900 font-semibold">Zander</span>
            </div>
        </div>
    </div>
    
</section>

<div class="bg-gray-50 dark:bg-gray-900/50 border border-gray-200 p-6 rounded-xl flex flex-col gap-4">
    <h3 class="text-lg font-bold text-gray-800">Metadata Provider</h3>
    
    <div class="grid grid-cols-[200px_1fr] gap-4">
        <span class="text-sm font-semibold text-gray-500">Organization</span>
        <span class="text-sm text-gray-900 font-medium">Gov Institute</span>
    </div>

    <div class="bg-white dark:bg-gray-800 border border-gray-150 p-4 rounded-lg shadow-sm">
        <h4 class="text-sm font-bold text-gray-700 mb-3">Individual Contact</h4>
        <div class="grid grid-cols-[180px_1fr] gap-3">
            <span class="text-sm text-gray-500">Name</span>
            <span class="text-sm text-gray-900">Zander</span>
        </div>
    </div>

    <div class="bg-white dark:bg-gray-800 border border-gray-150 p-4 rounded-lg shadow-sm">
        <h4 class="text-sm font-bold text-gray-700 mb-3">Address Details</h4>
        <div class="grid grid-cols-[180px_1fr] gap-3">
            <span class="text-sm text-gray-500">City</span>
            <span class="text-sm text-gray-900">Jena</span>
        </div>
    </div>
</div>-->
{#if complexComponent && complexComponent.type === 'object' && complexComponent.properties}
	{#each Object.entries(complexComponent.properties) as [key, value]}
		{@const p = path = path ? path + '.' + key : key}
		{@const l = label = key}
		{#if value.type === 'object' && value.properties && !value.properties['#text']}
			<div
				class="bg-gray-50 dark:bg-gray-900/50 border border-gray-200 p-2 rounded-xl flex flex-col ml-2"
			>
				{#if value.oneOf || value.anyOf || value.allOf}
					<ChoiceComponent choiceComponent={value} {path} />
				{:else}
					<!--<div class="grid grid-cols-1 card  gap-0 ">-->
					<!--only show if childs do have values-->

					{#if !$hideStore.includes(path) && $activeStore.includes(path)}
						<Header {required} {path} {p} description={value.description} />

						<div in:slide out:slide class="" id={path}>
							<ComplexComponent
								complexComponent={value}
								{path}
								required={requiredList.includes(key)}
							/>
						</div>
					{:else}
						<Header {required} {path} {p} description={value.description} />

						<div in:slide out:slide class="" id={path}>
							<ComplexComponent
								complexComponent={value}
								{path}
								required={requiredList.includes(key)}
							/>
						</div>
					{/if}
				{/if}
			</div>
		{:else if value.type === 'object' && value.properties['#text']}
			{#if hasValue(path)}
				<div class="mb-2">
					<div class="flex flex-col pl-5 md:flex-row md:items-center gap-2">
						<div class="flex-1 min-w-[100px]">
							<SimpleComponent
								simpleComponent={value}
								{path}
								required={requiredList.includes(key)}
							/>
						</div>
					</div>
				</div>
			{/if}
		{:else if value.type === 'array' && value.items}
			<div
				class="bg-gray-50 dark:bg-gray-900/50 border border-gray-200 p-2 rounded-xl flex flex-col ml-2"
			>
				<ArrayComponent arrayComponent={value} {path} />
			</div>
		{/if}
	{/each}
{/if}
