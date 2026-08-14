<script lang="ts">
	import { onMount, createEventDispatcher } from 'svelte';
	import {
		updateMetadataStore,
		getFullConfig,
		getTargetVariablesWithValues,
		resolveNode,
		updateValidationState,
		registerValidationItem,
		getMetadata,
		setValidationLengthConstraints
	} from '../../utils/metadata/metadataComponentUtils';

	import {TextArea} from '@bexis2/bexis2-core-ui';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';
	import { validationStore } from '$lib/components/utils/metadata/stores';

	const dispatch = createEventDispatcher();
	let res = suite.get();
	let componentName: string = 'textField_v1.0.0';

	export let anchor: string;
	export let path: string = '';

	let config = getFullConfig(componentName, anchor);
	if (!config) {
		console.error('No configuration found for component:', componentName, 'with anchor:', anchor);
	}
	let targetVars = getTargetVariablesWithValues(config);

	let text_field_path = targetVars?.find((v) => v.target_variable === 'text_field')?.value ?? '';
	// console.log('🚀 ~ text_field_path:', text_field_path, 'anchor:', anchor, 'path:', path);
	if (text_field_path && text_field_path == anchor.split('.').slice(0, -1).join('.')) {
		text_field_path = anchor;
	}
	// console.log('🚀 ~ text_field_path after check:', text_field_path, 'anchor:', anchor, 'path:', path);
	let { value, ref, label, description, required } = getMetadata(text_field_path);

	let validationRegistered = false;
	let validationReady = false;

	let disabled =
		(targetVars?.find((v) => v.target_variable == 'disable')?.value ?? false) === 'true';
	let defaultValue = targetVars?.find((v) => v.target_variable === 'defaultValue')?.value ?? '';
	let maxLength = targetVars?.find((v) => v.target_variable === 'maxLength')?.value ?? '';
	let minLength = targetVars?.find((v) => v.target_variable === 'minLength')?.value ?? '';
	// check for custom description from target variables, if not use default description
	let descriptionCustom = targetVars?.find((v) => v.target_variable === 'description')?.value ?? '';
	if (descriptionCustom && descriptionCustom.trim() !== '') {
		description = descriptionCustom;
	}

	if (
		(value == undefined || value == null || value == '') &&
		defaultValue != undefined &&
		defaultValue != null &&
		defaultValue != ''
	) {
		value = defaultValue;
	}

	$: validationItem = $validationStore?.simpleTypeValidationItems?.find(
		(i) => i.path === text_field_path
	);

	onMount(async () => {
		const { node: schemaNode } = resolveNode(text_field_path);
		console.log('COLD LOAD schemaNode for', text_field_path, ':', schemaNode);
		// The terminology component has custom validation rules, including for optional fields.
		registerValidationItem(text_field_path, label, required, schemaNode, true);
		validationRegistered = true;
		setValidationLengthConstraints(
			text_field_path,
			minLength === '' ? undefined : Number(minLength),
			maxLength === '' ? undefined : Number(maxLength)
		);
		syncTermValue();
		// console.log(
		// 	'AFTER syncTermValue, validationItem:',
		// 	$validationStore?.simpleTypeValidationItems?.find((i) => i.path === text_field_path)
		// );
	});

	// Keep validation state when component is temporarily unmounted (e.g., collapsed section).

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	function onChangeHandler(e: Event) {
		const nextValue = (e.currentTarget as HTMLInputElement).value;

		value = nextValue;
		updateMetadataStore(text_field_path, nextValue, false, ref != null ? String(ref) : '');

		updateValue(text_field_path);
  dispatch('change');

	}
	function updateValue(_path: string) {
		res = suite(_path);
		updateValidationState(_path, res);
	}

	function syncTermValue() {
		// console.log(
		// 	'🚀 ~ syncTermValue ~ text_field_path:',
		// 	text_field_path,
		// 	'value:',
		// 	value,
		// 	'ref:',
		// 	ref,
		// 	validationRegistered
		// );
		if (!validationRegistered) return;

		updateMetadataStore(
			text_field_path,
			value != undefined && value != null ? value.toString() : '',
			false,
			ref != undefined && ref != null ? ref.toString() : ''
		);

		updateValue(text_field_path);
		validationReady = true;
	}

	$: commonProps = {
		id: path,
		label: label,
		required,
		invalid: validationReady && validationItem ? !validationItem.isValid : false,
		valid: validationReady && validationItem ? validationItem.isValid : false,
		feedback:
			validationItem && validationItem.errorMessage ? validationItem.errorMessage.split('\n') : [],
		description: description,
		showDescription: false,
		showIcon: false,
		disabled: disabled
	};
	// $: console.log('commonProps:', commonProps);
	// $: console.log('RAW STORE:', $validationStore);
</script>

{#key validationReady}
	<span id={text_field_path}>
		<TextArea {...commonProps} bind:value on:input={onChangeHandler} />
	</span>
{/key}
