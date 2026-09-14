<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';
	import ArrayComponent from './arrayComponentWrapper.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';

	import { slide, fade } from 'svelte/transition';
	import { activeStore, hideStore, metadataStore } from '$lib/components/utils/metadata/stores';
	import Header from './MetadataComponentHeader.svelte';
	import { getValueByPath, hasValue, hasValueAtPath, getSchemaAttributes, getAttributeValue } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';

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

	// Schema-driven attributes on this compound node (excluding @ref and @partyid)
	$: schemaAttrs = getSchemaAttributes(complexComponent).filter(a => a !== '@partyid');
	$: storeData = $metadataStore;
	$: attrValues = schemaAttrs.reduce((acc: Record<string, any>, attr: string) => {
		acc[attr] = getAttributeValue(path, attr);
		return acc;
	}, {});

	$: propertyEntries = Object.entries(complexComponent?.properties ?? {}).filter(([key]: [string, any]) => !key.startsWith('@')) as [string, any][];

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
						<div class="pl-2  dark:bg-surface-800/50 rounded-sm flex flex-col" id={p}>
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
				class="pl-2 dark:bg-surface-800/50  "
			>
				<ArrayComponent arrayComponent={value} path={p} backgroundClass={backgroundClass} />
			</div>
		{/if}
	{/each}

	{#if schemaAttrs.length > 0}
		<div class="mt-1 pl-4">
			{#each schemaAttrs as attr}
				{#if attrValues[attr]}
					<div class="entry">
						<span class="key text-sm italic">{attr.replace('@', '')}</span>
						<span class="val text-sm text-gray-900 dark:text-gray-100">{attrValues[attr]}</span>
					</div>
				{/if}
			{/each}
		</div>
	{/if}
{/if}

<style>
.cont {
  margin-left: 1em;
}
.entry {
  display: flex;
  flex-direction: row;
  padding-bottom: 0.35rem;
}
.val {
  display: inline-block;
  width: 30vw;
}
.key {
  display: inline-block;
  flex-grow: 1;
}
@media (max-width: 768px) {
  .val {
    width: 50vw;
  }
}
</style>