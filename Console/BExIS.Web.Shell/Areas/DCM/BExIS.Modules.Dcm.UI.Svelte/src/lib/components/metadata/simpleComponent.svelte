<script lang="ts">
	import {
		TextInput,
		NumberInput,
		TextArea,
		Dropdown,
		MultiSelect,
		DatePickerInput,
		Checkbox
	} from '@bexis2/bexis2-core-ui';

	import { onMount, createEventDispatcher } from 'svelte';
	import {
		updateMetadataStore,
		showDescriptionHandler,
		hideDescriptionHandler,
		updateValidationState,
		registerValidationItem
	} from '$lib/components/utils/metadata/metadataComponentUtils';

	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import type {
		MappingComponentConfig,
	} from '$lib/components/utils/metadata/models';
	import { convertDisplayName } from '../utils/metadata/metadataShared';
	import type { JsonListItem } from '../../../routes/m/components/types';
	import Blocked from './Blocked.svelte';
	import PartySelector from './PartySelector.svelte';
	import { getMappingComponentConfig } from '$lib/components/utils/metadata/mappingHelper';
	import { showAllDescriptionsStore, validationStore } from '$lib/components/utils/metadata/stores';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let value: any;
	export let label: string;
	export let isMulti: boolean = false; // for array	of simple types, that should use multiselect ui component
	export let description: string = '';
	export let disabled: string = "false";

	let date: Date = undefined as unknown as Date;
	// load form result object
	let res = suite.get();
	let min: number | undefined = -10000000;
	let max: number | undefined = 1000000;
	$:showDescription = $showAllDescriptionsStore !== null && $showAllDescriptionsStore !== undefined ? $showAllDescriptionsStore : false;

// dispatch event to parent component to reload the metadata form, so the validation state is updated
	const dispatch = createEventDispatcher();

	// if mulitselect for array of simple types, create items array for multiselect component
	// we need to convert the enum of the schema to a list entry of the jsons because we more informations on each value then only the value
	// like ref and partyid
	let jsonItems: JsonListItem[] = [];

	$: updateValidationState(path, res);
	// update metadata store on value change
	$: updateMetadataStore(path, value, isMulti);
 $: value, dispatch('x', { path, value });

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


		//#### VALIDATION	 ####
		registerValidationItem(path, convertDisplayName(label), required, simpleComponent);

		// System	mapping
		mappingComponentConfig = getMappingComponentConfig(path, value);
	
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
		console.log('🚀 ~ onChangeHandler ~ path:', path, 'value:', value);
		dispatch('updated');
		

		setTimeout(async () => {
			updateValue(value, path);
			//dispatch('reload');
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
		// console.log("🚀 ~ updateValue ~ value:", value)
		// check changed field only
		res = suite(_path);

		setTimeout(async () => {
					//console.log("🚀 ~ path:", path, res.isValid(path))
			updateValidationState(_path, res);
		}, 10);
	}

  // set description if not set from parent component, use the description from the simpleComponent
  if (description === undefined || description === null) {
	description = simpleComponent?.description ?? '';
  }
 $: commonProps = {
    id: path,
				label: convertDisplayName(label),
    required,
    invalid: res.hasErrors(path),
				valid: res.isValid(path),
    feedback: res.getErrors(path),
    description: description,
				showDescription: showDescription,
	   disabled: disabled === "true" ? true : false
  };

</script>

<div class="pr-2" id={path}>
		<!--	if the field is mapped to a party or key, show blocked component with info, otherwise show the normal input component based on the type and format of the field -->
		{#if mappingComponentConfig && ((mappingComponentConfig.isMappedToParty && !mappingComponentConfig.isSelector) || mappingComponentConfig.isMappedToKey)}
			<Blocked
				{...commonProps}
				isKeyMapped={mappingComponentConfig.isMappedToKey}
				isPartyMapped={mappingComponentConfig.isMappedToParty}
				bind:value
				{path}
			/>
		{:else if mappingComponentConfig && mappingComponentConfig.isMappedToParty && mappingComponentConfig.isSelector}
			<PartySelector
				{...commonProps}
				{path}
				{value}
				label= {convertDisplayName(label)}
				{mappingComponentConfig}
				{required}
				{isMulti}
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
							{...commonProps}
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
							{...commonProps}
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
							{...commonProps}
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
							complexSource={true}
							complexTarget={true}
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

