<script lang="ts">
	
	import { onMount } from 'svelte';
	import { getValueByPath, updateMetadataStore, getVariableSoursePathFromConfig, ValidationStoreSetSimpleTypeValid } from '../../utils';
	import SveltyPicker, {  } from 'svelty-picker';
	import suite from '../../components/edit/simpleComponent';
		//import { en, de } from 'svelty-picker/dist/i18n';

	let componentName: string = 'DateRangePicker_0.1';

	// load form result object
	let res = suite.get();

	// set overall validity
	//$: ValidationStoreSetSimpleTypeValid(path, res.isValid());

	export let label: string;
	export let anchor: string;
	
	let date: Date = new Date();
	let valuePath: string = getVariableSoursePathFromConfig(componentName, anchor, 'value');
	let value: any = getValueByPath(valuePath).split(',');
	
	// update metadata store on value change
	$: updateMetadataStore(valuePath, value!= undefined && value != null ? value.toString() : '');
	onMount(async () => {
			// initial check
			setTimeout(async () => {
					res = suite(value, '');
			}, 10);

			date = value !== undefined || value == '' ? value as Date : Date.now() as unknown as Date;
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