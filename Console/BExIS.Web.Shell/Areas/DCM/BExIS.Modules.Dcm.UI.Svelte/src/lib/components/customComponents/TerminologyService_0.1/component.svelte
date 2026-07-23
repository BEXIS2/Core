<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import {
		updateMetadataStore,
		getFullConfig,
		getTargetVariablesWithValues,
		ValidationStoreSetSimpleTypeValid,
		resolveNode,
		updateValidationState,
		registerValidationItem,
		getMetadata
	} from '../../utils/metadata/metadataComponentUtils';
	import * as ts4nfdiWidgets from '@ts4nfdi/terminology-service-suite-js';
	import { InputContainer } from '@bexis2/bexis2-core-ui';
	import suite from '$lib/components/utils/metadata/simpleComponentSuite';

	let res = suite.get();
	let componentName: string = 'terminology_v1.0.0';

	export let anchor: string;
	export let path: string = '';

	export let disabled: boolean = false;

	let showDescription: boolean = false;
	let showIcon: boolean = false;

	let config = getFullConfig(componentName, anchor);
	let targetVars = getTargetVariablesWithValues(config);
	console.log('Target Variables with Values:', targetVars, config);

	let term_field_path = targetVars?.find((v) => v.target_variable === 'term_field')?.value ?? '';
	if (term_field_path && term_field_path == anchor.split('.').slice(0, -1).join('.')) {
		term_field_path = anchor;
	}
	let { value, ref, label, description, required } = getMetadata(term_field_path);

	$: updateValidationState(term_field_path, res);

	$: {
		console.log(
			'Updating metadata store with value:',
			value,
			'and ref:',
			ref,
			'for term_field_path:',
			term_field_path
		);
		updateMetadataStore(
			term_field_path,
			value != undefined && value != null ? value.toString() : '',
			false,
			ref != undefined && ref != null ? ref.toString() : ''
		);
		setTimeout(async () => {
			updateValue(value, term_field_path);
		}, 10);
	}

	let initViewExtent = targetVars?.find((v) => v.target_variable === 'initViewExtent')?.value ?? '';
	let parameter = targetVars?.find((v) => v.target_variable === 'parameter')?.value ?? '';
	let TerminologyServiceUrl =
		targetVars?.find((v) => v.target_variable === 'TerminologyServiceUrl')?.value ??
		'https://semanticlookup.zbmed.de/api/';


	let containerElement: HTMLDivElement;
	let data: Array<{ iri: string; label: string }> = [];
	let preselectedItems: Array<{ iri: string; label: string }> = [];
	if (value && ref) preselectedItems = [{ iri: ref.toString(), label: value.toString() }];
	console.log('Preselected Items:', preselectedItems, value, ref);

	onMount(async () => {
		const { node: schemaNode } = resolveNode(term_field_path);
		registerValidationItem(term_field_path, label, required, schemaNode);

		setTimeout(async () => {
			updateValue(value, term_field_path);
		}, 100);

		if (containerElement) {
			console.log('Rendering TS4NFDI AutocompleteWidget...');

			try {
				ts4nfdiWidgets.createAutocomplete(
					{
						api: TerminologyServiceUrl,
						selectionChangedEvent: (props) => {
							value = props.map((item) => item.label).toString();
							ref = props.map((item) => item.iri).toString();
							data = props;
							console.log('Autocomplete selection changed:', data);
						},
						parameter: initViewExtent,
						placeholder: 'Select a term within pre-selected ontologies ..',
						singleSelection: true,
						preselected: preselectedItems,
						className: 'tswidget-input'
					},
					containerElement
				);

				console.log('TS4NFDI AutocompleteWidget rendered.');
			} catch (error) {
				console.error('Error creating autocomplete widget:', error);
			}
		} else {
			console.error('Autocomplete widget container not found!');
		}
	});

	onDestroy(() => {
		ValidationStoreSetSimpleTypeValid(term_field_path, true, '');
	});

	function updateValue(value: any, _path: string) {
		res = suite(_path);

		setTimeout(async () => {
			updateValidationState(_path, res);
		}, 10);
	}

	async function getDescriptionFromAPI(ref: string): Promise<string> {
		// Implement your logic to fetch the description from the API using the ref
		// For example, you can use fetch or any other method to get the description
		// Return the description as a string
		// https://semanticlookup.zbmed.de/ols/api/terms?iri=http:%2F%2Fpurl.obolibrary.org%2Fobo%2FNCBITaxon_146500
		const response = await fetch(
			`https://semanticlookup.zbmed.de/ols/api/terms?iri=${encodeURIComponent(ref)}`
		);
		const data = await response.json();
		console.log('🚀 ~ getDescriptionFromAPI ~ data:', data._embedded.terms[0].description[0]);
		return data._embedded.terms[0].description[0] || 'No description available';
	}

	$: commonProps = {
		id: path,
		label: label,
		required,
		invalid: res.hasErrors(term_field_path),
		valid: res.isValid(term_field_path),
		feedback: res.getErrors(term_field_path),
		description: description,
		showDescription: showDescription,
		disabled: disabled
	};
	console.log('Common Props:', commonProps);
</script>

<InputContainer {...commonProps} {showIcon} on:showDescription on:hideDescription>
	<div
		bind:this={containerElement}
		class="tswidget-host input variant-form-material {commonProps.valid
			? 'input-success'
			: ''} {commonProps.invalid ? 'input-error' : ''} {commonProps.disabled
			? 'opacity-60 pointer-events-none'
			: ''}"
	></div>

	{#if data}
		<ul>
			{#each data as item}
				{#await getDescriptionFromAPI(item.iri) then description}
					<li title={description.toString()} class="text-xs text-gray-500 mt-1">
						({item.iri})
					</li>
				{/await}
			{/each}
		</ul>
	{/if}
</InputContainer>

<style>
	:global(.tswidget-host) {
		display: flex;
		align-items: center;
		width: 100%;
		min-height: 2.625rem;
	}

	:global(.tswidget-host .tswidget-input) {
		width: 100%;
	}

	:global(.tswidget-host .tswidget-input:focus-within) {
		outline: none !important;
		box-shadow: none !important;
		border-color: inherit !important;
	}

	:global(.tswidget-host:focus-within) {
		box-shadow: none !important;
		outline: none !important;
		border-color: inherit !important;
	}

	:global(.tswidget-host .tswidget-input input) {
		width: 100%;
		height: 100%;
		background: transparent;
		border: 0;
		outline: none;
		padding: 0 0.75rem;
		font: inherit;
		color: inherit;
	}

	:global(.tswidget-host .tswidget-input input:focus),
	:global(.tswidget-host .tswidget-input input:focus-visible) {
		outline: none !important;
		box-shadow: none !important;
		border-color: transparent !important;
	}

	:global(.tswidget-host .tswidget-input *:focus),
	:global(.tswidget-host .tswidget-input *:focus-visible) {
		outline: none !important;
		box-shadow: none !important;
		border-color: inherit !important;
		--tw-ring-color: transparent !important;
		--tw-ring-offset-shadow: 0 0 #0000 !important;
		--tw-ring-shadow: 0 0 #0000 !important;
		--euiFormControlStateColor: transparent !important;
		--euiFormControlStateHoverColor: transparent !important;
		--euiFormControlStateWidth: 0 !important;
	}

	:global(.css-hakdsy-euiComboBoxInputWrapper-plainText) {
		outline: none !important;
	}

	:global(.euiComboBox),
	:global(.euiComboBox__inputWrap),
	:global(.euiComboBox__input) {
		outline: none !important;
		box-shadow: none !important;
		border-color: transparent !important;
	}

	:global(.tswidget-host .tswidget-input input::placeholder) {
		opacity: 0.6;
	}

	:global([class*='euiComboBox__inputWrap']) {
		outline: none !important;
		box-shadow: none !important;
	}

	:global([class*='euiComboBox__inputWrap']:focus-within) {
		outline: none !important;
		box-shadow: none !important;
	}
</style>
