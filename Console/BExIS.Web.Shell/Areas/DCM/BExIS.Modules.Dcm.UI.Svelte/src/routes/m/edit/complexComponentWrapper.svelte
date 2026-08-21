<script lang="ts">
	import ComplexComponent from './complexComponentWrapper.svelte';
	import SimpleComponent from './simpleComponentWrapper.svelte';
	import ArrayComponent from './arrayComponentWrapper.svelte';
	import ChoiceComponent from './choiceComponentWrapper.svelte';

	import { slide } from 'svelte/transition';
	import { activeStore, hideStore, metadataStore, validationStore } from '$lib/components/utils/metadata/stores';
	import Header from './MetadataComponentHeader.svelte';
	import { convertDisplayName } from '$lib/components/utils/metadata/metadataShared';
	import { registerValidationItem, updateValidationState, getSchemaAttributes, getAttributeValue, updateAttribute } from '$lib/components/utils/metadata/metadataComponentUtils';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';

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


	//#### VALIDATION	 ####
	registerValidationItem(path, convertDisplayName(label), required, complexComponent);

	let res = suite.get();

	// init
	setTimeout(async () => {
		updateValidationState(path, res);
	}, 100);
 

	// Schema-driven attributes on this compound node (excluding @ref and @partyid)
	$: schemaAttrs = getSchemaAttributes(complexComponent).filter(a => a !== '@partyid');
	$: storeData = $metadataStore;
	$: attrValues = schemaAttrs.reduce((acc: Record<string, any>, attr: string) => {
		acc[attr] = getAttributeValue(path, attr);
		return acc;
	}, {});

	function onAttrChange(attr: string, e: any) {
		updateAttribute(path, attr, e.target?.value ?? '');
	}

	function onChangeHandler(e: CustomEvent<any>) {
  //console.log("🚀 ~ complex child onChangeHandler:", path, res.isValid(path))
		res = suite(path);
		setTimeout(async () => {
			updateValidationState(path, res);
		}, 10);

}

//console.log("end of complex item scipt")


</script>
{#if complexComponent && complexComponent.type === 'object' && complexComponent.properties}
	{#each Object.entries(complexComponent.properties) as [key, value]}
		{@const p = path = path ? path + '.' + key : key}
		{@const l = label = key}
		{#if (value.type === 'object' && value.properties && !value.properties['#text']) }
			{#if value.oneOf || value.anyOf || value.allOf}
				<ChoiceComponent choiceComponent={value} {path} on:updated={onChangeHandler}/>
			{:else}
				<div class="grid grid-cols-1 gap-0 ">

					<Header	{required} {path} {p} description={value.description}  />

					{#if !$hideStore.includes(path) && $activeStore.includes(path)}
						<div in:slide out:slide class="card pl-5 py-1" id={path}>

						 <ComplexComponent
								complexComponent={value}
								{path}
								required={isRequiredKey(key)}
								on:updated={onChangeHandler}
							/>

						</div>
						{/if}
				</div>
			{/if}
		{:else if value.type === 'object' && value.properties['#text']}
			<div class="mb-1">
				<div class="flex flex-col md:flex-row md:items-center gap-2 mb">
					<div class="flex-1 min-w-[100px] pt-1">
						<SimpleComponent simpleComponent={value} {path} required={isRequiredKey(key)} on:updated={onChangeHandler} />
					</div>
				</div>
			
			</div>
		{:else if value.type === 'array' && value.items}
			<ArrayComponent arrayComponent={value} {path} on:updated={onChangeHandler} />
		{/if}
	{/each}

	{#if schemaAttrs.length > 0}
		<div class="flex flex-col gap-1 mt-1 pl-2 border-l-2 border-surface-200 dark:border-surface-700">
			{#each schemaAttrs as attr}
				<div class="flex items-center gap-2">
					<span class="text-xs text-surface-600 dark:text-surface-300 w-24 shrink-0 font-medium">{attr.replace('@', '')}</span>
					<input
						type="text"
						class="input variant-form-material text-xs py-1 flex-1"
						value={attrValues[attr] ?? ''}
						on:input={(e) => onAttrChange(attr, e)}
					/>
				</div>
			{/each}
		</div>
	{/if}
{/if}


