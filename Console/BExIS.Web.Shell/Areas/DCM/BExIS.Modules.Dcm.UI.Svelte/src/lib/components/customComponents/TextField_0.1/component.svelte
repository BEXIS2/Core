<script lang="ts">
	
	import { onMount } from 'svelte';
	import { getValueByPath, updateMetadataStore, getVariableSoursePathFromConfig , getFullConfig, getTargetVariablesWithValues, ValidationStoreSetSimpleTypeValid } from '../../utils/metadata/metadataComponentUtils';
	import {TextInput} from '@bexis2/bexis2-core-ui';
	import {convertDisplayName} from '../../../../routes/metadata/metadataShared';
	//import { en, de } from 'svelty-picker/dist/i18n';

	let componentName: string = 'textField_v4.2.26';

	export let label: string;
	export let anchor: string;

	let valuePath: string = getVariableSoursePathFromConfig(componentName, anchor, 'value');
	let value: any = getValueByPath(valuePath);
	let config = getFullConfig(componentName, anchor);
	let targetVar = getTargetVariablesWithValues(config)
	console.log("target", targetVar, config, componentName, anchor)
	// update metadata store on value change
	$: updateMetadataStore(valuePath, value!= undefined && value != null ? value.toString() : '');

	let disabled = (targetVar?.find(v => v.target_variable == 'disable')?.value ?? false) === 'true';
	let defaultValue = targetVar?.find(v => v.target_variable === 'defaultValue')?.value ?? '';
	// console.log("default", defaultValue)
	// console.log("value before", value)
	// console.log("disabled", disabled)

	if((value == undefined || value == null || value == '') && (defaultValue != undefined && defaultValue != null && defaultValue != '')){
		value = defaultValue;
	}

	onMount(async () => {

		ValidationStoreSetSimpleTypeValid(anchor, true, "");
			
	});


//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: any) {
		console.log(e);
	}

	</script>
		<span id = {anchor}>
	
			<TextInput 
					id={anchor}
					label={convertDisplayName(label)}
					
					bind:value
					on:input={onChangeHandler}
					
					disabled={disabled} 				
				/>
			
		</span>