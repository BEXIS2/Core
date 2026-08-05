<script lang="ts">
	import {
		TextInput,
		NumberInput,
		TextArea,
		DropdownKVP,
		Dropdown,
		helpStore,
		CodeEditor,
		MultiSelect,
		DatePickerInput,
		Checkbox

	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { onMount } from 'svelte';
	import {
		ValidationStoreSetSimpleTypeValid,
		updateMetadataStore,
		getConfigStore,
		getValidationStore,
		showDescriptionHandler,
		hideDescriptionHandler,
		updateValidationState,
		registerValidationItem
	} from '$lib/components/utils/metadata/metadataComponentUtils';
	import { customComponentsCatalog } from '$lib/components/customComponents/componentCatalog';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import type {
		MappingComponentConfig,
		SimpleComponentData
	} from '$lib/components/utils/metadata/models';
	import SveltyPicker from 'svelty-picker';
	import { convertDisplayName } from '../../../lib/components/utils/metadata/metadataShared';
	import type { JsonListItem } from '../components/types';
	import Blocked from './Blocked.svelte';
	import PartySelector from './PartySelector.svelte';
	import { getMappingComponentConfig } from '$lib/components/utils/metadata/mappingHelper';
	import { showAllDescriptionsStore, descriptionStore } from '$lib/components/utils/metadata/stores';

	//import { en, de } from 'svelty-picker/dist/i18n';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let value: any;
	export let label: string;
	export let isMulti: boolean = false; // for array	of simple types, that should use multiselect ui component

	let date: Date = undefined as unknown as Date;
	// load form result object
	let res = suite.get();
	let config: any;
	let isAnchor: boolean = false;
	let isVisible: boolean = true;
	let customComponent: any;
	let min: number | undefined = -10000000;
	let max: number | undefined = 1000000;
	$:showDescription = $showAllDescriptionsStore !== null && $showAllDescriptionsStore !== undefined ? $showAllDescriptionsStore : false;

	// if mulitselect for array of simple types, create items array for multiselect component
	// we need to convert the enum of the schema to a list entry of the jsons because we more informations on each value then only the value
	// like ref and partyid
	let jsonItems: JsonListItem[] = [];

	$: updateValidationState(path, res);
	// update metadata store on value change
	$: updateMetadataStore(path, value, isMulti);

	// System mapping
	let mappingComponentConfig: MappingComponentConfig;



	onMount(async () => {
		//console.log('🚀 ~ onMount ~ simpleComponent:', value)

		// checks for date
		if (
			simpleComponent.properties['#text'].format === 'date' ||
			simpleComponent.properties['#text'].format === 'datetime' ||
			simpleComponent.properties['#text'].format === 'date and time' ||
			simpleComponent.properties['#text'].format === 'time'
		) {
			// console.log("date format detected, set date value", value, value as Date);
			date = value !== undefined || value == '' ? (value as Date) : (Date.now() as unknown as Date);
			// console.log("date format detected, set date", date);
		}

		// numeric - set min and max if exist	in schema
		if (simpleComponent.properties['#text'].minimum !== undefined) {
			min = simpleComponent.properties['#text'].minimum;
		}
		if (simpleComponent.properties['#text'].maximum !== undefined) {
			max = simpleComponent.properties['#text'].maximum;
		}

		if (isMulti && simpleComponent.properties['#text'].enum) {
			jsonItems = simpleComponent.properties['#text'].enum.map((item: any) => {
				return {
					'@ref': '',
					'@partyid': 0,
					'#text': item
				};
			});
		}

		mappingComponentConfig = getMappingComponentConfig(path, value);

		//#### VALIDATION	 ####
		registerValidationItem(path, convertDisplayName(label), required, simpleComponent);

		//#### CONFIGURATION	 ####
		config = getConfigStore();
		// check if this component is an anchor point
		//console.log("check for anchorpoin", config)
		for (const component of config.components) {
			console.log("ghjgJ", component.globalSettings.anchorpoint, path)
			// check if path is array which is indicated if the last part after the point is a number
			let isPathArray = path.includes('.') && !isNaN(Number(path.split('.').pop()));

			if (component.globalSettings.anchorpoint == path || (isPathArray && component.globalSettings.anchorpoint == path.split('.').slice(0, -1).join('.'))) {
				isAnchor = true;
				customComponent = customComponentsCatalog[component.meta.component_name].component;
			} 
			for (const variable of component.mode.variables.variable) {
				if (variable.JSONPath == path && variable.is_visible == false) {
					isVisible = false;
				}
			}
		}

		// initial check
		setTimeout(async () => {
			updateValue(value, path);
		}, 100);
	});

	// Do not mutate validation state on unmount.
	// Collapsing sections via toggleShow unmounts child fields temporarily.

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			updateValue(value, path);
		}, 10);
	}

	function handleShowDescription(e: CustomEvent<any>) {
		showDescriptionHandler(e, 'simple');
	}

	function handleHideDescription(e: CustomEvent<any>) {
		hideDescriptionHandler(e, 'simple');
	}

	function handleShowDescriptionFallback() {
		showDescriptionHandler(
			{
				detail: {
					description: simpleComponent?.description ?? '',
					id: path
				}
			},
			'simple'
		);
	}

	function handleHideDescriptionFallback() {
		hideDescriptionHandler({}, 'simple');
	}

	function updateValue(value: any, _path: string) {
		// check changed field only
		res = suite(_path);

		setTimeout(async () => {
			updateValidationState(_path, res);
		}, 10);
	}

 $: commonProps = {
    id: path,
	label: convertDisplayName(label),
    required,
    invalid: res.hasErrors(path),
	valid: res.isValid(path),
    feedback: res.getErrors(path),
    description: simpleComponent.description,
	showDescription: showDescription,
//	disabled: mappingComponentConfig?.isDisabled ?? false
  };

</script>

<!-- Simple Component Rendering -->
{#if isVisible && !isAnchor}
<!--on:mouseover={() => descriptionStore.set({ type: 'simple', content: simpleComponent.description, path })} -->

<div class="pr-2" id={path}>
		<!--	if the field is mapped to a party or key, show blocked component with info, otherwise show the normal input component based on the type and format of the field -->
		{#if mappingComponentConfig && ((mappingComponentConfig.isMappedToParty && !mappingComponentConfig.isSelector) || mappingComponentConfig.isMappedToKey)}
			<Blocked
				isKeyMapped={mappingComponentConfig.isMappedToKey}
				isPartyMapped={mappingComponentConfig.isMappedToParty}
				label={convertDisplayName(label)}
				bind:value
				{path}
				{required}
				description={simpleComponent.description}
			/>
		{:else if mappingComponentConfig && mappingComponentConfig.isMappedToParty && mappingComponentConfig.isSelector}
			<PartySelector
				{path}
				{value}
				label= {convertDisplayName(label)}
				{mappingComponentConfig}
				{required}
				{isMulti}
				description={simpleComponent.description}
				{handleShowDescription}
				{handleHideDescription}
			/>
		{:else if path && simpleComponent.properties}
			<!-- Handle different formats and types -->
			{#if simpleComponent.properties['#text'].format !== undefined && simpleComponent.properties['#text'].format !== null}
				<!-- Handle date format -->
				{#if simpleComponent.properties['#text'].format.toLowerCase() === 'date'}
					<div
						id={path}
						role="group"
						on:mouseover={handleShowDescriptionFallback}
						on:focus={handleShowDescriptionFallback}
						on:mouseleave={handleHideDescriptionFallback}
						on:blur={handleHideDescriptionFallback}
					>
						<DatePickerInput
							label={convertDisplayName(label)}
							{required}
							mode="date"
							name={label}
							format="yyyy-mm-dd"
							initialDate={date}
							bind:value
							inputClasses="input variant-form-material dark:bg-zinc-700 bg-zinc-50 placeholder:text-gray-400 w-32"
							on:change={onChangeHandler}
							on:input={onChangeHandler}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
							description={simpleComponent.description}
							valid={res.isValid(path)}
							invalid={res.hasErrors(path)}
						/>
					</div>

					<!-- Handle datetime format -->
				{:else if simpleComponent.properties['#text'].format.toLowerCase() === 'datetime' || simpleComponent.properties['#text'].format.toLowerCase() === 'date and time'}
					<div
						id={path}
						role="group"
						on:mouseover={handleShowDescriptionFallback}
						on:focus={handleShowDescriptionFallback}
						on:mouseleave={handleHideDescriptionFallback}
						on:blur={handleHideDescriptionFallback}
					>
						<DatePickerInput
							label={convertDisplayName(label)}
							{required}
							mode="datetime"
							name={label}
							format="yyyy-mm-dd hh:ii"
							displayFormat="yyyy-mm-dd hh:mm"
							initialDate={date}
							bind:value
							inputClasses="input variant-form-material dark:bg-zinc-700 bg-zinc-50 placeholder:text-gray-400 w-32"
							on:change={onChangeHandler}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
							on:input={onChangeHandler}	
							description={simpleComponent.description}
							valid={res.isValid(path)}
							invalid={res.hasErrors(path)}
						/>
					</div>
						
					<!-- Handle time format -->
				{:else if simpleComponent.properties['#text'].format.toLowerCase() === 'time'}
					<div
						id={path}
						role="group"
						on:mouseover={handleShowDescriptionFallback}
						on:focus={handleShowDescriptionFallback}
						on:mouseleave={handleHideDescriptionFallback}
						on:blur={handleHideDescriptionFallback}
					>
					<DatePickerInput
							label={convertDisplayName(label)}
							{required}
							mode="time"
							name={label}
							format="hh:ii"
							displayFormat="hh:mm"
							initialDate={date}
							bind:value
							inputClasses="input variant-form-material dark:bg-zinc-700 bg-zinc-50 placeholder:text-gray-400 w-32"
							on:change={onChangeHandler}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
							on:input={onChangeHandler}
							description={simpleComponent.description}
							valid={res.isValid(path)}
							invalid={res.hasErrors(path)}
						/>
					</div>
					<!-- Handle textarea format -->
				{:else if (simpleComponent.properties['#text'].type === 'string' && simpleComponent.properties['#text'].format.toLowerCase() === 'textarea') || simpleComponent.properties['#text'].format.toLowerCase() === 'text' || (simpleComponent.properties['#text'].type === 'string' && value.length >= 25)}
					<TextArea
						{... commonProps}
						bind:value
						on:input={onChangeHandler}
						on:showDescription={handleShowDescription}
						on:hideDescription={handleHideDescription}
					/>
				{/if}
				<!-- Handle different types without specific format -->
				<!-- Handle string type -->
			{:else if simpleComponent.properties['#text'].type === 'string' && simpleComponent.properties['#text'].enum === undefined}
				<TextInput
					{... commonProps}
					bind:value
					on:input={onChangeHandler}
					on:showDescription={handleShowDescription}
					on:hideDescription={handleHideDescription}
				/>
				<!-- Handle string type with enum  -->
			{:else if simpleComponent.properties['#text'].type === 'string' && simpleComponent.properties['#text'].enum}
				{#if !isMulti}
					<!-- Handle single select -->
					{#if simpleComponent.properties['#text'].enum.length <= 10}<!-- Handle string type with enum with short numer of  entries -->
						<Dropdown
							{... commonProps}
							title={convertDisplayName(label)}
							bind:target={value}
							source={simpleComponent.properties['#text'].enum}
							on:change={onChangeHandler}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
						/>
					{:else}
						<!-- Handle string type with enum with many entries -->
						<MultiSelect
							{... commonProps}
							title={convertDisplayName(label)}
							source={simpleComponent.properties['#text'].enum}
							bind:target={value}
							isMulti={false}
							clearable={required ? false : true}
							on:change={onChangeHandler}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
						/>
					{/if}
				{:else}
					<!-- Handle multi select for array of simple types -->
					{#if isMulti}
						<MultiSelect
							{... commonProps}
							title={convertDisplayName(label)}
							source={jsonItems}
							itemId="#text"
							itemLabel="#text"
							bind:target={value}
							isMulti={true}
							clearable={required ? false : true}
							on:change={onChangeHandler}
							on:showDescription={handleShowDescription}
							on:hideDescription={handleHideDescription}
						/>
					{/if}
				{/if}

				<!-- Handle number and integer types -->
			{:else if simpleComponent.properties['#text'].type === 'number' || simpleComponent.properties['#text'].type === 'integer'}
				<NumberInput
					{... commonProps}
					bind:value
					on:input={onChangeHandler}
					{min}
					{max}
					on:showDescription={handleShowDescription}
					on:hideDescription={handleHideDescription}
				/>

				<!-- Handle boolean type -->
			{:else if simpleComponent.properties['#text'].type === 'boolean'}
				<!-- {@const v = value = true} -->
				<div
					class="inline-flex items-center gap-2 py-1"
					role="group"
					on:mouseover={handleShowDescriptionFallback}
					on:focus={handleShowDescriptionFallback}
					on:mouseleave={handleHideDescriptionFallback}
					on:blur={handleHideDescriptionFallback}
				>
				<Checkbox
					{... commonProps}
					id={path}
					bind:checked={value}
					on:showDescription={handleShowDescription}
					on:hideDescription={handleHideDescription}
					
					on:change={onChangeHandler}
					/>
				</div>
				{/if}

				
		{/if}
	</div>
{:else if isAnchor}
	<div class="pr-2" id={path}>
		<svelte:component this={customComponent} anchor={path}
						on:showDescription={handleShowDescription}
						on:hideDescription={handleHideDescription}
						path={path}
					/>
	</div>
{/if}
