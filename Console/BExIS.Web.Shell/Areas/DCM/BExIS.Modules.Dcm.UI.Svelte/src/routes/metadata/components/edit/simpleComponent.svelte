<script lang="ts">
	import {
		TextInput,
		NumberInput,
		TextArea,
		DropdownKVP,
		helpStore,
		CodeEditor
	} from '@bexis2/bexis2-core-ui';
	import { SlideToggle } from '@skeletonlabs/skeleton';
	import { onMount } from 'svelte';
	import { ValidationStoreAddSimpleComponent, ValidationStoreSetAllValid, getValueByPath, setValueByPath, updateMetadataStore } from '../../utils';
	import suite from './simpleComponent';


	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;
	export let value: any;
	export let label: string;

	// load form result object
	let res = suite.get();

	// set overall validity
	$: ValidationStoreSetAllValid(res.isValid());
	// update metadata store on value change
	$: updateMetadataStore(path, value);

	onMount(async () => {
			// initial check
			setTimeout(async () => {
				if(value == undefined || value == null || value == '') {
					res = suite(value, '');
				}
				else {
					res = suite(value, path);
				}
			}, 10);
			// add to validation store
			ValidationStoreAddSimpleComponent({label: label,path: path, required: required, value: value});
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
</script>

{#if path && simpleComponent.properties}
	<div class="px-5" id={path + '.item'}>
		{simpleComponent.properties['#text'].type}
		{#if simpleComponent.properties['#text'].type === 'string'}
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
