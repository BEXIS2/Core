<script lang="ts">
	import {
		TextInput,
		NumberInput,
		TextArea,
		DropdownKVP,
		Dropdown,
		helpStore,
		CodeEditor,

		MultiSelect

	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { onMount } from 'svelte';
	import { ValidationStoreAddSimpleComponent, ValidationStoreSetSimpleTypeValid, updateMetadataStore, createSimpleComponentValidationItem, getConfigStore, getValidationStore} from '$lib/components/utils/metadata/metadataComponentUtils';
	import { customComponentsCatalog } from '$lib/components/customComponents/componentCatalog';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import type { MappingComponentConfig, SimpleComponentData } from '$lib/components/utils/metadata/models';
	import SveltyPicker from 'svelty-picker';
	import {convertDisplayName} from '../metadataShared';
	import type { JsonListItem } from '../components/types';
	import Blocked from './Blocked.svelte';
	import PartySelector from './PartySelector.svelte';
	import { getMappingComponentConfig } from '$lib/components/utils/metadata/mappingHelper';

	//import { en, de } from 'svelty-picker/dist/i18n';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let value: any;
	export let label: string;
	export let isMulti:boolean	= false; // for array	of simple types, that should use multiselect ui component
	
	let date: Date = undefined as unknown as Date;
	// load form result object
	let res = suite.get();
	let config: any;
	let isAnchor: boolean = false;
	let isVisible: boolean = true;
	let customComponent: any;
	let min: number | undefined = -10000000;
	let max: number | undefined = 1000000;

	// if mulitselect for array of simple types, create items array for multiselect component
	// we need to convert the enum of the schema to a list entry of the jsons because we more informations on each value then only the value
	// like ref and partyid
	let jsonItems:	JsonListItem[] = []; 

	// set overall validity
	$: ValidationStoreSetSimpleTypeValid(path, res.isValid(path), res.hasErrors(path) ? res.getErrors(path).join('.  ') : '');
	// update metadata store on value change
	$: updateMetadataStore(path, value, isMulti);

	// System mapping
let mappingComponentConfig: MappingComponentConfig;

	onMount(async () => {

   //console.log('🚀 ~ onMount ~ simpleComponent:', value)

			// checks for date
			if(simpleComponent.properties['#text'].format === 'date' || simpleComponent.properties['#text'].format === 'datetime' || simpleComponent.properties['#text'].format === 'date and time' || simpleComponent.properties['#text'].format === 'time'){
				// console.log("date format detected, set date value", value, value as Date);
				date = value !== undefined || value == '' ? value as Date : Date.now() as unknown as Date;
				// console.log("date format detected, set date", date);

			}

			// numeric - set min and max if exist	in schema
			if(simpleComponent.properties['#text'].minimum !== undefined){
				min = simpleComponent.properties['#text'].minimum;
			}
			if(simpleComponent.properties['#text'].maximum !== undefined){
				max = simpleComponent.properties['#text'].maximum;
			}

			if(isMulti && simpleComponent.properties['#text'].enum){
				jsonItems = simpleComponent.properties['#text'].enum.map((item: any) => {
					return {
						"@ref": '',
						"@partyid": 0,
						"#text": item
					}
				
				});
			}

			//#### VALIDATION	 ####
			// create validation item and add to store
			let simpleComponentValidationItem: SimpleComponentData = createSimpleComponentValidationItem(path, label, required, simpleComponent); 
			//console.log("🚀 ~ onMount ~ simpleComponentValidationItem:", simpleComponentValidationItem	)
			// add to validation store
			ValidationStoreAddSimpleComponent(simpleComponentValidationItem);


			

			//#### CONFIGURATION	 ####
			config = getConfigStore();
			// check if this component is an anchor point
			//console.log("check for anchorpoin", config)
			for (const component of config.components) {
				//console.log("ghjgJ", component.globalSettings.anchorpoint, path)
				if (component.globalSettings.anchorpoint == path){
					isAnchor = true;
					customComponent = customComponentsCatalog[component.meta.component_name].component;
				}
				for (const variable of component.mode.variables.variable) {
					if (variable.JSONPath == path && variable.is_visible == false){
						isVisible = false;
					}
				}
			}

			mappingComponentConfig = getMappingComponentConfig(path, value);
			//console.log("🚀 ~ mappingComponentConfig:", mappingComponentConfig)

			// initial check
			setTimeout(async () => {

				updateValue(value, path);
			}, 100);
	});

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		// add some delay so the entityTemplate is updated
			// otherwise the values are old
			setTimeout(async () => {

				updateValue(value, path);

			}, 10);	
	}



	function updateValue(value: any, _path:string){

			// check changed field only
			res = suite(_path);

			setTimeout(async () => {
						//console.log("🚀 ~ updateValue ~ res:",res, res.isValid(_path), path, value)

						let errorMessage = '';
						if(res.hasErrors(_path)){
								errorMessage = res.getErrors(_path).join('.  ');
						}

						// update validationstore
						ValidationStoreSetSimpleTypeValid(_path, res.isValid(_path), errorMessage);
			}, 10);
	}


</script>

<!-- Simple Component Rendering -->
{#if isVisible && !isAnchor}
		<div class="pr-2" id={path + '.item'}>

  <!--	if the field is mapped to a party or key, show blocked component with info, otherwise show the normal input component based on the type and format of the field -->
	{#if (mappingComponentConfig && ((mappingComponentConfig.isMappedToParty && !mappingComponentConfig.isSelector)  || mappingComponentConfig.isMappedToKey))}
		<Blocked	isKeyMapped={mappingComponentConfig.isMappedToKey} isPartyMapped={mappingComponentConfig.isMappedToParty} label={label} bind:value={value} path={path}/>
	{:else if mappingComponentConfig && mappingComponentConfig.isMappedToParty && mappingComponentConfig.isSelector}

			<PartySelector
			 id={path}

				path={path}
				value={value}
				label={label}
				mappingComponentConfig={mappingComponentConfig}	
				required={required}
				isMulti={isMulti}
				description={simpleComponent.description}
				on:reload
				/>

	{:else if path && simpleComponent.properties}

			<!-- Handle different formats and types -->
			{#if simpleComponent.properties['#text'].format !== undefined && simpleComponent.properties['#text'].format !== null} 		
				<!-- Handle date format -->
				{#if simpleComponent.properties['#text'].format.toLowerCase() === 'date'}
					<span id = {path}>
						<span class="mr-2">
						{convertDisplayName(label)}	
						 </span>
						<SveltyPicker
							mode="date"
							name={label}
							format="yyyy-mm-dd"
							initialDate={date}
							bind:value
							inputClasses="input variant-form-material dark:bg-zinc-700 bg-zinc-50 placeholder:text-gray-400 w-32"			
							/>
					</span>
				<!-- Handle datetime format -->
				{:else if simpleComponent.properties['#text'].format.toLowerCase() === 'datetime' || simpleComponent.properties['#text'].format.toLowerCase() === 'date and time'}
					<span id = {path}>
						<span class="mr-2">
						{convertDisplayName(label)}	
						</span>
						<SveltyPicker
							mode="datetime"
							name={label}
							format="yyyy-mm-dd hh:ii"
							initialDate={date}
							bind:value
							inputClasses="input variant-form-material dark:bg-zinc-700 bg-zinc-50 placeholder:text-gray-400 w-32"
							/>
					</span>
				<!-- Handle time format -->
				{:else if simpleComponent.properties['#text'].format.toLowerCase() === 'time'}
					<span id = {path}>
						<span class="mr-2">
						{convertDisplayName(label)}	
						</span>
						<SveltyPicker
							mode="time"
							name={label}
							format="hh:ii" 
							displayFormat="hh:mm"
							initialDate={date}
							bind:value
							inputClasses="input variant-form-material dark:bg-zinc-700 bg-zinc-50 placeholder:text-gray-400 w-32"
						
							/>
					</span>
				<!-- Handle textarea format -->
				{:else if simpleComponent.properties['#text'].type === 'string' &&simpleComponent.properties['#text'].format.toLowerCase() === 'textarea' || simpleComponent.properties['#text'].format.toLowerCase() === 'text' || (simpleComponent.properties['#text'].type === 'string' && value.length >= 25)}
					<TextArea 
						id={path}
						label={convertDisplayName(label)}
						required={required} 
						bind:value
						on:input={onChangeHandler}
						valid={res.isValid(path)}
						invalid={res.hasErrors(path)}
						feedback={res.getErrors(path)}
						description={simpleComponent.description}
					/>
				{/if}
			<!-- Handle different types without specific format -->
			<!-- Handle string type -->
			{:else if simpleComponent.properties['#text'].type === 'string' && simpleComponent.properties['#text'].enum	=== undefined}	

				<TextInput 
					id={path}
					label={convertDisplayName(label)}
					required={required} 
					bind:value
					on:input={onChangeHandler}
					valid={res.isValid(path)}
					invalid={res.hasErrors(path)}
					feedback={res.getErrors(path)}	 				
					description={simpleComponent.description}
				/>
			<!-- Handle string type with enum  -->
			{:else if simpleComponent.properties['#text'].type === 'string' && simpleComponent.properties['#text'].enum}

					{#if !isMulti} <!-- Handle single select -->
					{#if simpleComponent.properties['#text'].enum.length <= 10}<!-- Handle string type with enum with short numer of  entries -->
						<Dropdown
								id="{path}"
								title="{label}"
								bind:target={value}
								source={simpleComponent.properties['#text'].enum}
								on:change={onChangeHandler}
								invalid={res.hasErrors(path)}
								feedback={res.getErrors(path)}	 
								description={simpleComponent.description}
								required={required}
							/>
					{:else} <!-- Handle string type with enum with many entries -->
								<MultiSelect
										id="{path}"
										title="{label}"
										required={required}
										source={simpleComponent.properties['#text'].enum}
										bind:target={value}
										isMulti={false}
										clearable={required	? false : true} 
										on:change={onChangeHandler}
										invalid={res.hasErrors(path)}
										feedback={res.getErrors(path)}	 
										description={simpleComponent.description}
									/>
						{/if}
						{:else} <!-- Handle multi select for array of simple types -->
						{#if isMulti}
						
							<MultiSelect
										id="{path}"
										title="{label}"
										complexSource={true}
										complexTarget={true}
										source={jsonItems}
										itemId="#text"
										itemLabel="#text"
										bind:target={value}
										isMulti={true}
										clearable={required	? false : true} 
										on:change={onChangeHandler}
										invalid={res.hasErrors(path)}
										feedback={res.getErrors(path)}	 
										description={simpleComponent.description}
									/>
						{/if}
						{/if}

			<!-- Handle number and integer types -->
			{:else if simpleComponent.properties['#text'].type === 'number'||simpleComponent.properties['#text'].type === 'integer'}

				<NumberInput 
					id={path}
					label={convertDisplayName(label)}
					required={required} 
					bind:value
					on:input={onChangeHandler}
			  min={min}
					max={max}
					valid={res.isValid(path)}
					invalid={res.hasErrors(path)}
					feedback={res.getErrors(path)}
					description={simpleComponent.description}
				/>

			<!-- Handle boolean type -->
			{:else if simpleComponent.properties['#text'].type === 'boolean'}
				<!-- {@const v = value = true} -->
				<SlideToggle 
					id={path}
					label={convertDisplayName(label)}
					name={convertDisplayName(label)}
					bind:checked={value}
					on:input={onChangeHandler}
					description={simpleComponent.description}
					size="sm"
				>{label}</SlideToggle>
			{/if}

	{/if}
			</div>
{:else if isAnchor}
	<div class="" id={path + '.item'}>
		<svelte:component this={customComponent} anchor={path} label={convertDisplayName(label)}/>
	</div>	
{/if}
