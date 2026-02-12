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
	import { ValidationStoreAddSimpleComponent, ValidationStoreSetSimpleTypeValid, updateMetadataStore, createSimpleComponentValidationItem, getConfigStore } from '../../../../lib/components/utils/metadata/metadataComponentUtils';
	import { customComponentsCatalog } from '../../../../lib/components/customComponents/componentCatalog';
	import suite from './simpleComponent';
	import type { SimpleComponentData } from '../../../../lib/components/utils/metadata/models';
	import SveltyPicker from 'svelty-picker';
	//import { en, de } from 'svelty-picker/dist/i18n';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let value: any;
	export let label: string;
	
	let date: Date = new Date();
	// load form result object
	let res = suite.get();
	let config: any;
	let isAnchor: boolean = false;
	let isVisible: boolean = true;
	let customComponent: any;

	// set overall validity
	$: ValidationStoreSetSimpleTypeValid(path, res.isValid());
	// update metadata store on value change
	$: updateMetadataStore(path, value);

	onMount(async () => {

		 console.log("value",value);
		 console.log("enum", simpleComponent.properties['#text'].enum);

			// initial check
			setTimeout(async () => {
				if(value == undefined || value == null || value == '') {
					res = suite(value, '');
				}
				else {
					res = suite(value, path);
				}
			}, 10);
			if(simpleComponent.properties['#text'].format === 'date' || simpleComponent.properties['#text'].format === 'datetime' || simpleComponent.properties['#text'].format === 'date and time' || simpleComponent.properties['#text'].format === 'time'){
				date = value !== undefined || value == '' ? value as Date : Date.now() as unknown as Date;
			}
			// create validation item and add to store
			let simpleComponentValidationItem: SimpleComponentData = createSimpleComponentValidationItem(path, label, required, simpleComponent); 
			// add to validation store
			ValidationStoreAddSimpleComponent(simpleComponentValidationItem);
			config = getConfigStore();
			// check if this component is an anchor point
			for (const component of config.components) {
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
	});

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		// add some delay so the entityTemplate is updated
		// otherwise the values are old
		setTimeout(async () => {
			// check changed field
			res = suite(value, e.target.id);
		}, 10);
	}


	let x:string = null;

</script>
<!-- Simple Component Rendering -->
{#if isVisible && !isAnchor}
	{#if path && simpleComponent.properties}
		<div class="pl-5" id={path + '.item'}>
			<!-- Handle different formats and types -->
			{#if simpleComponent.properties['#text'].format !== undefined && simpleComponent.properties['#text'].format !== null} 		
				<!-- Handle date format -->
				{#if simpleComponent.properties['#text'].format.toLowerCase() === 'date'}
					<span id = {path}>
						{label}	
						<SveltyPicker
							mode="date"
							name={label}
							format="yyyy-mm-dd"
							initialDate={date}
							bind:value
							/>
					</span>
				<!-- Handle datetime format -->
				{:else if simpleComponent.properties['#text'].format.toLowerCase() === 'datetime' || simpleComponent.properties['#text'].format.toLowerCase() === 'date and time'}
					<span id = {path}>
						{label}	
						<SveltyPicker
							mode="datetime"
							name={label}
							format="yyyy-mm-dd hh:ii"
							initialDate={date}
							bind:value
							/>
					</span>
				<!-- Handle time format -->
				{:else if simpleComponent.properties['#text'].format.toLowerCase() === 'time'}
					<span id = {path}>
						{label}	
						<SveltyPicker
							mode="time"
							name={label}
							format="hh:ii"
							initialDate={date}
							bind:value
							/>
					</span>
				<!-- Handle textarea format -->
				{:else if simpleComponent.properties['#text'].type === 'string' &&simpleComponent.properties['#text'].format.toLowerCase() === 'textarea' || simpleComponent.properties['#text'].format.toLowerCase() === 'text'}
					<TextArea 
						id={path}
						label={label}
						required={required} 
						bind:value
						on:input={onChangeHandler}
						valid={res.isValid(path)}
						invalid={res.hasErrors(path)}
						feedback={res.getErrors(path)}
					/>
				{/if}
			<!-- Handle different types without specific format -->
			<!-- Handle string type -->
			{:else if simpleComponent.properties['#text'].type === 'string' && simpleComponent.properties['#text'].enum	=== undefined}	
				<TextInput 
					id={path}
					label={label}
					required={required} 
					bind:value
					on:input={onChangeHandler}
					valid={res.isValid(path)}
					invalid={res.hasErrors(path)}
					feedback={res.getErrors(path)}	 				
				/>
			<!-- Handle string type with enum  -->
			{:else if simpleComponent.properties['#text'].type === 'string' && simpleComponent.properties['#text'].enum}
			
					{#if simpleComponent.properties['#text'].enum.length <= 10}<!-- Handle string type with enum with short numer of  entries -->
						<Dropdown
								id="{path}"
								title="{label}"
								bind:target={value}
								source={simpleComponent.properties['#text'].enum}
								on:change={onChangeHandler}
								invalid={res.hasErrors(path)}
								feedback={res.getErrors(path)}	 
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
										invalid={res.hasErrors(path)}
										feedback={res.getErrors(path)}	 
									/>
						{/if}

			<!-- Handle number and integer types -->
			{:else if simpleComponent.properties['#text'].type === 'number'||simpleComponent.properties['#text'].type === 'integer'}
				<NumberInput 
					id={path}
					label={label}
					required={required} 
					bind:value
					on:input={onChangeHandler}
					valid={res.isValid(path)}
					invalid={res.hasErrors(path)}
					feedback={res.getErrors(path)}
				/>
			<!-- Handle boolean type -->
			{:else if simpleComponent.properties['#text'].type === 'boolean'}
				{@const v = value = true}
				<SlideToggle 
					id={path}
					label={label}
					name={label}
					required={required} 
					bind:value
					on:input={onChangeHandler}
					valid={res.isValid(path)}
					invalid={res.hasErrors(path)}
					feedback={res.getErrors(path)}
				>{label}</SlideToggle>
			{/if}
		</div>
	{/if}
{:else if isAnchor}
	<div class="pl-5" id={path + '.item'}>
		<svelte:component this={customComponent} anchor={path} label={label}/>
	</div>	
{/if}
