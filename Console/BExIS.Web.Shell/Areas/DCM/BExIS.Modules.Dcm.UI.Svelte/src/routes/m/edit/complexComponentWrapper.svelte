<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';
	import ArrayComponent from './arrayComponentWrapper.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';

	import { slide, fade } from 'svelte/transition';
	import { activeStore, hideStore } from '$lib/components/utils/metadata/stores';
	import Header from './MetadataComponentHeader.svelte';

	export let complexComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let description: string = '';


	let label: string =
		path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;

	let requiredList =
		complexComponent && complexComponent.type === 'object' && complexComponent.required
			? complexComponent.required
			: [];

	function normalizeRequiredKey(value: string): string {
		return String(value ?? '')
			.toLowerCase()
			.replace(/[^a-z0-9]/g, '');
	}

	function isRequiredKey(key: string): boolean {
		const normalizedKey = normalizeRequiredKey(key);
		return requiredList.some((requiredKey: string) => normalizeRequiredKey(requiredKey) === normalizedKey);
	}

</script>
<!-- {#key reloading} -->
{#if complexComponent && complexComponent.type === 'object' && complexComponent.properties}
	{#each Object.entries(complexComponent.properties) as [key, value]}
		{@const p = path = path ? path + '.' + key : key}
		{@const l = label = key}
		{#if (value.type === 'object' && value.properties && !value.properties['#text']) }
			{#if value.oneOf || value.anyOf || value.allOf}
				<ChoiceComponent choiceComponent={value} {path} />
			{:else}
				<div class="grid grid-cols-1 gap-0 ">

					<Header	{required} {path} {p} description={value.description}  />

					{#if !$hideStore.includes(path) && $activeStore.includes(path)}
						<div in:slide out:slide class="card pl-5 py-1" id={path}>
						 <ComplexComponent
								complexComponent={value}
								{path}
								required={isRequiredKey(key)}
							/>

						</div>
						{/if}
				</div>
			{/if}
		{:else if value.type === 'object' && value.properties['#text']}
			<div class="mb-1">
				<div class="flex flex-col md:flex-row md:items-center gap-2 mb">
					<div class="flex-1 min-w-[100px] pt-1">
						<SimpleComponent simpleComponent={value} {path} required={isRequiredKey(key)} />
					</div>
				</div>
			
			</div>
		{:else if value.type === 'array' && value.items}
			<ArrayComponent arrayComponent={value} {path} />
		{/if}
	{/each}
	
{/if}

