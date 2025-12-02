<script lang="ts">
	
	import { onMount } from 'svelte';
	import { getValueByPath, updateMetadataStore, getVariableSoursePathFromConfig, ValidationStoreSetSimpleTypeValid } from '../../utils/metadata/metadataComponentUtils';
	import SveltyPicker, {  } from 'svelty-picker';
	//import { en, de } from 'svelty-picker/dist/i18n';

	let componentName: string = 'DateRangePicker_0.1';

	export let label: string;
	export let anchor: string;
	
	let date: Date = new Date();
	let valuePath: string = getVariableSoursePathFromConfig(componentName, anchor, 'value');
	let value: any = getValueByPath(valuePath).split(',');
	
	// update metadata store on value change
	$: updateMetadataStore(valuePath, value!= undefined && value != null ? value.toString() : '');

	onMount(async () => {

			date = value !== undefined || value == '' ? value as Date : Date.now() as unknown as Date;
	});

	</script>
		<span id = {anchor}>
			{label}	
			<SveltyPicker
				mode="date"
				isRange={true}
				name={label}
				format="yyyy-mm-dd"
				initialDate={date}
				bind:value
			/>
		</span>